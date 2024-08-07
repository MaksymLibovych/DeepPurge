using System;
using System.IO;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Controls;
using Assembly = System.Reflection.Assembly;

const string projectName = "DeepPurge";

var currentDirectory = Directory.GetCurrentDirectory();
var installerPath = Path.Combine(currentDirectory, "publish", "Installer");
var tempPath = new DirectoryInfo(Path.Combine(installerPath, "bin", "temp"));

var project = new Project
{
    OutDir = "output",
    Name = projectName,
    Platform = Platform.x64,
    UI = WUI.WixUI_FeatureTree,
    MajorUpgrade = MajorUpgrade.Default,
    GUID = new Guid("13930E14-C915-4246-8873-BF30C9080F46"),
    BannerImage = Path.Combine(installerPath, @"Resources\Icons\BannerImage.png"),
    BackgroundImage = Path.Combine(installerPath, @"Resources\Icons\BackgroundImage.png"),
    Version = Assembly.GetExecutingAssembly().GetName().Version.ClearRevision(),
    ControlPanelInfo =
    {
        Manufacturer = "RD Studio",
        ProductIcon = Path.Combine(installerPath, @"Resources\Icons\ShellIcon.ico")
    }
};

var installerWixEntities = GenerateWixEntities(tempPath.FullName);
project.RemoveDialogsBetween(NativeDialogs.WelcomeDlg, NativeDialogs.VerifyReadyDlg);

BuildSingleUserMsi();

Directory.Delete(tempPath.FullName, true);

void BuildSingleUserMsi()
{
    project.InstallScope = InstallScope.perUser;
    project.OutFileName = $"{projectName} {project.Version}";
    project.Dirs =
    [
        new InstallDir(@"%AppDataFolder%\Autodesk\Revit\Addins\", installerWixEntities)
    ];
    project.BuildMsi();
}

WixEntity[] GenerateWixEntities(string releaseDir)
{
    return new Files().GetAllItems(releaseDir);
}