using System.Reflection;
using BeyondStorage.Scripts;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeyondStorage;

[UsedImplicitly]
public class BeyondStorage : IModApi
{
    private static BeyondStorage _context;
    internal static ModConfig Config;
    internal static Mod ModInstance;

    public void InitMod(Mod modInstance)
    {
        _context = this;
        Config = ModConfig.LoadConfig(_context);
        ModInstance = modInstance;
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}