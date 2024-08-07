using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;

sealed partial class Build : NukeBuild
{
    const string Version = "1.0.0.0";
    const string BinDirectoryName = "bin";
    const string ObjDirectoryName = "obj";
    const string TemporaryDirectoryName = "temp";

    string[] _debugConfigurations;
    string[] _releaseConfigurations;
    Project[] _sourceProjects;
    string _microsoftSignToolFilePath = string.Empty;
    string _certificateFilePath = string.Empty;
    readonly AbsolutePath _artifactsDirectory = RootDirectory / "output";

    static Build()
    {
        Logging.Level = LogLevel.Normal;
    }

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [Parameter(nameof(CertificatePassword))]
    readonly string CertificatePassword;

    [Parameter(nameof(TimestampServer))]
    readonly string TimestampServer;

    [Parameter(nameof(CertificateHashAlgorithm))]
    readonly string CertificateHashAlgorithm;

    public static int Main() => Execute<Build>(x => x.Compile);

    protected override void OnBuildInitialized()
    {
        _releaseConfigurations = GetSolutionConfigurations("Release");
        _debugConfigurations = GetSolutionConfigurations("Debug");
        _sourceProjects = GetSourceProjects();
        //_microsoftSignToolFilePath = GetMicrosoftSignToolFilePath();
        //_certificateFilePath = GetCertificateFilePath();
    }

    string[] GetSolutionConfigurations(string searchPattern)
    {
        return Solution.Configurations
            .Select(keyValuePair => keyValuePair.Key)
            .Select(configuration => configuration.Remove(configuration.LastIndexOf('|')))
            .Where(configuration => configuration.StartsWith(searchPattern, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    Project[] GetSourceProjects()
    {
        return Solution.AllProjects.Where(
            project => project != Solution.publish.Build && project != Solution.publish.Installer)
            .ToArray();
    }

    string GetMicrosoftSignToolFilePath()
    {
        string programFilesX86Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string microsoftSignToolDirectoryPath = Path.Combine(programFilesX86Path, "Microsoft SDKs", "ClickOnce", "SignTool");

        string signToolFilePath = GetFirstFilePathFromDirectory(microsoftSignToolDirectoryPath, "*.exe");

        Log.Information("Retrieving sign tool from: {SignToolFilePath}", signToolFilePath);
        return signToolFilePath;
    }

    string GetCertificateFilePath()
    {
        string certificateDirectory = Path.Combine(Solution.Directory, "Signature", "Certificates");

        string certificateFilePath = GetFirstFilePathFromDirectory(certificateDirectory, "*.pfx");

        Log.Information("Retrieving certificate from: {CertificateFilePath}", certificateFilePath);
        return certificateFilePath;
    }

    void CleanDirectory(AbsolutePath absolutePath)
    {
        Log.Information("Cleaning directory: {Directory}", absolutePath);
        absolutePath.CreateOrCleanDirectory();
    }

    void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
    {
        var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

        FileInfo[] sourceDirectoryFiles = sourceDirectory.GetFiles();

        DirectoryInfo[] nestedDirectories = sourceDirectory.GetDirectories();

        if (!Directory.Exists(destinationDirectoryPath))
        {
            Directory.CreateDirectory(destinationDirectoryPath);
        }

        foreach (FileInfo sourceFile in sourceDirectoryFiles)
        {
            string sourceFileDestinationPath = Path.Combine(destinationDirectoryPath, sourceFile.Name);
            sourceFile.CopyTo(sourceFileDestinationPath);
        }

        foreach (DirectoryInfo nestedDirectory in nestedDirectories)
        {
            string nestedDirectoryDestinationPath = Path.Combine(destinationDirectoryPath, nestedDirectory.Name);
            CopyDirectory(nestedDirectory.FullName, nestedDirectoryDestinationPath);
        }
    }

    string GetAddinFilePath()
    {
        string addinFilePath = GetFirstFilePathFromDirectory(Solution.src.RDStudio_Application.Directory, "*.addin");
        Assert.FileExists(addinFilePath, $"{addinFilePath} is missing in the \"{Solution.src.RDStudio_Application}\"");
        Log.Information("Retrieving {SolutionName}.addin from: {AddinFilePath}", Solution.Name, addinFilePath);
        return addinFilePath;
    }

    string GetAppsettingsFilePath(AppsettingsEnvironment appsettingsEnvironment)
    {
        string appsettingsFileName = appsettingsEnvironment == AppsettingsEnvironment.Production
            ? "appsettings.json"
            : "appsettings.Development.json";

        string appsettingsFilePath = GetFirstFilePathFromDirectory(Solution.src.RDStudio_Application.Directory, $"*{appsettingsFileName}");
        Assert.FileExists(appsettingsFilePath, $"{appsettingsFileName} is not missing in the \"{Solution.src.RDStudio_Application.Directory}\"");
        Log.Information("Retrieving {appsettingsFileName} from: {AppsettingsFilePath}", appsettingsFileName, appsettingsFilePath);
        return appsettingsFilePath;
    }

    void StartSignProcess(string projectToSignPath)
    {
        var process = new Process();

        string arguments = $"sign /f \"{_certificateFilePath}\" /p {CertificatePassword} " +
            $"/t {TimestampServer} /fd {CertificateHashAlgorithm} \"{projectToSignPath}\"";

        process.StartInfo.FileName = _microsoftSignToolFilePath;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.OutputDataReceived += (sender, args) => Log.Information(args.Data);
        process.ErrorDataReceived += (sender, args) => Log.Information(args.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();
    }

    string GetFirstFilePathFromDirectory(string directoryPath, string fileSearchPattern)
    {
        Assert.DirectoryExists(directoryPath, $"Directory: \"{directoryPath}\" is missing");

        string filePath = Directory.EnumerateFiles(
            directoryPath, fileSearchPattern, SearchOption.AllDirectories)
            .FirstOrDefault();

        Assert.FileExists(filePath, $"File: \"{filePath}\" is missing");

        return filePath;
    }
}
