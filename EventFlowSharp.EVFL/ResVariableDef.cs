using System.Runtime.InteropServices;
using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Reversible]
public partial struct ResVariableDef
{
    public ResVariableDefValue Value;
    public ushort Count;
    public ResMetaData.DataType Type;

}

[Reversible]
[StructLayout(LayoutKind.Explicit, Size = 8)]
public partial struct ResVariableDefValue
{
    /// <summary>
    /// Also used for booleans. Anything that is != 0 is treated as true.
    /// </summary>
    [FieldOffset(0)]
    public int Int;

    [FieldOffset(0)]
    public float Float;

    [FieldOffset(0)]
    public BinaryPointer<int> IntArray;

    [FieldOffset(0)]
    public BinaryPointer<float> FloatArray;
}
