using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CraftFromContainers.Scripts;
using HarmonyLib;

namespace CraftFromContainers.XUIC
{
    public static class RecipeList
    {
        [HarmonyPatch(typeof(XUiC_RecipeList), nameof(XUiC_RecipeList.Update))]
        private static class XUiC_RecipeList_Update_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                LogUtil.DebugLog("Transpiling XUiC_RecipeList.Update");
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                    if (i > 2 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand ==
                        AccessTools.Method(typeof(XUiC_RecipeList), "BuildRecipeInfosList"))
                    {
                        var ci = codes[i - 2];
                        var ciNew = new CodeInstruction(OpCodes.Ldloc_0);
                        ci.MoveLabelsTo(ciNew);
                        LogUtil.DebugLog("XUiC_RecipeList.Update: Adding method to add items from all storages");
                        codes.Insert(i - 2,
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(ContainerUtils),
                                    nameof(ContainerUtils.AddAllStorageStacks))));
                        codes.Insert(i - 2, ciNew);
                        break;
                    }

                return codes.AsEnumerable();
            }
        }
    }
}