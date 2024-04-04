using System.Collections.Generic;
using HarmonyLib;

namespace InfoLoom;

public static class Patcher
{
    private const string HarmonyId = MyPluginInfo.PLUGIN_GUID + "_Cities2Harmony";
    private static bool patched = false;
    private static Dictionary<string, System.Reflection.MethodBase> myPatches = new();

    public static void PatchAll()
    {
        if (patched) { Plugin.Log($"WARNING: Not patched!"); return; }
        //Harmony.DEBUG = true;
        var harmony = new Harmony(HarmonyId);
        harmony.PatchAll();
        if (Harmony.HasAnyPatches(HarmonyId))
        {
            Plugin.Log($"OK methods patched");
            patched = true;
            //var myOriginalMethods = harmony.GetPatchedMethods();
            foreach (System.Reflection.MethodBase method in harmony.GetPatchedMethods())
            {
                Plugin.Log($"...method {method.DeclaringType}.{method.Name}");
                myPatches[method.DeclaringType + "." + method.Name] = method;
            }
        }
        else
            Plugin.Log($"ERROR methods not patched.");
        //Harmony.DEBUG = false;
    }

    /// <summary>
    /// Removes Transpilers and Prefixes from other plugins.
    /// </summary>
    /// <param name="original">Method to clean.</param>
    private static void RemoveConflictingPatches(System.Reflection.MethodBase original, params string[] mods)
    {
        var harmony = new Harmony(HarmonyId);
        Patches patches = Harmony.GetPatchInfo(original);
        foreach (Patch patch in patches.Transpilers)
        {
            //Debug.Log($"  Transpiler: {patch.index} {patch.owner} {patch.PatchMethod.Name}");
            if (patch.owner != HarmonyId)
            {
                Plugin.Log($"REMOVING {patch.PatchMethod.Name} from {patch.owner}");
                harmony.Unpatch(original, HarmonyPatchType.Transpiler, patch.owner);
            }
        }
        foreach (Patch patch in patches.Prefixes)
        {
            //Debug.Log($"  Prefix: {patch.index} {patch.owner} {patch.PatchMethod.Name}");
            if (patch.owner != HarmonyId)
            {
                Plugin.Log($"REMOVING {patch.PatchMethod.Name} from {patch.owner}");
                harmony.Unpatch(original, HarmonyPatchType.Prefix, patch.owner);
            }
        }
    }


    /// <summary>
    /// Removes Postix from the specified plugin.
    /// </summary>
    /// <param name="original">Method to clean.</param>
    private static void RemovePostfixPatch(System.Reflection.MethodBase original, string plugin)
    {
        var harmony = new Harmony(HarmonyId);
        Patches patches = Harmony.GetPatchInfo(original);
        foreach (Patch patch in patches.Postfixes)
        {
            //Debug.Log($"  Transpiler: {patch.index} {patch.owner} {patch.PatchMethod.Name}");
            if (patch.owner != HarmonyId && patch.owner == plugin)
            {
                Plugin.Log($"REMOVING {patch.PatchMethod.Name} from {patch.owner}");
                harmony.Unpatch(original, HarmonyPatchType.Postfix, patch.owner);
            }
        }
    }


    public static void RemoveConflictingPatches()
    {
        const string lrr = "com.github.algernon-A.csl.lifecyclerebalancerevisited";
        // OutsideConnectionAI.StartConnectionTransferImpl
        RemoveConflictingPatches(myPatches["OutsideConnectionAI.StartConnectionTransferImpl"]);
        RemoveConflictingPatches(myPatches["Citizen.GetAgeGroup"]);
        RemoveConflictingPatches(myPatches["Citizen.GetAgePhase"]);
        RemovePostfixPatch(myPatches["Citizen.GetCitizenHomeBehaviour"], lrr);
        RemoveConflictingPatches(myPatches["HumanAI.FindVisitPlace"]);
        RemoveConflictingPatches(myPatches["ResidentAI.CanMakeBabies"]);
        RemoveConflictingPatches(myPatches["ResidentAI.UpdateAge"]);
        RemoveConflictingPatches(myPatches["ResidentAI.UpdateWorkplace"]);
    }

    /// <summary>
    /// Dumps to a log all Harmony patches, marking potential conflicts.
    /// </summary>
    public static void ListAllPatches()
    {
        foreach (System.Reflection.MethodBase original in Harmony.GetAllPatchedMethods())
        {
            bool check = myPatches.ContainsKey(original.DeclaringType + "." + original.Name);
            Plugin.Log($"Method: {original.DeclaringType}.{original.Name}");
            Patches patches = Harmony.GetPatchInfo(original);
            foreach (Patch patch in patches.Transpilers)
                Plugin.Log($"  Transpiler {patch.index}: {patch.owner} {patch.PatchMethod.DeclaringType}.{patch.PatchMethod.Name}" + ((check && patch.owner != HarmonyId) ? " potential CONFLICT" : ""));
            foreach (Patch patch in patches.Postfixes)
                Plugin.Log($"  Postfix {patch.index}: {patch.owner} {patch.PatchMethod.DeclaringType}.{patch.PatchMethod.Name}" + ((check && patch.owner != HarmonyId) ? " could be safe, but check for CONFLICT" : ""));
            foreach (Patch patch in patches.Prefixes)
                Plugin.Log($"  Prefix {patch.index}: {patch.owner} {patch.PatchMethod.DeclaringType}.{patch.PatchMethod.Name}" + ((check && patch.owner != HarmonyId) ? " potential CONFLICT" : ""));
        }
    }

    public static void UnpatchAll()
    {
        if (!patched) { Plugin.Log($"WARNING: Not patched!"); return; }
        //Harmony.DEBUG = true;
        var harmony = new Harmony(HarmonyId);
        //harmony.UnpatchAll(HarmonyId); // deprecated
        harmony.UnpatchSelf(); // deprecated
        Plugin.Log($"OK methods unpatched.");
        patched = false;
        //Harmony.DEBUG = false;
    }
}
