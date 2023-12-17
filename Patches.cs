using Colossal.UI.Binding;
using Game;
using Game.Economy;
using Game.Prefabs;
using Game.UI.InGame;
using HarmonyLib;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace InfoLoom;

[HarmonyPatch]
static class GamePatches
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

    [HarmonyPatch(typeof(Game.UI.InGame.ProductionUISystem), "GetData")]
    [HarmonyPrefix]
    static bool GetData(ref (int, int, int) __result, Game.UI.InGame.ProductionUISystem __instance, Entity entity,
        // private members that are used in the routine, start with 3 underscores
        PrefabSystem ___m_PrefabSystem,
        NativeList<int> ___m_ProductionCache,
        NativeList<int> ___m_CommercialConsumptionCache,
        NativeList<int> ___m_IndustrialConsumptionCache)
    {
        int resourceIndex = EconomyUtils.GetResourceIndex(EconomyUtils.GetResource(___m_PrefabSystem.GetPrefab<ResourcePrefab>(entity).m_Resource));
        //int num = ___m_ProductionCache[resourceIndex];
        //int num2 = ___m_CommercialConsumptionCache[resourceIndex] + ___m_IndustrialConsumptionCache[resourceIndex];
        //int num3 = math.min(num2, num);
        //int num4 = math.min(num2, num3);
        //int item = num - num3;
        //int item2 = num2 - num4;
        __result = (___m_ProductionCache[resourceIndex], ___m_CommercialConsumptionCache[resourceIndex], ___m_IndustrialConsumptionCache[resourceIndex]);
        return false;
    }

}
