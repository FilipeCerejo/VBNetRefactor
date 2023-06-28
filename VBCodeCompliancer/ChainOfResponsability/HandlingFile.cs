using System.Text;

namespace VBCodeCompliancer.ChainOfResponsability;
public class HandlingFile
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string FilePath { get; init; }
    public Encoding Encoding { get; init; }
    public IEnumerable<string> Content { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
