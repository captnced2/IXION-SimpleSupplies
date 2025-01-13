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
    private const string Version = "1.0.0";
    internal new static ManualLogSource Log;
    internal static SettingsHelper.BooleanSetting setting;
    internal static bool helperPresent;

    public override void Load()
    {
        Log = base.Log;
        SceneManager.activeSceneChanged += (UnityAction<Scene, Scene>)sceneChangedListener;
        var harmony = new Harmony(Guid);
        harmony.PatchAll();
        foreach (var patch in harmony.GetPatchedMethods())
            Log.LogInfo("Patched " + patch.DeclaringType + ":" + patch.Name);
        if (IL2CPPChainloader.Instance.Plugins.ContainsKey("captnced.IMHelper")) addHelperSetting();
        Log.LogInfo("Loaded \"" + Name + "\" version " + Version + "!");
    }

    internal static void addHelperSetting()
    {
        helperPresent = true;
        var section = new SettingsHelper.SettingsSection("SimpleSupplies");
        setting = new SettingsHelper.BooleanSetting(section, "Enabled", "Enables/disables this mod", true, false);
    }

    internal static bool getSettingValue()
    {
        return setting.getValue();
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