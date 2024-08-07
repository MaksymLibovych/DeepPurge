using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

sealed partial class Build
{
    Target Debug => _ => _
        .Executes(() =>
        {
            Assert.False(Process.GetProcessesByName("Revit").Any(),
                "The Revit process is running. Close all the Revit instances in order to copy files.");

            string appsettingsFilePath = GetAppsettingsFilePath(AppsettingsEnvironment.Development);
            string addinFilePath = GetAddinFilePath();
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            foreach (string debugConfiguration in _debugConfigurations)
            {
                DotNetClean(settings => settings.SetProject(Solution.src.RDStudio_Application)
                                                .SetConfiguration(debugConfiguration)
                                                .SetVerbosity(DotNetVerbosity.quiet));

                string sourceDebugDirectoryPath = Path.Combine(
                    Solution.src.RDStudio_Application.Directory, BinDirectoryName, debugConfiguration);

                string revitVersion = debugConfiguration.Split(" ").Last();

                string destinationRevitDirectoryPath = Path.Combine(
                    appDataFolderPath, "Autodesk", "Revit", "Addins", revitVersion);

                string destinationRevitProjectDirectoryPath = Path.Combine(
                    destinationRevitDirectoryPath, Solution.src.RDStudio_Application.Name);

                CleanDirectory(destinationRevitProjectDirectoryPath);
                File.Delete(addinFilePath);

                CleanDirectory(Solution.src.RDStudio_Application.Directory / BinDirectoryName);
                CleanDirectory(Solution.src.RDStudio_Application.Directory / ObjDirectoryName);

                DotNetBuild(settings => settings.SetProjectFile(Solution.src.RDStudio_Application)
                                                .SetConfiguration(debugConfiguration)
                                                .SetVerbosity(DotNetVerbosity.quiet));

                Log.Information("Copying \"{sourceDebugDirectoryPath}\" to \"{destinationRevitProjectDirectoryPath}\"",
                    sourceDebugDirectoryPath, destinationRevitProjectDirectoryPath);
                CopyDirectory(sourceDebugDirectoryPath, destinationRevitProjectDirectoryPath);

                File.Copy(appsettingsFilePath, Path.Combine(
                    destinationRevitProjectDirectoryPath, Path.GetFileName(appsettingsFilePath)));

                File.Copy(addinFilePath, Path.Combine(
                    destinationRevitDirectoryPath, Path.GetFileName(addinFilePath)));
            }
        });
}
