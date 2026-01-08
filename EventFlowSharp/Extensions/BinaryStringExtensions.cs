using System.Text;
using CommunityToolkit.HighPerformance;
using EventFlowSharp.ORE;

namespace EventFlowSharp.Extensions;

public static class BinaryStringExtensions
{
    extension(ref BinaryString<byte> binStr)
    {
        public string String => binStr.IsEmpty ? string.Empty : Encoding.UTF8.GetString(binStr.Data);
    }
    
    extension(ref BinaryString<char> binStr)
    {
        public string String => Encoding.Unicode.GetString(binStr.Data.Cast<char, byte>());
    }
}