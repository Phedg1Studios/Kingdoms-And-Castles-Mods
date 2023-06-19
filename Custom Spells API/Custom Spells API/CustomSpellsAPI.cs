using Harmony;
using System;
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
            const string interopName = "Phedg1CustomSpellsAPI";
            static public KCModHelper helper;

            static public List<SpellDataCustom> spellData = new List<SpellDataCustom>();
            static public int spellDataOriginalCount = 0;

            static public int Add(Dictionary<int, object> givenProperties) {
                spellData.Add(SpellDataCustom.GetSpellDataCustom(givenProperties));
                return spellData.Count - 1;
            }

            // Initialize systems
            void Preload(KCModHelper givenHelper) {
                Porg.InteropClient.Register(interopName);
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                helper = givenHelper;
                Shared.Util.Setup(helper, this.gameObject);
            }

            public void PreScriptLoad(KCModHelper helper) {
                Shared.RegisterSpells.Setup(helper);
            }

            // Modify loaded save data to be compatible with new custom spells
            [HarmonyPatch(typeof(WitchHut.WitchHutSaveData))]
            [HarmonyPatch("Unpack")]
            public static class WitchHutSaveDatatUnpack {
                static void Postfix(WitchHut.WitchHutSaveData __instance, ref WitchHut __result, WitchHut obj) {
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

            // Prevent custom spell costs from scaling
            [HarmonyPatch(typeof(WitchHut))]
            [HarmonyPatch("GetSpellCost")]
            public static class WitchHutGetSpellCost {
                static void Postfix(WitchHut __instance, int i, ref int __result) {
                    Shared.SpellDataCustom spellDataCustom = __instance.GetSpellData(i) as SpellDataCustom;
                    if (spellDataCustom != null) {
                        if (spellDataCustom != null && spellDataCustom.scaleCost == false) {
                            __result = spellDataCustom.cost;
                        } else {
                            __result = Mathf.RoundToInt(__result / (1 + i / 10f));
                        }
                    }
                }
            }

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
                        Assembly assembly;
                        bool resultA = Porg.InteropClient.TryGetMod(currentSpellData.interopName, out assembly);
                        Type registerSpells = assembly.GetType("Phedg1Studios.Shared.RegisterSpells");
                        MethodInfo setOriginalCountInfo = registerSpells.GetMethod("SetOriginalCount", BindingFlags.NonPublic | BindingFlags.Static);
                        setOriginalCountInfo.Invoke(null, new object[] { spellDataOriginalCount });
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

            static public bool TryActivateWithProps(Dictionary<int, object> properties) {
                WitchHut witchHut = null;
                int spellIndex = -1;
                int activations = -1;
                if (properties.ContainsKey((int)SpellDataCustom.TryActivate.WitchHut)) {
                    witchHut = (WitchHut)Convert.ChangeType(properties[(int)SpellDataCustom.TryActivate.WitchHut], typeof(WitchHut));
                }
                if (properties.ContainsKey((int)SpellDataCustom.TryActivate.SpellIndex)) {
                    spellIndex = (int)Convert.ChangeType(properties[(int)SpellDataCustom.TryActivate.SpellIndex], typeof(int));
                }
                if (properties.ContainsKey((int)SpellDataCustom.TryActivate.Activations)) {
                    activations = (int)Convert.ChangeType(properties[(int)SpellDataCustom.TryActivate.Activations], typeof(int));
                }
                return TryActivate(witchHut, spellIndex, activations);
            }

            static public void InvokeInteropMethod(string interopName, string classFullName, string methodName,   object[] properties) {
                Assembly assembly;
                bool resultA = Porg.InteropClient.TryGetMod(interopName, out assembly);
                Type registerSpells = assembly.GetType("Phedg1Studios.Shared.RegisterSpells");
                MethodInfo buttonClickedInfo = registerSpells.GetMethod("ButtonClicked", BindingFlags.NonPublic | BindingFlags.Static);
                buttonClickedInfo.Invoke(null, properties);
            }
        }
    }
}
