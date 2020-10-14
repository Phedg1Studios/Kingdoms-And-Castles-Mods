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
            System.Reflection.MethodInfo methodInfo;

            public new static string GetTermSegment() {
                return "IncreaseElevation";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
                methodInfo = typeof(MapEdit).GetMethod("SetWaterTileColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            public override void OnClick(Cell cell, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<Cell> cells, bool isClick) {
                if (cells[0].deepWater == false) {
                    cells[0].Type = ResourceType.None;
                    TerrainGen.inst.SetLandTile(cells[0].x, cells[0].z);
                    TerrainGen.inst.SetFertileTile(cells[0].x, cells[0].z, cells[0].fertile);
                } else {
                    cells[0].deepWater = false;
                    TerrainGen.inst.SetTileHeight(cells[0], TerrainGen.waterHeightShallow - SRand.value * 0.1f);
                    UpdateTileColour(cells[0]);
                }
                if (isClick) {
                    cells[0].landMassIdx = QueryForCriteria.neighbourLandmassIdx;
                    cells[0].StorePostGenerationType();
                }
            }

            public override void RollbackData(Cell cell) {
                if (cell.Type == ResourceType.None) {
                    cell.Type = ResourceType.Water;
                } else {
                    cell.deepWater = true;
                }
            }

            public override void UpdateDisplay(Cell cell) {
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
 