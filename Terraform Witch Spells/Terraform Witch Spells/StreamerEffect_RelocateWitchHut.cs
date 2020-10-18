using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_RelocateWitchHut : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "witch",
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
            public override bool draggable => false;

            public new static string GetTermSegment() {
                return "RelocateWitchHut";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(List<Cell> cells, int criteriaIndex) {
                if (criterias[criteriaIndex].Contains("witch")) {
                    for (int childIndex = 0; childIndex < World.inst.caveContainer.transform.childCount; childIndex++) {
                        Vector3 position = World.inst.caveContainer.transform.GetChild(childIndex).position;
                        if (Mathf.RoundToInt(position.x) == cells[0].x && Mathf.RoundToInt(position.z) == cells[0].z) {
                            World.inst.caveContainer.transform.GetChild(childIndex).gameObject.SetActive(false);
                        }
                    }
                }
            }

            public override void UpdateDataAndDisplay(List<List<Cell>> cells, bool isClick) {
                if (isClick) {

                    for (int childIndex = 0; childIndex < World.inst.caveContainer.transform.childCount; childIndex++) {
                        Vector3 position = World.inst.caveContainer.transform.GetChild(childIndex).position;
                        if (Mathf.RoundToInt(position.x) == cells[0][0].x && Mathf.RoundToInt(position.z) == cells[0][0].z) {
                            GameObject witchHut = World.inst.caveContainer.transform.GetChild(childIndex).gameObject;
                            witchHut.SetActive(true);
                            witchHut.transform.position = new Vector3(cells[1][0].x, witchHut.transform.position.y, cells[1][0].z);
                            cells[1][0].Type = ResourceType.WitchHut;
                            cells[1][0].StorePostGenerationType();
                        }
                    }
                    cells[0][0].Type = ResourceType.None;
                    cells[0][0].StorePostGenerationType();
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