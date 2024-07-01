using System.Reflection;
using BeyondStorage.Scripts;
using HarmonyLib;

namespace BeyondStorage
{
    public class BeyondStorage : IModApi
    {
        private static BeyondStorage _context;
        public static ModConfig Config;
        public static Mod ModInstance;

        public void InitMod(Mod modInstance)
        {
            _context = this;
            Config = ModConfig.LoadConfig(_context);
            ModInstance = modInstance;
            var harmony = new Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}