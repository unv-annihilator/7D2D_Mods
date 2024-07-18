using System.Reflection;
using HarmonyLib;

namespace AlphaJump;

public class AlphaJumpInit : IModApi {
    public void InitMod(Mod modInstance) {
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}