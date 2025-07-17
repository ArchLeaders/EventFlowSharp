using System.Runtime.CompilerServices;

namespace EventFlowSharp.ORE;

public readonly unsafe ref struct ResEndian(byte* @base)
{
    public readonly byte* Base = @base;

    public ResEndian(void* @base) : this((byte*)@base)
    {
    }

    public static ResEndian FromRef<T>(ref T @base)
    {
        return new ResEndian(Unsafe.AsPointer(ref @base));
    }
}