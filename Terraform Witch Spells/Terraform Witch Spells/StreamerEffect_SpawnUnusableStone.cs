using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_SpawnUnusuableStone : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "noResourseLand",
                    "noStructure",
                    "noUnits",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
            };
            public override bool draggable => false;

            public new static string GetTermSegment() {
                return "SpawnUnusableStone";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(List<Cell> cells, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<List<Cell>> cells, bool isClick) {
                if (isClick) {
                    foreach (Cell cell in cells[0]) {
                        World.inst.PlaceStone(cell.x, cell.z, ResourceType.UnusableStone);
                        //cell.StorePostGenerationType();
                    }
                    World.inst.CombineStone();
                }
            }

            public override void RollbackData(List<Cell> cells) {
            }

            public override void UpdateDisplay(List<Cell> cells) {
            }

            public override void OnStart() {
            }

            public override void OnEnd() {
            }
        }
    }
}