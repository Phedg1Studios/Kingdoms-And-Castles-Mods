using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;
using Assets.Code;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_TransmuteRock : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "rock",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
            };
            public override bool draggable => true;

            public new static string GetTermSegment() {
                return "TransmuteRock";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(List<Cell> cells, int criteriaIndex) {
            }

            public override void UpdateDataAndDisplay(List<List<Cell>> cells, bool isClick) {
                if (isClick) {
                    ResourceType newType = ResourceType.UnusableStone;
                    if (cells[0][0].Type == ResourceType.Stone) {
                        newType = ResourceType.IronDeposit;
                    } else if (cells[0][0].Type == ResourceType.IronDeposit) {
                        newType = ResourceType.Stone;
                    } else if (cells[0][0].Type == ResourceType.UnusableStone) {
                        if (Random.Range(0, 2) == 0) {
                            newType = ResourceType.Stone;
                        } else {
                            newType = ResourceType.IronDeposit;
                        }
                    }
                    QueryForCriteria.cellModels.Clear();
                    World.inst.RemoveStone(cells[0][0], false);

                    World.inst.PlaceStone(cells[0][0], newType);
                    cells[0][0].StorePostGenerationType();
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