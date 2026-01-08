using System.Runtime.CompilerServices;
using Entish;
using EventFlowSharp.EVFL;
using EventFlowSharp.Internal;

[assembly: InternalsVisibleTo("EventFlowSharp.Console")]

namespace EventFlowSharp;

public sealed class CafeEventFlowFile
{
    public string? Name { get; set; }

    public int Alignment { get; set; } = 8;

    public List<CafeFlowchart> Flowcharts { get; set; } = [];

    public CafeEventFlowFile()
    {
    }

    public unsafe CafeEventFlowFile(Span<byte> buffer)
    {
        ResEventFlowFile* evfl = GetResPtr(buffer);

        if (evfl->Header.Magic != ResEventFlowFile.Magic) {
            throw new InvalidDataException(
                $"Invalid EventFlowFile magic. Expected 'BFEVFL..' but found '0x{evfl->Header.Magic:x}'");
        }

        if (evfl->Header.Version != ResEventFlowFile.Version) {
            throw new InvalidDataException(
                $"Invalid EventFlowFile version. Expected '0.3.0.0' but found '{evfl->Header.Version}'");
        }

        if (EndianUtils.ShouldSwap(evfl->Header.ByteOrder)) {
            ResEventFlowFile.Swap(evfl);
        }

        Name = evfl->Header.GetFileName().ToString();
        Alignment = 1 << evfl->Header.Alignment;

        // Rewrite offsets as pointers
        // relative to the input buffer
        evfl->Relocate();

        // Flowcharts
        var flowchartNameDicEntries = evfl->FlowchartNames.GetPtr()->GetEntries() + 1;
        var flowcharts = evfl->Flowcharts.GetPtr()->GetPtr();
        for (int i = 0; i < evfl->FlowchartCount; i++) {
            Flowcharts.Add(
                FromRes.Flowchart(ref flowchartNameDicEntries[i], ref flowcharts[i])
            );
        }

        // Timelines
    }

    public void Write(Stream output)
    {
    }

    public static ref ResEventFlowFile GetRes(Span<byte> buffer)
        => ref Unsafe.As<byte, ResEventFlowFile>(ref buffer[0]);

    public static unsafe ResEventFlowFile* GetResPtr(Span<byte> buffer)
        => (ResEventFlowFile*)Unsafe.AsPointer(ref GetRes(buffer));
}