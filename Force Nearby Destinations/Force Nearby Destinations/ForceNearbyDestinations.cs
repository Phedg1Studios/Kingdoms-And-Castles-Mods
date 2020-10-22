using Harmony;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zat.Shared.ModMenu.API;
using Zat.Shared.ModMenu.Interactive;

namespace Phedg1Studios {
    namespace ForceNearbyDestinations {
        public class ForceNearbyDestinations : MonoBehaviour {
            static public KCModHelper helper;
            private UnityAction<Exception> modMenuException;
            private InteractiveConfiguration<ModMenuSettings> config;
            static private new bool enabled = true;
            static private bool pathingCost = false;

            void Preload(KCModHelper helper) {
                ForceNearbyDestinations.helper = helper;
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            [Mod("Force Nearby Destinations", "v1.0", "Phedg1 Studios")]
            public class ModMenuSettings {
                [Setting("Enabled", "Whether this mod is enabled")]
                [Toggle(true, "True")]
                public InteractiveToggleSetting Enabled { get; private set; }
                [Setting("Assess Destination Speed", "Whether faster desinations should be prioritised (ie. road vs water)")]
                [Toggle(false, "False")]
                public InteractiveToggleSetting PathingCost { get; private set; }
            }

            void Start() {
                modMenuException += ModMenuException;
                config = new InteractiveConfiguration<ModMenuSettings>();
                ModSettingsBootstrapper.Register(config.ModConfig, SettingsRegistered, modMenuException);
            }

            void SettingsRegistered(ModSettingsProxy proxy, SettingsEntry[] saved) {
                config.Install(proxy, saved);
                config.Settings.Enabled.OnUpdate.AddListener((setting) => {
                    enabled = setting.toggle.value;
                    config.Settings.Enabled.Label = setting.toggle.value.ToString();
                });
                config.Settings.PathingCost.OnUpdate.AddListener((setting) => {
                    pathingCost = setting.toggle.value;
                    config.Settings.PathingCost.Label = setting.toggle.value.ToString();
                });
                config.Settings.Enabled.TriggerUpdate();
                config.Settings.PathingCost.TriggerUpdate();
            }

            void ModMenuException(Exception exception) {
                Log(exception);
                Log(exception.Message);
                Log(exception.StackTrace);
            }


            static public void UpdateFormations(Quaternion quaternion, List<IMoveableUnit> moveableUnitList2, List<Vector3>[] formationOffsets) {
                if (enabled) {
                    Cell cellDataClamped = World.inst.GetCellDataClamped(GameUI.inst.GridPointerIntersection());

                    if (cellDataClamped != null && cellDataClamped.landMassIdx != -1) {
                        Vector3 center = cellDataClamped.Center;
                        int teamId = moveableUnitList2[0].TeamID();
                        List<Vector3> selectedCells = new List<Vector3>();
                        List<Vector3> mappedCells = new List<Vector3>();
                        Dictionary<int, List<Vector3>> adjacentCells = new Dictionary<int, List<Vector3>>() { { 0, new List<Vector3>() { center } } };

                        while (adjacentCells.Keys.Count > 0 && selectedCells.Count < moveableUnitList2.Count) {
                            List<int> costs = adjacentCells.Keys.ToList();
                            costs.Sort();
                            int cost = costs[0];

                            Vector3 newPos = adjacentCells[cost][0];
                            selectedCells.Add(newPos);
                            adjacentCells[cost].Remove(newPos);
                            if (adjacentCells[cost].Count == 0) {
                                adjacentCells.Remove(cost);
                            }

                            AddAdjacentPos(teamId, mappedCells, adjacentCells, cost, newPos, newPos + new Vector3(1, 0, 0));
                            AddAdjacentPos(teamId, mappedCells, adjacentCells, cost, newPos, newPos + new Vector3(0, 0, 1));
                            AddAdjacentPos(teamId, mappedCells, adjacentCells, cost, newPos, newPos + new Vector3(-1, 0, 0));
                            AddAdjacentPos(teamId, mappedCells, adjacentCells, cost, newPos, newPos + new Vector3(0, 0, -1));
                        }

                        if (selectedCells.Count == moveableUnitList2.Count) {
                            int formationIdx = moveableUnitList2[0].FormationIdx();
                            Quaternion inverseQaternion = Quaternion.Inverse(quaternion);
                            formationOffsets[formationIdx].Clear();
                            foreach (Vector3 selectedCell in selectedCells) {
                                formationOffsets[formationIdx].Add(inverseQaternion * (selectedCell - center));
                            }
                        }
                    }
                }
            }

            static private void AddAdjacentPos(int teamId, List<Vector3> mappedCells, Dictionary<int, List<Vector3>> adjacentCells, int currentCost, Vector3 currentPos, Vector3 newPos) {
                if (!mappedCells.Contains(newPos)) {
                    Cell newCell = World.inst.GetCellDataClamped(newPos);
                    if (newCell != null && newCell.landMassIdx != -1) {
                        Cell currentCell = World.inst.GetCellDataClamped(currentPos);
                        if (currentCell.isUpperGridBlocked == newCell.isUpperGridBlocked || currentCell.isStairs || newCell.isStairs) {
                            int newCost = 1;
                            if (pathingCost && newCell.villagerFootPathCost.Length > teamId) {
                                newCost = currentCost + World.GetFootPathCost(newCell, teamId);
                            }
                            if (!adjacentCells.ContainsKey(newCost)) {
                                adjacentCells.Add(newCost, new List<Vector3>());
                            }
                            adjacentCells[newCost].Add(newPos);
                            mappedCells.Add(newPos);
                        }
                    } else {
                        mappedCells.Add(newPos);
                    }
                }
            }

            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("Update")]
            public static class GameUIUpdate {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    int startIndex = -1;
                    for (int codeIndex = 0; codeIndex < codes.Count - 2; codeIndex++) {
                        if (codes[codeIndex].opcode == OpCodes.Ldloc_S && NullSafeToString(codes[codeIndex].operand) == "UnityEngine.Vector3 (41)") {
                            if (codes[codeIndex + 1].opcode == OpCodes.Call && NullSafeToString(codes[codeIndex + 1].operand) == "UnityEngine.Quaternion LookRotation(UnityEngine.Vector3)") {
                                if (codes[codeIndex + 2].opcode == OpCodes.Stloc_S && NullSafeToString(codes[codeIndex + 2].operand) == "UnityEngine.Quaternion (42)") {
                                    startIndex = codeIndex + 3;
                                    break;
                                }
                            }
                        }
                        //helper.Log(codes[codeIndex].opcode.ToString() + " " + NullSafeToString(codes[codeIndex].operand));
                    }
                    if (startIndex != -1) {
                        MethodInfo updateFormationsInfo = typeof(ForceNearbyDestinations).GetMethod("UpdateFormations", BindingFlags.Public | BindingFlags.Static);
                        FieldInfo formationOffsetsInfo = typeof(GameUI).GetField("formationOffsets", BindingFlags.NonPublic | BindingFlags.Instance);

                        codes.Insert(startIndex, new CodeInstruction(OpCodes.Ldloc_S, 42));
                        codes.Insert(startIndex + 1, new CodeInstruction(OpCodes.Ldloc_S, 31));
                        codes.Insert(startIndex + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(startIndex + 3, new CodeInstruction(OpCodes.Ldfld, formationOffsetsInfo));
                        codes.Insert(startIndex + 4, new CodeInstruction(OpCodes.Call, updateFormationsInfo));
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
        }
    }
}