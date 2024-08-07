using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

sealed partial class Build
{
    Target Clean => _ => _
        .Executes(() =>
        {
            foreach (var project in _sourceProjects)
            {
                CleanDirectory(project.Directory / BinDirectoryName);
                CleanDirectory(project.Directory / ObjDirectoryName);
            }

            foreach (string releaseConfiguration in _releaseConfigurations)
            {
                DotNetClean(settings => settings.SetConfiguration(releaseConfiguration)
                                                .SetVerbosity(DotNetVerbosity.quiet));
            }

            DotNetClean(settings => settings.SetProject(Solution.publish.Installer)
                                            .SetVerbosity(DotNetVerbosity.quiet));

            CleanDirectory(Solution.publish.Installer.Directory / BinDirectoryName);
            CleanDirectory(Solution.publish.Installer.Directory / ObjDirectoryName);

            CleanDirectory(_artifactsDirectory);
        });
}
