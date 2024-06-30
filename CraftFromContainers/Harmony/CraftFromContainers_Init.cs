using System.Reflection;
using CraftFromContainers.Scripts;
using HarmonyLib;

namespace CraftFromContainers
{
    public class CraftFromContainers : IModApi
    {
        private static CraftFromContainers _context;
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