using System.Reflection;
using BeyondStorage.Scripts.Configuration;
using HarmonyLib;
#if DEBUG
using HarmonyLib.Tools;

// ReSharper disable ClassNeverInstantiated.Global
#endif

namespace BeyondStorage;

public class BeyondStorage : IModApi {
    private static BeyondStorage _context;
    internal static ModConfig.Config Config;
    internal static Mod ModInstance;

    public void InitMod(Mod modInstance) {
        _context = this;
        Config = ModConfig.LoadConfig(_context);
        ModInstance = modInstance;
        var harmony = new Harmony(GetType().ToString());
#if DEBUG
        HarmonyFileLog.Enabled = true;
#endif
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}