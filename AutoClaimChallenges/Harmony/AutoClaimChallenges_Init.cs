using System.Reflection;
using HarmonyLib;

namespace AutoClaimChallenges;

public class AutoClaimChallenges : IModApi {
    public void InitMod(Mod modInstance) {
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}