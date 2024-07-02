using System.Reflection;
using HarmonyLib;

namespace SprintToggleFix;

public class SprintToggleFix : IModApi
{
    public void InitMod(Mod modInstance)
    {
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}