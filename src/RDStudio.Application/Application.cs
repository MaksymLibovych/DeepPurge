using RDStudio.DeepPurge;
using RevitPluginFramework;

namespace RDStudio.Application;

public class Application : ExternalApplicationBase
{
    private const string IconsFolder = "/RDStudio.Application;component/Resources/Icons/";

    public override void OnStartup()
    {
        var ribbonTabBuilder = Application.CreateRibbonTabBuilder("RD Studio");

        ribbonTabBuilder.AddRibbonPanel("Family Management", ribbonPanelBuilder =>
        {
            ribbonPanelBuilder.WithRibbonButton<DeepPurgeExternalCommand>("Deep Purge")
                .SetImage($"{IconsFolder}DeepPurge16.png")
                .SetLargeImage($"{IconsFolder}DeepPurge32.png")
                .AddToolTip("");
        });
    }
}
