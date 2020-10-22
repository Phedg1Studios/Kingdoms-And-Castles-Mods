using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Phedg1Studios {
    namespace Shared {
        public class RegisterResearch : MonoBehaviour {
            static public string researchTranslation = "";
            static public UpgradeTypeEvent OnGetResearchTranslation = new UpgradeTypeEvent();
            static public List<GreatLibrary.ResearchDef> researchDefs = new List<GreatLibrary.ResearchDef>();
            static private bool updateUI = false;

            [System.Serializable]
            public class UpgradeTypeEvent : UnityEvent<Player.UpgradeType> { }

            [HarmonyPatch(typeof(ResearchUI))]
            [HarmonyPatch("AddOption")]
            public static class ResearchUIAddOption {
                static void Postfix() {
                    updateUI = true;
                }
            }

            // Add buttons to menu for custom research
            [HarmonyPatch(typeof(ResearchUI))]
            [HarmonyPatch("Init")]
            public static class ResearchUIInit {
                static void Postfix(ResearchUI __instance) {
                    if (updateUI) {
                        MethodInfo addOptionInfo = typeof(ResearchUI).GetMethod("AddOption", BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (GreatLibrary.ResearchDef researchDef in researchDefs) {
                            addOptionInfo.Invoke(__instance, new object[] { researchDef.upgrade });
                        }
                        updateUI = false;
                    }
                }
            }

            [HarmonyPatch(typeof(Building))]
            [HarmonyPatch("CompleteBuild")]
            public static class BuildingCompleteBuild {
                static void Postfix(Building __instance) {
                    GreatLibrary greatLibrary = __instance.GetComponent<GreatLibrary>();
                    if (greatLibrary != null) {
                        AddResearchDefs(greatLibrary);
                    }
;               }
            }

            [HarmonyPatch(typeof(GreatLibrary.GreatLibrarySaveData))]
            [HarmonyPatch("Unpack")]
            public static class GreatLibrarySaveDataUnpack {
                static void Postfix(GreatLibrary s) {
                    AddResearchDefs(s);
                }
            }

            static private void AddResearchDefs(GreatLibrary greatLibrary) {
                foreach (GreatLibrary.ResearchDef researchDef in researchDefs) {
                    greatLibrary.researchDefs.Add(researchDef);
                }
            }

            // Introduce custom translations
            [HarmonyPatch(typeof(ResearchUI))]
            [HarmonyPatch("LocalizationManagerOnOnLocalizeEvent")]
            public static class ResearchUILocalizationManagerOnOnLocalizeEvent {
                static void Postfix(ResearchUI __instance) {
                    FieldInfo optionMapInfo = typeof(ResearchUI).GetField("optionMap", BindingFlags.NonPublic | BindingFlags.Instance);
                    IDictionary optionMapDictionary = (IDictionary)optionMapInfo.GetValue(__instance);
                    foreach (object upgradeTypeObject in optionMapDictionary.Keys) {
                        Player.UpgradeType upgradeType = (Player.UpgradeType)upgradeTypeObject;
                        Transform optionTransform = (Transform)optionMapDictionary[upgradeTypeObject];
                        researchTranslation = optionTransform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text;
                        OnGetResearchTranslation.Invoke(upgradeType);
                        optionTransform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = researchTranslation;
                    }
                }
            }
        }
    }
}
