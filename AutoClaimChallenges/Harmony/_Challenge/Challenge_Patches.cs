using Challenges;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace AutoClaimChallenges._Challenge;

[HarmonyPatch(typeof(Challenge))]
internal static class Challenge_Patches {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Challenge.HandleComplete))]
    private static void Challenge_HandleComplete_Postfix(Challenge __instance) {
        if (!__instance.ReadyToComplete)
            return;
        __instance.ChallengeState = Challenge.ChallengeStates.Redeemed;
        __instance.Redeem();
        QuestEventManager.Current.ChallengeCompleted(__instance.ChallengeClass, true);
    }
}