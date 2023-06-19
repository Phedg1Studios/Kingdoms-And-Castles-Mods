using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zat.Shared.ModMenu.Interactive;
using Zat.Shared.ModMenu.API;

namespace Phedg1Studios {
    namespace ShipsTravelAnnually {
        public class ShipsTravelAnnually : MonoBehaviour {
            static public KCModHelper helper;
            static private string modName = "Phedg1Studios.ShipsTravelAnnually";
            static private FieldInfo delayTimerInfo = typeof(Ship).GetField("delayTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            static private System.Reflection.MethodInfo checkShipStartYearInfo = typeof(ShipsTravelAnnually).GetMethod("CheckShipStartYear", BindingFlags.Public | BindingFlags.Static);
            private UnityAction<Exception> modMenuException;
            private InteractiveConfiguration<ModMenuSettings> config;
            static private int yearsPerTrip = 1;
            static private Dictionary<Guid, int> startYears = new Dictionary<Guid, int>();


            void Preload(KCModHelper helper) {
                ShipsTravelAnnually.helper = helper;
                Application.logMessageReceived += OnLogMessageReceived;
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            [HarmonyPatch(typeof(ShipBase))]
            [HarmonyPatch("Awake")]
            public static class ShipBaseAwake {
                static void Postfix(ShipBase __instance) {
                    if (__instance.type == ShipBase.ShipType.Transport) {
                        startYears.Add(__instance.guid, 0);
                    }
                }
            }

            [HarmonyPatch(typeof(Ship))]
            [HarmonyPatch("VisitNextOrder")]
            public static class ShipVisitNextOrder {
                static void Postfix(Ship __instance) {
                    if (__instance.orderIdx == 0) {
                        startYears[__instance.shipBase.guid] = Player.inst.CurrYear;
                    }
                }
            }

            static private List<Code> shipTickCode = new List<Code>() {
                new Code(OpCodes.Ldarg_0, "null"),
                new Code(OpCodes.Ldfld, "System.Collections.Generic.List`1[Ship+PortOrder] orders"),
                new Code(OpCodes.Callvirt, "Int32 get_Count()"),
                new Code(OpCodes.Ldc_I4_0, "null"),
                new Code(OpCodes.Ble, "System.Reflection.Emit.Label"),
                new Code(OpCodes.Ldarg_0, "null"),
                new Code(OpCodes.Ldfld, "ShipBase shipBase"),
                new Code(OpCodes.Ldfld, "System.Boolean arrived"),
            };

            [HarmonyPatch(typeof(Ship))]
            [HarmonyPatch("Tick")]
            public static class ShipTick {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    int startIndex = -1;
                    for (int codeIndex = 0; codeIndex < codes.Count - shipTickCode.Count + 1; codeIndex++) {
                        for (int keyIndex = 0; keyIndex < shipTickCode.Count; keyIndex++) {
                            if (codes[codeIndex + keyIndex].opcode == shipTickCode[keyIndex].opCode && NullSafeToString(codes[codeIndex + keyIndex].operand) == shipTickCode[keyIndex].operand) {
                                if (keyIndex == shipTickCode.Count - 1) {
                                    startIndex = codeIndex + keyIndex + 1;
                                    break;
                                }
                            } else {
                                if (keyIndex > 1) {
                                    Log(keyIndex);
                                }
                                break;
                            }
                        }
                        //Log(codes[codeIndex].opcode.ToString() + " " + NullSafeToString(codes[codeIndex].operand));
                    }
                    if (startIndex != -1) {
                        codes.Insert(startIndex + 0, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Ldarg_1));
                        codes.Insert(startIndex + 2, new CodeInstruction(OpCodes.Callvirt, checkShipStartYearInfo));
                        codes.Insert(startIndex + 3, new CodeInstruction(OpCodes.And));
                    }
                    return codes.AsEnumerable();
                }
            }

            class Code {
                public OpCode opCode;
                public string operand;

                public Code(OpCode givenOpCode, string givenOperand) {
                    opCode = givenOpCode;
                    operand = givenOperand;
                }
            }

            static public string NullSafeToString(object givenObject) {
                if (givenObject != null) {
                    return givenObject.ToString();
                } else {
                    return "null";
                }
            }

            static public bool CheckShipStartYear(Ship ship, float dt) {
                bool shouldContinue = ship.orderIdx != 0 || Player.inst.CurrYear > startYears[ship.shipBase.guid] - 1 + yearsPerTrip;
                if (ship.orders.Count > 0 && ship.shipBase.arrived && !shouldContinue) {
                    float delayTimerOld = (float)delayTimerInfo.GetValue(ship);
                    delayTimerInfo.SetValue(ship, delayTimerOld - dt);
                }
                return shouldContinue;
            }








            [HarmonyPatch(typeof(ShipSystem.ShipSystemSaveData))]
            [HarmonyPatch("Pack")]
            public static class ShipSystemSaveDataPack {
                static void Postfix(ShipSystem obj) {
                    foreach (ShipBase shipBase in obj.ships.data) {
                        if (shipBase != null) {
                            if (shipBase.type == ShipBase.ShipType.Transport) {
                                LoadSave.SaveDataGeneric(modName, shipBase.guid.ToString(), startYears[shipBase.guid].ToString());
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(LoadSaveContainer))]
            [HarmonyPatch("Unpack")]
            public static class LoadSaveContainerUnpack {
                static void Postfix() {
                    startYears.Clear();
                    foreach (ShipBase shipBase in ShipSystem.inst.ships.data) {
                        if (shipBase != null) {
                            if (shipBase.type == ShipBase.ShipType.Transport) {
                                startYears.Add(shipBase.guid, 0);
                                string startYearString = LoadSave.ReadDataGeneric(modName, shipBase.guid.ToString());
                                if (startYearString != null) {
                                    int startYear = 0;
                                    if (int.TryParse(startYearString, out startYear)) {
                                        startYears[shipBase.guid] = startYear;
                                    }
                                }
                            }
                        }
                    }
                }
            }








            void Start() {
                modMenuException += ModMenuException;
                config = new InteractiveConfiguration<ModMenuSettings>();
                ModSettingsBootstrapper.Register(config.ModConfig, SettingsRegistered, modMenuException);
            }

            [Mod("Ships Travel Annually", "v1.0", "Phedg1 Studios")]
            public class ModMenuSettings {
                [Setting("Travel Interval", "How many years after the start of the previous journey to start the next")]
                [Slider(0, 10, 1, "1", true)]
                public InteractiveSliderSetting YearsPerTrip { get; private set; }
            }

            void SettingsRegistered(ModSettingsProxy proxy, SettingsEntry[] saved) {
                config.Install(proxy, saved);
                config.Settings.YearsPerTrip.OnUpdate.AddListener((setting) => {
                    int value = Mathf.RoundToInt(setting.slider.value);
                    yearsPerTrip = value;
                    config.Settings.YearsPerTrip.Label = value.ToString();
                });
                config.Settings.YearsPerTrip.TriggerUpdate();
            }

            void ModMenuException(Exception exception) {
                Log(exception);
                Log(exception.Message);
                Log(exception.StackTrace);
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
