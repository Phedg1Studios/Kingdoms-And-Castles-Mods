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
using Zat.Shared.ModMenu.API;
using Zat.Shared.ModMenu.Interactive;

namespace Phedg1Studios {
    namespace RotateBuildings {
        public class RotateBuildings : MonoBehaviour {
            static private string modName = "Phedg1Studios.RotateBuildings";
            static private char splitChar = ',';
            static public KCModHelper helper;
            private UnityAction<Exception> modMenuException;
            private InteractiveConfiguration<ModMenuSettings> config;
            private KeyCode secondaryKeycode = KeyCode.LeftShift;
            private KeyCode resetKeycode = KeyCode.Semicolon;
            private KeyCode anticlockwiseKeycode = KeyCode.K;
            private KeyCode clockwiseKeycode = KeyCode.L;

            private bool secondaryKeyDown = false;
            private bool clockwiseKeyDown = false;
            private bool anticlockwiseKeyDown = false;


            void Preload(KCModHelper helper) {
                RotateBuildings.helper = helper;
                Application.logMessageReceived += OnLogMessageReceived;
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            void Update() {
                if (Input.GetKeyDown(secondaryKeycode)) {
                    secondaryKeyDown = true;
                }
                if (Input.GetKeyUp(secondaryKeycode)) {
                    secondaryKeyDown = false;
                }
                if (Input.GetKeyDown(clockwiseKeycode)) {
                    clockwiseKeyDown = true;
                }
                if (Input.GetKeyUp(clockwiseKeycode)) {
                    clockwiseKeyDown = false;
                }
                if (Input.GetKeyDown(anticlockwiseKeycode)) {
                    anticlockwiseKeyDown = true;
                }
                if (Input.GetKeyUp(anticlockwiseKeycode)) {
                    anticlockwiseKeyDown = false;
                }
                bool resetKeyDown = false;
                if (Input.GetKeyUp(resetKeycode)) {
                    resetKeyDown = true;
                }
                if (Input.GetKeyDown(KeyCode.T)) {
                    Building selectedBuilding = GameUI.inst.GetBuildingSelected();
                    if (selectedBuilding != null) {
                        selectedBuilding.transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, 0, 0);
                    }
                }
                if ((secondaryKeyDown || secondaryKeycode == KeyCode.F1) && (clockwiseKeyDown || anticlockwiseKeyDown || resetKeyDown)) {
                    Building selectedBuilding = GameUI.inst.GetBuildingSelected();
                    if (selectedBuilding != null) {
                        Cell cell = selectedBuilding.GetCell();
                        if (cell != null) {
                            foreach (Building occupyingStructure in cell.OccupyingStructure) {
                                if (occupyingStructure.transform.GetChild(0).childCount > 0) {
                                    if (resetKeyDown) {
                                        occupyingStructure.transform.GetChild(0).GetChild(0).localEulerAngles = new Vector3(0, 0, 0);
                                    } else if (clockwiseKeyDown) {
                                        occupyingStructure.transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, selectedBuilding.transform.GetChild(0).GetChild(0).eulerAngles.y + 1, 0);
                                    } else if (anticlockwiseKeyDown) {
                                        occupyingStructure.transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, selectedBuilding.transform.GetChild(0).GetChild(0).eulerAngles.y - 1, 0);
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


            [Mod("Rotate Buildings", "v1.0", "Phedg1 Studios")]
            public class ModMenuSettings {
                [Setting("Secondary Key", "Press this and one of the keys below to change building rotation.")]
                [Hotkey(KeyCode.LeftShift)]
                public InteractiveHotkeySetting SecondaryKey { get; private set; }

                [Setting("Reset Key", "Press this and the secondary key to reset a building's rotation.")]
                [Hotkey(KeyCode.Semicolon)]
                public InteractiveHotkeySetting ResetKey { get; private set; }

                [Setting("Clockwise Key", "Press this and the secondary key to rotate a building clockwise.")]
                [Hotkey(KeyCode.L)]
                public InteractiveHotkeySetting ClockwiseKey { get; private set; }

                [Setting("Anticlockwise Key", "Press this and the secondary key to rotate a building clockwise.")]
                [Hotkey(KeyCode.K)]
                public InteractiveHotkeySetting AnticlockwiseKey { get; private set; }
            }

            void SettingsRegistered(ModSettingsProxy proxy, SettingsEntry[] saved) {
                config.Install(proxy, saved);
                config.Settings.SecondaryKey.OnUpdate.AddListener((setting) => {
                    secondaryKeycode = config.Settings.SecondaryKey.Key;
                });
                config.Settings.ResetKey.OnUpdate.AddListener((setting) => {
                    resetKeycode = config.Settings.ResetKey.Key;
                });
                config.Settings.ClockwiseKey.OnUpdate.AddListener((setting) => {
                    clockwiseKeycode = config.Settings.ClockwiseKey.Key;
                });
                config.Settings.AnticlockwiseKey.OnUpdate.AddListener((setting) => {
                    anticlockwiseKeycode = config.Settings.AnticlockwiseKey.Key;
                });
                config.Settings.SecondaryKey.TriggerUpdate();
                config.Settings.ResetKey.TriggerUpdate();
                config.Settings.ClockwiseKey.TriggerUpdate();
                config.Settings.AnticlockwiseKey.TriggerUpdate();
            }

            void ModMenuException(Exception exception) {
                Log(exception);
                Log(exception.Message);
                Log(exception.StackTrace);
            }








            [HarmonyPatch(typeof(Building))]
            [HarmonyPatch("Awake")]
            public static class BuildingAwake {
                static void Postfix(Building __instance) {
                    if (__instance.transform.childCount > 0) {
                        Transform oldChild = __instance.transform.GetChild(0);
                        GameObject newGameObject = new GameObject();
                        newGameObject.transform.SetParent(__instance.transform);
                        newGameObject.transform.localPosition = oldChild.localPosition;
                        newGameObject.transform.localScale = oldChild.localScale;
                        newGameObject.transform.localRotation = oldChild.localRotation;
                        oldChild.transform.SetParent(newGameObject.transform);
                        oldChild.transform.localScale = Vector3.one;
                        Transform resourceDropoff = oldChild.Find("resourceDropoff");
                        if (resourceDropoff != null) {
                            resourceDropoff.SetParent(newGameObject.transform);
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(Building))]
            [HarmonyPatch("Init")]
            public static class GameUIUpdate {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    int startIndex = -1;
                    for (int codeIndex = 0; codeIndex < codes.Count - 4; codeIndex++) {
                        if (codes[codeIndex].opcode == OpCodes.Ldarg_0 && NullSafeToString(codes[codeIndex].operand) == "null") {
                            if (codes[codeIndex + 1].opcode == OpCodes.Call && NullSafeToString(codes[codeIndex + 1].operand) == "UnityEngine.Transform get_transform()") {
                                if (codes[codeIndex + 2].opcode == OpCodes.Ldc_I4_0 && NullSafeToString(codes[codeIndex + 2].operand) == "null") {
                                    if (codes[codeIndex + 3].opcode == OpCodes.Callvirt && NullSafeToString(codes[codeIndex + 3].operand) == "UnityEngine.Transform GetChild(Int32)") {
                                        if (codes[codeIndex + 4].opcode == OpCodes.Stloc_S && NullSafeToString(codes[codeIndex + 4].operand) == "UnityEngine.Transform (7)") {
                                            startIndex = codeIndex + 5;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        //helper.Log(codes[codeIndex].opcode.ToString() + " " + NullSafeToString(codes[codeIndex].operand));
                    }
                    if (startIndex != -1) {
                        MethodInfo GetNestedChild = typeof(RotateBuildings).GetMethod("GetNestedChild", BindingFlags.NonPublic | BindingFlags.Static);
                        codes.Insert(startIndex, new CodeInstruction(OpCodes.Ldloc_S, 7));
                        codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Callvirt, GetNestedChild));
                        codes.Insert(startIndex + 2, new CodeInstruction(OpCodes.Stloc_S, 7));
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

            static private Transform GetNestedChild(Transform child) {
                if (child.childCount > 0) {
                    child = child.GetChild(0);
                }
                return child;
            }

            [HarmonyPatch(typeof(World))]
            [HarmonyPatch("Place")]
            public static class WorldPlace {
                static void Postfix(Building PendingObj) {
                    if (PendingObj.transform.GetChild(0).childCount > 0) {
                        Cell cell = PendingObj.GetCell();
                        if (cell != null) {
                            if (cell.OccupyingStructure.Count > 1) {
                                Building existingBuilding = cell.OccupyingStructure[0];
                                if (existingBuilding.transform.GetChild(0).childCount > 0) {
                                    PendingObj.transform.GetChild(0).GetChild(0).rotation = existingBuilding.transform.GetChild(0).GetChild(0).rotation;
                                }
                            }
                        }
                    }
                }
            }





            [HarmonyPatch(typeof(Building.BuildingSaveData))]
            [HarmonyPatch("Pack")]
            public static class BuildingSaveDataPack {
                static void Postfix(Building b) {
                    Quaternion buildingRotation = Quaternion.identity;
                    if (b.transform.GetChild(0).childCount > 0) {
                        buildingRotation = b.transform.GetChild(0).GetChild(0).rotation;
                    }
                    SerializableQuaternion serializableQuaternion = (SerializableQuaternion)buildingRotation;
                    string rotationString = serializableQuaternion.x.ToString() + splitChar + serializableQuaternion.y.ToString() + splitChar + serializableQuaternion.z.ToString() + splitChar + serializableQuaternion.w.ToString();
                    LoadSave.SaveDataBuilding(modName, b, rotationString);
                }
            }
            
            [HarmonyPatch(typeof(LoadSaveContainer))]
            [HarmonyPatch("Unpack")]
            public static class LoadSaveContainerUnpack {
                static void Postfix() {
                    World.inst.ForEachTile(0, 0, World.inst.GridWidth, World.inst.GridHeight, (World.EvaluateCell)((x, z, cell) => {
                        foreach (Building building in cell.OccupyingStructure) {
                            if (building.transform.GetChild(0).childCount > 0) {
                                string rotationString = LoadSave.ReadDataBuilding(modName, building);
                                if (rotationString != null) {
                                    string[] rotationStringSplit = rotationString.Split(splitChar);
                                    if (rotationStringSplit.Length == 4) {
                                        List<float> rotationList = new List<float>();
                                        foreach (string rotation in rotationStringSplit) {
                                            float parsedFloat = 0;
                                            if (float.TryParse(rotation, out parsedFloat)) {
                                                rotationList.Add(parsedFloat);
                                            }
                                        }
                                        if (rotationList.Count == 4) {
                                            SerializableQuaternion serializableQuaternion = new SerializableQuaternion(rotationList[0], rotationList[1], rotationList[2], rotationList[3]);
                                            building.transform.GetChild(0).GetChild(0).rotation = (Quaternion)serializableQuaternion;
                                        }
                                    }
                                }
                            }
                        }
                    }));
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
