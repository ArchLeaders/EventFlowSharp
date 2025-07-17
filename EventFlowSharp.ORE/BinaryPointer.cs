using System.Runtime.CompilerServices;
using Entish;

namespace EventFlowSharp.ORE;

public unsafe struct BinaryPointer<T> : ISwappable<BinaryPointer<T>> where T : unmanaged
{
    public ulong OffsetOrPtr;

    public void Set(ref T value)
    {
        fixed (T* ptr = &value) {
            OffsetOrPtr = (ulong)ptr;
        }
    }

    /// <summary>
    /// Only use this after relocation.
    /// </summary>
    /// <returns></returns>
    public ref T Get()
    {
        return ref MemUtils.GetRelativeTo<T>((void*)OffsetOrPtr, 0);
    }

    /// <summary>
    /// Only use this after relocation.
    /// </summary>
    /// <returns></returns>
    public T* GetPtr()
    {
        return (T*)OffsetOrPtr;
    }
    
    public void Clear()
    {
        OffsetOrPtr = 0;
    }

    public void SetOffset(void* owner, ref T value)
    {
        fixed (T* valuePtr = &value) {
            OffsetOrPtr = (uint)(valuePtr - (ulong)owner);
        }
    }

    public ref T ToRef(void* owner)
    {
        if (OffsetOrPtr == 0) {
            Unsafe.NullRef<T>();
        }
        
        return ref MemUtils.GetRelativeTo<T>(owner, (uint)OffsetOrPtr);
    }

    public T* ToPtr(void* owner)
    {
        if (OffsetOrPtr == 0) {
            return null;
        }
        
        return (T*)((byte*)owner + OffsetOrPtr);
    }

    public void Relocate(void* owner) => Set(ref ToRef(owner));
    
    public void UnRelocate(void* owner) => SetOffset(owner, ref Get());
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap(BinaryPointer<T>* value)
    {
        EndianUtils.Swap(&value->OffsetOrPtr);
    }
}