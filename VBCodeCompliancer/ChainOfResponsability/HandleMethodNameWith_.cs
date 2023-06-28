using System.Text;
using System.Text.RegularExpressions;

namespace VBCodeCompliancer.ChainOfResponsability;
public class HandleMethodNameWith_ : AbstractHandler
{
    private readonly Regex _methodRgx = new Regex(@"( |\.)(?<method>\w+)\(");

    public override void Handle(HandlingFile handlingFile)
    {
        base.CanHandle(handlingFile);

        bool inSubOrFunction = false;
        List<string> updatedFile = new();
        int replacementsCount = 0;

        var enumerator = handlingFile.Content.GetEnumerator();
        while (enumerator.MoveNext())
        {
            string line = enumerator.Current;

            // ignore commented lines
            if (base.CommentLineRgx.IsMatch(line))
            {
                updatedFile.Add(line);
                continue;
            }

            if (!inSubOrFunction && base.FuncProcBeginRgx.IsMatch(line))
            {
                inSubOrFunction = true;
                line = GetMultilineMethodDeclaration(enumerator, line);
                line = ReplaceMethodName(line, out bool replaced);
                replacementsCount += (replaced ? 1 : 0);
            }
            else if (inSubOrFunction)
            {
                foreach(string m in FindMethods(line))
                {
                    line = ReplaceMethodCall(line, m);
                }
            }

            if (inSubOrFunction && base.FuncProcEndRgx.IsMatch(line))
            {
                inSubOrFunction = false;
            }

            updatedFile.Add(line);
        }

        base.PrintReplacements(nameof(HandleMethodNameWith_), replacementsCount);

        handlingFile.Content = updatedFile;
        base.Handle(handlingFile);
    }

    private IEnumerable<string> FindMethods(string line)
    {
        List<string> methodsName = new();

        MatchCollection names = _methodRgx.Matches(line);
        foreach(Match match in names)
        {
            methodsName.Add(match.Groups["method"].Value);
        }

        return methodsName;
    }

    private string ReplaceMethodCall(string line, string oldName)
    {
        Regex methodNameRgx = GetMethodNameRgx(oldName);
        string newName = NewMethodName(oldName);

        if (!oldName.Equals(newName))
        {
            return methodNameRgx.Replace(line, newName);
        }

        return line;
    }

    private Regex GetMethodNameRgx(string methodName) => new Regex(@$"(?<=[ |\.]){methodName}(?=\()");

    ////MakeSomething_LOG       -> MakeSomethingLog
    ////ESC_ADD_SomethingElse   -> EscAddSomethingElse
    private string ReplaceMethodName(string line, out bool replaced)
    {
        replaced = false;

        string oldName = _methodRgx.Match(line).Groups["method"].Value;
        string newName = NewMethodName(oldName);

        if(!oldName.Equals(newName))
        {
            Regex methodNameRgx = GetMethodNameRgx(oldName);
            replaced = true;
            return methodNameRgx.Replace(line, newName);
        }

        return line;
    }

    private string NewMethodName(string oldName)
    {
        string[] parts = oldName.Split('_');

        StringBuilder sb = new();

        foreach (string part in parts)
        {
            string first;
            string second;

            if(string.IsNullOrEmpty(part))
            {
                sb.Append('_');
            }
            else if (part.Length <= 3)
            {
                first = part[0].ToString().ToUpper();
                second = "";
                if (part.Length > 1)
                {
                    second = part[1..].ToString().ToLower();
                }
                sb.Append(first + second);
            }
            else
            {
                sb.Append(part);
            }
        }

        return sb.ToString();
    }
}
