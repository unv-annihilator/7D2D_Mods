using Challenges;
using HarmonyLib;

namespace AutoClaimChallenges.Challenges
{
    internal static class ChallengeComplete
    {
        [HarmonyPatch(typeof(Challenge), nameof(Challenge.HandleComplete))]
        private static class Challenge_HandleComplete_Patch
        {
            private static void Postfix(Challenge __instance)
            {
                if (!__instance.ReadyToComplete)
                    return;
                __instance.ChallengeState = Challenge.ChallengeStates.Redeemed;
                __instance.Redeem();
                QuestEventManager.Current.ChallengeCompleted(__instance.ChallengeClass, true);
            }
        }
    }
}