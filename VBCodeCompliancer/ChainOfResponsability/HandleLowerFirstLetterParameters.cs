using System.Text.RegularExpressions;

namespace VBCodeCompliancer.ChainOfResponsability;
public class HandleLowerFirstLetterParameters : AbstractHandler
{
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
            if (base.CommentLineRgx.IsMatch(line))
            {
                updatedFile.Add(line);
                continue;
            }

            if (!inSubOrFunction && base.FuncProcBeginRgx.IsMatch(line))
            {
                inSubOrFunction = true;
                line = GetMultilineMethodDeclaration(enumerator, line);
                parameters.AddRange(CollectParameters(line));
            }

            if (inSubOrFunction)
            {
                foreach (string p in parameters)
                {
                    line = ReplaceParameter(line, p, out bool replaced);
                    replacementsCount += replaced ? 1 : 0;
                }
            }

            if (inSubOrFunction && base.FuncProcEndRgx.IsMatch(line))
            {
                inSubOrFunction = false;
                parameters.Clear();
            }

            updatedFile.Add(line);
        }

        base.PrintReplacements(nameof(HandleLowerFirstLetterParameters), replacementsCount);

        handlingFile.Content = updatedFile;
        base.Handle(handlingFile);
    }

    private List<string> CollectParameters(string line)
    {
        Regex paramRgx = new Regex(@"(ByVal|ByRef)\s+\b(?<param>\w+)\b\sAs");
        List<string> result = new();

        MatchCollection matches = paramRgx.Matches(line);

        foreach (Match match in matches)
        {
            string parameter = match.Groups["param"].Value;

            //Collect only paramters where first letter is capital (CamelCase)
            if (char.IsUpper(parameter[0]))
                result.Add(parameter);
        }

        return result;
    }

    private string ReplaceParameter(string line, string oldName, out bool replaced)
    {
        replaced = false;
        //avoid string literals
        Regex varNameRgx = new Regex(@$"(?<![""\.\w*])(?<name>{oldName})(?![""\w*])");

        if (varNameRgx.Matches(line).Count > 0)
        {
            string newName = NewParamterName(oldName);

            if(!oldName.Equals(newName))
            {
                replaced = true;
                return varNameRgx.Replace(line, newName);
            }
        }

        return line;
    }

    private string NewParamterName(string oldName)
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
