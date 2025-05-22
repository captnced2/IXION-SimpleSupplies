using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BulwarkStudios.Stanford.Torus.Sectors;
using HarmonyLib;
using IMHelper;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SimpleSupplies;

[BepInPlugin(Guid, Name, Version)]
[BepInProcess("IXION.exe")]
[BepInDependency("captnced.IMHelper", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    private const string Guid = "captnced.SimpleSupplies";
    private const string Name = "SimpleSupplies";
    private const string Version = "1.1.0";
    internal new static ManualLogSource Log;
    private static Harmony harmony;
    private static bool enabled = true;

    public override void Load()
    {
        Log = base.Log;
        harmony = new Harmony(Guid);
        if (IL2CPPChainloader.Instance.Plugins.ContainsKey("captnced.IMHelper")) setEnabled();
        if (!enabled)
            Log.LogInfo("Disabled by IMHelper!");
        else
            init();
    }

    private static void setEnabled()
    {
        enabled = ModsMenu.isSelfEnabled();
    }

    private static void init()
    {
        SceneManager.activeSceneChanged += (UnityAction<Scene, Scene>)sceneChangedListener;
        harmony.PatchAll();
        foreach (var patch in harmony.GetPatchedMethods())
            Log.LogInfo("Patched " + patch.DeclaringType + ":" + patch.Name);
        Log.LogInfo("Loaded \"" + Name + "\" version " + Version + "!");
    }

    private static void disable()
    {
        SceneManager.activeSceneChanged -= (UnityAction<Scene, Scene>)sceneChangedListener;
        harmony.UnpatchSelf();
        Log.LogInfo("Unloaded \"" + Name + "\" version " + Version + "!");
    }

    public static void enable(bool value)
    {
        enabled = value;
        if (enabled) init();
        else disable();
    }

    internal static void sceneChangedListener(Scene current, Scene next)
    {
        if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
        {
            Patches.init = false;
            Patches.buildings = new Dictionary<TorusSectorInstance, List<Patches.Building>>();
        }
    }
}