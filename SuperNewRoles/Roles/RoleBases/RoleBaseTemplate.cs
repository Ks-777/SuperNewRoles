namespace SuperNewRoles.Roles.RoleBases;
//◯作り方◯
// 1.ICrewmateかINeutralかIImpostorのどれかを継承する
// 2.必要なインターフェースを実装する
// 3.Roleinfo,Optioninfo,Introinfoを設定する
// 4.設定を作成する(CreateOptionが必要なければOptioninfoのoptionCreatorをnullにする)
// 5.インターフェースの内容を実装していく
/*

using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;

public class RoleBaseTemplate : RoleBase
{
    public static new RoleInfo Roleinfo = new(
        typeof(RoleBaseTemplate),
        (p) => new RoleBaseTemplate(p),
        RoleId.RoleBaseTemplate,
        "RoleBaseTemplate",
        RoleClass.ImpostorRed,
        new(RoleId.RoleBaseTemplate, TeamTag.TEAMTAG),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.RoleBaseTemplate, 200000, ,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.RoleBaseTemplate, introSound: RoleTypes.Impostor);
    public static void CreateOption()
    {
    }
    public RoleBaseTemplate(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }
}
*/