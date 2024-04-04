using Gooee.Plugins.Attributes;
using Gooee.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using Game;

namespace InfoLoom.Gooee;

public class InfoLoomModel : Model
{
    // simple data to control the window visibility
    public bool IsVisibleDemographics { get; set; }
    public bool IsVisibleWorkforce { get; set; }
    public bool IsVisibleWorkplaces { get; set; }
    public bool IsVisibleDemand { get; set; }
    public bool IsVisibleResidential { get; set; }
    public bool IsVisibleCommercial { get; set; }
    public bool IsVisibleIndustrial { get; set; }
}


public partial class InfoLoomController : Controller<InfoLoomModel>
{
    public override InfoLoomModel Configure()
    {
        return new InfoLoomModel();
    }

    [OnTrigger]
    private void OnToggleVisible(string key)
    {
        // stub method to avoid errors
    }

    [OnTrigger]
    private void OnToggleVisibleDemographics(string key)
    {
        base.Model.IsVisibleDemographics = !Model.IsVisibleDemographics;
        base.TriggerUpdate();
    }

    [OnTrigger]
    private void OnToggleVisibleWorkforce(string key)
    {
        base.Model.IsVisibleWorkforce = !Model.IsVisibleWorkforce;
        base.TriggerUpdate();
    }

    [OnTrigger]
    private void OnToggleVisibleWorkplaces(string key)
    {
        base.Model.IsVisibleWorkplaces = !Model.IsVisibleWorkplaces;
        base.TriggerUpdate();
    }

    [OnTrigger]
    private void OnToggleVisibleDemand(string key)
    {
        base.Model.IsVisibleDemand = !Model.IsVisibleDemand;
        base.TriggerUpdate();
    }

    [OnTrigger]
    private void OnToggleVisibleResidential(string key)
    {
        base.Model.IsVisibleResidential = !Model.IsVisibleResidential;
        base.TriggerUpdate();
    }

    [OnTrigger]
    private void OnToggleVisibleCommercial(string key)
    {
        base.Model.IsVisibleCommercial = !Model.IsVisibleCommercial;
        base.TriggerUpdate();
    }

    [OnTrigger]
    private void OnToggleVisibleIndustrial(string key)
    {
        base.Model.IsVisibleIndustrial = !Model.IsVisibleIndustrial;
        base.TriggerUpdate();
    }
}


[ControllerTypes(typeof(InfoLoomController))]
//[PluginToolbar(typeof(InfoLoom_Controller), "OnToggleVisible", "InfoLoom", "Media/Game/Icons/Workers.svg")] // generic hook version
[PluginToolbar(typeof(InfoLoomController), "InfoLoom.ui_src.gooee-menu.json")]
public class InfoLoomGooeePluginWithControllers : IGooeePluginWithControllers, IGooeeStyleSheet
{
    public string Name => "InfoLoom";
    public string Version => "0.8.5";
    public string ScriptResource => "InfoLoom.dist.ui.js";
    public string StyleResource => "InfoLoom.dist.ui.css";

    public IController[] Controllers { get; set; }

}

// MENU
/*
OnClickMethod = controllerType.GetMethod(pluginToolbarItem.OnClick, BindingFlags.Instance | BindingFlags.NonPublic),
OnGetChildren = (string.IsNullOrEmpty(pluginToolbarItem.OnGetChildren) ? null : controllerType.GetMethod(pluginToolbarItem.OnGetChildren, BindingFlags.Instance | BindingFlags.NonPublic)),
OnClickKey = pluginToolbarItem.OnClickKey,
Label = pluginToolbarItem.Label,
Icon = pluginToolbarItem.Icon,
IconClassName = pluginToolbarItem.IconClassName,
IsFAIcon = pluginToolbarItem.IsFAIcon
*/



// UM

// window.$_gooee.register("ultimatemonitor", "UltimateMonitorWindow", ToolWindow, "main-container", "ultimatemonitor");

/*

//[ControllerDepends(SystemUpdatePhase.GameSimulation, typeof(UnemploymentMonitorSystem))]
//[ControllerDepends(SystemUpdatePhase.GameSimulation, typeof(UltimateMonitorDataSystem))]
public class UltimateMonitorController : Controller<UltimateMonitorViewModel>
{
    public override UltimateMonitorViewModel Configure()
    {
        UltimateMonitorViewModel model = new UltimateMonitorViewModel();
        return model;
    }

    [OnTrigger]
    private void OnToggleAddWindow()
    {
        base.Model.ShowAddWindow = !base.Model.ShowAddWindow;
        base.TriggerUpdate();
    }

    [OnTrigger]
    private void OnAddMonitor(string json)
    {
    }
}


[ControllerTypes(new Type[] { typeof(UltimateMonitorController) })]
[PluginToolbar(typeof(UltimateMonitorController), "UltimateMonitor.Resources.gooee-menu.json")]
public class UltimateMonitorPlugin : IGooeePluginWithControllers, IGooeePlugin //, IGooeeChangeLog, IGooeeLanguages, IGooeeStyleSheet
{
    public string Name => "UltimateMonitor";

    public string Version => "1.4.0";

    public string ScriptResource => "UltimateMonitor.Resources.ui.js";

    //public string ChangeLogResource => "UltimateMonitor.Resources.changelog.md";

    //public string StyleResource => null;

    public IController[] Controllers { get; set; }

    //public string LanguageResourceFolder => "UltimateMonitor.Resources.lang";
}

public class UltimateMonitorViewModel : Model
{
    //public List<MonitorItem> Items { get; set; }

    //public List<MonitorWindow> Windows { get; set; }

    public bool ShowAddWindow { get; set; }
}

*/