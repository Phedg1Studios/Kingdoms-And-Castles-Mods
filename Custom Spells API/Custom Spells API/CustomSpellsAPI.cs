using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Phedg1Studios.Shared;

namespace Phedg1Studios {
    namespace CustomSpellsAPI {
        public class CustomSpellsAPI : MonoBehaviour {
            static public KCModHelper helper;

            // Initialize systems
            void Preload(KCModHelper helper) {
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                CustomSpellsAPI.helper = helper;
            }

            // Modify loaded save data to be compatible with new custom spells
            [HarmonyPatch(typeof(WitchHut.WitchHutSaveData))]
            [HarmonyPatch("Unpack")]
            public static class WitchHutSaveDatatUnpack {
                static void Postfix(WitchUI __instance, ref WitchHut __result, WitchHut obj) {
                    System.Reflection.FieldInfo fieldInfo = typeof(WitchHut).GetField("spellData", BindingFlags.NonPublic | BindingFlags.Instance);
                    ICollection spellDataCollection = fieldInfo.GetValue(__result) as ICollection;
                    List<WitchHut.SpellData> spellDataList = new List<WitchHut.SpellData>();
                    foreach (object spellDataObject in spellDataCollection) {
                        spellDataList.Add((WitchHut.SpellData)spellDataObject);
                    }

                    fieldInfo = typeof(WitchHut).GetField("currSpellCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
                    ICollection cooldownCollection = fieldInfo.GetValue(__result) as ICollection;
                    List<int> cooldownList = new List<int>();
                    foreach (object cooldownObject in cooldownCollection) {
                        cooldownList.Add((int)cooldownObject);
                    }

                    List<int> cooldownListAdjusted = new List<int>();

                    for (int cooldownIndex = 0; cooldownIndex < cooldownList.Count; cooldownIndex++) {
                        if (cooldownIndex < spellDataList.Count) {
                            cooldownListAdjusted.Add(cooldownList[cooldownIndex]);
                        } else {
                            break;
                        }
                    }

                    for (int spellIndex = 0; spellIndex < spellDataList.Count - cooldownList.Count; spellIndex++) {
                        cooldownListAdjusted.Add(0);
                    }

                    int[] cooldownArray = new int[cooldownListAdjusted.Count];
                    for (int cooldownIndex = 0; cooldownIndex < cooldownListAdjusted.Count; cooldownIndex++) {
                        cooldownArray[cooldownIndex] = cooldownListAdjusted[cooldownIndex];
                    }
                    fieldInfo.SetValue(__result, cooldownArray);
                }
            }

            // Upgrade spell list into a scroll
            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("Start")]
            public static class WitchUIStart {
                [HarmonyPriority(200)]
                static void Postfix(WitchUI __instance) {
                    SpellUIUpdater.UpdateUI(__instance.spellList);
                    GameUI.inst.witchUI.RefreshAvailableSpells();
                }
            }

            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("RefreshAvailableSpells")]
            public static class WitchUIRefreshAvailableSpells {
                [HarmonyPriority(200)]
                static void Postfix(WitchUI __instance) {
                    SpellUIUpdater.UpdateContentSize(__instance.spellList);
                }
            }

            // Move the list back to the top each time the menu is open
            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("RefreshUI")]
            public static class WitchUIRefreshUI {
                static void Postfix(WitchUI __instance) {
                    if (SpellUIUpdater.scrollrect != null) {
                        //SpellUIUpdater.scrollrect.verticalNormalizedPosition = 1;
                    }
                }
            }
        }
    }
}
