using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace AutoClaimChallenges;

[UsedImplicitly]
public class AutoClaimChallenges : IModApi
{
    public void InitMod(Mod modInstance)
    {
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}