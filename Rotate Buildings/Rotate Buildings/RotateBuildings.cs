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
            static private char propertyChar = ':';
            static public KCModHelper helper;
            private UnityAction<Exception> modMenuException;
            private InteractiveConfiguration<ModMenuSettings> config;
            private KeyCode secondaryKeycode = KeyCode.LeftShift;
            private KeyCode resetKeycode = KeyCode.Semicolon;
            private KeyCode anticlockwiseKeycode = KeyCode.K;
            private KeyCode clockwiseKeycode = KeyCode.L;
            private KeyCode scaleUpKeycode = KeyCode.Period;
            private KeyCode scaleDownKeycode = KeyCode.Comma;
            private KeyCode positionXUpKeycode = KeyCode.RightArrow;
            private KeyCode positionXDownKeycode = KeyCode.LeftArrow;
            private KeyCode positionZUpKeycode = KeyCode.UpArrow;
            private KeyCode positionZDownKeycode = KeyCode.DownArrow;
            static private FieldInfo gravesRenderInstanceInfo = typeof(Cemetery).GetField("gravesRenderInstance", BindingFlags.NonPublic | BindingFlags.Instance);

            private bool secondaryKeyDown = false;
            private bool clockwiseKeyDown = false;
            private bool anticlockwiseKeyDown = false;
            private bool scaleUpDown = false;
            private bool scaleDownDown = false;
            private bool positionXUpDown = false;
            private bool positionXDownDown = false;
            private bool positionZUpDown = false;
            private bool positionZDownDown = false;

            private float rotationStep = 0.5f;
            private float scaleStep = 0.005f;
            private float positionStep = 0.025f;

            static private FieldInfo aqueductDoRandom180 = typeof(Aqueduct).GetField("doRandom180", BindingFlags.NonPublic | BindingFlags.Instance);
            static private FieldInfo roadDoRandom180 = typeof(Road).GetField("doRandom180", BindingFlags.NonPublic | BindingFlags.Instance);


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
                if (Input.GetKeyDown(scaleUpKeycode)) {
                    scaleUpDown = true;
                }
                if (Input.GetKeyUp(scaleUpKeycode)) {
                    scaleUpDown = false;
                }
                if (Input.GetKeyDown(scaleDownKeycode)) {
                    scaleDownDown = true;
                }
                if (Input.GetKeyUp(scaleDownKeycode)) {
                    scaleDownDown = false;
                }
                if (Input.GetKeyDown(positionXUpKeycode)) {
                    positionXUpDown = true;
                }
                if (Input.GetKeyUp(positionXUpKeycode)) {
                    positionXUpDown = false;
                }

                if (Input.GetKeyDown(positionXDownKeycode)) {
                    positionXDownDown = true;
                }
                if (Input.GetKeyUp(positionXDownKeycode)) {
                    positionXDownDown = false;
                }

                if (Input.GetKeyDown(positionZUpKeycode)) {
                    positionZUpDown = true;
                }
                if (Input.GetKeyUp(positionZUpKeycode)) {
                    positionZUpDown = false;
                }
                if (Input.GetKeyDown(positionZDownKeycode)) {
                    positionZDownDown = true;
                }
                if (Input.GetKeyUp(positionZDownKeycode)) {
                    positionZDownDown = false;
                }



                bool resetKeyDown = false;
                if (Input.GetKeyUp(resetKeycode)) {
                    resetKeyDown = true;
                }

                if ((secondaryKeyDown || secondaryKeycode == KeyCode.F1) && (clockwiseKeyDown || anticlockwiseKeyDown || resetKeyDown || scaleUpDown || scaleDownDown || positionXUpDown || positionXDownDown || positionZUpDown || positionZDownDown)) {
                    Building selectedBuilding = GameUI.inst.GetBuildingSelected();
                    if (selectedBuilding != null) {
                        Cell cell = selectedBuilding.GetCell();
                        if (cell != null) {
                            foreach (Building occupyingStructure in cell.OccupyingStructure) {
                                if (occupyingStructure.transform.GetChild(0).childCount > 0) {
                                    if (resetKeyDown) {
                                        occupyingStructure.transform.GetChild(0).GetChild(0).localEulerAngles = Vector3.zero;
                                        occupyingStructure.transform.GetChild(0).GetChild(0).localScale = Vector3.one;
                                        occupyingStructure.transform.GetChild(0).GetChild(0).localPosition = Vector3.zero;
                                    } else if (clockwiseKeyDown) {
                                        occupyingStructure.transform.GetChild(0).GetChild(0).localEulerAngles += new Vector3(0, rotationStep, 0);
                                    } else if (anticlockwiseKeyDown) {
                                        occupyingStructure.transform.GetChild(0).GetChild(0).localEulerAngles += new Vector3(0, -rotationStep, 0);
                                    } else if (scaleUpDown) {
                                        occupyingStructure.transform.GetChild(0).GetChild(0).localScale = new Vector3(occupyingStructure.transform.GetChild(0).GetChild(0).localScale.x + scaleStep, occupyingStructure.transform.GetChild(0).GetChild(0).localScale.y + scaleStep, occupyingStructure.transform.GetChild(0).GetChild(0).localScale.z + scaleStep);
                                        occupyingStructure.transform.GetChild(0).GetChild(0).position = new Vector3(occupyingStructure.transform.GetChild(0).GetChild(0).position.x, occupyingStructure.transform.GetChild(0).position.y * occupyingStructure.transform.GetChild(0).GetChild(0).localScale.y, occupyingStructure.transform.GetChild(0).GetChild(0).position.z);
                                    } else if (scaleDownDown) {
                                        occupyingStructure.transform.GetChild(0).GetChild(0).localScale = new Vector3(Mathf.Max(scaleStep, occupyingStructure.transform.GetChild(0).GetChild(0).localScale.x - scaleStep), Mathf.Max(scaleStep, occupyingStructure.transform.GetChild(0).GetChild(0).localScale.y - scaleStep), Mathf.Max(scaleStep, occupyingStructure.transform.GetChild(0).GetChild(0).localScale.z - scaleStep));
                                        occupyingStructure.transform.GetChild(0).GetChild(0).position = new Vector3(occupyingStructure.transform.GetChild(0).GetChild(0).position.x, occupyingStructure.transform.GetChild(0).position.y * occupyingStructure.transform.GetChild(0).GetChild(0).localScale.y, occupyingStructure.transform.GetChild(0).GetChild(0).position.z);
                                    } else {
                                        if (positionXUpDown) {
                                            occupyingStructure.transform.GetChild(0).GetChild(0).position += positionStep * GetRotatedPosition(1, 0);
                                        } else if (positionXDownDown) {
                                            occupyingStructure.transform.GetChild(0).GetChild(0).position += positionStep * GetRotatedPosition(-1, 0);
                                        }
                                        if (positionZUpDown) {
                                            occupyingStructure.transform.GetChild(0).GetChild(0).position += positionStep * GetRotatedPosition(0, 1);
                                        } else if (positionZDownDown) {
                                            occupyingStructure.transform.GetChild(0).GetChild(0).position += positionStep * GetRotatedPosition(0, -1);
                                        }
                                    }
                                }
                            }
                            UpdateGraveRotations(cell);
                        }
                    }
                }
            }

            static private Vector3 GetRotatedPosition(int x, int z) {
                List<Vector3> rotatedVectors = new List<Vector3>() {
                    new Vector3(1, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(-1, 0, 0),
                    new Vector3(0, 0, -1),
                };
                int startIndex = 0;
                if (x == 1) {
                    startIndex = 0;
                } else if (z == 1) {
                    startIndex = 1;
                } else if (x == -1) {
                    startIndex = 2;
                } else if (z == -1) {
                    startIndex = 3;
                }
                int offset = Mathf.RoundToInt(Cam.inst.cam.transform.eulerAngles.y / 90);
                return rotatedVectors[((startIndex - offset) % 4 + 4) % 4];
            }

            static private void UpdateGraveRotations(Cell cell) {
                if (cell.OccupyingStructure.Count > 0) {
                    if (cell.OccupyingStructure[0].categoryHash == World.cemeteryHash) {
                        Cemetery cemetery = cell.OccupyingStructure[0].GetComponent<Cemetery>();
                        ICollection gravesRenderInstanceColection = (ICollection)gravesRenderInstanceInfo.GetValue(cemetery);
                        int graveIndex = 0;
                        foreach (object renderInstanceObject in gravesRenderInstanceColection) {
                            ((RenderInstance)renderInstanceObject).UpdateTRS(cemetery.graveOffsets[graveIndex].position, cemetery.graveOffsets[graveIndex].rotation, cemetery.transform.GetChild(0).GetChild(0).localScale);
                            graveIndex += 1;
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

                [Setting("Scale Up Key", "Press this and the secondary key to make the building larger.")]
                [Hotkey(KeyCode.Period)]
                public InteractiveHotkeySetting ScaleUpKey { get; private set; }

                [Setting("Scale Down Key", "Press this and the secondary key to make the building smaller.")]
                [Hotkey(KeyCode.Comma)]
                public InteractiveHotkeySetting ScaleDownKey { get; private set; }

                [Setting("Move Right Key", "Press this and the secondary key to make the building move to the right.")]
                [Hotkey(KeyCode.RightArrow)]
                public InteractiveHotkeySetting PositionRightKey { get; private set; }

                [Setting("Move Left Key", "Press this and the secondary key to make the building move to the left.")]
                [Hotkey(KeyCode.LeftArrow)]
                public InteractiveHotkeySetting PositionLeftKey { get; private set; }

                [Setting("Move Forward Key", "Press this and the secondary key to make the building move further away.")]
                [Hotkey(KeyCode.UpArrow)]
                public InteractiveHotkeySetting PositionForwardKey { get; private set; }

                [Setting("Move Backward Key", "Press this and the secondary key to make the building move closer.")]
                [Hotkey(KeyCode.DownArrow)]
                public InteractiveHotkeySetting PositionBackwardKey { get; private set; }
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
                config.Settings.ScaleUpKey.OnUpdate.AddListener((setting) => {
                    scaleUpKeycode = config.Settings.ScaleUpKey.Key;
                });
                config.Settings.ScaleDownKey.OnUpdate.AddListener((setting) => {
                    scaleDownKeycode = config.Settings.ScaleDownKey.Key;
                });

                config.Settings.PositionRightKey.OnUpdate.AddListener((setting) => {
                    positionXUpKeycode = config.Settings.PositionRightKey.Key;
                });
                config.Settings.PositionLeftKey.OnUpdate.AddListener((setting) => {
                    positionXDownKeycode = config.Settings.PositionLeftKey.Key;
                });
                config.Settings.PositionForwardKey.OnUpdate.AddListener((setting) => {
                    positionZUpKeycode = config.Settings.PositionForwardKey.Key;
                });
                config.Settings.PositionBackwardKey.OnUpdate.AddListener((setting) => {
                    positionZDownKeycode = config.Settings.PositionBackwardKey.Key;
                });
                config.Settings.SecondaryKey.TriggerUpdate();
                config.Settings.ResetKey.TriggerUpdate();
                config.Settings.ClockwiseKey.TriggerUpdate();
                config.Settings.AnticlockwiseKey.TriggerUpdate();
                config.Settings.ScaleUpKey.TriggerUpdate();
                config.Settings.ScaleDownKey.TriggerUpdate();
                config.Settings.PositionLeftKey.TriggerUpdate();
                config.Settings.PositionRightKey.TriggerUpdate();
                config.Settings.PositionForwardKey.TriggerUpdate();
                config.Settings.PositionBackwardKey.TriggerUpdate();
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
                        newGameObject.name = "Offset";
                        newGameObject.transform.SetParent(__instance.transform);
                        newGameObject.transform.SetSiblingIndex(oldChild.GetSiblingIndex());
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
                [HarmonyPriority(200)]
                static void Postfix(Building PendingObj) {
                    if (PendingObj.transform.GetChild(0).childCount > 0) {
                        Cell cell = PendingObj.GetCell();
                        if (cell != null) {
                            if (cell.OccupyingStructure.Count > 1) {
                                Building existingBuilding = null;
                                foreach (Building otherBuilding in cell.OccupyingStructure) {
                                    if (PendingObj.guid != otherBuilding.guid) {
                                        if (otherBuilding.transform.GetChild(0).childCount > 0) {
                                            existingBuilding = otherBuilding;
                                            break;
                                        }
                                    }
                                }
                                if (existingBuilding != null) {
                                    PendingObj.transform.GetChild(0).GetChild(0).localEulerAngles = existingBuilding.transform.GetChild(0).GetChild(0).localEulerAngles;
                                    PendingObj.transform.GetChild(0).GetChild(0).localScale = existingBuilding.transform.GetChild(0).GetChild(0).localScale;
                                    PendingObj.transform.GetChild(0).GetChild(0).position = new Vector3(existingBuilding.transform.GetChild(0).GetChild(0).position.x, PendingObj.transform.GetChild(0).GetChild(0).position.y, existingBuilding.transform.GetChild(0).GetChild(0).position.z);
                                    PendingObj.transform.GetChild(0).GetChild(0).position = new Vector3(PendingObj.transform.GetChild(0).GetChild(0).position.x, PendingObj.transform.GetChild(0).position.y * PendingObj.transform.GetChild(0).GetChild(0).localScale.y, PendingObj.transform.GetChild(0).GetChild(0).position.z);
                                }
                            }
                        }
                    }
                }
            }

            static private Vector3 castleBlockScaleOld = Vector3.one;
            [HarmonyPatch(typeof(CastleBlock))]
            [HarmonyPatch("ResetRotation")]
            public static class CastleBlockResetRotation {
                static void Postfix(CastleBlock __instance) {
                    castleBlockScaleOld = __instance.transform.GetChild(0).GetChild(0).localScale;
                    __instance.transform.GetChild(0).GetChild(0).eulerAngles = Vector3.zero;
                    __instance.transform.GetChild(0).GetChild(0).localScale = Vector3.one;
                    __instance.transform.GetChild(0).GetChild(0).localPosition = Vector3.zero; ;
                }
            }

            [HarmonyPatch(typeof(CemeteryKeeper))]
            [HarmonyPatch("OnBuildingPlacement")]
            public static class CemeteryKeeperOnBuildingPlacement {
                static bool Prefix(CemeteryKeeper __instance) {
                    __instance.transform.GetChild(0).localPosition = new Vector3(0.5f, 0, 0.5f);
                    __instance.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, 0);
                    __instance.transform.GetChild(0).GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, 0);
                    return false;
                }
            }


            static Vector3 eulerAnglesOld = Vector3.zero;
            static Vector3 positionOld = Vector3.zero;

            static private void SaveTransform(Transform givenTransform) {
                eulerAnglesOld = givenTransform.transform.GetChild(0).GetChild(0).localEulerAngles;
                positionOld = givenTransform.transform.GetChild(0).GetChild(0).position - givenTransform.transform.GetChild(0).position;
            }

            static private void RestoreTransform(Transform givenTransform) {
                givenTransform.transform.GetChild(0).GetChild(0).localEulerAngles = eulerAnglesOld;
                givenTransform.transform.GetChild(0).GetChild(0).position = givenTransform.transform.GetChild(0).position + positionOld;
            }

            [HarmonyPatch(typeof(CastleBlock))]
            [HarmonyPatch("UpdateBlock")]
            public static class CastleBlockUpdateBlock {
                static void Prefix(CastleBlock __instance) {
                    SaveTransform(__instance.transform);
                }

                static void Postfix(CastleBlock __instance) {
                    RestoreTransform(__instance.transform);
                    __instance.transform.GetChild(0).GetChild(0).localScale = castleBlockScaleOld;
                }
            }
            
            [HarmonyPatch(typeof(Road))]
            [HarmonyPatch("UpdateRotationForRoad")]
            public static class RoadUpdateRotationForRoad {
                [HarmonyPriority(600)]
                static void Prefix(Road __instance) {
                    SaveTransform(__instance.transform);
                }

                [HarmonyPriority(200)]
                static void Postfix(Road __instance) {
                    RestoreTransform(__instance.transform);
                }
            }
            

            [HarmonyPatch(typeof(Aqueduct))]
            [HarmonyPatch("UpdateRotationForAqueduct")]
            public static class AqueductUpdateRotationForAqueduct {
                static void Prefix(Aqueduct __instance) {
                    SaveTransform(__instance.transform);
                }

                static void Postfix(Aqueduct __instance) {
                    RestoreTransform(__instance.transform);
                }
            }

            [HarmonyPatch(typeof(Aqueduct))]
            [HarmonyPatch("OnInit")]
            public static class AqueductOnInit {
                static void Postfix(Aqueduct __instance) {
                    aqueductDoRandom180.SetValue(__instance, false);
                }
            }

            [HarmonyPatch(typeof(Road))]
            [HarmonyPatch("Start")]
            public static class RoadStart {
                static void Postfix(Road __instance) {
                    roadDoRandom180.SetValue(__instance, false);
                }
            }








            [HarmonyPatch(typeof(Building.BuildingSaveData))]
            [HarmonyPatch("Pack")]
            public static class BuildingSaveDataPack {
                static void Postfix(Building b) {
                    Quaternion buildingRotation = Quaternion.identity;
                    Vector3 buildingScale = Vector3.one;
                    Vector3 buildingPosition = Vector3.zero;
                    if (b.transform.GetChild(0).childCount > 0) {
                        buildingRotation = b.transform.GetChild(0).GetChild(0).rotation;
                        buildingScale = b.transform.GetChild(0).GetChild(0).localScale;
                        buildingPosition = b.transform.GetChild(0).GetChild(0).localPosition;
                    }
                    SerializableQuaternion serializableQuaternion = (SerializableQuaternion)buildingRotation;
                    string rotationString = serializableQuaternion.x.ToString() + splitChar + serializableQuaternion.y.ToString() + splitChar + serializableQuaternion.z.ToString() + splitChar + serializableQuaternion.w.ToString();

                    SerializableVector3 serializableVector3A = (SerializableVector3)buildingScale;
                    string scaleString = serializableVector3A.x.ToString() + splitChar + serializableVector3A.y.ToString() + splitChar + serializableVector3A.z.ToString();

                    SerializableVector3 serializableVector3B = (SerializableVector3)buildingPosition;
                    string positionString = serializableVector3B.x.ToString() + splitChar + serializableVector3B.y.ToString() + splitChar + serializableVector3B.z.ToString();


                    LoadSave.SaveDataBuilding(modName, b, rotationString + propertyChar + scaleString + propertyChar + positionString);
                }
            }
            
            [HarmonyPatch(typeof(LoadSaveContainer))]
            [HarmonyPatch("Unpack")]
            public static class LoadSaveContainerUnpack {
                static void Postfix() {
                    World.inst.ForEachTile(0, 0, World.inst.GridWidth, World.inst.GridHeight, (World.EvaluateCell)((x, z, cell) => {
                        foreach (Building building in cell.OccupyingStructure) {
                            if (building.transform.GetChild(0).childCount > 0) {
                                string propertyString = LoadSave.ReadDataBuilding(modName, building);
                                if (propertyString != null) {
                                    string[] propertyStringSplit = propertyString.Split(propertyChar);
                                    
                                    string[] rotationStringSplit = propertyStringSplit[0].Split(splitChar);
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
                                            if (building.categoryHash == World.cemeteryHash) {
                                                UpdateGraveRotations(cell);
                                            }
                                        }
                                    }

                                    if (propertyStringSplit.Length > 1) {
                                        string[] scaleStringSplit = propertyStringSplit[1].Split(splitChar);
                                        if (scaleStringSplit.Length == 3) {
                                            List<float> scaleList = new List<float>();
                                            foreach (string scale in scaleStringSplit) {
                                                float parsedFloat = 0;
                                                if (float.TryParse(scale, out parsedFloat)) {
                                                    scaleList.Add(parsedFloat);
                                                }
                                            }
                                            if (scaleList.Count == 3) {
                                                SerializableVector3 serializableVector3 = new SerializableVector3(scaleList[0], scaleList[1], scaleList[2]);
                                                building.transform.GetChild(0).GetChild(0).localScale = (Vector3)serializableVector3;
                                                if (building.categoryHash == World.cemeteryHash) {
                                                    UpdateGraveRotations(cell);
                                                }
                                            }
                                        }
                                    }

                                    if (propertyStringSplit.Length > 2) {
                                        string[] positionStringSplit = propertyStringSplit[2].Split(splitChar);
                                        if (positionStringSplit.Length == 3) {
                                            List<float> positionList = new List<float>();
                                            foreach (string position in positionStringSplit) {
                                                float parsedFloat = 0;
                                                if (float.TryParse(position, out parsedFloat)) {
                                                    positionList.Add(parsedFloat);
                                                }
                                            }
                                            if (positionList.Count == 3) {
                                                SerializableVector3 serializableVector3 = new SerializableVector3(positionList[0], positionList[1], positionList[2]);
                                                building.transform.GetChild(0).GetChild(0).localPosition = (Vector3)serializableVector3;
                                                if (building.categoryHash == World.cemeteryHash) {
                                                    UpdateGraveRotations(cell);
                                                }
                                            }
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
