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

namespace PopStruct;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
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

    private void Awake()
    {
        Logger = base.Logger;

        // CO logging standard as described here https://cs2.paradoxwikis.com/Logging
        s_Log = LogManager.GetLogger(MyPluginInfo.PLUGIN_NAME);

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID + "_Cities2Harmony");
        var patchedMethods = harmony.GetPatchedMethods().ToArray();

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} made patches! Patched methods: " + patchedMethods.Length);

        foreach (var patchedMethod in patchedMethods) {
            Logger.LogInfo($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
        }
    }
}

public class PopStruct_Demographics : UIExtension
{
    public new readonly string extensionID = "infixo.demographics";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public PopStruct_Demographics()
    {
        this.extensionContent = this.LoadEmbeddedResource("PopStruct.dist.demographics.js");
    }
}

public class PopStruct_Workforce : UIExtension
{
    public new readonly string extensionID = "infixo.workforce";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public PopStruct_Workforce()
    {
        this.extensionContent = this.LoadEmbeddedResource("PopStruct.dist.workforce.js");
    }
}

public class PopStruct_DemandFactors : UIExtension
{
    public new readonly string extensionID = "infixo.demandfactors";
    public new readonly string extensionContent;
    public new readonly ExtensionType extensionType = ExtensionType.Panel;

    public PopStruct_DemandFactors()
    {
        this.extensionContent = this.LoadEmbeddedResource("PopStruct.dist.demandfactors.js");
    }
}
