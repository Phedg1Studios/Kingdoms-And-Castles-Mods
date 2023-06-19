using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;
using Assets.Code;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_IncreaseElevation : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "dontJoinLandmass",
                    "elevationNotMax",
                    "noUnits",
                    "noStructure",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
            };
            public override bool draggable => true;
            System.Reflection.MethodInfo setWaterTileColorInfo;
            System.Reflection.MethodInfo bakePathingCostsForCell;
            System.Reflection.FieldInfo unitsInfo;

            public new static string GetTermSegment() {
                return "IncreaseElevation";
            }

            public override void Activate() {
                SetupInterfaces();
                setWaterTileColorInfo = typeof(MapEdit).GetMethod("SetWaterTileColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                bakePathingCostsForCell = typeof(World).GetMethod("BakePathingCostsForCell", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                unitsInfo = typeof(OrdersManager).GetField("units", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(List<Cell> cells, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<List<Cell>> cells, bool isClick) {
                int cellIndex = 0;
                int minX = 1000000;
                int minZ = 1000000;
                int maxX = -1000000;
                int maxZ = -1000000;
                foreach (Cell cell in cells[0]) {
                    minX = Mathf.Min(minX, cell.x);
                    minZ = Mathf.Min(minZ, cell.z);
                    maxX = Mathf.Max(maxX, cell.x);
                    maxZ = Mathf.Max(maxZ, cell.z);
                    if (cell.deepWater == false) {
                        cell.Type = ResourceType.None;
                        TerrainGen.inst.SetLandTile(cell.x, cell.z);
                        TerrainGen.inst.SetFertileTile(cell.x, cell.z, cell.fertile);
                        TerrainGen.inst.UpdateTileFertility(cell.x, cell.z);
                    } else {
                        cell.deepWater = false;
                        TerrainGen.inst.SetTileHeight(cell, TerrainGen.waterHeightShallow - SRand.value * 0.1f);
                        UpdateTileColour(cell);
                    }
                    if (isClick) {
                        cell.landMassIdx = QueryForCriteria.neighbourIdxs[cellIndex];
                        cell.StorePostGenerationType();
                        bakePathingCostsForCell.Invoke(World.inst, new object[] { cell });
                    }
                    cellIndex += 1;
                }
                TerrainGen.inst.UpdateTextures();
                if (isClick) {
                    UpdateUnitPathing(cells[0], minX, minZ, maxX, maxZ);
                }
            }

            private void UpdateUnitPathing(List<Cell> cells, int minX, int minZ, int maxX, int maxZ) {
                int sizeX = Mathf.Abs(maxX - minX) + 1;
                int sizeZ = Mathf.Abs(maxZ - minZ) + 1;
                int offset = 4;

                System.Collections.ICollection units = (System.Collections.ICollection)unitsInfo.GetValue(OrdersManager.inst);
                foreach (object unitObject in units) {
                    IMoveableUnit unit = (IMoveableUnit)unitObject;
                    Vector3 unitPos = (unit).GetPos();
                    if (unitPos.x > minX - offset && unitPos.x < minX + sizeX + offset) {
                        if (unitPos.z > minZ - offset && unitPos.z < minZ + sizeZ + offset) {
                            if (unit is UnitSystem.Army army) {
                                army.moveTimer.Update(army.moveTimer.Duration);
                            }
                        }
                    }
                }
            }

            public override void RollbackData(List<Cell> cells) {
                foreach (Cell cell in cells) {
                    if (cell.Type == ResourceType.None) {
                        cell.Type = ResourceType.Water;
                    } else {
                        cell.deepWater = true;
                    }
                }
            }

            public override void UpdateDisplay(List<Cell> cells) {
                foreach (Cell cell in cells) {
                    if (cell.deepWater) {
                        TerrainGen.inst.SetTileHeight(cell, TerrainGen.waterHeightDeep);
                    } else {
                        int fertileOld = cell.fertile;
                        TerrainGen.inst.SetWaterTile(cell.x, cell.z);
                        cell.fertile = fertileOld;
                        TerrainGen.inst.SetTileHeight(cell, TerrainGen.waterHeightShallow - SRand.value * 0.1f);
                    }
                    UpdateTileColour(cell);
                }
            }

            private void UpdateTileColour(Cell cell) {
                setWaterTileColorInfo.Invoke(GameState.inst.mainMenuMode.mapEditUI.GetComponent<MapEdit>(), new object[] { cell, Water.inst.waterMat.GetColor("_Color"), Water.inst.waterMat.GetColor("_DeepColor"), Water.inst.waterMat.GetColor("_SaltColor"), Water.inst.waterMat.GetColor("_SaltDeepColor") });
            }

            public override void OnStart() {
            }

            public override void OnEnd() {
            }
        }
    }
}
 