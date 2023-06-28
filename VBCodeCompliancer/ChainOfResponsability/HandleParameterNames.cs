using System.Text.RegularExpressions;
using VBCodeCompliancer.NamesGenerator;

namespace VBCodeCompliancer.ChainOfResponsability;
public class HandleParameterNames : AbstractHandler
{
    private Regex _compliantParamNameRegex;
    private Dictionary<string, string> _oldNewParams;

    public HandleParameterNames()
    {
        _compliantParamNameRegex = new Regex("^[a-z][a-z0-9]*([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$");
        _oldNewParams = new();
    }

    public override void Handle(HandlingFile handlingFile)
    {
        base.CanHandle(handlingFile);

        bool inSubOrFunction = false;
        List<string> parameters = new();
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
                    line = base.GetMultilineMethodDeclaration(enumerator, line);
                    parameters.AddRange(CollectNonCompliantParametersNames(line));
                }

                if (inSubOrFunction)
                {
                    foreach (string p in parameters)
                    {
                        line = ReplaceNonCompliantParameter(line, p, out bool replaced);
                        replacementsCount += replaced ? 1 : 0;
                    }
                }

                if (inSubOrFunction && base.FuncProcEndRgx.IsMatch(line))
                {
                    inSubOrFunction = false;
                    parameters.Clear();
                }
            }

            updatedFile.Add(line);
        }

        base.PrintReplacements(nameof(HandleParameterNames), replacementsCount);

        handlingFile.Content = updatedFile;

        base.Handle(handlingFile);
    }

    private List<string> CollectNonCompliantParametersNames(string line)
    {
        List<string> parameters = new();
        Regex paramRgx = new Regex(@"(ByVal|ByRef)\s+\b(?<param>\w+)\b\sAs");

        MatchCollection matches = paramRgx.Matches(line);

        foreach (Match match in matches)
        {
            string parameter = match.Groups["param"].Value;

            if(!_compliantParamNameRegex.IsMatch(parameter))
            {
                parameters.Add(parameter);

                GenerateNewParameterName(parameter);
            }
        }

        return parameters;
    }

    private void GenerateNewParameterName(string oldName)
    {
        if (!_oldNewParams.ContainsKey(oldName))
        {
            string camelCaseName = CompliantNamesGenerator.Instance.Generate_camelCaseName(oldName);
            _oldNewParams.Add(oldName, camelCaseName);
        }
    }

    private string ReplaceNonCompliantParameter(string line, string oldName, out bool replaced)
    {
        replaced = false;
        //avoid string literals
        Regex paramNameRgx = new Regex(@$"(?<![""\.\w*])({oldName})(?![""\w*])");

        if (paramNameRgx.Matches(line).Count > 0)
        {
            replaced = true;

            string compliantName = _oldNewParams[oldName];
            return paramNameRgx.Replace(line, compliantName);
        }

        return line;
    }
}
