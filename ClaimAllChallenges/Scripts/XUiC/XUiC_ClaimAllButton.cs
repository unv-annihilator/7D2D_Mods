using System.Linq;
using Challenges;

namespace ClaimAllChallenges.Scripts.XUiC;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global
public class XUiC_ClaimAllButton : XUiController
{
    private XUiC_SimpleButton _btnCompleteAll;

    public override void Init()
    {
        base.Init();

        _btnCompleteAll = GetChildById("btnCompleteAll").GetChildByType<XUiC_SimpleButton>();
        if (_btnCompleteAll == null)
            Log.Error("[ClaimAllChallenges] Failed to find button 'btnCompleteAll'");
        else
            _btnCompleteAll.OnPressed += BtnCompleteAll_Controller_OnPress;
    }

    private void BtnCompleteAll_Controller_OnPress(XUiController _sender, int _mousebutton)
    {
        CompleteAllChallenges();
    }

    private void CompleteAllChallenges()
    {
        var entityPlayer = xui.playerUI.entityPlayer;
        var challengeJournal = entityPlayer.challengeJournal;
        foreach (var challenge in challengeJournal.ChallengeDictionary.Values
                     .Where(challenge => challenge.ReadyToComplete).Where(challenge => !challenge.needsPrerequisites))
        {
            challenge.ChallengeState = Challenge.ChallengeStates.Redeemed;
            challenge.Redeem();
            QuestEventManager.Current.ChallengeCompleted(challenge.ChallengeClass, true);
        }
    }
}
