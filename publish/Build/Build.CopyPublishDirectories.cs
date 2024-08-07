using Nuke.Common;
using Serilog;
using System.IO;
using System.Linq;

sealed partial class Build
{
    Target CopyPublishDirectories => _ => _
        .TriggeredBy(Compile)
        .Executes(() =>
        {
            string appsettingsFilePath = GetAppsettingsFilePath(AppsettingsEnvironment.Production);
            string addinFilePath = GetAddinFilePath();

            foreach (string releaseConfiguration in _releaseConfigurations)
            {
                string sourceReleaseDirectoryPath = Path.Combine(
                    Solution.src.RDStudio_Application.Directory, BinDirectoryName, releaseConfiguration);

                string revitVersion = releaseConfiguration.Split(" ").Last();

                string destinationPublishDirectoryPath = Path.Combine(
                    Solution.publish.Installer.Directory, BinDirectoryName, TemporaryDirectoryName, revitVersion);

                string destinationPublishProjectDirectoryPath = Path.Combine(destinationPublishDirectoryPath, Solution.src.RDStudio_Application.Name);

                Log.Information("Copying \"{sourceReleaseConfigurationDirectory}\" to \"{destinationPublishProjectDirectory}\"",
                    sourceReleaseDirectoryPath, destinationPublishProjectDirectoryPath);
                CopyDirectory(sourceReleaseDirectoryPath, destinationPublishProjectDirectoryPath);

                File.Copy(appsettingsFilePath, Path.Combine(destinationPublishProjectDirectoryPath, Path.GetFileName(appsettingsFilePath)));
                File.Copy(addinFilePath, Path.Combine(destinationPublishDirectoryPath, Path.GetFileName(addinFilePath)));
            }
        });
}
