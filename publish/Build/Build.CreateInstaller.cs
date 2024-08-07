using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;
using System.Diagnostics;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

sealed partial class Build
{
    Target CreateInstaller => _ => _
        .TriggeredBy(CopyPublishDirectories)
        .Executes(() =>
        {
            DotNetBuild(settings => settings.SetProjectFile(Solution.publish.Installer));

            string installerExeFileSearchPattern = $"*{Solution.publish.Installer.Name}.exe";

            string installerFilePath = GetFirstFilePathFromDirectory(Solution.publish.Installer.Directory, installerExeFileSearchPattern);

            Log.Information("Running installer exe builder from: {InstallerFilePath}", installerFilePath);
            Process installerProcess = Process.Start(installerFilePath);
            installerProcess.WaitForExit();
        });
}
