using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;
using Assets.Code;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_DecreaseElevation : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "dontSplitLandmass",
                    "elevationNotMin",
                    "noResource",
                    "noStructure",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
            };
            public override bool draggable => true;
            System.Reflection.MethodInfo methodInfo;

            public new static string GetTermSegment() {
                return "DecreaseElevation";
            }

            public override void Activate() {
                SetupInterfaces();
                methodInfo = typeof(MapEdit).GetMethod("SetWaterTileColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(List<Cell> cells, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<List<Cell>> cells, bool isClick) {
                foreach (Cell cell in cells[0]) {
                    int fertileOld = cell.fertile;
                    bool deepwater = false;
                    if (cell.Type == ResourceType.Water) {
                        deepwater = true;
                    }
                    TerrainGen.inst.SetWaterTile(cell.x, cell.z);
                    cell.fertile = fertileOld;
                    cell.deepWater = deepwater;
                    if (deepwater) {
                        TerrainGen.inst.SetTileHeight(cell, TerrainGen.waterHeightDeep);
                    } else {
                        TerrainGen.inst.SetTileHeight(cell, TerrainGen.waterHeightShallow - SRand.value * 0.1f);
                    }
                    UpdateTileColour(cell);

                    if (isClick) {
                        if (deepwater) {
                            cell.landMassIdx = -1;
                        }
                        cell.StorePostGenerationType();
                    }
                }
            }

            public override void RollbackData(List<Cell> cells) {
                foreach (Cell cell in cells) {
                    if (cell.deepWater) {
                        cell.deepWater = false;
                    } else {
                        cell.Type = ResourceType.None;
                    }
                }
            }

            public override void UpdateDisplay(List<Cell> cells) {
                foreach (Cell cell in cells) {
                    if (cell.Type == ResourceType.Water) {
                        TerrainGen.inst.SetTileHeight(cell, TerrainGen.waterHeightShallow - SRand.value * 0.1f);
                        UpdateTileColour(cell);
                    } else {
                        TerrainGen.inst.SetLandTile(cell.x, cell.z);
                        TerrainGen.inst.SetFertileTile(cell.x, cell.z, cell.fertile);
                    }
                }
            }

            private void UpdateTileColour(Cell cell) {
                methodInfo.Invoke(GameState.inst.mainMenuMode.mapEditUI.GetComponent<MapEdit>(), new object[] { cell, Water.inst.waterMat.GetColor("_Color"), Water.inst.waterMat.GetColor("_DeepColor"), Water.inst.waterMat.GetColor("_SaltColor"), Water.inst.waterMat.GetColor("_SaltDeepColor") });
            }

            public override void OnStart() {
            }

            public override void OnEnd() {
            }
        }
    }
}