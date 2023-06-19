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
            static public KCModHelper helper;
            const string interopName = "Phedg1CustomSpellsAPI";
            static public List<SpellDataCustom> spellData = new List<SpellDataCustom>();
            static private Dictionary<int, int> customIndexSpellData = new Dictionary<int, int>();
            static private int spellDataOriginalCount = 0;

            static public System.Reflection.MethodInfo addInfo;
            static public System.Reflection.MethodInfo tryActivateInfo;

            static public SpellDataCustom GetSpellCustomIndex(int givenIndex) {
                return spellData[customIndexSpellData[givenIndex]];
            }

            static public SpellDataCustom GetSpellGlobalIndex(int givenIndex) {
                return GetSpellCustomIndex(givenIndex - spellDataOriginalCount);
            }

            static public void Setup(KCModHelper givenHelper) {
                helper = givenHelper;
                Assembly assembly;
                Porg.InteropClient.TryGetMod(interopName, out assembly);
                Type customSpellsAPI = assembly.GetType("Phedg1Studios.CustomSpellsAPI.CustomSpellsAPI");
                addInfo = customSpellsAPI.GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
                tryActivateInfo = customSpellsAPI.GetMethod("TryActivateWithProps", BindingFlags.Public | BindingFlags.Static);
            }

            static public void Add(SpellDataCustom givenSpellData) {
                spellData.Add(givenSpellData);
                int globalIndex = (int)addInfo.Invoke(null, new object[] { givenSpellData.GetProperties() });
                customIndexSpellData.Add(globalIndex, spellData.Count - 1);
            }

            static private void SetOriginalCount(int givenSpellDataCount) {
                spellDataOriginalCount = givenSpellDataCount;
            }

            static private void ButtonClicked(Button.ButtonClickedEvent onClick, WitchUI __instance, int globalSpellIndex) {
                onClick.AddListener(() => {
                    System.Reflection.FieldInfo fieldInfo = typeof(WitchUI).GetField("witch", BindingFlags.NonPublic | BindingFlags.Instance);
                    WitchHut witchHut = (WitchHut)fieldInfo.GetValue(__instance);
                    if (TryActivate(witchHut, globalSpellIndex)) {
                        StreamerEffectCustom currentSpell = Activator.CreateInstance(GetSpellGlobalIndex(globalSpellIndex).spellImpl) as StreamerEffectCustom;
                        currentSpell.witchUI = __instance;
                        currentSpell.witchHut = witchHut;
                        currentSpell.spellData = GetSpellGlobalIndex(globalSpellIndex);
                        currentSpell.spellIndex = globalSpellIndex;
                        currentSpell.Activate();
                    }
                });
            }

            static public bool TryActivate(WitchHut witchHut, int spellIndex, int activations = 1) {
                Dictionary<int, object> properties = new Dictionary<int, object>();
                properties.Add((int)SpellDataCustom.TryActivate.WitchHut, witchHut);
                properties.Add((int)SpellDataCustom.TryActivate.SpellIndex, spellIndex);
                properties.Add((int)SpellDataCustom.TryActivate.Activations, activations);
                object result = tryActivateInfo.Invoke(null, new object[] { properties });
                return (bool)Convert.ChangeType(result, typeof(bool));
            }

            // NEED TO EVALUATE THIS TWITCH STUFF STILL


            static private bool addedToTwitchList = false;

            static public void AddToTwitchList() {
                if (!addedToTwitchList) {
                    addedToTwitchList = true;
                    Color previousColour = new Color();
                    List<object> uiVotableLayoutList = new List<object>();
                    for (int votableIndex = 0; votableIndex < TwitchBonus.UIVotableLayout.Length; votableIndex++) {
                        if (votableIndex != 0 && (votableIndex == TwitchBonus.UIVotableLayout.Length - 1 || TwitchBonus.UIVotableLayout[votableIndex] is VoteListHeading)) {
                            foreach (SpellDataCustom spellDataCustom in spellData) {
                                if (spellDataCustom.twitchVotable && spellDataCustom.color == previousColour) {
                                    uiVotableLayoutList.Add(new Votable() {
                                        Implementation = spellDataCustom.spellImpl,
                                        color = spellDataCustom.color,
                                    });
                                }
                            }
                        }
                        if (TwitchBonus.UIVotableLayout[votableIndex] is Votable votable) {
                            previousColour = votable.color;
                            uiVotableLayoutList.Add(votable);
                        }
                    }
                }
            }
        }
    }
}
