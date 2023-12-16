using Colossal.UI.Binding;
using Game;
using Game.UI.InGame;
using HarmonyLib;
using Unity.Collections;
using Unity.Jobs;

namespace InfoLoom;

[HarmonyPatch]
class Patches
{
    [HarmonyPatch(typeof(Game.Common.SystemOrder), "Initialize")]
    [HarmonyPostfix]
    public static void Initialize_Postfix(UpdateSystem updateSystem)
    {
        updateSystem.UpdateAt<InfoLoom.PopulationStructureUISystem>(SystemUpdatePhase.UIUpdate);
    }

    [HarmonyPatch(typeof(Game.UI.InGame.CityInfoUISystem), "WriteDemandFactors")]
    [HarmonyPrefix]
    public static bool WriteDemandFactors_Prefix(CityInfoUISystem __instance, IJsonWriter writer, NativeArray<int> factors, JobHandle deps)
    {
        deps.Complete();
        NativeList<FactorInfo> list = FactorInfo.FromFactorArray(factors, Allocator.Temp);
        list.Sort();
        try
        {
            // int num = math.min(5, list.Length);
            int num = list.Length;
            writer.ArrayBegin(num);
            for (int i = 0; i < num; i++)
            {
                list[i].WriteDemandFactor(writer);
            }
            writer.ArrayEnd();
        }
        finally
        {
            list.Dispose();
        }
        return false; // don't execute the original
    }
}
