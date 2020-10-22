using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Phedg1Studios {
    namespace Shared {
        public class RegisterSpells : MonoBehaviour {
            static public List<SpellDataCustom> spellData = new List<SpellDataCustom>();
            static public List<GameObject> additionalSpellButtons = new List<GameObject>();
            static public int spellDataOriginalCount = 0;

            // Add custom spell data
            [HarmonyPatch(typeof(WitchHut))]
            [HarmonyPatch("Awake")]
            public static class WitchHutAwake {
                static void Postfix(WitchHut __instance) {
                    System.Reflection.FieldInfo fieldInfo = typeof(WitchHut).GetField("spellData", BindingFlags.NonPublic | BindingFlags.Instance);
                    ICollection spellDataCollection = fieldInfo.GetValue(__instance) as ICollection;
                    List<WitchHut.SpellData> spellDataList = new List<WitchHut.SpellData>();
                    foreach (object spellDataObject in spellDataCollection) {
                        spellDataList.Add((WitchHut.SpellData)spellDataObject);
                    }
                    spellDataOriginalCount = spellDataList.Count;
                    foreach (SpellDataCustom currentSpellData in spellData) {
                        spellDataList.Add(currentSpellData);
                    }
                    WitchHut.SpellData[] spellDataArray = new WitchHut.SpellData[spellDataList.Count];
                    for (int spellIndex = 0; spellIndex < spellDataList.Count; spellIndex++) {
                        spellDataArray[spellIndex] = spellDataList[spellIndex];
                    }
                    fieldInfo.SetValue(__instance, spellDataArray);
                    fieldInfo = typeof(WitchHut).GetField("currSpellCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
                    fieldInfo.SetValue(__instance, new int[spellDataArray.Length]);
                }
            }


            // Upgrade spell list into a scroll
            // Add buttons to menu for custom spells
            
            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("Start")]
            public static class WitchUIStart {
                static void Postfix() {
                    additionalSpellButtons.Clear();
                    GameObject buttonOriginal = GameUI.inst.witchUI.spellList.transform.GetChild(0).gameObject;
                    for (int spellIndex = 0; spellIndex < spellData.Count; spellIndex++) {
                        GameObject buttonNew = GameObject.Instantiate(buttonOriginal, buttonOriginal.transform.parent);
                        buttonNew.transform.localScale = buttonOriginal.transform.localScale;
                        buttonNew.transform.localPosition = buttonOriginal.transform.localPosition;
                        Button button = buttonNew.GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        int lambdaSpellIndex = spellIndex;
                        button.onClick.AddListener(() => {
                            System.Reflection.FieldInfo fieldInfo = typeof(WitchUI).GetField("witch", BindingFlags.NonPublic | BindingFlags.Instance);
                            WitchHut witchHut = (WitchHut)fieldInfo.GetValue(GameUI.inst.witchUI);
                            if (TryActivate(witchHut, spellDataOriginalCount + lambdaSpellIndex)) {
                                StreamerEffectCustom currentSpell = Activator.CreateInstance(spellData[lambdaSpellIndex].spellImpl) as StreamerEffectCustom;
                                currentSpell.witchUI = GameUI.inst.witchUI;
                                currentSpell.witchHut = witchHut;
                                currentSpell.spellData = spellData[lambdaSpellIndex];
                                currentSpell.spellIndex = spellDataOriginalCount + lambdaSpellIndex;
                                currentSpell.Activate();
                            }
                        });
                        additionalSpellButtons.Add(buttonNew);
                    }
                }
            }

            // Check if spell can be activated without activating it
            static public bool TryActivate(WitchHut witchHut, int spellIndex, int activations = 1) {
                int landMassIdx1 = World.inst.GetCellData(witchHut.transform.position).landMassIdx;
                if (World.GetLandmassOwner(landMassIdx1).Gold < witchHut.GetSpellCost(spellIndex) * activations) {
                } else if (witchHut.GetSpellCooldown(spellIndex) > 0 || (witchHut.GetSpellData(spellIndex).cooldown > 0 && activations > 1)) {
                } else {
                    return true;
                }
                return false;
            }

            // Only make spell buttons visible when a certain friendship is reached
            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("RefreshAvailableSpells")]
            public static class WitchUIRefreshAvailableSpells {
                static void Postfix(WitchUI __instance) {
                    bool anySpellsAvailable = false;
                    System.Reflection.FieldInfo fieldInfo = typeof(WitchUI).GetField("witch", BindingFlags.NonPublic | BindingFlags.Instance);
                    WitchHut witch = (WitchHut)fieldInfo.GetValue(__instance);
                    fieldInfo = typeof(WitchHut).GetField("spellData", BindingFlags.NonPublic | BindingFlags.Instance);
                    ICollection spellDataCollection = (ICollection)fieldInfo.GetValue(witch);
                    int spellDataCount = 0;
                    foreach (object spellData in spellDataCollection) {
                        spellDataCount += 1;
                    }
                    if (spellDataCount == __instance.spellList.transform.childCount) {
                        for (int spellIndex = 0; spellIndex < spellDataCount; spellIndex++) {
                            SpellDataCustom spellDataCustom = witch.GetSpellData(spellIndex) as SpellDataCustom;
                            if (spellDataCustom != null) {
                                bool spellButtonsActive = witch.relationship >= spellDataCustom.relationship;
                                if (!spellButtonsActive) {
                                }
                                __instance.spellList.transform.GetChild(spellIndex).gameObject.SetActive(spellButtonsActive);
                                if (spellButtonsActive) {
                                    anySpellsAvailable = true;
                                }
                            }
                        }
                        if (anySpellsAvailable) {
                            __instance.spellContainer.gameObject.SetActive(true);
                        }
                    }
                }
            }

            // Prevent custom spell costs from scaling
            [HarmonyPatch(typeof(WitchHut))]
            [HarmonyPatch("GetSpellCost")]
            public static class WitchHutGetSpellCost {
                static void Postfix(WitchHut __instance, int i, ref int __result) {
                    SpellDataCustom spellDataCustom = __instance.GetSpellData(i) as SpellDataCustom;
                    if (spellDataCustom != null) {
                        if (spellDataCustom != null && spellDataCustom.scaleCost == false) {
                            __result = spellDataCustom.cost;
                        } else {
                            __result = Mathf.RoundToInt(__result / (1 + i / 10f));
                        }
                    }
                }
            }
        }
    }
}
