/* ◯作り方◯
    1.ICrewmateかINeutralかIImpostorのどれかを継承する // [x]
    2.必要なインターフェースを実装する // [x]
    3.Roleinfo,Optioninfo,Introinfoを設定する // [x]
    4.設定を作成する(CreateOptionが必要なければOptioninfoのoptionCreatorをnullにする) // [x]
    5.インターフェースの内容を実装していく // [ ]
*/

using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class MedicalTechnologist : RoleBase, ICrewmate, ISupportSHR, ICustomButton, INameHandler, IMeetingHandler, ICheckMurderHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(MedicalTechnologist),
        (p) => new MedicalTechnologist(p),
        RoleId.MedicalTechnologist,
        "MedicalTechnologist",
        new(37, 159, 148, byte.MaxValue),
        new(RoleId.MedicalTechnologist, TeamTag.Crewmate, RoleTag.Information),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );

    public static new OptionInfo Optioninfo =
        new(RoleId.MedicalTechnologist, 406700, true,
            CoolTimeOption: (15f, 2.5f, 60f, 2.5f, true),
            AbilityCountOption: (1, 1, 15, 1, true),
            optionCreator: null);

    public static new IntroInfo Introinfo = new(RoleId.MedicalTechnologist, introSound: RoleTypes.Scientist);

    // RoleClass

    /// <summary>
    /// 残りアビリティ使用可能回数
    /// </summary>
    public int AbilityRemainingCount;
    /// <summary>
    /// サンプル取得中のクルー
    /// </summary>
    private (byte FirstCrew, byte SecondCrew) SampleCrews;

    public MedicalTechnologist(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        MTButtonInfo = new(
            null,
            this,
            () => ButtonOnClick(),
            (isAlive) => isAlive,
            CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            OnMeetingEnds: MTButtonReset,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MedicalTechnologistButton.png", 115f),
            () => Optioninfo.CoolTime,
            new(-2f, 1, 0),
            "MedicalTechnologistButtonName",
            KeyCode.F,
            49,
            baseButton: HudManager.Instance.AbilityButton,
            CouldUse: () => OnCouldUse(),
            SetTargetUntargetPlayer: () => SetTargetUntargetPlayer(),
            isUseSecondButtonInfo: true
        ); // [x]MEMO : 残り回数表示の更新等をできるように追加する

        CustomButtonInfos = new CustomButtonInfo[1] { MTButtonInfo };

        AbilityRemainingCount = Optioninfo.AbilityMaxCount;
        SampleCrews = (byte.MaxValue, byte.MaxValue);
    }

    // ISupportSHR
    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole => RoleTypes.Impostor;
    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
        ChangePlayers[this.Player.PlayerId] = $"{ChangeName.GetNowName(ChangePlayers, this.Player)}\n{MtButtonCountString()}";
    }

    // ICustomButton
    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo MTButtonInfo { get; }
    private void ButtonOnClick()
    {
        if (SampleCrews.FirstCrew == byte.MaxValue) SampleCrews.FirstCrew = MTButtonInfo.CurrentTarget.PlayerId;
        else if (SampleCrews.SecondCrew == byte.MaxValue)
        {
            SampleCrews.SecondCrew = MTButtonInfo.CurrentTarget.PlayerId;
            AbilityRemainingCount--;
        }
        else Logger.Error("既に検体を取得済みにもかかわらず, 対象が取得されました。", "MedicalTechnologist");

        MTButtonInfo.customButton.SecondButtonInfoText.text = MtButtonCountString();
    }
    private void MTButtonReset() // [x]MEMO : 対象のリセット, [ターン中使用回数をリセット => ターン中使用回数と対象は同じ変数で管理に]
    {
        SampleCrews = (byte.MaxValue, byte.MaxValue);
        MTButtonInfo.customButton.SecondButtonInfoText.text = MtButtonCountString();
    }
    private string MtButtonCountString() // [x]MEMO : 残り全体回数\n現在フェイズ残り指定回数 => 選択対象の名前 // [ ]MEMO : SHRではシェリフと同じように名前で表示
    {
        string remainingCountText = $"{ModTranslation.GetString("MedicalTechnologistAbilityRemainingCount")}{AbilityRemainingCount}";
        string targetText = $"{ModTranslation.GetString("MedicalTechnologistSelectTarget")}";

        string mark = ModHelpers.Cs(new Color(179f / 255f, 0f, 0f), " \u00A9"); // © => 赤血球
        string targetInfoText =
            SampleCrews.FirstCrew == byte.MaxValue && SampleCrews.SecondCrew == byte.MaxValue
                ? $"{ModTranslation.GetString("MedicalTechnologistUnselected")}" // 未選択
                : SampleCrews.FirstCrew != byte.MaxValue && SampleCrews.SecondCrew == byte.MaxValue
                    ? $"{mark}" // 一人選択済み
                    : $"{mark}{mark}"; // 対象選択完了

        string infoText = AbilityRemainingCount > 0 || !(SampleCrews.FirstCrew == byte.MaxValue && SampleCrews.SecondCrew == byte.MaxValue)
            ? $"{remainingCountText}\n{targetText}{targetInfoText}"
            : $"{remainingCountText}";

        return infoText;
    }
    private bool OnCouldUse() => AbilityRemainingCount > 0 && (SampleCrews.FirstCrew == byte.MaxValue || SampleCrews.SecondCrew == byte.MaxValue);
    private List<PlayerControl> SetTargetUntargetPlayer()
    {
        List<PlayerControl> untargetPlayer = new();
        if (SampleCrews.FirstCrew != byte.MaxValue) untargetPlayer.Add(ModHelpers.PlayerById(SampleCrews.FirstCrew));
        if (SampleCrews.SecondCrew != byte.MaxValue) untargetPlayer.Add(ModHelpers.PlayerById(SampleCrews.SecondCrew));
        return untargetPlayer;
    }

    // INameHandler
    public void OnHandleName()
    {
        string suffix = ModHelpers.Cs(new Color(179f / 255f, 0f, 0f), " \u00A9"); // © => 赤血球

        if (SampleCrews.FirstCrew != byte.MaxValue)
        {
            PlayerControl first = ModHelpers.PlayerById(SampleCrews.FirstCrew);
            SetNamesClass.SetPlayerNameText(first, $"{first.NameText().text}{suffix}");
        }
        if (SampleCrews.SecondCrew != byte.MaxValue)
        {
            PlayerControl Second = ModHelpers.PlayerById(SampleCrews.SecondCrew);
            SetNamesClass.SetPlayerNameText(Second, $"{Second.NameText().text}{suffix}");
        }
    }

    // IMeetingHandler
    public void StartMeeting() { }
    public void CloseMeeting() { }

    // ICheckMurderHandler
    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        return true; // [ ]MEMO : クールはリセットしたい。~> キルを守護で防ぐ事が必要?
    }
}