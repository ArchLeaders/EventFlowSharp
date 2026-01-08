using System.Text;

namespace EventFlowSharp;

public sealed class CafeVariableDefs : Dictionary<string, CafeVariableDef>
{
    public string ToMarkup()
    {
        StringBuilder output = new();

        foreach (var (name, value) in this) {
            output.Append(name);
            output.Append(": ");
            output.AppendLine(value.ToString());
        }

        return output.ToString();
    }
    
    public static CafeVariableDefs FromMarkup(string markup)
    {
        throw new NotImplementedException();
    }
    
    public string ToMarkupInline()
    {
        StringBuilder output = new("{ ");

        int i = 0;
        foreach (var (name, value) in this) {
            output.Append(name);
            output.Append(": ");
            output.Append(value);

            if (++i != Count) {
                output.Append(", ");
            }
        }
        
        output.Append(" }");

        return output.ToString();
    }

    public override string ToString()
    {
        return ToMarkupInline();
    }
}