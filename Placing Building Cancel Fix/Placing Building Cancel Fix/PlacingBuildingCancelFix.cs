using Harmony;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Code;

namespace Phedg1Studios {
    namespace PlacingBuildingCancelFix {
        public class PlacingBuildingCancelFix : MonoBehaviour {
            static public KCModHelper helper;

            void Preload(KCModHelper helper) {
                PlacingBuildingCancelFix.helper = helper;
                var harmony = HarmonyInstance.Create("harmony");

                Assembly assembly = Assembly.GetAssembly(typeof(Water));
                System.Type type = assembly.GetType("Assets.Code.KeyboardControl");
                MethodInfo methodInfoA = type.GetMethod("UpdatePlaymodeKeys", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo methodInfoB = typeof(KeyboardControlUpdatePlaymodeKeys).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static);
                harmony.Patch(methodInfoA.GetBaseDefinition(), null, null, new HarmonyMethod(methodInfoB));
            }

            static public string NullSafeToString(object givenObject) {
                if (givenObject != null) {
                    return givenObject.ToString();
                } else {
                    return "null";
                }
            }

            public static class KeyboardControlUpdatePlaymodeKeys {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    int startIndex = -1;
                    for (int codeIndex = 0; codeIndex < codes.Count - 2; codeIndex++) {
                        if (codes[codeIndex].opcode == OpCodes.Ldsfld && NullSafeToString(codes[codeIndex].operand) == "GameUI inst") {
                            if (codes[codeIndex + 1].opcode == OpCodes.Ldfld && NullSafeToString(codes[codeIndex + 1].operand) == "PlacementMode CurrPlacementMode") {
                                if (codes[codeIndex + 2].opcode == OpCodes.Callvirt && NullSafeToString(codes[codeIndex + 2].operand) == "Boolean IsPlacing()") {
                                    startIndex = codeIndex + 3;
                                }
                            }
                        }
                    }
                    if (startIndex != -1) {
                        FieldInfo gameUI = typeof(GameUI).GetField("inst", BindingFlags.Public | BindingFlags.Static);
                        MethodInfo waitingToPlaceAgain = typeof(GameUI).GetMethod("WaitingToPlaceAgain", BindingFlags.Public | BindingFlags.Instance);

                        codes.Insert(startIndex, new CodeInstruction(OpCodes.Ldsfld, gameUI));
                        codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Callvirt, waitingToPlaceAgain));
                        codes.Insert(startIndex + 2, new CodeInstruction(OpCodes.Or));
                    }
                    return codes.AsEnumerable();
                }
            }
        }
    }
}
