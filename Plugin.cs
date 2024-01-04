using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using Colossal.Logging;
using HookUILib.Core;

#if BEPINEX_V6
    using BepInEx.Unity.Mono;
#endif

namespace InfoLoom;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private const string HarmonyId = MyPluginInfo.PLUGIN_GUID + "_Cities2Harmony";

    internal static new ManualLogSource Logger; // BepInEx logging
    private static ILog s_Log; // CO logging

    public static void Log(string text, bool bMethod = false)
    {
        if (bMethod) text = GetCallingMethod(2) + ": " + text;
        Logger.LogInfo(text);
        s_Log.Info(text);
    }

    public static void LogStack(string text)
    {
        //string msg = GetCallingMethod(2) + ": " + text + " STACKTRACE";
        Logger.LogInfo(text + " STACKTRACE");
        s_Log.logStackTrace = true;
        s_Log.Info(text + "STACKTRACE");
        s_Log.logStackTrace = false;
    }

    /// <summary>
    /// Gets the method from the specified <paramref name="frame"/>.
    /// </summary>
    public static string GetCallingMethod(int frame)
    {
        StackTrace st = new StackTrace();
        MethodBase mb = st.GetFrame(frame).GetMethod(); // 0 - GetCallingMethod, 1 - Log, 2 - actual function calling a Log method
        return mb.DeclaringType + "." + mb.Name;
    }

    // mod settings
    public static ConfigEntry<bool> SeparateConsumption;

    private void Awake()
    {
        Logger = base.Logger;

        // CO logging standard as described here https://cs2.paradoxwikis.com/Logging
        s_Log = LogManager.GetLogger(MyPluginInfo.PLUGIN_NAME);

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), HarmonyId);
        var patchedMethods = harmony.GetPatchedMethods().ToArray();

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} made patches! Patched methods: " + patchedMethods.Length);

        foreach (var patchedMethod in patchedMethods) {
            Logger.LogInfo($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
        }

        // settings
        SeparateConsumption = base.Config.Bind<bool>("Settings", "SeparateConsumption", false, "Enables showing commercial and industrial consumption instead of surplus/deficit in the Production UI");

        // check if SeparateConsumption feature is enabled
        if (!SeparateConsumption.Value)
        {
            MethodBase mb = typeof(Game.UI.InGame.ProductionUISystem).GetMethod("GetData", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mb != null)
            {
                Plugin.Log($"REMOVING {mb.Name} patch from {HarmonyId}");
                harmony.Unpatch(mb, HarmonyPatchType.Prefix, HarmonyId);
            }
            else
                Plugin.Log("WARNING: Cannot remove GetData patch.");
        }

        //Plugin.Log("===== all patches =====");
        //Patcher.ListAllPatches();
    }
}

public class InfoLoom_Demographics : UIExtension
{
    public new readonly string extensionID = "infoloom.demographics";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public InfoLoom_Demographics()
    {
        this.extensionContent = this.LoadEmbeddedResource("InfoLoom.dist.demographics.js");
    }
}

public class InfoLoom_Workforce : UIExtension
{
    public new readonly string extensionID = "infoloom.workforce";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public InfoLoom_Workforce()
    {
        this.extensionContent = this.LoadEmbeddedResource("InfoLoom.dist.workforce.js");
    }
}

public class InfoLoom_Workplaces : UIExtension
{
    public new readonly string extensionID = "infoloom.workplaces";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public InfoLoom_Workplaces()
    {
        this.extensionContent = this.LoadEmbeddedResource("InfoLoom.dist.workplaces.js");
    }
}

public class InfoLoom_DemandFactors : UIExtension
{
    public new readonly string extensionID = "infoloom.demandfactors";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public InfoLoom_DemandFactors()
    {
        this.extensionContent = this.LoadEmbeddedResource("InfoLoom.dist.demandfactors.js");
    }
}

public class InfoLoom_Commercial : UIExtension
{
    public new readonly string extensionID = "infoloom.commercial";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public InfoLoom_Commercial()
    {
        this.extensionContent = this.LoadEmbeddedResource("InfoLoom.dist.commercial.js");
    }
}

public class InfoLoom_Residential : UIExtension
{
    public new readonly string extensionID = "infoloom.residential";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public InfoLoom_Residential()
    {
        this.extensionContent = this.LoadEmbeddedResource("InfoLoom.dist.residential.js");
    }
}
