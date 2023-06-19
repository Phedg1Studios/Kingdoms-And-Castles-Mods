using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_SpawnFish : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "deepWater",
                    "noFish",
                    "noStructure",
                    "fishSurvive",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
            };
            public override bool draggable => true;

            public new static string GetTermSegment() {
                return "SpawnFish";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(List<Cell> cells, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<List<Cell>> cells, bool isClick) {
                foreach (Cell cell in cells[0]) {
                    for (int fishIndex = 0; fishIndex < FishSystem.inst.MaxFishPerTile; fishIndex++) {

                    }
                }
                TerrainGen.inst.UpdateTextures();
            }

            public override void RollbackData(List<Cell> cells) {
                foreach (Cell cell in cells) {
                    TerrainGen.inst.SetFertileTile(cell.x, cell.z, cell.fertile - 1);
                    TerrainGen.inst.UpdateTileFertility(cell.x, cell.z);
                }
            }

            public override void UpdateDisplay(List<Cell> cells) {
                TerrainGen.inst.UpdateTextures();
            }

            public override void OnStart() {
            }

            public override void OnEnd() {
            }
        }
    }
}