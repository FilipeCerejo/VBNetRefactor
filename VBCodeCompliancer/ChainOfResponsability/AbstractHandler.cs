using System.Text;
using System.Text.RegularExpressions;

namespace VBCodeCompliancer.ChainOfResponsability;
public abstract class AbstractHandler : IHandler
{
    private IHandler? _next;
    protected readonly Regex FuncProcBeginRgx = new Regex(@"(Sub|Function)\s+\w+(?=\()");
    protected readonly Regex FuncProcEndRgx = new Regex(@"End\s+(Sub|Function)");
    protected readonly Regex CommentLineRgx = new Regex(@" +'");

    public IHandler SetNext(IHandler iHandler)
    {
        _next = iHandler;
        return iHandler;
    }

    public virtual void Handle(HandlingFile handlingFile)
    {
        if(_next is not null)
        {
            _next.Handle(handlingFile);
        }
        else
        {
            SaveFile(handlingFile);
        }
    }

    private void SaveFile(HandlingFile handlingFile)
    {
        using (StreamWriter sw = new StreamWriter(handlingFile.FilePath, false, handlingFile.Encoding))
        {
            foreach (string line in handlingFile.Content)
            {
                sw.WriteLine(line);
            }
        }
    }

    protected void CanHandle(HandlingFile handlingFile)
    {
        if (handlingFile is null || handlingFile.Content is null)
        {
            throw new ArgumentNullException();
        }
    }

    protected string GetMultilineMethodDeclaration(IEnumerator<string> enumerator, string line)
    {
        if (!line.Contains(")"))
        {
            StringBuilder sb = new();
            sb.AppendLine(enumerator.Current);
            while (enumerator.MoveNext() && !enumerator.Current.Contains(")"))
            {
                sb.AppendLine(enumerator.Current);
            }
            sb.Append(enumerator.Current);
            return sb.ToString();
        }

        return line;
    }

    protected void PrintReplacements(string className, int replacementsCount)
    {
        if (replacementsCount > 0)
            Utils.PrintHeader($"{className}: {replacementsCount} replacement{(replacementsCount != 1 ? "s" : string.Empty)}", 4);
    }
}
