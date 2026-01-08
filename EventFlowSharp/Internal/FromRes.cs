using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using EventFlowSharp.Events;
using EventFlowSharp.EVFL;
using EventFlowSharp.Extensions;
using EventFlowSharp.ORE;

namespace EventFlowSharp.Internal;

internal static unsafe class FromRes
{
    public static CafeFlowchart Flowchart(ref ResDicEntry entry, ref ResFlowchart flowchart)
    {
        var name = entry.GetKey().ToString();
        ArgumentNullException.ThrowIfNull(name, "Flowchart.Name");

        var result = new CafeFlowchart(name) {
            Actors = new List<CafeActor>(flowchart.ActorCount),
            EntryPoints = new List<CafeEntryPoint>(flowchart.EntryPointCount)
        };

        Dictionary<int, List<CafeActor>>? actorEntryPoints = null;

        var actors = flowchart.Actors.GetPtr();
        for (int i = 0; i < flowchart.ActorCount; i++) {
            var actor = Actor(ref actors[i], out int entryPointIndex);

            if (entryPointIndex > -1) {
                ref var actorsForEntryPoint =
                    ref CollectionsMarshal.GetValueRefOrAddDefault(actorEntryPoints ??= [], entryPointIndex, out bool exists);

                if (!exists || actorsForEntryPoint is null) {
                    actorsForEntryPoint = [];
                }

                actorsForEntryPoint.Add(actor);
            }

            result.Actors.Add(actor);
        }

        using var eventSpanOwner = Events(ref flowchart, result);
        var events = eventSpanOwner.Span;

        var entryPointResDicEntries = flowchart.EntryPointNames.GetPtr()->GetEntries() + 1;
        var entryPoints = flowchart.EntryPoints.GetPtr();
        for (int i = 0; i < flowchart.EntryPointCount; i++) {
            var entryPoint = EntryPoint(ref entryPointResDicEntries[i], ref entryPoints[i], events);
            result.EntryPoints.Add(entryPoint);

            if (actorEntryPoints?.TryGetValue(i, out var actorsForEntryPoint) == true) {
                foreach (var actor in actorsForEntryPoint) {
                    actor.EntryPoint = entryPoint;
                }
            }
        }

        return result;
    }

    public static CafeActor Actor(ref ResActor actor, out int entryPointIndex)
    {
        string actorName = actor.Name.Get().String;
        entryPointIndex = actor.EntryPointIndex;

        var result = new CafeActor {
            Name = actorName,
            SecondaryName = actor.SecondaryName.Get().String,
            ArgumentName = actor.ArgumentName.Get().String,
            CutNumber = actor.CutNumber,
            ActionList = new List<CafeAction>(actor.ActionsCount),
            QueryList = new List<CafeQuery>(actor.QueriesCount),
        };

        var actionList = actor.Actions.GetPtr();
        for (int i = 0; i < actor.ActionsCount; i++) {
            result.ActionList.Add(new CafeAction {
                Action = actionList[i].Name.Get().String,
                Actor = result
            });
        }

        var queryList = actor.Queries.GetPtr();
        for (int i = 0; i < actor.QueriesCount; i++) {
            result.QueryList.Add(new CafeQuery {
                Query = queryList[i].Name.Get().String,
                Actor = result
            });
        }

        result.Parameters = Parameters(ref actor.Params.Get());

        return result;
    }

    public static CafeEntryPoint EntryPoint(ref ResDicEntry entry, ref ResEntryPoint entryPoint, Span<CafeEvent> events)
    {
        var name = entry.GetKey().ToString();
        ArgumentNullException.ThrowIfNull(name, "EntryPoint.Name");

        return new CafeEntryPoint(name) {
            Event = events[entryPoint.MainEventIndex],
            VariableDefs = VariableDefs(ref entryPoint, name)
        };
    }

    public static Dictionary<string, CafeVariableDef> VariableDefs(ref ResEntryPoint entryPoint, string entryPointName)
    {
        if (entryPoint.VariableDefsCount <= 0) {
            return [];
        }
        
        Dictionary<string, CafeVariableDef> results = new(entryPoint.VariableDefsCount);

        var variableDefsDicEntries = entryPoint.VariableDefNames.GetPtr()->GetEntries() + 1;
        var variableDefs = entryPoint.VariableDefs.GetPtr();
        for (int i = 0; i < entryPoint.VariableDefsCount; i++) {
            var key = variableDefsDicEntries[i].GetKey().ToString();
            ArgumentNullException.ThrowIfNull(key, $"EntryPoint[{entryPointName}].VariableDefs.Key");
            results.Add(key, VariableDef(ref variableDefs[i]));
        }

        return results;
    }

    public static CafeVariableDef VariableDef(ref ResVariableDef variableDef)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return variableDef.Type switch {
            ResMetaData.DataType.Float => variableDef.Value.Float,
            ResMetaData.DataType.Int => variableDef.Value.Int,
            ResMetaData.DataType.FloatArray => ToArray(variableDef.Value.FloatArray.GetPtr(), variableDef.Count),
            ResMetaData.DataType.IntArray => ToArray(variableDef.Value.IntArray.GetPtr(), variableDef.Count),
            _ => throw new NotSupportedException($"Unsupported VariableDef DataType: {variableDef.Type}")
        };

        T[] ToArray<T>(T* values, int count) where T : unmanaged
        {
            var result = new T[count];
            for (int i = 0; i < count; i++) {
                result[i] = values[i];
            }

            return result;
        }
    }

    public static SpanOwner<CafeEvent> Events(ref ResFlowchart flowchart, CafeFlowchart result)
    {
        var results = SpanOwner<CafeEvent>.Allocate(flowchart.EventCount);
        var events = results.Span;

        var resEvents = flowchart.Events.GetPtr();
        for (int i = 0; i < flowchart.EventCount; i++) {
            ref var resEvent = ref resEvents[i];
            var name = resEvent.Name.Get().String;
            events[i] = resEvent.Type switch {
                ResEvent.EventType.Action => ActionEvent(name, resEvent.ActionEvent, result),
                ResEvent.EventType.Switch => SwitchEvent(name, resEvent.SwitchEvent, result),
                ResEvent.EventType.Fork => ForkEvent(name),
                ResEvent.EventType.Join => JoinEvent(name),
                ResEvent.EventType.SubFlow => SubFlowEvent(name, resEvent.SubFlowEvent),
                _ => throw new NotSupportedException($"Unsupported event type: {resEvent.Type}")
            };
        }

        for (int i = 0; i < flowchart.EventCount; i++) {
            ref var resEvent = ref resEvents[i];

            switch (events[i]) {
                case ILinearEvent linearEvent:
                    linearEvent.NextEvent = resEvent.ActionEvent.NextEventIndex is -1
                        ? null : events[resEvent.ActionEvent.NextEventIndex];
                    break;
                case CafeForkEvent forkEvent:
                    var forkIndices = resEvent.ForkEvent.ForkEventIndices.GetPtr();
                    for (int pi = 0; pi < resEvent.ForkEvent.ForkCount; pi++) {
                        forkEvent.Branches.Add(events[forkIndices[pi]]);
                    }

                    break;
                case CafeSwitchEvent switchEvent:
                    var cases = resEvent.SwitchEvent.Cases.GetPtr();
                    for (int caseIndex = 0; caseIndex < resEvent.SwitchEvent.CaseCount; caseIndex++) {
                        ref var switchCase = ref cases[caseIndex];
                        switchEvent.Cases.Add(new CafeSwitchEventCase {
                            Value = (int)switchCase.Value,
                            Event = events[switchCase.EventIndex]
                        });
                    }

                    break;
            }
        }

        return results;
    }

    public static CafeActionEvent ActionEvent(string name, ResActionEvent actionEvent, CafeFlowchart result)
    {
        return new CafeActionEvent {
            Name = name,
            Action = result.Actors[actionEvent.ActorIndex].ActionList[actionEvent.ActorActionIndex],
            Parameters = Parameters(ref actionEvent.Params.Get())
        };
    }

    public static CafeSwitchEvent SwitchEvent(string name, ResSwitchEvent switchEvent, CafeFlowchart result)
    {
        return new CafeSwitchEvent {
            Name = name,
            Query = result.Actors[switchEvent.ActorIndex].QueryList[switchEvent.ActorQueryIndex],
            Parameters = Parameters(ref switchEvent.Params.Get())
        };
    }

    public static CafeForkEvent ForkEvent(string name)
    {
        return new CafeForkEvent {
            Name = name,
            JoinEvent = null!
        };
    }

    public static CafeJoinEvent JoinEvent(string name)
    {
        return new CafeJoinEvent {
            Name = name
        };
    }

    public static CafeSubFlowEvent SubFlowEvent(string name, ResSubFlowEvent subFlowEvent)
    {
        return new CafeSubFlowEvent {
            Name = name,
            Flowchart = subFlowEvent.SubFlowFlowchart.Get().String,
            EntryPoint = subFlowEvent.SubFlowEntryPoints.Get().String,
            Parameters = Parameters(ref subFlowEvent.Params.Get())
        };
    }

    private static CafeUserData Parameters(ref ResMetaData metaData)
    {
        if (Unsafe.IsNullRef(ref metaData)) {
            return [];
        }
        
        if (metaData.Type != ResMetaData.DataType.Container) {
            throw new InvalidDataException($"Expected container type but found {metaData.Type}");
        }

        CafeUserData result = [];

        var keys = metaData.Dictionary.Get().GetEntries() + 1;
        fixed (BinaryPointer<ResMetaData>* valuePointers = &metaData.Value.Container) {
            for (int i = 0; i < metaData.ItemCount; i++) {
                var key = keys[i].GetKey().ToString();
                ref var value = ref valuePointers[i].Get();

                ArgumentNullException.ThrowIfNull(key);

                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                result[key] = value.Type switch {
                    ResMetaData.DataType.Argument
                        => new CafeUserDataEntry(value.Value.String.Get().String, isArgument: true),
                    ResMetaData.DataType.Bool
                        => new CafeUserDataEntry(value.Value.Int != 0),
                    ResMetaData.DataType.Int
                        => new CafeUserDataEntry(value.Value.Int),
                    ResMetaData.DataType.Float
                        => new CafeUserDataEntry(value.Value.Float),
                    ResMetaData.DataType.String
                        => new CafeUserDataEntry(value.Value.String.Get().String),
                    ResMetaData.DataType.WString
                        => new CafeUserDataEntry(value.Value.WideString.Get().String, isWideString: true),
                    ResMetaData.DataType.ActorIdentifier
                        => new CafeUserDataEntry(
                            (name: value.Value.Actor.Name.Get().String, subName: value.Value.Actor.SubName.Get().String)
                        ),
                    ResMetaData.DataType.BoolArray
                        => new CafeUserDataEntry(GetArray(ref value, x => x.Int != 0)),
                    ResMetaData.DataType.IntArray
                        => new CafeUserDataEntry(GetArray(ref value, x => x.Int)),
                    ResMetaData.DataType.FloatArray
                        => new CafeUserDataEntry(GetArray(ref value, x => x.Float)),
                    ResMetaData.DataType.StringArray
                        => new CafeUserDataEntry(GetArray(ref value, x => x.String.Get().String)),
                    ResMetaData.DataType.WStringArray
                        => new CafeUserDataEntry(GetArray(ref value, x => x.WideString.Get().String), isWideString: true),
                    _ => throw new InvalidDataException($"Invalid EVFL parameter type: {value.Type}")
                };
            }
        }

        return result;
    }

    private static T[] GetArray<T>(ref ResMetaData parameter, Func<ResMetaDataValue, T> select)
    {
        var result = new T[parameter.ItemCount];
        fixed (ResMetaDataValue* first = &parameter.Value) {
            for (int i = 0; i < parameter.ItemCount; i++) {
                result[i] = select(first[i]);
            }
        }

        return result;
    }
}