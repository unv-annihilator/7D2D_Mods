using System.Linq;
using Challenges;

namespace ClaimAllChallenges.Scripts.XUiC;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global
public class XUiC_ClaimAllButton : XUiController {
    private const string LogPrefix = "[ClaimAllChallenges]";
    private const string CompleteAllButtonId = "btnCompleteAll";
    private XUiC_SimpleButton _btnCompleteAll;

    public override void Init() {
        base.Init();

        _btnCompleteAll = GetChildById(CompleteAllButtonId).GetChildByType<XUiC_SimpleButton>();
        if (_btnCompleteAll == null)
            Log.Error($"{LogPrefix} Failed to find button '{CompleteAllButtonId}'");
        else
            _btnCompleteAll.OnPressed += BtnCompleteAll_Controller_OnPress;
    }

    // ReSharper disable InconsistentNaming
    private void BtnCompleteAll_Controller_OnPress(XUiController _sender, int _mousebutton) {
        // ReSharper restore InconsistentNaming
        CompleteAllChallenges();
    }

    // ReSharper disable InconsistentNaming
    public override void Update(float _dt) {
        // ReSharper restore InconsistentNaming
        base.Update(_dt);
        if (!IsDirty)
            return;
        RefreshBindings(true);
        IsDirty = false;
    }

    public override void OnOpen() {
        base.OnOpen();
        IsDirty = true;
        RefreshBindings(true);
    }

    private void CompleteAllChallenges() {
        var entityPlayer = xui.playerUI.entityPlayer;
        var challengeJournal = entityPlayer.challengeJournal;
        foreach (var challenge in challengeJournal.ChallengeDictionary.Values.Where(challenge => challenge.ReadyToComplete).Where(challenge => challenge.ChallengeClass.ChallengeGroup.IsVisible())) {
            challenge.ChallengeState = Challenge.ChallengeStates.Redeemed;
            challenge.Redeem();
            QuestEventManager.Current.ChallengeCompleted(challenge.ChallengeClass, true);
        }

        RefreshBindings(true);
        IsDirty = true;
    }

    public override bool GetBindingValue(ref string value, string bindingName) {
        switch (bindingName) {
            // ReSharper disable once StringLiteralTypo
            case "hascompletedchallenges":
                var entityPlayer = xui.playerUI.entityPlayer;
                value = entityPlayer == null
                    ? "false"
                    : entityPlayer.challengeJournal.HasCompletedChallenges().ToString();
                return true;
            default:
                return false;
        }
    }
}