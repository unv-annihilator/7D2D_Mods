using System.Reflection;
using BeyondStorage.Scripts.Common;
using BeyondStorage.Scripts.Configuration;
using BeyondStorage.Scripts.Server;
using HarmonyLib;
#if DEBUG
using HarmonyLib.Tools;
#endif

// ReSharper disable ClassNeverInstantiated.Global

namespace BeyondStorage;

public class BeyondStorage : IModApi {
    private static BeyondStorage _context;

    internal static Mod ModInstance;

    public void InitMod(Mod modInstance) {
        _context = this;
        ModConfig.LoadConfig(_context);
        ModInstance = modInstance;
        var harmony = new Harmony(GetType().ToString());
#if DEBUG
        HarmonyFileLog.Enabled = true;
#endif
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        ModEvents.PlayerSpawnedInWorld.RegisterHandler(ServerUtils.PlayerSpawnedInWorld);

        ModEvents.GameStartDone.RegisterHandler(EventsUtil.GameStartDone);
        ModEvents.GameShutdown.RegisterHandler(EventsUtil.GameShutdown);
    }
}