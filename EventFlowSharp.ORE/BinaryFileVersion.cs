namespace EventFlowSharp.ORE;

public readonly struct BinaryFileVersion : IEquatable<BinaryFileVersion>
{
    public readonly byte Major;
    public readonly byte Minor;
    public readonly byte Patch;
    public readonly byte Sub;

    public BinaryFileVersion()
    {
    }
    
    public BinaryFileVersion(byte major, byte minor, byte patch, byte sub)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Sub = sub;
    }

    public static bool operator ==(BinaryFileVersion left, BinaryFileVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BinaryFileVersion left, BinaryFileVersion right)
    {
        return !left.Equals(right);
    }

    public bool Equals(BinaryFileVersion other)
    {
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch && Sub == other.Sub;
    }

    public override bool Equals(object? obj)
    {
        return obj is BinaryFileVersion other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, Sub);
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}.{Sub}";
    }
}