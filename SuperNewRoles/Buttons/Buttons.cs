using HarmonyLib;
using Hazel;
using System;
using UnityEngine;
using SuperNewRoles.Buttons;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Roles;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Buttons
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        public static CustomButton SheriffKillButton;
        public static CustomButton ClergymanLightOutButton;
        public static CustomButton SpeedBoosterBoostButton;
        public static CustomButton EvilSpeedBoosterBoostButton;
        public static CustomButton LighterLightOnButton;
        public static CustomButton CustomSabotageButton;
        public static CustomButton MovingSetButton;
        public static CustomButton MovingTpButton;
        public static CustomButton TeleporterButton;
        public static CustomButton DoorrDoorButton;
        public static CustomButton SelfBomberButton;

        public static CustomButton FreezerFreezeButton;
        public static CustomButton SpeederSpeedDownButton;
        public static CustomButton JackalKillButton;
        public static CustomButton JackalSidekickButton;

        public static TMPro.TMP_Text securityGuardButtonScrewsText;
        public static TMPro.TMP_Text vultureNumCorpsesText;
        public static TMPro.TMP_Text pursuerButtonBlanksText;
        public static TMPro.TMP_Text hackerAdminTableChargesText;
        public static TMPro.TMP_Text hackerVitalsChargesText;

        public static void setCustomButtonCooldowns()
        {
            Sheriff.ResetKillCoolDown();
            Clergyman.ResetCoolDown();
            Teleporter.ResetCoolDown();
            Jackal.resetCoolDown();
        }


        private static PlayerControl SheriffKillTarget;

        public static void Postfix(HudManager __instance)
        {
            RoleClass.clearAndReloadRoles();
            SuperNewRolesPlugin.Logger.LogInfo("HudMangerButton");

            JackalSidekickButton = new CustomButton(
                () =>
                {
                    var target = Jackal.JackalFixedPatch.JackalsetTarget();
                    if (target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && RoleClass.Jackal.IsCreateSidekick)
                    {
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CreateSidekick, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        CustomRPC.RPCProcedure.CreateSidekick(target.PlayerId);
                        RoleClass.Jackal.IsCreateSidekick = false;
                        Jackal.resetCoolDown();
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) && RoleClass.Jackal.IsCreateSidekick; },
                () =>
                {
                    return Jackal.JackalFixedPatch.JackalsetTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Jackal.EndMeeting(); },
                RoleClass.Jackal.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            JackalSidekickButton.buttonText = ModTranslation.getString("JackalCreateSidekickButtonName");
            JackalSidekickButton.showButtonText = true;

            JackalKillButton = new CustomButton(
                () =>
                {
                    if (Jackal.JackalFixedPatch.JackalsetTarget() && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                    {
                        ModHelpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, Jackal.JackalFixedPatch.JackalsetTarget());
                        Jackal.resetCoolDown();
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer); },
                () =>
                {
                    return Jackal.JackalFixedPatch.JackalsetTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Jackal.EndMeeting(); },
                __instance.KillButton.graphic.sprite,
                new Vector3(0, 1, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q,
                8
            );

            JackalKillButton.buttonText = HudManager.Instance.KillButton.buttonLabelText.text;
            JackalKillButton.showButtonText = true;

            SelfBomberButton = new Buttons.CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove) {
                        SelfBomber.SelfBomb();
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && SelfBomber.isSelfBomber(PlayerControl.LocalPlayer); },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { SelfBomber.EndMeeting(); },
                RoleClass.SelfBomber.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            DoorrDoorButton = new Buttons.CustomButton(
                () =>
                {
                    if (Doorr.CheckTarget() && PlayerControl.LocalPlayer.CanMove)
                    {
                        Doorr.DoorrBtn();
                        Roles.RoleClass.Doorr.ButtonTimer = DateTime.Now;
                        Doorr.ResetCoolDown();
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Doorr.isDoorr(PlayerControl.LocalPlayer); },
                () =>
                {
                    return Doorr.CheckTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Doorr.EndMeeting(); },
                RoleClass.Doorr.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            DoorrDoorButton.buttonText = ModTranslation.getString("DoorrButtonText");
            DoorrDoorButton.showButtonText = true;
            
            TeleporterButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    Roles.RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                    TeleporterButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Teleporter.TeleportStart();
                    Teleporter.ResetCoolDown();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Teleporter.IsTeleporter(PlayerControl.LocalPlayer); },
                () =>
                {
                    return true && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Teleporter.EndMeeting(); },
                RoleClass.Teleporter.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            TeleporterButton.buttonText = ModTranslation.getString("TeleporterTeleportButton");
            TeleporterButton.showButtonText = true;

            MovingSetButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    if (!Moving.IsSetPostion()) {
                        Moving.SetPostion();
                    }
                    Moving.ResetCoolDown();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Moving.IsMoving(PlayerControl.LocalPlayer) && !Moving.IsSetPostion(); },
                () =>
                {
                    return true && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Moving.EndMeeting(); },
                RoleClass.Moving.getNoSetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            MovingSetButton.buttonText = ModTranslation.getString("MovingButtonSetName");
            MovingSetButton.showButtonText = true;

            MovingTpButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    if (Moving.IsSetPostion())
                    {
                        Moving.TP();
                    }
                    Moving.ResetCoolDown();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Moving.IsMoving(PlayerControl.LocalPlayer) && Moving.IsSetPostion(); },
                () =>
                {
                    return true && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Moving.EndMeeting(); },
                RoleClass.Moving.getSetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            MovingTpButton.buttonText = ModTranslation.getString("MovingButtonTpName");
            MovingTpButton.showButtonText = true;

            SheriffKillButton = new Buttons.CustomButton(
                () =>
                {
                    if (RoleClass.Sheriff.KillMaxCount >= 1)
                    {
                        RoleClass.Sheriff.KillMaxCount--;
                        var Target = PlayerControlFixedUpdatePatch.setTarget();
                        var misfire = !Roles.Sheriff.IsSheriffKill(Target);
                        var TargetID = Target.PlayerId;
                        var LocalID = PlayerControl.LocalPlayer.PlayerId;

                        CustomRPC.RPCProcedure.SheriffKill(LocalID, TargetID, misfire);

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SheriffKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(LocalID);
                        killWriter.Write(TargetID);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        Sheriff.ResetKillCoolDown();
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Roles.Sheriff.IsSheriff(PlayerControl.LocalPlayer) && RoleClass.Sheriff.KillMaxCount >= 1; },
                () =>
                {
                    return PlayerControlFixedUpdatePatch.setTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Sheriff.EndMeeting(); },
                RoleClass.Sheriff.getButtonSprite(),
                new Vector3(0f, 1f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q,
                8
            );

            ClergymanLightOutButton = new Buttons.CustomButton(
                () =>
                {
                    RoleClass.Clergyman.IsLightOff = true;
                    Roles.RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                    ClergymanLightOutButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Clergyman.LightOutStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Clergyman.isClergyman(PlayerControl.LocalPlayer); },
                () =>
                {
                    if (ClergymanLightOutButton.Timer <= 0)
                    {
                        return true;
                    }
                    return false;
                },
                () => { Clergyman.EndMeeting(); },
                RoleClass.Clergyman.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            ClergymanLightOutButton.buttonText = ModTranslation.getString("ClergymanLightOutButtonName");
            ClergymanLightOutButton.showButtonText = true;

            SpeedBoosterBoostButton = new Buttons.CustomButton(
                () =>
                {
                    Roles.RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
                    SpeedBoosterBoostButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    SpeedBooster.BoostStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && SpeedBooster.IsSpeedBooster(PlayerControl.LocalPlayer); },
                () =>
                {
                    if (SpeedBoosterBoostButton.Timer <= 0)
                    {
                        return true;
                    }
                    return false;
                },
                () => { SpeedBooster.EndMeeting(); },
                RoleClass.SpeedBooster.GetSpeedBoostButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            SpeedBoosterBoostButton.buttonText = ModTranslation.getString("SpeedBoosterBoostButtonName");
            SpeedBoosterBoostButton.showButtonText = true;
            SpeedBoosterBoostButton.HasEffect = true;

            EvilSpeedBoosterBoostButton = new Buttons.CustomButton(
                () =>
                {
                    Roles.RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
                    EvilSpeedBoosterBoostButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    EvilSpeedBooster.BoostStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && EvilSpeedBooster.IsEvilSpeedBooster(PlayerControl.LocalPlayer); },
                () =>
                {
                    if (EvilSpeedBoosterBoostButton.Timer <= 0)
                    {
                        return true;
                    }
                    return false;
                },
                () => { EvilSpeedBooster.EndMeeting(); },
                RoleClass.SpeedBooster.GetSpeedBoostButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            EvilSpeedBoosterBoostButton.buttonText = ModTranslation.getString("EvilSpeedBoosterBoostButtonName");
            EvilSpeedBoosterBoostButton.showButtonText = true;
            LighterLightOnButton = new Buttons.CustomButton(
                () =>
                {
                    RoleClass.Lighter.IsLightOn= true;
                    Roles.RoleClass.Lighter.ButtonTimer = DateTime.Now;
                    LighterLightOnButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Lighter.LightOnStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Lighter.isLighter(PlayerControl.LocalPlayer); },
                () =>
                {
                    if (LighterLightOnButton.Timer <= 0)
                    {
                        return true;
                    }
                    return false;
                },
                () => { Lighter.EndMeeting(); },
                RoleClass.Lighter.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );

            ClergymanLightOutButton.buttonText = ModTranslation.getString("ClergymanLightOutButtonName");
            ClergymanLightOutButton.showButtonText = true;
            // Set the default (or settings from the previous game) timers/durations when spawning the buttons
            setCustomButtonCooldowns();
        }
    }
}
