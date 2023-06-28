using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using VBCodeCompliancer.NamesGenerator;

namespace VBCodeCompliancer.ChainOfResponsability;
public class HandleVariableNames : AbstractHandler
{
    private Regex _compliantVarNameRegex;
    private Dictionary<string, string> _oldNewVars;

    public HandleVariableNames()
    {
        _compliantVarNameRegex = new Regex("^[a-z][a-z0-9]*([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$");
        _oldNewVars = new();
    }

    public override void Handle(HandlingFile handlingFile)
    {
        base.CanHandle(handlingFile);

        bool inSubOrFunction = false;
        HashSet<string> variables = new();
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
                }
                else if (inSubOrFunction)
                {
                    CollectNonCompliantVariablesNames(variables, line);

                    foreach (string variable in variables)
                    {
                        line = ReplaceNonCompliantVariable(line, variable, out bool replaced);
                        replacementsCount += replaced ? 1 : 0;
                    }
                }

                if (inSubOrFunction && base.FuncProcEndRgx.IsMatch(line))
                {
                    inSubOrFunction = false;
                    variables.Clear();
                }
            }

            updatedFile.Add(line);
        }

        base.PrintReplacements(nameof(HandleVariableNames), replacementsCount);

        handlingFile.Content = updatedFile;
        base.Handle(handlingFile);
    }

    private void CollectNonCompliantVariablesNames(HashSet<string> variables, string line)
    {
        Regex varDeclarationRgx = new Regex(@"\w+\s+As");
        
        if (varDeclarationRgx.IsMatch(line))
        {
            int endIdx = line.Length;
            int startIdx;
            while (true)
            {
                endIdx = line.LastIndexOf("As ", endIdx - 1);
                if (endIdx < 0) break;
                startIdx = line.LastIndexOf(" ", endIdx - 2);

                string varName = line.Substring(startIdx + 1, (endIdx - 2) - startIdx);
                TryAddVariableName(variables, varName);

                // in the case you have this syntax --> Dim _v1, _v2 As Integer
                while (line[startIdx - 1].Equals(','))
                {
                    endIdx = startIdx - 2;
                    startIdx = line.LastIndexOf(" ", endIdx - 1);

                    varName = line.Substring(startIdx + 1, endIdx - startIdx);
                    TryAddVariableName(variables, varName);
                }
            }
        }
    }

    private void TryAddVariableName(HashSet<string> variables, string varName)
    {
        Regex noVarNameRgx = new Regex(@"\W+");

        if (!noVarNameRgx.IsMatch(varName) && !_compliantVarNameRegex.IsMatch(varName))
        {
            variables.Add(varName);

            GenerateNewParameterName(varName);
        }
    }

    private void GenerateNewParameterName(string oldName)
    {
        if (!_oldNewVars.ContainsKey(oldName))
        {
            string camelCaseName = CompliantNamesGenerator.Instance.Generate_camelCaseName(oldName);
            _oldNewVars.Add(oldName, camelCaseName);
        }
    }

    private string ReplaceNonCompliantVariable(string line, string oldName, out bool replaced)
    {
        replaced = false;
        //avoid string literals
        Regex varNameRgx = new Regex(@$"(?<![""\.\w*]){oldName}(?![""\w*])");

        if (varNameRgx.Matches(line).Count > 0)
        {
            replaced = true;

            string compliantName = _oldNewVars[oldName];
            return varNameRgx.Replace(line, compliantName);
        }

        return line;
    }
}
