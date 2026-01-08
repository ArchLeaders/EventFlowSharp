using System.Globalization;
using EventFlowSharp.ORE;

namespace EventFlowSharp;

public sealed class CafeUserDataEntry
{
    public ResMetaData.DataType Type { get; set; }

    public bool Bool {
        get => Int != 0;
        set => Int = value ? 1 : 0;
    }
    
    public int Int { get; set; }

    public float Float { get; set; }

    public string? String { get; set; }

    public (string Name, string SubName) ActorIdentifier { get; set; }

    public bool[]? BoolArray { get; set; }

    public int[]? IntArray { get; set; }

    public float[]? FloatArray { get; set; }

    public string[]? StringArray { get; set; }

    public static implicit operator CafeUserDataEntry(bool value) => new(value);
    public CafeUserDataEntry(bool value)
    {
        Bool = value;
        Type = ResMetaData.DataType.Bool;
    }

    public static implicit operator CafeUserDataEntry(int value) => new(value);
    public CafeUserDataEntry(int value)
    {
        Int = value;
        Type = ResMetaData.DataType.Int;
    }

    public static implicit operator CafeUserDataEntry(float value) => new(value);
    public CafeUserDataEntry(float value)
    {
        Float = value;
        Type = ResMetaData.DataType.Float;
    }

    public static implicit operator CafeUserDataEntry(string value) => new(value);
    public CafeUserDataEntry(string value, bool isWideString = false, bool isArgument = false)
    {
        String = value;
        Type = isArgument ? ResMetaData.DataType.Argument :
            isWideString ? ResMetaData.DataType.WString : ResMetaData.DataType.String;
    }

    public static implicit operator CafeUserDataEntry((string name, string subName) actorIdentifier) => new(actorIdentifier);
    public CafeUserDataEntry((string name, string subName) actorIdentifier)
    {
        ActorIdentifier = actorIdentifier;
        Type = ResMetaData.DataType.ActorIdentifier;
    }
    
    public static implicit operator CafeUserDataEntry(bool[] value) => new(value);
    public CafeUserDataEntry(bool[] value)
    {
        BoolArray = value;
        Type = ResMetaData.DataType.BoolArray;
    }
    
    public static implicit operator CafeUserDataEntry(int[] value) => new(value);
    public CafeUserDataEntry(int[] value)
    {
        IntArray = value;
        Type = ResMetaData.DataType.IntArray;
    }
    
    public static implicit operator CafeUserDataEntry(float[] value) => new(value);
    public CafeUserDataEntry(float[] value)
    {
        FloatArray = value;
        Type = ResMetaData.DataType.FloatArray;
    }
    
    public static implicit operator CafeUserDataEntry(string[] value) => new(value);
    public CafeUserDataEntry(string[] value, bool isWideString = false)
    {
        StringArray = value;
        Type = isWideString ? ResMetaData.DataType.WStringArray : ResMetaData.DataType.StringArray;
    }

    /// <summary>
    /// Checks if the data is valid for the <see cref="Type"/>. Float, Int and ActorIdentifier will always be valid.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidDataException">Throws an InvalidDataException if the type is an unexpected value</exception>
    public bool IsValid()
    {
        return Type switch {
            ResMetaData.DataType.Int or ResMetaData.DataType.Float => true,
            ResMetaData.DataType.String or ResMetaData.DataType.WString
                or ResMetaData.DataType.Argument => String is not null,
            ResMetaData.DataType.ActorIdentifier => true,
            ResMetaData.DataType.BoolArray => BoolArray is not null,
            ResMetaData.DataType.IntArray => IntArray is not null,
            ResMetaData.DataType.FloatArray => FloatArray is not null,
            ResMetaData.DataType.StringArray or ResMetaData.DataType.WStringArray => StringArray is not null,
            _ => throw new InvalidDataException($"Invalid parameter type: {Type}")
        };
    }

    public override string ToString()
    {
        if (!IsValid()) {
            throw new InvalidDataException($"Invalid parameter data for the defined type: {Type}");
        }
        
        return Type switch {
            ResMetaData.DataType.Bool => Bool.ToString(),
            ResMetaData.DataType.Int => Int.ToString(),
            ResMetaData.DataType.Float => Float.ToString(CultureInfo.InvariantCulture),
            ResMetaData.DataType.String or ResMetaData.DataType.WString or ResMetaData.DataType.Argument => String!,
            ResMetaData.DataType.ActorIdentifier when ActorIdentifier is var (name, subName) => $"{name}[{subName}]",
            ResMetaData.DataType.BoolArray => $"[{string.Join(", ", BoolArray!)}]",
            ResMetaData.DataType.IntArray => $"[{string.Join(", ", IntArray!)}]",
            ResMetaData.DataType.FloatArray => $"[{string.Join(", ", FloatArray!)}]",
            ResMetaData.DataType.StringArray or ResMetaData.DataType.WStringArray => $"[{string.Join(", ", StringArray!)}]",
            _ => throw new InvalidDataException($"Invalid parameter type: {Type}")
        };
    }
}