using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches.CursedTasks;

public class EnterCodeTask
{
    public static string TragetCode;
    public static string AnswerCode;
    public static string GetCode(out string card)
    {
        string code = string.Empty;
        card = string.Empty;
        if (Main.Random(1, 5) != 1)
        {
            int length = Main.Random(5, 20);
            string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            StringBuilder sb = new(length);
            for (int i = 0; i < length; i++)
                sb.Append(chars[Main.Random(0, chars.Length - 1)]);
            code = sb.ToString();
            card = code.Length <= 10 ? code : code.Insert(10, "\n");
        }
        else if (Main.Random(1, 2) != 1)
        {
            List<string> codes = new()
            {
                "slnur-3tyu8ki25",
                "ossn692hdtinpponch",
            };
            code = codes.GetRandom();
            if (!code.Contains("-")) card = code.Length <= 10 ? code : code.Insert(10, "\n");
            else
            {
                for (int i = 0; i < code.Length; i++)
                {
                    char text = code[i];
                    card += text;
                    if (text == '\u002d') card += "\n";
                }
            }
        }
        else
        {
            List<string> codes = new()
            {
                "ykundesu-01",
                "Kurato-Tsukishiro-07",
                "SabaCan33333333-09",
                "Glaceon-471-11",
                "seono968-12",
            };
            code = codes.GetRandom();
            if (!code.Contains("-")) card = code.Length <= 10 ? code : code.Insert(10, "\n");
            else
            {
                for (int i = 0; i < code.Length; i++)
                {
                    char text = code[i];
                    card += text;
                    if (text == '\u002d') card += "\n";
                }
            }
        }
        return code.ToUpper();
    }

    [HarmonyPatch(typeof(EnterCodeMinigame))]
    public static class EnterCodeMinigamePatch
    {
        [HarmonyPatch(nameof(EnterCodeMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(EnterCodeMinigame __instance)
        {
            if (!Main.IsCursed) return;

            __instance.transform.FindChild("Background").GetComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cursed.EnterCode.png", 100f);

            // テキスト類
            TragetCode = GetCode(out string card);
            AnswerCode = string.Empty;

            __instance.NumberText.enableAutoSizing = true;
            __instance.NumberText.fontSizeMax = 3.85f;
            __instance.NumberText.transform.localPosition = new(0.0559f, -0.4027f, -1.5f);

            __instance.TargetText.enableAutoSizing = true;
            __instance.TargetText.fontSizeMin = 0.9f;
            __instance.TargetText.text = card.ToUpper();

            //ボタン類
            int line = 0;
            foreach (UiElement ui in __instance.ControllerSelectable)
            {
                if (ui.name.Contains("reactorButton"))
                {
                    ui.transform.localScale = new(0.67f, 0.67f, 1f);
                    Vector2 pos = new(-2.055f, 2.295f);
                    ui.transform.localPosition = new(pos.x + (0.43f * line), pos.y, -0.1f);
                    line++;
                }
                if (ui.gameObject.name.Contains("admin_keypad_x")) ui.transform.localPosition = new(-2.18f, 0.456f, -0.1f);
                if (ui.gameObject.name.Contains("admin_keypad_check")) ui.transform.localPosition = new(2.195f, 0.456f, -0.1f);
            }

            for (int i = 0; i < 27; i++)
            {
                Vector2 pos = i switch
                {
                    <= 09 => new(-1.931f, 1.906f),
                    <= 18 => new(-1.8f, 1.519f),
                    <= 26 => new(-1.668f, 1.128f),
                    _ => new()
                };
                if (i is 0 or 10 or 19) line = 0;

                GameObject ui = Object.Instantiate(__instance.ControllerSelectable[0].gameObject);
                ui.transform.SetParent(__instance.transform.FindChild("Background"));
                ui.layer = 5;
                ui.name = $"reactorButton{i + 11}";
                ui.transform.localPosition = new(pos.x + (0.43f * line), pos.y, -0.1f);
                int key = i;
                ui.GetComponent<ButtonBehavior>().OnClick = new();
                ui.GetComponent<ButtonBehavior>().OnClick.AddListener((Action)(() => __instance.EnterDigit(key + 10)));
                line++;
            }

            
        }

        [HarmonyPatch(nameof(EnterCodeMinigame.ClearDigits)), HarmonyPostfix]
        public static void ClearDigitsPostfix() => AnswerCode = string.Empty;

        [HarmonyPatch(nameof(EnterCodeMinigame.EnterDigit)), HarmonyPrefix]
        public static bool EnterDigitPrefix(EnterCodeMinigame __instance, int i)
        {
            if (!Main.IsCursed) return true;
            if (__instance.animating) return false;
            if (__instance.done) return false;

            if (__instance.NumberText.text.Length >= 20)
            {
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.RejectSound, false, 1f, null);
                return false;
            }
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.NumberSound, false, 1f, null).pitch = Mathf.Lerp(0.8f, 1.2f, i / 36f);
            if (i < 10) __instance.numString += i.ToString();
            else if (i != 36) __instance.numString += (KeyCode)i;
            else __instance.numString += "-";
            AnswerCode = __instance.numString;
            __instance.NumberText.text = __instance.numString;
            return false;
        }

        [HarmonyPatch(nameof(EnterCodeMinigame.AcceptDigits)), HarmonyPrefix]
        public static bool AcceptDigitsPrefix(EnterCodeMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            Logger.Info($"回答 : {AnswerCode}, 答え : {TragetCode}", "EnterCodeTask");
            if (__instance.animating) return false;
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.NumberSound, false, 1f, null);

            Animate(__instance);
            if (AnswerCode == TragetCode) __instance.MyNormTask.NextStep();
            return false;
        }

        public static async void Animate(EnterCodeMinigame __instance)
        {
            __instance.animating = true;
            await Task.Delay(100);

            __instance.NumberText.text = string.Empty;
            await Task.Delay(100);

            string text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK);
            if (AnswerCode == TragetCode)
            {
                __instance.done = true;
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.AcceptSound, false, 1f, null);
                __instance.NumberText.text = text;
                await Task.Delay(100);

                __instance.NumberText.text = string.Empty;
                await Task.Delay(100);

                __instance.NumberText.text = text;
                await Task.Delay(100);

                __instance.NumberText.text = string.Empty;
                __instance.StartCoroutine(__instance.CoStartClose(0.5f));
            }
            else
            {
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.RejectSound, false, 1f, null);
                text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bad);
                __instance.NumberText.text = text;
                await Task.Delay(100);

                __instance.NumberText.text = string.Empty;
                await Task.Delay(100);

                __instance.NumberText.text = text;
                await Task.Delay(100);

                __instance.numString = string.Empty;
                __instance.NumberText.text = string.Empty;
                AnswerCode = string.Empty;
            }
            __instance.animating = false;
            Logger.Info($"終了", "EnterCodeTask");
        }
    }

    public enum KeyCode
    {
        Q = 10,
        W = 11,
        E = 12,
        R = 13,
        T = 14,
        Y = 15,
        U = 16,
        I = 17,
        O = 18,
        P = 19,
        A = 20,
        S = 21,
        D = 22,
        F = 23,
        G = 24,
        H = 25,
        J = 26,
        K = 27,
        L = 28,
        Z = 29,
        X = 30,
        C = 31,
        V = 32,
        B = 33,
        N = 34,
        M = 35,
    }
}