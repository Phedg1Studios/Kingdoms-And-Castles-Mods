using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_RelocateStone : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "stone",
                },
                new List<string>() {
                    "noResourseLand",
                    "noStructure",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
                new Vector3(1, 1, 1),
            };
            /*
            List<ResourceType> modeleTypes = new List<ResourceType>() {
                ResourceType.None,
                ResourceType.Stone,
            };
            */

            public new static string GetTermSegment() {
                return "RelocateStone";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(Cell cell, int criteriaIndex) {
                if (criterias[criteriaIndex].Contains("stone")) {
                    ResourceType typeOld = cell.Type;
                    cell.Type = ResourceType.None;
                    World.inst.CombineStone();
                    cell.Type = typeOld;
                }
            }

            public override void UpdateDataAndDisplay(List<Cell> cells, bool isClick) {
                if (cells.Count == 2) {
                    World.inst.RemoveStone(cells[0], false);
                    cells[0].StorePostGenerationType();

                    /*
                    System.Reflection.MethodInfo methodInfo = typeof(MapEdit).GetMethod("ApplyBrush", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    methodInfo.Invoke(TerraformWitchSpells.mapEdit,
                        new object[] { MapEdit.BrushMode.Stone, cells[1], Water.inst.waterMat.GetColor("_Color"), Water.inst.waterMat.GetColor("_DeepColor"), Water.inst.waterMat.GetColor("_SaltColor"), Water.inst.waterMat.GetColor("_SaltDeepColor")
                    });
                    */

                    cells[1].Type = ResourceType.Stone;
                    QueryForCriteria.SetCellModel(cells[1]);
                    cells[1].StorePostGenerationType();

                    World.inst.CombineStone();
                }
            }

            public override void RollbackData(Cell cell) {
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