using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
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
                    "playerLandmass",
                },
            };
            public override List<Vector3> sizes => new List<Vector3>() {
                new Vector3(1, 1, 1),
                new Vector3(3, 1, 3),
            };
            public override bool draggable => false;
            MethodInfo methodInfo;

            public new static string GetTermSegment() {
                return "RelocateKeep";
            }

            public override void Activate() {
                SetupInterfaces();
                methodInfo = typeof(World).GetMethod("PlaceInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                QueryForCriteria.SetCriterias(this);
            }

            public override void OnClick(List<Cell> cells, int criteriaIndex) {
                if (criteriaIndex == 0) {
                    cells[0].OccupyingStructure[cells[0].OccupyingStructure.Count - 1].transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            public override void UpdateDataAndDisplay(List<List<Cell>> cells, bool isClick) {
                if (isClick) {
                    Building building = cells[0][0].OccupyingStructure[cells[0][0].OccupyingStructure.Count - 1];
                    methodInfo.Invoke(World.inst, new object[] { building, true, false, false });
                    building.transform.GetChild(0).gameObject.SetActive(true);
                    building.transform.position = new Vector3(cells[1][0].x, 0, cells[1][0].z);
                    building.transform.GetChild(0).localPosition = new Vector3(sizes[QueryForCriteria.criteriaIndex].x / 2f, 0, sizes[QueryForCriteria.criteriaIndex].z / 2f);
                    building.transform.GetChild(0).eulerAngles = new Vector3(0, 0, 0);
                    GameUI.inst.RotateBuilding(building, QueryForCriteria.cursorObject.transform.GetChild(0).eulerAngles.y);
                    building.RefreshCachedValues();
                    methodInfo.Invoke(World.inst, new object[] { building, false, false, false });
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