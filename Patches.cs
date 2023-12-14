using Game;
using HarmonyLib;

namespace PopStruct;

[HarmonyPatch]
class Patches
{
    [HarmonyPatch(typeof(Game.Common.SystemOrder), "Initialize")]
    [HarmonyPostfix]
    public static void Initialize_Postfix(UpdateSystem updateSystem)
    {
        updateSystem.UpdateAt<PopStruct.PopulationStructureUISystem>(SystemUpdatePhase.UIUpdate);
    }
}
