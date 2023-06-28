using System.Text.RegularExpressions;
using VBCodeCompliancer.NamesGenerator;

namespace VBCodeCompliancer.ChainOfResponsability;
public class HandleMethodNames : AbstractHandler
{
    private readonly Regex _methodRgx = new Regex(@"( |\.)(?<method>\w+)\(");
    private readonly Regex _compliantMethodNameRegex;
    private Dictionary<string, string> _oldNewMetNames;
    private readonly HashSet<string> _blackList; // VB syntax for dictionaries and methods is the same -.-

    public HandleMethodNames()
    {
        _compliantMethodNameRegex = new Regex(@"^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$");
        _oldNewMetNames = new();
        _blackList = new() { "dr", "dic", "dtRow", "iDicResources" };
    }

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
            if (!base.CommentLineRgx.IsMatch(line))
            {
                if (!inSubOrFunction && base.FuncProcBeginRgx.IsMatch(line))
                {
                    inSubOrFunction = true;
                    line = GetMultilineMethodDeclaration(enumerator, line);
                    
                    IEnumerable<string> methodNames = CollectNonCompliantMethodNames(line);
                    // at this point, it will have just the method declaration name
                    if(methodNames.Count() == 1)
                    {
                        line = ReplaceMethodName(line, methodNames.First());
                        replacementsCount += 1;
                    }
                }
                else if (inSubOrFunction)
                {
                    foreach (string m in CollectNonCompliantMethodNames(line))
                    {
                        line = ReplaceMethodName(line, m);
                    }
                }

                if (inSubOrFunction && base.FuncProcEndRgx.IsMatch(line))
                {
                    inSubOrFunction = false;
                }
            }
            
            updatedFile.Add(line);
        }

        base.PrintReplacements(nameof(HandleMethodNames), replacementsCount);

        handlingFile.Content = updatedFile;
        base.Handle(handlingFile);
    }

    private IEnumerable<string> CollectNonCompliantMethodNames(string line)
    {
        Regex methodCallNameRgx = new Regex(@"( |\.)(?<method>\w+)\(");
        List<string> methodsName = new();

        MatchCollection names = methodCallNameRgx.Matches(line);
        foreach (Match match in names)
        {
            string methodName = match.Groups["method"].Value;
            if (!_compliantMethodNameRegex.IsMatch(methodName) && !_blackList.Contains(methodName))
            {
                GenerateNewMethodName(methodName);
                methodsName.Add(methodName);
            }
        }

        return methodsName;
    }

    private void GenerateNewMethodName(string oldName)
    {
        if (!_oldNewMetNames.ContainsKey(oldName))
        {
            string CamelCaseName = CompliantNamesGenerator.Instance.Generate_CamelCaseName(oldName);
            _oldNewMetNames.Add(oldName, CamelCaseName);
        }
    }

    private string ReplaceMethodName(string line, string oldName)
    {
        Regex methodNameRgx = new Regex(@$"(?<=[ |\.]){oldName}(?=\()");
        string newName = _oldNewMetNames[oldName];

        return methodNameRgx.Replace(line, newName);
    }
}
