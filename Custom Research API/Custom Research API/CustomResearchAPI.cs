using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;

namespace Phedg1Studios {
    namespace CustomResearchAPI {
        public class CustomResearchAPI : MonoBehaviour {
            static public KCModHelper helper;
            static private bool updateUI = false;

            // Initialize systems
            void Preload(KCModHelper helper) {
                CustomResearchAPI.helper = helper;
                Application.logMessageReceived += OnLogMessageReceived;
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            [HarmonyPatch(typeof(ResearchUI))]
            [HarmonyPatch("AddOption")]
            public static class ResearchUIAddOption {
                static void Postfix() {
                    updateUI = true;
                }
            }

            [HarmonyPatch(typeof(ResearchUI))]
            [HarmonyPatch("Init")]
            public static class ResearchUISInit {
                [HarmonyPriority(200)]
                static void Postfix(ResearchUI __instance) {
                    if (updateUI) {
                        ResearchUIUpdater.UpdateUI(__instance.optionContainer.gameObject);
                        ResearchUIUpdater.UpdateContentSize(__instance.optionContainer.gameObject);
                        MethodInfo localizationManagerOnOnLocalizeEventInfo = typeof(ResearchUI).GetMethod("LocalizationManagerOnOnLocalizeEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                        localizationManagerOnOnLocalizeEventInfo.Invoke(__instance, null);
                        updateUI = false;
                    }
                }
            }

            [HarmonyPatch(typeof(ResearchUI))]
            [HarmonyPatch("RefreshForSelectedLibrary")]
            public static class ResearchUIRefreshForSelectedLibrary {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    int startIndexA = -1;
                    int startIndexB = -1;
                    for (int codeIndex = 0; codeIndex < codes.Count - 2; codeIndex++) {
                        if (codes[codeIndex].opcode == OpCodes.Ldloc_S && NullSafeToString(codes[codeIndex].operand) == "System.Int32 (5)") {
                            if (codes[codeIndex + 1].opcode == OpCodes.Ldc_I4_8 && NullSafeToString(codes[codeIndex + 1].operand) == "null") {
                                if (codes[codeIndex + 2].opcode == OpCodes.Blt && NullSafeToString(codes[codeIndex + 2].operand) == "System.Reflection.Emit.Label") {
                                    startIndexA = codeIndex + 1;
                                }
                            }
                        }
                        if (codes[codeIndex].opcode == OpCodes.Ldloc_S && NullSafeToString(codes[codeIndex].operand) == "System.Int32 (5)") {
                            if (codes[codeIndex + 1].opcode == OpCodes.Stloc_S && NullSafeToString(codes[codeIndex + 1].operand) == "Player+UpgradeType (6)") {
                                startIndexB = codeIndex + 2;
                            }
                        }
                        //helper.Log(codes[codeIndex].opcode.ToString() + " " + NullSafeToString(codes[codeIndex].operand));
                    }
                    if (startIndexA != -1 && startIndexB != -1) {
                        FieldInfo optionMapField = typeof(ResearchUI).GetField("optionMap", BindingFlags.NonPublic | BindingFlags.Instance);
                        MethodInfo dictionaryCount = typeof(Dictionary<Player.UpgradeType, Transform>).GetProperty("Count", BindingFlags.Public | BindingFlags.Instance).GetMethod;
                        MethodInfo dictionaryKeys = typeof(Dictionary<Player.UpgradeType, Transform>).GetProperty("Keys", BindingFlags.Public | BindingFlags.Instance).GetMethod;
                        MethodInfo dictionaryKeysToList = typeof(Enumerable).GetMethod("ToList", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(new Type[] { typeof(Player.UpgradeType) });
                        MethodInfo listGetItem = typeof(List<Player.UpgradeType>).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance).GetMethod;

                        codes.RemoveAt(startIndexA + 0);
                        codes.Insert(startIndexA + 0, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(startIndexA + 1, new CodeInstruction(OpCodes.Ldfld, optionMapField));
                        codes.Insert(startIndexA + 2, new CodeInstruction(OpCodes.Callvirt, dictionaryCount));

                        codes.Insert(startIndexB + 0, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(startIndexB + 1, new CodeInstruction(OpCodes.Ldfld, optionMapField));
                        codes.Insert(startIndexB + 2, new CodeInstruction(OpCodes.Callvirt, dictionaryKeys));
                        codes.Insert(startIndexB + 3, new CodeInstruction(OpCodes.Callvirt, dictionaryKeysToList));
                        codes.Insert(startIndexB + 4, new CodeInstruction(OpCodes.Ldloc_S, 5));
                        codes.Insert(startIndexB + 5, new CodeInstruction(OpCodes.Callvirt, listGetItem));
                        codes.Insert(startIndexB + 6, new CodeInstruction(OpCodes.Stloc_S, 6));
                    }
                    return codes.AsEnumerable();
                }
            }

            static public string NullSafeToString(object givenObject) {
                if (givenObject != null) {
                    return givenObject.ToString();
                } else {
                    return "null";
                }
            }




            static public void Log(object givenObject, bool traceBack = false) {
                if (givenObject == null) {
                    helper.Log("null");
                } else {
                    helper.Log(givenObject.ToString());
                }
                if (traceBack) {
                    helper.Log(StackTraceUtility.ExtractStackTrace());
                }
            }

            static public void OnLogMessageReceived(string condition, string stackTrace, LogType type) {
                if (type == LogType.Exception) {
                    Log("Unhandled Exception: " + condition + "\n" + stackTrace);
                }
            }
        }
    }
}
