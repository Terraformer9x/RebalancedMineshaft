using BepInEx;
using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace RebalancedMineshaft;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
    public static Plugin Instance;

    private void Awake()
    {
        Instance ??= this;

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        RebalancedMineshaftConfig.Bind(base.Config);
        Assets.LoadAssets();
        harmony.PatchAll(typeof(MineshaftPatch));
    }

    public static void Log(string msg) => Instance.Logger.LogInfo(msg);
    public static void LogDebug(string msg) => Instance.Logger.LogDebug(msg);
    public static void LogWarning(string msg) => Instance.Logger.LogWarning(msg);
    public static void LogError(string msg) => Instance.Logger.LogWarning(msg);
}

internal static class Assets
{
    internal static AssetBundle assetBundle;
    internal static Mesh waterTileCollision;

    internal static void LoadAssets()
    {
        try
        {
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "rebalancedmineshaft"));
            waterTileCollision = assetBundle.LoadAsset<Mesh>("WaterTileCollision");
            assetBundle.Unload(false);
        }
        catch (Exception e) 
        {
            Plugin.LogError(e.Message + " - Failed to load mod assets!");
            return;
        }
    }
}

public class MineshaftPatch
{
    [HarmonyPatch(typeof(Dungeon), "PreGenerateDungeon")]
    [HarmonyPostfix]
    private static void PreGenerateDungeonPostfix(Dungeon __instance)
    {
        if (__instance.DungeonFlow.name == "Level3Flow")
        {
            Plugin.LogDebug(__instance.DungeonFlow.name + $" detected, changing generation size.");
            __instance.DungeonFlow.Length.Min = 12;
            __instance.DungeonFlow.Length.Max = 14;
            __instance.DungeonFlow.Lines[0].Length = 0.35f;
            __instance.DungeonFlow.Lines[1].Position = 0.35f;
            __instance.DungeonFlow.Lines[1].Length = 0.3f;
            __instance.DungeonFlow.Lines[2].Position = 0.65f;
            __instance.DungeonFlow.Lines[2].Length = 0.35f;
            /*__instance.DungeonFlow.Lines[0].DungeonArchetypes[0].BranchCount.Min = 11;
            __instance.DungeonFlow.Lines[0].DungeonArchetypes[0].BranchCount.Max = 13;
            __instance.DungeonFlow.Lines[1].DungeonArchetypes[0].BranchCount.Min = 9;
            __instance.DungeonFlow.Lines[1].DungeonArchetypes[0].BranchCount.Max = 11;
            __instance.DungeonFlow.Lines[2].DungeonArchetypes[0].BranchCount.Min = 11;
            __instance.DungeonFlow.Lines[2].DungeonArchetypes[0].BranchCount.Max = 13;*/
        }
    }

    [HarmonyPatch(typeof(Dungeon), "PostGenerateDungeon")]
    [HarmonyPostfix]
    private static void PostGenerateDungeonPostfix(Dungeon __instance)
    {
        if (__instance.DungeonFlow.name != "Level3Flow") return;

        foreach (Tile tile in __instance.AllTiles)
        {
            switch (tile.name)
            {
                case "CaveSmallIntersectTile(Clone)":
                    Transform tempTransform = tile.transform.Find("GeneralScrapSpawn")?.transform;
                    if (tempTransform != null)
                    {
                        Plugin.LogDebug($"Converting to random spawn at " + tempTransform.position.x + ", " + tempTransform.position.y + ", " + tempTransform.position.z);
                        Functions.ConvertToRandomSpawn(tempTransform, 0.3f, 0.5f);
                    }
                    break;

                case "CaveCrampedIntersectTile(Clone)":
                    tempTransform = tile.transform.Find("GeneralScrapSpawn")?.transform;
                    if (tempTransform != null)
                    {
                        Plugin.LogDebug($"Converting to random spawn at " + tempTransform.position.x + ", " + tempTransform.position.y + ", " + tempTransform.position.z);
                        Functions.ConvertToRandomSpawn(tempTransform, 0.3f, 0.5f);
                    }
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(DungeonGenerator), "ProcessGlobalProps")]
    [HarmonyPostfix]
    private static void ProcessGlobalPropsPostfix(DungeonGenerator __instance)
    {
        if (__instance.DungeonFlow.name != "Level3Flow") return;

        foreach (Tile tile in __instance.CurrentDungeon.AllTiles)
        {
            Transform tempTransform;
            GameObject tempObject;
            switch (tile.name)
            {
                case "TunnelSplit(Clone)":
                    tempTransform = tile.transform.Find("SouthWallProps/Shelf1 (14)")?.transform;
                    if (tempTransform != null)
                    {
                        Plugin.LogDebug($"Found a shelf at " + tempTransform.position.x + ", " + tempTransform.position.y + ", " + tempTransform.position.z);
                        Functions.CreateItemSpawn(tempTransform, new Vector3(-7.67999983f, -6.5999999f, 1.78999996f), "SmallItems", 15, true);
                    }

                    tempTransform = tile.transform.Find("Props/PropSet2/WoodPalletPile2x")?.transform;
                    if (tempTransform != null)
                    {
                        Plugin.LogDebug($"Found a pallet at " + tempTransform.position.x + ", " + tempTransform.position.y + ", " + tempTransform.position.z);
                        Functions.CreateItemSpawn(tempTransform, new Vector3(0.300000012f, 0.0399999991f, 2.53999996f), "TabletopItems", 3, false);
                    }

                    tempTransform = tile.transform.Find("Props/PropSet2/Minecart (1)")?.transform;
                    if (tempTransform != null)
                    {
                        Plugin.LogDebug($"Found a minecart at " + tempTransform.position.x + ", " + tempTransform.position.y + ", " + tempTransform.position.z);
                        Transform itemSpawn = Functions.CreateItemSpawn(tempTransform, new Vector3(-0.74000001f, 1.22000003f, 4.11000013f), "SmallItems", 1, false)?.transform;
                        if (itemSpawn != null) Functions.ConvertToRandomSpawn(itemSpawn, 0.25f, 0.8f);
                    }
                    break;

                case "TunnelSplitEndTile(Clone)":
                    tempTransform = tile.transform.Find("SouthWallProps/Shelf1 (14)")?.transform;
                    if (tempTransform != null)
                    {
                        Plugin.LogDebug($"Found a shelf at " + tempTransform.position.x + ", " + tempTransform.position.y + ", " + tempTransform.position.z);
                        Functions.CreateItemSpawn(tempTransform, new Vector3(-7.67999983f, -6.5999999f, 1.78999996f), "SmallItems", 15, true);
                    }
                    break;

                case "CaveCrampedIntersectTile(Clone)":
                    tempObject = tile.transform.Find("TablePropSpawn")?.gameObject;
                    if (tempObject != null)
                    {
                        Plugin.LogDebug($"Destroying table prop spawn at " + tempObject.transform.position.x + ", " + tempObject.transform.position.y + ", " + tempObject.transform.position.z);
                        GameObject.Destroy(tempObject);
                    }
                    break;

                case "CaveWaterTile(Clone)":
                    tempObject = tile.transform.Find("MapHazardSpawnType1")?.gameObject;
                    if (tempObject != null)
                    {
                        Plugin.LogDebug($"Destroying hazard spawn at " + tempObject.transform.position.x + ", " + tempObject.transform.position.y + ", " + tempObject.transform.position.z);
                        GameObject.Destroy(tempObject);
                    }
                    Mesh sharedMesh = tile.transform.Find("WaterTileMesh")?.gameObject.GetComponent<MeshCollider>()?.sharedMesh;
                    if(sharedMesh != null)
                    {
                        Plugin.LogDebug($"Changing collision on water tile at " + tile.transform.position.x + ", " + tile.transform.position.y + ", " + tile.transform.position.z);
                        sharedMesh = Assets.waterTileCollision;
                    }
                    break;
            };
        }
    }

    private class Functions
    {
        internal static GameObject CreateItemSpawn(Transform transform, Vector3 localPosition, string spawn, int range, bool copy)
        {
            GameObject obj = new GameObject();
            obj.name = $"AddedItemSpawn_" + spawn;
            obj.transform.SetParent(transform);
            obj.transform.localPosition = localPosition;
            RandomScrapSpawn randomScrapSpawn = obj.AddComponent<RandomScrapSpawn>();
            randomScrapSpawn.spawnableItems = GetItemGroupFromString(spawn);
            randomScrapSpawn.itemSpawnRange = range;
            randomScrapSpawn.spawnedItemsCopyPosition = copy;

            return obj;
        }

        internal static GameObject ConvertToRandomSpawn(Transform transform, float mainWeight, float branchWeight)
        {
            GameObject obj = new GameObject();
            obj.name = $"AddedRandomSpawn_" + transform.name;
            obj.transform.SetParent(transform.parent);
            transform.SetParent(obj.transform);
            LocalPropSet localPropSet = obj.AddComponent<LocalPropSet>();

            GameObjectChance objSpawnChance = new GameObjectChance();
            objSpawnChance.Value = transform.gameObject;
            objSpawnChance.MainPathWeight = mainWeight;
            objSpawnChance.BranchPathWeight = branchWeight;

            localPropSet.Props.Weights.Add(objSpawnChance);
            localPropSet.PropCount.Min = 0;
            localPropSet.PropCount.Max = 1;

            return obj;
        }

        internal static ItemGroup GetItemGroupFromString(string str)
        {
            foreach (ItemGroup itr in Resources.FindObjectsOfTypeAll<ItemGroup>())
            {
                if (itr.name == str) return itr;
            }
            return null;
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    [HarmonyPostfix]
    private static void ChangeManeaterData(StartOfRound __instance)
    {
        foreach (EnemyType enemyType in Resources.FindObjectsOfTypeAll<EnemyType>())
        {
            if(enemyType.name == "CaveDweller")
            {
                enemyType.increasedChanceInterior = RebalancedMineshaftConfig.increasedManeaterChance.Value ? 4 : -1;
                break;
            }
        }
    }

    [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ChangeMineshaftAmount(IEnumerable<CodeInstruction> instructions)
    {
        CodeMatcher codeMatcher = new CodeMatcher(instructions)
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(RoundManager), "currentDungeonType"))
            )
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4_6)
            )
            .SetInstruction(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MineshaftPatch), "ShouldSpawnAdditionalScrap"))
            );

        return codeMatcher.InstructionEnumeration();
    }

    public static int ShouldSpawnAdditionalScrap()
    {
        return RebalancedMineshaftConfig.extraScrapSpawn.Value ? 6 : 0;
    }
}