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
        // Game Start Done Called when:
        //      - Loading into singleplayer world
        //      - Starting client hosted multiplayer world
        // Not Called during connecting TO server
        ModEvents.GameStartDone.RegisterHandler(EventsUtil.GameStartDone);
        // Game Shutdown Called when:
        //      - Quitting GAME (EXE)
        // NOT called when LEAVING a world
        ModEvents.GameShutdown.RegisterHandler(EventsUtil.GameShutdown);
        // Player Disconnected Called When:
        //      - Player disconnects from server YOU'RE hosting
        // NOT called when YOU disconnect
        // ModEvents.PlayerDisconnected.RegisterHandler(EventsUtil.PlayerDisconnected);
    }
}