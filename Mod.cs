using System;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Prefabs;
using Game.Economy;
using Game.Common;
using HarmonyLib;

namespace InfoLoom;

public class Mod : IMod
{
    public static readonly string harmonyId = "Infixo." + nameof(InfoLoom);

    // mod's instance and asset
    public static Mod instance { get; private set; }
    public static ExecutableAsset modAsset { get; private set; }
	// logging
    public static ILog log = LogManager.GetLogger($"{nameof(InfoLoom)}").SetShowsErrorsInUI(false);
		
    // setting
    public static Setting setting { get; private set; }

    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info(nameof(OnLoad));

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
        {
            log.Info($"Current mod asset at {asset.path}");
            modAsset = asset;
        }

        // Setting
        setting = new Setting(this);
        setting.RegisterInOptionsUI();
        setting._Hidden = false;								
        AssetDatabase.global.LoadSettings(nameof(InfoLoom), setting, new Setting(this));

        // Locale
        GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(setting));

        // Harmony
        var harmony = new Harmony(harmonyId);
        harmony.PatchAll(typeof(Mod).Assembly);
        var patchedMethods = harmony.GetPatchedMethods().ToArray();
        log.Info($"Plugin {harmonyId} made patches! Patched methods: " + patchedMethods.Length);
        foreach (var patchedMethod in patchedMethods)
        {
            log.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.DeclaringType.Name}.{patchedMethod.Name}");
        }

        // Systems
        updateSystem.UpdateAt<InfoLoom.Systems.BuildingDemandUISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<InfoLoom.Systems.PopulationStructureUISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<InfoLoom.Systems.WorkplacesInfoLoomUISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<InfoLoom.Systems.WorkforceInfoLoomUISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<InfoLoom.Systems.CommercialDemandUISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<InfoLoom.Systems.ResidentialDemandUISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<InfoLoom.Systems.IndustrialDemandUISystem>(SystemUpdatePhase.UIUpdate);

        // Check if SeparateConsumption feature is enabled
        if (!setting.SeparateConsumption)
        {
            MethodBase mb = typeof(Game.UI.InGame.ProductionUISystem).GetMethod("GetData", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mb != null)
            {
                log.Info($"Removing {mb.Name} patch from {harmonyId} because Separate Consumption is disabled.");
                harmony.Unpatch(mb, HarmonyPatchType.Prefix, harmonyId);
            }
            else
                log.Warn("Cannot remove GetData patch.");
        }
    }

    public void OnDispose()
    {
        log.Info(nameof(OnDispose));
        if (setting != null)
        {
            setting.UnregisterInOptionsUI();
            setting = null;
        }

        // Harmony
        var harmony = new Harmony(harmonyId);
        harmony.UnpatchAll(harmonyId);
    }
}
