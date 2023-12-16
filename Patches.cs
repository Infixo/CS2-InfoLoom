using Game;
using HarmonyLib;

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
}
