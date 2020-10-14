using Harmony;
using System.Reflection;
using UnityEngine;


namespace Phedg1Studios {
    namespace BuildingRotationFix {
        public class BuildingRotationFix : MonoBehaviour {
            static public KCModHelper helper;
            static private float degrees = 0;

            void Preload(KCModHelper helper) {
                BuildingRotationFix.helper = helper;
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("SelectBuilding")]
            public static class GameUISelectBuilding {
                static void Postfix(Building building) {
                    degrees = building.transform.GetChild(0).eulerAngles.y;
                }
            }

            [HarmonyPatch(typeof(PlacementMode))]
            [HarmonyPatch("SetCursorObject")]
            public static class PlacementModeSetCursorObject {
                static void Postfix(PlacementMode __instance, bool notFirstPlacement) {
                    if (notFirstPlacement == false) {
                        degrees = 0;
                    } else {
                        float degreesOld = degrees;
                        int rotations = Mathf.RoundToInt(((degrees % 360 + 360) % 360) / 90f);
                        for (int rotation = 0; rotation < rotations; rotation++) {
                            GameUI.inst.RotateBuilding(__instance.GetHoverBuilding(), true);
                        }
                        degrees = degreesOld;
                    }
                }
            }
        }
    }
}
