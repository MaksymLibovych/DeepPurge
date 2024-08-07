using Nuke.Common;

sealed partial class Build
{
    Target SignMsi => _ => _
        .Requires(() => CertificatePassword)
        .Requires(() => TimestampServer)
        .Requires(() => CertificateHashAlgorithm)
        .TriggeredBy(CreateInstaller)
        .Executes(() =>
        {
            string outputMsiFilePath = GetFirstFilePathFromDirectory(_artifactsDirectory, "*.msi");
            StartSignProcess(outputMsiFilePath);
        });
}
