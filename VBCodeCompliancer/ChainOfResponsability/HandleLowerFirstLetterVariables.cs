using System.Text;
using System.Text.RegularExpressions;

namespace VBCodeCompliancer.ChainOfResponsability;
public class HandleLowerFirstLetterVariables : AbstractHandler
{
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
            if (base.CommentLineRgx.IsMatch(line))
            {
                updatedFile.Add(line);
                continue;
            }

            if (!inSubOrFunction && base.FuncProcBeginRgx.IsMatch(line))
            {
                inSubOrFunction = true;
                line = GetMultilineMethodDeclaration(enumerator, line);
            }
            else if (inSubOrFunction)
            {
                SniffVariables(variables, line);

                foreach (string variable in variables)
                {
                    line = ReplaceVariable(line, variable, out bool replaced);
                    replacementsCount += replaced ? 1 : 0;
                }
            }

            if (inSubOrFunction && base.FuncProcEndRgx.IsMatch(line))
            {
                inSubOrFunction = false;
                variables.Clear();
            }

            updatedFile.Add(line);
        }

        base.PrintReplacements(nameof(HandleLowerFirstLetterVariables), replacementsCount);

        handlingFile.Content = updatedFile;
        base.Handle(handlingFile);
    }

    private void SniffVariables(HashSet<string> variables, string line)
    {
        Regex varDeclarationRgx = new Regex(@"\w+\s+As");
        Regex noVarNameRgx = new Regex(@"\W+");

        if (varDeclarationRgx.IsMatch(line))
        {
            int endIdx = line.Length;
            int startIdx;
            while (true)
            {
                endIdx = line.LastIndexOf("As", endIdx - 1);
                if (endIdx < 0) break;
                startIdx = line.LastIndexOf(" ", endIdx - 2);

                string varName = line.Substring(startIdx + 1, (endIdx - 2) - startIdx);
                if(!noVarNameRgx.IsMatch(varName))
                {
                    variables.Add(varName);
                }
            }
        }
    }

    private string ReplaceVariable(string line, string oldName, out bool replaced)
    {
        replaced = false;
        //avoid string literals
        Regex varNameRgx = new Regex(@$"(?<![""\.\w*])(?<name>{oldName})(?![""\w*])");

        if (varNameRgx.Matches(line).Count > 0)
        {
            string newName = NewVariableName(oldName);

            if (!oldName.Equals(newName))
            {
                replaced = true;
                return varNameRgx.Replace(line, newName);
            }
        }

        return line;
    }

    private string NewVariableName(string oldName)
    {
        //First letter to consider
        int idx = 0;
        do
        {
            if (char.IsLetter(oldName[idx]))
            {
                if (char.IsUpper(oldName[idx]))
                {
                    return $"{char.ToLower(oldName[idx])}{oldName[(idx + 1)..]}";
                }
                else
                {
                    return oldName;
                }
            }
            else
            {
                idx++;
            }
        }
        while (true);
    }
}
