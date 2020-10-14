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
            System.Reflection.MethodInfo methodInfo;

            public new static string GetTermSegment() {
                return "DecreaseElevation";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
                methodInfo = typeof(MapEdit).GetMethod("SetWaterTileColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            public override void OnClick(Cell cell, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<Cell> cells, bool isClick) {
                int fertileOld = cells[0].fertile;
                bool deepwater = false;
                if (cells[0].Type == ResourceType.Water) {
                    deepwater = true;
                }
                TerrainGen.inst.SetWaterTile(cells[0].x, cells[0].z);
                cells[0].fertile = fertileOld;
                cells[0].deepWater = deepwater;
                if (deepwater) {
                    TerrainGen.inst.SetTileHeight(cells[0], TerrainGen.waterHeightDeep);
                } else {
                    TerrainGen.inst.SetTileHeight(cells[0], TerrainGen.waterHeightShallow - SRand.value * 0.1f);
                }
                UpdateTileColour(cells[0]);

                if (isClick) {
                    if (deepwater) {
                        cells[0].landMassIdx = -1;
                    }
                    cells[0].StorePostGenerationType();
                }
            }

            public override void RollbackData(Cell cell) {
                if (cell.deepWater) {
                    cell.deepWater = false;
                } else {
                    cell.Type = ResourceType.None;
                }
            }

            public override void UpdateDisplay(Cell cell) {
                if (cell.Type == ResourceType.Water) {
                    TerrainGen.inst.SetTileHeight(cell, TerrainGen.waterHeightShallow - SRand.value * 0.1f);
                    UpdateTileColour(cell);
                } else {
                    TerrainGen.inst.SetLandTile(cell.x, cell.z);
                    TerrainGen.inst.SetFertileTile(cell.x, cell.z, cell.fertile);
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