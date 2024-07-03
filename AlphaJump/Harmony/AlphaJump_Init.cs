using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace AlphaJump;

[UsedImplicitly]
public class AlphaJump_Init : IModApi
{
    public void InitMod(Mod modInstance)
    {
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}