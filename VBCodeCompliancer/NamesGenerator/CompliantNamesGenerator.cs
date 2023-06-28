using System.Text;

namespace VBCodeCompliancer.NamesGenerator;
public class CompliantNamesGenerator
{
    private CompliantNamesGenerator() 
    {
        _randomlyGeneratedNames = new();
    }
    private static CompliantNamesGenerator? _instance;

    private HashSet<string> _randomlyGeneratedNames;

    public static CompliantNamesGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CompliantNamesGenerator();
            }
            return _instance;
        }
    }

    public string Generate_CamelCaseName(string oldName)
    {
        StringBuilder result = new();
        string anOldName = RemoveNonAlfaNumericChars(oldName);
        int idx = 0;

        if (anOldName.Length == 0)
            return Random_CamelCaseParameterName();

        if (char.IsLower(anOldName[idx]))
        {
            result.Append(char.ToUpper(anOldName[idx++]));
        }
        
        while (idx < anOldName.Length)
        {
            if (char.IsUpper(anOldName[idx]) && idx < anOldName.Length - 1)
            {
                result.Append(anOldName[idx++]); // leave it upper

                while (char.IsUpper(anOldName[idx]) && idx + 1 < anOldName.Length && char.IsUpper(anOldName[idx + 1]))
                    result.Append(char.ToLower(anOldName[idx++]));

                if (idx < anOldName.Length - 1) // leave it upper
                    result.Append(anOldName[idx++]);
            }
            else
            {
                result.Append(char.ToLower(anOldName[idx++]));
            }
        }

        return result.ToString();
    }

    public string Generate_camelCaseName(string oldName)
    {
        StringBuilder result = new();
        string anOldName = RemoveNonAlfaNumericChars(oldName);
        int idx = 0;

        if (anOldName.Length == 0)
            return Random_camelCaseParameterName();

        result.Append(char.ToLower(anOldName[idx++]));

        while (idx < anOldName.Length)
        {
            if (char.IsUpper(anOldName[idx]) && idx < anOldName.Length - 1)
            {
                result.Append(anOldName[idx++]); // leave it upper

                while (char.IsUpper(anOldName[idx]) && idx + 1 < anOldName.Length && char.IsUpper(anOldName[idx + 1]))
                    result.Append(char.ToLower(anOldName[idx++]));

                if (idx < anOldName.Length - 1) // leave it upper
                    result.Append(anOldName[idx++]);
            }
            else
            {
                result.Append(char.ToLower(anOldName[idx++]));
            }
        }

        return result.ToString();
    }

    private string RemoveNonAlfaNumericChars(string oldName)
    {
        StringBuilder result = new();

        int idx = 0;
        while(idx < oldName.Length)
        {
            if (char.IsLetterOrDigit(oldName[idx]))
            {
                result.Append(oldName[idx]);
            }
            // ...abc_def --> abcDef
            else if (oldName[idx].Equals('_') && idx + 1 < oldName.Length)
            {
                result.Append(char.ToUpper(oldName[++idx]));
            }

            idx++;
        }

        return result.ToString();
    }

    private string Random_camelCaseParameterName()
    {
        StringBuilder result = new();

        for (int i = 0; i < 3; i++)
        {
            result.Append((char)Random.Shared.Next(97, 123));
        }

        string randomName = result.ToString();

        if (_randomlyGeneratedNames.Contains(randomName))
        {
            return Random_camelCaseParameterName();
        }
        else
        {
            _randomlyGeneratedNames.Add(randomName);
            return randomName;
        }
    }

    private string Random_CamelCaseParameterName()
    {
        StringBuilder result = new();

        result.Append((char)Random.Shared.Next(65, 91));
        for (int i = 1; i < 3; i++)
        {
            result.Append((char)Random.Shared.Next(97, 123));
        }

        string randomName = result.ToString();

        if (_randomlyGeneratedNames.Contains(randomName))
        {
            return Random_CamelCaseParameterName();
        }
        else
        {
            _randomlyGeneratedNames.Add(randomName);
            return randomName;
        }
    }
}
