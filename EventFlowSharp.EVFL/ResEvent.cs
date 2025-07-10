using System.Runtime.InteropServices;
using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Reversable]
[StructLayout(LayoutKind.Explicit)]
public partial struct ResEvent
{
    [FieldOffset(0)]
    public BinaryPointer<BinaryString<byte>> Name;
    
    [FieldOffset(8)]
    public EventType Type;
    
    [FieldOffset(10)]
    public ResActionEvent ActionEvent;
    
    [FieldOffset(10)]
    public ResSwitchEvent SwitchEvent;
    
    [FieldOffset(10)]
    public ResJoinEvent ForkEvent;
    
    [FieldOffset(10)]
    public ResJoinEvent JoinEvent;
    
    [FieldOffset(10)]
    public ResSubFlowEvent SubFlowEvent;
        
    public enum EventType : byte
    {
        Action,
        Switch,
        Fork,
        Join,
        SubFlow
    }
}

[Reversable]
[StructLayout(LayoutKind.Sequential, Size = 30)]
public partial struct ResActionEvent
{
    public ushort NextEventIndex;
    public ushort ActorIndex;
    public ushort ActorActionIndex;
    public BinaryPointer<ResMetaData> Params;
}

[Reversable]
[StructLayout(LayoutKind.Sequential, Size = 30)]
public partial struct ResSwitchEvent
{
    public ushort CaseCount;
    public ushort ActorIndex;
    public ushort ActorActionIndex;
    public BinaryPointer<ResMetaData> Params;
    public BinaryPointer<ResCase> Cases;
}

[Reversable]
[StructLayout(LayoutKind.Explicit, Size = 30)]
public partial struct ResForkEvent
{
    [FieldOffset(0)]
    public ushort ForkCount;
    
    [FieldOffset(2)]
    public ushort JoinEventIndex;

    [FieldOffset(6)]
    public BinaryPointer<ushort> ForkEventIndices;
}

[Reversable]
[StructLayout(LayoutKind.Sequential, Size = 30)]
public partial struct ResJoinEvent
{
    public ushort NextEventIndex;
}

[Reversable]
[StructLayout(LayoutKind.Explicit, Size = 30)]
public partial struct ResSubFlowEvent
{
    [FieldOffset(0)]
    public ushort NextEventIndex;
    
    [FieldOffset(6)]
    public BinaryPointer<ResMetaData> Params;
    
    [FieldOffset(14)]
    public BinaryPointer<BinaryString<byte>> SubFlowFlowchart;
    
    [FieldOffset(22)]
    public BinaryPointer<BinaryString<byte>> SubFlowEntryPoints;
}