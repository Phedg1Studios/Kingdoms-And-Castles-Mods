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
                //Zat.Shared.InterModComm.IMCPort imcPort = new Zat.Shared.InterModComm.IMCPort();
                //imcPort = gameObject.AddComponent<Zat.Shared.InterModComm.IMCPort>();
                //imcPort.RegisterReceiveListener<SpellDataCustom>("Phedg1Studios.CustomSpellsAPI.AddSpellData", AddSpellData);
            }

            /*
            void AddSpellData(Zat.Shared.InterModComm.IRequestHandler hander, string source, SpellDataCustom entry) {
                if (entry != null) {
                    spellData.Add(entry);
                    hander.SendResponse("server", "good", "Phedg1Studios.CustomSpellsAPI.SpellAdded");
                }
            }
            */

            // Initialize variables
            void SceneLoaded(KCModHelper helper) {
                CustomSpellsAPI.helper = helper;
                Zat.Shared.Debugging.Helper = helper;
                //spellData.Clear();
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

            /*
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
                    foreach (WitchHut.SpellData currentSpellData in spellData) {
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
            */

            /*
            // Upgrade spell list into a scroll
            // Add buttons to menu for custom spells
            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("Start")]
            public static class WitchUIStart {
                [HarmonyPriority(200)]
                static void Postfix(WitchUI __instance) {
                    SpellUIUpdater.UpdateUI(__instance.spellList);
                    additionalSpellButtons.Clear();
                    GameObject buttonOriginal = __instance.spellList.transform.GetChild(0).gameObject;
                    for (int spellIndex = 0; spellIndex < spellData.Count; spellIndex++) {
                        GameObject buttonNew = GameObject.Instantiate(buttonOriginal, buttonOriginal.transform.parent);
                        buttonNew.transform.localScale = buttonOriginal.transform.localScale;
                        buttonNew.transform.localPosition = buttonOriginal.transform.localPosition;
                        Button button = buttonNew.GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        int lambdaSpellIndex = spellIndex;
                        button.onClick.AddListener(() => {
                            System.Reflection.FieldInfo fieldInfo = typeof(WitchUI).GetField("witch", BindingFlags.NonPublic | BindingFlags.Instance);
                            WitchHut witchHut = (WitchHut)fieldInfo.GetValue(__instance);
                            if (TryActivate(witchHut, spellDataOriginalCount + lambdaSpellIndex)) {
                                StreamerEffectCustom currentSpell = (StreamerEffectCustom)Activator.CreateInstance(spellData[lambdaSpellIndex].spellImpl);
                                currentSpell.witchUI = __instance;
                                currentSpell.witchHut = witchHut;
                                currentSpell.spellData = spellData[lambdaSpellIndex];
                                currentSpell.spellIndex = spellDataOriginalCount + lambdaSpellIndex;
                                currentSpell.Activate();
                            }
                        });
                        additionalSpellButtons.Add(buttonNew);
                    }
                    SpellUIUpdater.UpdateContentSize(__instance.spellList);
                }
            }
            */

            // Upgrade spell list into a scroll
            // Add buttons to menu for custom spells
            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("Start")]
            public static class WitchUIStart {
                [HarmonyPriority(200)]
                static void Postfix(WitchUI __instance) {
                    SpellUIUpdater.UpdateUI(__instance.spellList);
                    SpellUIUpdater.UpdateContentSize(__instance.spellList);
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
