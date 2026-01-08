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

        // TODO: Parameters

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
        Dictionary<string, CafeVariableDef> results = new(entryPoint.VariableDefsCount);

        var variableDefsDicEntries = entryPoint.VariableDefNames.GetPtr()->GetEntries();
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
        throw new NotImplementedException();
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
                    linearEvent.NextEvent = events[resEvent.ActionEvent.NextEventIndex];
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
            // TODO: Parameters 
        };
    }

    public static CafeSwitchEvent SwitchEvent(string name, ResSwitchEvent switchEvent, CafeFlowchart result)
    {
        return new CafeSwitchEvent {
            Name = name,
            Query = result.Actors[switchEvent.ActorIndex].QueryList[switchEvent.ActorQueryIndex],
            // TODO: Parameters 
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
            EntryPoint = subFlowEvent.SubFlowEntryPoints.Get().String
            // TODO: Parameters 
        };
    }
}