using VBCodeCompliancer;
using VBCodeCompliancer.ChainOfResponsability;

try
{
    string? directory;
    string[] slnFiles;
    string backupDir = string.Empty;
    bool all = false;

    Console.WriteLine("------------------------------------------------------------");
    Console.WriteLine("                           VB.NET                           ");
    Console.WriteLine("------------------------------------------------------------");
    Console.WriteLine();
    Console.WriteLine();

    Utils.PrintHeader("INSERT PROJECT DIRECTORY:", 2);
    directory = Console.ReadLine();
    if (directory is null)
    {
        throw new Exception("Directoy can't be null");
    }

    if (!Directory.Exists(directory))
    {
        throw new Exception($"'vbproj' file not found in directory: {directory}!");
    }

    slnFiles = Directory.GetFiles(directory, "*.sln", SearchOption.TopDirectoryOnly);

    if (slnFiles.Length != 1)
    {
        throw new Exception($"There must be one (and only one) 'sln' file in directory: {directory}!");
    }

    all = Utils.ConfirmMsg("Confirm message before each file");
    backupDir = Utils.CreateBackupDirectory(directory);

    /*****************************************************************/
    /*****************************************************************/
    
    CoRClient cor = new CoRClient(slnFiles[0], backupDir, all);

    cor.Execute(true);

    if (Utils.ConfirmMsg("Execute replacements?")) 
        cor.Execute(false);

    /*****************************************************************/
    /*****************************************************************/

    if (Utils.ConfirmMsg("Delete Backup directory"))
    {
        Directory.Delete(backupDir, true);
    }
}
catch(Exception ex)
{
    Console.WriteLine("Error: " + ex.Message + ". " + ex.StackTrace);
}
