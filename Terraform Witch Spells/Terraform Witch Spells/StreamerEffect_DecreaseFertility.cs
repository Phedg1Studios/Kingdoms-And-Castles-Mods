using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_DecreaseFertility : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "fertilityNotMin",
                    "noResourseLand",
                    "noStructure",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
            };

            public new static string GetTermSegment() {
                return "DecreaseFertility";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(Cell cell, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<Cell> cells, bool isClick) {
                TerrainGen.inst.SetFertileTile(cells[0].x, cells[0].z, cells[0].fertile - 1);
            }

            public override void RollbackData(Cell cell) {
                TerrainGen.inst.SetFertileTile(cell.x, cell.z, cell.fertile + 1);
            }

            public override void UpdateDisplay(Cell cell) {
            }

            public override void OnStart() {
            }

            public override void OnEnd() {
            }
        }
    }
}