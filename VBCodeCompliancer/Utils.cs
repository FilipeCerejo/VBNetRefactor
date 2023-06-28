using System.Text;
using System.Text.RegularExpressions;

namespace VBCodeCompliancer;
internal static class Utils
{
    #region Printing Methods
    internal static void PrintHeader(string header, int nrtabs = 1)
    {
        string tab = "  "; 

        if(nrtabs <= 2)
            Console.WriteLine();
        Console.WriteLine($"{RepeatString(tab, nrtabs)} {(nrtabs <= 2 ? header.ToUpper() : header)}");
        if(nrtabs <= 1)
            Console.WriteLine(RepeatString("-", 50));
        if (nrtabs == 0)
            Console.WriteLine(RepeatString("-", 50));
    }

    internal static bool ConfirmMsg(string question, char yes = 'y', char no = 'n')
    {
        Console.Write($"{question} ({yes}/{no})? ");
        string answer = Console.ReadLine()!;
        return answer.Length == 1 && (answer[0]).Equals(yes);
    }
    #endregion

    #region File Load
    internal static IEnumerable<string> FileToStringBuilder(string filepath, out Encoding encoding)
    {
        List<string> result = new();

        using (StreamReader sr = new StreamReader(filepath, true))
        {
            string line;
            while ((line = sr.ReadLine()!) != null)
            {
                result.Add(line);
            }

            encoding = sr.CurrentEncoding;
        }

        return result;
    }
    #endregion

    internal static IEnumerable<string> GetVbFiles(string vbFilePath)
    {
        Regex vbFileRgx = new Regex(@"<Compile Include=""(?<vb>([\w\d\\\s]+)(\.ascx)?\.vb)""");
        List<string> result = new();

        using (StreamReader sr = new StreamReader(vbFilePath))
        {
            string slnContent = sr.ReadToEnd();

            MatchCollection matches = vbFileRgx.Matches(slnContent);

            foreach (Match match in matches)
            {
                string relativePath = match.Groups["vb"].Value;

                // avoid auto-generated code files
                if (relativePath.StartsWith("My Project")
                    || relativePath.StartsWith("Connected Services")
                    || relativePath.StartsWith("Web References")) 
                    continue;
                
                result.Add(relativePath);
            }
        }

        return result;
    }

    internal static IEnumerable<string> GetVbProjFiles(string slnFilePath)
    {
        Regex vbprojFileRgx = new Regex(@"Project\(""{[^)]+}""\)\s=\s\""[^,]+"",\s""(?<vbproj>[^""]+)""");
        List<string> result = new();

        using (StreamReader sr = new StreamReader(slnFilePath))
        {
            string slnContent = sr.ReadToEnd();
            
            MatchCollection matches = vbprojFileRgx.Matches(slnContent);

            foreach(Match match in matches)
            {
                result.Add(match.Groups["vbproj"].Value);
            }
        }

        return result;
    }

    internal static void MoveTempFileToBackupFolder(string directory, string backupDir, string tempVbFilePath)
    {
        string relativePath = Path.GetRelativePath(directory, tempVbFilePath);

        string backupFilePath = Path.Combine(backupDir, relativePath);

        string fileDirectory = Path.GetDirectoryName(backupFilePath)!;

        if(!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }

        File.Move(tempVbFilePath, backupFilePath, true);
    }


    #region Utils
    private static string RepeatString(this string text, int count)
    {
        return new StringBuilder(text.Length * count).Insert(0, text, count).ToString();
    }

    internal static string CreateBackupDirectory(string directory)
    {
        string backupDir = Path.Combine(directory, "ComplianceFilesBackup");

        if(Directory.Exists(backupDir))
        {
            if (ConfirmMsg("Delete Backup directory?"))
                Directory.Delete(backupDir, true);
        }

        Directory.CreateDirectory(backupDir);

        return backupDir;
    }
}
    #endregion