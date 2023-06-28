using System.Text;

namespace VBCodeCompliancer.ChainOfResponsability;
public class CoRClient
{
    private readonly string _directory;
    private readonly string _slnFilePath;
    private readonly string _backupDir;
    private readonly bool _all;

    private IHandler? _firstInChain;

    public CoRClient(string slnFilePath, string backupDir, bool all)
    {
        _directory = Path.GetDirectoryName(slnFilePath)!;
        _slnFilePath = slnFilePath;
        _backupDir = backupDir;
        _all = all;

        InitiateChainOfResponsability();
    }

    private void InitiateChainOfResponsability()
    {
        // Create all handlers needed
        IHandler handleParameterNames = new HandleParameterNames();
        IHandler handleVariableNames = new HandleVariableNames();
        IHandler handleMethodNames = new HandleMethodNames();

        // Order handlers
        handleParameterNames
            .SetNext(handleVariableNames)
            .SetNext(handleMethodNames);

        // Select first in Chain
        _firstInChain = handleParameterNames;
    }

    private void BeginChain(string tempVbFilePath, string vbFilePath)
    {
        // Create object to be handled by the Chain of Responsability
        IEnumerable<string> fileContent = Utils.FileToStringBuilder(tempVbFilePath, out Encoding encoding);
        HandlingFile vbFile = new()
        {
            FilePath = vbFilePath,
            Content = fileContent,
            Encoding = encoding
        };

        // Call first handler in chain
        _firstInChain?.Handle(vbFile);
    }

    public void Execute(bool list)
    {
        IEnumerable<string> vbprojFiles = Utils.GetVbProjFiles(_slnFilePath);
        foreach (string vbprojFile in vbprojFiles)
        {
            Utils.PrintHeader($"Project {vbprojFile}", 1);

            string vbprojFilePath = Path.Combine(_directory, vbprojFile);
            string projectPath = Path.GetDirectoryName(vbprojFilePath)!;

            IEnumerable<string> vbFiles = Utils.GetVbFiles(vbprojFilePath);

            foreach (string file in vbFiles)
            {
                string vbFilePath = Path.Combine(projectPath, file);

                if (list)
                {
                    Console.WriteLine(vbFilePath);
                }
                else
                {
                    if (!_all || Utils.ConfirmMsg($"Work on file: '{vbFilePath}' "))
                    {
                        Utils.PrintHeader($"File {file}", 3);

                        string tempVbFilePath = Path.Combine(projectPath, 
                                                    Path.GetDirectoryName(file)!, 
                                                    Path.GetFileNameWithoutExtension(file) + "__" + Path.GetExtension(file));

                        if (File.Exists(vbFilePath))
                        {
                            File.Move(vbFilePath, tempVbFilePath, true);
                        }

                        BeginChain(tempVbFilePath, vbFilePath);

                        Utils.MoveTempFileToBackupFolder(_directory, _backupDir, tempVbFilePath);
                    }
                }
            }
        }
    }
}
