using System.Globalization;
using EventFlowSharp.ORE;

namespace EventFlowSharp;

public sealed class CafeVariableDef
{
    public ResMetaData.DataType Type { get; set; }
    
    public int Int { get; set; }

    public float Float { get; set; }
    
    public int[]? IntArray { get; set; }

    public float[]? FloatArray { get; set; }
    
    public static implicit operator CafeVariableDef(int value) => new(value);
    public CafeVariableDef(int value)
    {
        Int = value;
        Type = ResMetaData.DataType.Int;
    }

    public static implicit operator CafeVariableDef(float value) => new(value);
    public CafeVariableDef(float value)
    {
        Float = value;
        Type = ResMetaData.DataType.Float;
    }
    
    public static implicit operator CafeVariableDef(int[] value) => new(value);
    public CafeVariableDef(int[] value)
    {
        IntArray = value;
        Type = ResMetaData.DataType.IntArray;
    }
    
    public static implicit operator CafeVariableDef(float[] value) => new(value);
    public CafeVariableDef(float[] value)
    {
        FloatArray = value;
        Type = ResMetaData.DataType.FloatArray;
    }
    
    /// <summary>
    /// Checks if the data is valid for the <see cref="Type"/>. Float and Int will always be valid.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidDataException">Throws an InvalidDataException if the type is an unexpected value</exception>
    public bool IsValid()
    {
        return Type switch {
            ResMetaData.DataType.Int or ResMetaData.DataType.Float => true,
            ResMetaData.DataType.IntArray => IntArray is not null,
            ResMetaData.DataType.FloatArray => FloatArray is not null,
            _ => throw new InvalidDataException($"Invalid VariableDef type: {Type}")
        };
    }

    public override string ToString()
    {
        if (!IsValid()) {
            throw new InvalidDataException($"Invalid VariableDef data for the defined type: {Type}");
        }
        
        return Type switch {
            ResMetaData.DataType.Int => Int.ToString(),
            ResMetaData.DataType.Float => Float.ToString(CultureInfo.InvariantCulture),
            ResMetaData.DataType.IntArray => $"[{string.Join(", ", IntArray!)}]",
            ResMetaData.DataType.FloatArray => $"[{string.Join(", ", FloatArray!)}]",
            _ => throw new InvalidDataException($"Invalid VariableDef type: {Type}")
        };
    }
}