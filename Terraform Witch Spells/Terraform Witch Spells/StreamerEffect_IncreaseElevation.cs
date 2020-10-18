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
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
            };
            public override bool draggable => true;
            System.Reflection.MethodInfo methodInfo;

            public new static string GetTermSegment() {
                return "IncreaseElevation";
            }

            public override void Activate() {
                SetupInterfaces();
                methodInfo = typeof(MapEdit).GetMethod("SetWaterTileColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(List<Cell> cells, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<List<Cell>> cells, bool isClick) {
                int cellIndex = 0;
                foreach (Cell cell in cells[0]) {
                    if (cell.deepWater == false) {
                        cell.Type = ResourceType.None;
                        TerrainGen.inst.SetLandTile(cell.x, cell.z);
                        TerrainGen.inst.SetFertileTile(cell.x, cell.z, cell.fertile);
                    } else {
                        cell.deepWater = false;
                        TerrainGen.inst.SetTileHeight(cell, TerrainGen.waterHeightShallow - SRand.value * 0.1f);
                        UpdateTileColour(cell);
                    }
                    if (isClick) {
                        cell.landMassIdx = QueryForCriteria.neighbourIdxs[cellIndex];
                        cell.StorePostGenerationType();
                    }
                    cellIndex += 1;
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
                methodInfo.Invoke(GameState.inst.mainMenuMode.mapEditUI.GetComponent<MapEdit>(), new object[] { cell, Water.inst.waterMat.GetColor("_Color"), Water.inst.waterMat.GetColor("_DeepColor"), Water.inst.waterMat.GetColor("_SaltColor"), Water.inst.waterMat.GetColor("_SaltDeepColor") });
            }

            public override void OnStart() {
            }

            public override void OnEnd() {
            }
        }
    }
}
 