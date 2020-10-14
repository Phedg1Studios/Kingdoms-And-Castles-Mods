using UnityEngine;
using System.Collections.Generic;
using static Phedg1Studios.TerraformWitchSpells.Util;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class StreamerEffect_RelocateKeep : StreamerEffectQuery {
            public override List<List<string>> criterias => new List<List<string>>() {
                new List<string>() {
                    "keep",
                },
                new List<string>() {
                    "noResourseLand",
                    "noStructure",
                    "sameLandmass",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
                new Vector3(3, 1, 3),
            };

            public new static string GetTermSegment() {
                return "RelocateKeep";
            }

            public override void Activate() {
                SetupInterfaces();
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(Cell cell, int criteriaIndex) {
                if (criteriaIndex == 0) {
                    cell.OccupyingStructure[cell.OccupyingStructure.Count - 1].transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            public override void UpdateDataAndDisplay(List<Cell> cells, bool isClick) {
                if (isClick) {
                    Building building = cells[0].OccupyingStructure[cells[0].OccupyingStructure.Count - 1];
                    float oldRotation = building.transform.GetChild(0).eulerAngles.y;
                    Vector3 cellOldSize = new Vector3();
                    List<int> cellOldCorner = Util.GetCornerOfBuilding(building.transform.position, building.size, oldRotation, ref cellOldSize);
                    for (int x = 0; x < cellOldSize.x; x++) {
                        for (int z = 0; z < cellOldSize.z; z++) {
                            Cell cell = World.inst.GetCellData(cellOldCorner[0] + x, cellOldCorner[1] + z);
                            if (cell != null) {
                                cell.OccupyingStructure.RemoveAt(cell.OccupyingStructure.Count - 1);
                            }
                        }
                    }
                    float newRotation = QueryForCriteria.cursorObject.transform.GetChild(0).eulerAngles.y;
                    Vector3 cellNewSize = new Vector3();
                    List<int> cellNewCorner = Util.GetCornerOfBuilding(new Vector3(cells[1].x, 0, cells[1].z), building.size, newRotation, ref cellNewSize);
                    for (int x = 0; x < cellNewSize.x; x++) {
                        for (int z = 0; z < cellNewSize.z; z++) {
                            Cell cell = World.inst.GetCellData(cellNewCorner[0] + x, cellNewCorner[1] + z);
                            if (cell != null) {
                                cell.OccupyingStructure.Add(building);
                            }
                        }
                    }
                    building.transform.GetChild(0).gameObject.SetActive(true);
                    building.transform.position = new Vector3(cells[1].x, 0, cells[1].z);
                    building.transform.GetChild(0).localPosition = new Vector3(sizes[QueryForCriteria.criteriaIndex].x / 2f, 0, sizes[QueryForCriteria.criteriaIndex].z / 2f);
                    building.transform.GetChild(0).eulerAngles = new Vector3(0, 0, 0);
                    GameUI.inst.RotateBuilding(building, QueryForCriteria.cursorObject.transform.GetChild(0).eulerAngles.y);
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