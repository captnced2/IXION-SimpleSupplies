using System.Collections.Generic;
using System.Linq;
using BulwarkStudios.Stanford.Core.GameResources;
using BulwarkStudios.Stanford.Core.GameStates;
using BulwarkStudios.Stanford.Torus.Buildings;
using BulwarkStudios.Stanford.Torus.Sectors;
using HarmonyLib;
using Random = System.Random;

namespace SimpleSupplies;

public class Patches
{
    internal static Dictionary<TorusSectorInstance, List<Building>> buildings = new();
    internal static bool init;

    [HarmonyPatch(typeof(Game), nameof(Game.SetState))]
    public static class GameStatePatcher
    {
        public static void Postfix(Game.STATE state)
        {
            if (state == Game.STATE.PLAYING)
            {
                if (init) return;
                init = true;
                var rn = new Random();
                foreach (var dictionary in buildings)
                {
                    var resources = new List<ResourceData>();
                    foreach (var b in dictionary.Value)
                    {
                        var newResource = true;
                        foreach (var r in resources)
                            if (r.type == b.resource.type)
                                newResource = false;

                        if (newResource)
                            resources.Add(b.resource);
                    }

                    foreach (var r in resources)
                    {
                        var resourceBuildings = new List<Building>();
                        foreach (var b in dictionary.Value)
                            if (b.resource.type == r.type)
                                resourceBuildings.Add(b);

                        var newSupply = resourceBuildings[rn.Next(resourceBuildings.Count)];
                        foreach (var b in resourceBuildings)
                            if (b != newSupply)
                            {
                                newSupply.amount += b.amount;
                                b.instance.availableStock.GetStored().Resources.RemoveAllResources();
                            }

                        newSupply.instance.availableStock.GetStored().Resources.SetResource(r.type, newSupply.amount);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(BuildingBehaviour), nameof(BuildingBehaviour.ApplyState))]
    public static class BuildingInstancePatcher
    {
        public static void Postfix(BuildingBehaviour __instance)
        {
            if (__instance.Data.name.ToLower().Contains("supplies") ||
                __instance.Data.name.ToLower().Contains("debris"))
            {
                var res = __instance.instance.availableStock.GetStored().Resources.GetListResources()._items.First();
                var building = new Building(__instance.instance, res,
                    __instance.instance.availableStock.GetStored().Resources.GetResourceCount(res));
                foreach (var dictionary in buildings)
                    if (dictionary.Key == __instance.instance.GetSector())
                    {
                        dictionary.Value.Add(building);
                        return;
                    }

                var newList = new List<Building>();
                newList.Add(building);
                buildings.Add(__instance.instance.GetSector(), newList);
            }
        }
    }

    internal class Building(BuildingInstance Instance, ResourceData Resource, int Amount)
    {
        internal readonly BuildingInstance instance = Instance;
        internal readonly ResourceData resource = Resource;
        internal int amount = Amount;
    }
}