using System.Reflection;

namespace AutoClaimChallenges;

public class AutoClaimChallenges : IModApi
{
    public void InitMod(Mod modInstance)
    {
        var harmony = new HarmonyLib.Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}