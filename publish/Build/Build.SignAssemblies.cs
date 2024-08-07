using Nuke.Common;
using System.IO;
using System.Linq;

sealed partial class Build
{
    Target SignAssemblies => _ => _
        .Requires(() => CertificatePassword)
        .Requires(() => TimestampServer)
        .Requires(() => CertificateHashAlgorithm)
        .TriggeredBy(CopyPublishDirectories)
        .Executes(() =>
        {
            foreach (string releaseConfiguration in _releaseConfigurations)
            {
                string revitVersion = releaseConfiguration.Split(" ").Last();

                string projectDirectoryPath = Path.Combine(Solution.publish.Installer.Directory, BinDirectoryName,
                    TemporaryDirectoryName, revitVersion, Solution.src.RDStudio_Application.Name);

                string projectToSignPath = GetFirstFilePathFromDirectory(
                    projectDirectoryPath, $"*{Solution.src.RDStudio_Application.Name}.dll");

                StartSignProcess(projectToSignPath);
            }
        });
}
