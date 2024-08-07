using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

sealed partial class Build
{
    Target Compile => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            foreach (string releaseConfiguration in _releaseConfigurations)
            {
                DotNetBuild(settings => settings
                    .SetProjectFile(Solution.src.RDStudio_Application)
                                                .SetConfiguration(releaseConfiguration)
                                                .SetVersion(Version)
                                                .SetAssemblyVersion(Version)
                                                .SetVerbosity(DotNetVerbosity.quiet));
            }

            DotNetBuild(settings => settings.SetProjectFile(Solution.publish.Installer)
                                            .SetVersion(Version)
                                            .SetAssemblyVersion(Version)
                                            .SetVerbosity(DotNetVerbosity.quiet));
        });
}
