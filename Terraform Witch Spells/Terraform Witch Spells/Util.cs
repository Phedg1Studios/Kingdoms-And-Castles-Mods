using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class Util : MonoBehaviour {
            private bool f2Down = false;
            private bool mousePointerDown = false;

            void UpdateB() {
                if (Input.GetKeyDown(KeyCode.F2)) {
                    f2Down = true;
                }
                if (f2Down) {
                    if (PointingSystem.GetPointer().GetPrimaryDown()) {
                        mousePointerDown = true;
                    }
                    if (PointingSystem.GetPointer().GetPrimaryUp()) {
                        if (mousePointerDown) {
                            mousePointerDown = false;
                            Cell cell = World.inst.GetCellData(GameUI.inst.GridPointerIntersection());
                            int modelsLength = 0;
                            if (cell.Models != null) {
                                modelsLength = cell.Models.Count;
                            }
                            Log("CELL MODELS: " + modelsLength.ToString());
                            Log("CELL TYPE: " + cell.Type.ToString());
                            Vector2 cellCenter = new Vector2(cell.x + 0.5f, cell.z + 0.5f);

                            int modelsOutsideTheirTile = 0;
                            List<Vector2> allCellModelPositions = new List<Vector2>();
                            for (int x = 0; x < World.inst.GridWidth; x++) {
                                for (int z = 0; z < World.inst.GridHeight; z++) {
                                    Cell worldCell = World.inst.GetCellData(x, z);
                                    if (worldCell.Models != null) {
                                        Vector2 worldCellCenter = new Vector2(worldCell.x + 0.5f, worldCell.z + 0.5f);
                                        foreach (GameObject model in worldCell.Models) {
                                            if (DistanceBetweenTwoPoints(worldCellCenter, model.transform.position) > 0.75f) {
                                                modelsOutsideTheirTile += 1;
                                            }
                                            allCellModelPositions.Add(RoundToThreePlaces(model.transform.position));
                                        }
                                    }
                                }
                            }
                            Log("MODELS OUTSIDE THEIR TILE: " + modelsOutsideTheirTile.ToString());
                            int modelsNotAttachedToACell = 0;
                            int ghostModels = 0;
                            for (int childIndex = 0; childIndex < World.inst.resourceContainer.transform.childCount; childIndex++) {
                                if (!allCellModelPositions.Contains(RoundToThreePlaces(World.inst.resourceContainer.transform.GetChild(childIndex).position))) {
                                    modelsNotAttachedToACell += 1;
                                }
                                if (DistanceBetweenTwoPoints(cellCenter, World.inst.resourceContainer.transform.GetChild(childIndex).position) < 0.71f) {
                                    ghostModels += 1;
                                }
                            }
                            Log("MODELS NOT ATTACHED TO A CELL: " + modelsNotAttachedToACell.ToString());
                            Log("GHOST MODELS: " + ghostModels.ToString());
                        }
                    }
                }
                if (Input.GetKeyUp(KeyCode.F2)) {
                    f2Down = false;
                }
            }

            static private Vector2 RoundToThreePlaces(Vector3 givenPosition) {
                return new Vector2(Mathf.RoundToInt(givenPosition.x * 1000), Mathf.RoundToInt(givenPosition.z * 1000));
            }

            static private float DistanceBetweenTwoPoints(Vector2 pointA, Vector3 pointB) {
                return Mathf.Sqrt(Mathf.Pow(pointA.x - pointB.x, 2) + Mathf.Pow(pointA.y - pointB.z, 2));
            }

            static public void Log(object givenObject, bool traceBack = false) {
                if (givenObject == null) {
                    TerraformWitchSpells.helper.Log("null");
                } else {
                    TerraformWitchSpells.helper.Log(givenObject.ToString());
                }
                if (traceBack) {
                    TerraformWitchSpells.helper.Log(StackTraceUtility.ExtractStackTrace());
                }
            }

            void Update() {
                if (Input.GetKeyDown(KeyCode.F2)) {
                    //SaveSceneHierarchy();
                    //LogComponentsOfType(typeof(BuildInfoUI));
                    /*
                    Transform newObject = null;
                    List<string> objectHierarchyB = new List<string>() { "MainMenu", "MENU: Title", "TitleMenu", "SafeZone", "GenericMenuButtonPanel", "JuicePanel", "GenericMenuButton (Logbook)" };
                    if (Util.util.GetObjectFromHierarchy(ref newObject, objectHierarchyB, 0, null)) {
                        //newObject.gameObject;
                    }
                    */
                    //SaveSceneHierarchy();
                    //LogComponentsOfObject(GameObject.Find("ItemEntryIcon(Clone)"));
                    //LogComponentsOfType(typeof(RoR2.UI.TooltipController));
                }
            }

            static public List<int> GetCornerOfBuilding(Vector3 position, Vector3 size, float rotation, ref Vector3 sizeAdjusted) {
                int xOffset = 0;
                int zOffset = 0;
                sizeAdjusted = size;
                if (Mathf.RoundToInt(size.x) != Mathf.RoundToInt(size.z)) {
                    int rotationRounded = Mathf.RoundToInt(rotation);
                    if (rotationRounded == 90) {
                        zOffset = -(Mathf.RoundToInt(size.x) - 1);
                    } else if (rotationRounded == 180) {
                        xOffset = -(Mathf.RoundToInt(size.x) - 1);
                        zOffset = -(Mathf.RoundToInt(size.z) - 1);
                    } else if (rotationRounded == 270) {
                        xOffset = -(Mathf.RoundToInt(size.z) - 1);
                    }

                    if (rotationRounded == 90 || rotationRounded == 270) {
                        sizeAdjusted = new Vector3(size.z, size.y, size.x);
                    }
                }
                return new List<int>() { Mathf.RoundToInt(position.x) + xOffset, Mathf.RoundToInt(position.z) + zOffset };
            }

            static public List<int> GetCornerOfBuildingB(Cell cell, Vector3 size) {
                List<int> corner = new List<int>();
                if (cell.OccupyingStructure.Count == 0) {
                    corner = new List<int>() {
                        Mathf.RoundToInt(cell.x - size.x / 2f),
                        Mathf.RoundToInt(cell.z - size.z / 2f),
                    };
                } else {
                    Building building = cell.OccupyingStructure[cell.OccupyingStructure.Count - 1];
                    corner = new List<int>() {
                        Mathf.RoundToInt(building.transform.GetChild(0).position.x - size.x / 2f),
                        Mathf.RoundToInt(building.transform.GetChild(0).position.z - size.z / 2f),
                    };
                }
                return corner;
            }


            static public void OnLogMessageReceived(string condition, string stackTrace, LogType type) {
                if (type == LogType.Exception) {
                    Log("Unhandled Exception: " + condition + "\n" + stackTrace);
                }
            }

            static public void LogComponentsOfObject(GameObject givenObject) {
                if (givenObject != null) {
                    Component[] components = givenObject.GetComponents(typeof(Component));
                    foreach (Component component in components) {
                        Log(component);
                    };
                }
            }

            static public void LogComponentsOfType(Type givenType) {
                UnityEngine.Object[] sceneObjects = GameObject.FindObjectsOfType(givenType);
                if (sceneObjects != null) {
                    Log(sceneObjects.Length);
                    foreach (UnityEngine.Object sceneObject in sceneObjects) {
                        Log(sceneObject.name);
                    }
                } else {
                    Log(0);
                }
            }

            static public void LogComponentsOfChildren(Transform givenTransform) {
                LogComponentsOfObject(givenTransform.gameObject);
                for (int childIndex = 0; childIndex < givenTransform.childCount; childIndex++) {
                    LogComponentsOfChildren(givenTransform.GetChild(childIndex));
                }
            }

            static public void SaveSceneHierarchy() {
                GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                string sceneTree = "";
                foreach (GameObject rootObject in rootObjects) {
                    sceneTree = MapHierarchy(rootObject.transform, 0, sceneTree);
                }
                Log(sceneTree);
            }

            static string MapHierarchy(Transform givenTransform, int givenDepth, string givenTree) {
                string workingString = "";
                for (int spaceNumber = 0; spaceNumber < givenDepth * 4; spaceNumber++) {
                    workingString += " ";
                }
                workingString += givenTransform.gameObject.name + "\n";
                givenTree += workingString;
                for (int childIndex = 0; childIndex < givenTransform.childCount; childIndex++) {
                    givenTree = MapHierarchy(givenTransform.GetChild(childIndex), givenDepth + 1, givenTree);
                }
                return givenTree;
            }

            static public bool GetObjectFromScene(ref Transform desiredObject, List<string> hierarchy, int hierarchyIndex, Transform parent) {
                GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++) {
                    if (rootObjects[rootIndex].name == hierarchy[hierarchyIndex]) {
                        parent = rootObjects[rootIndex].transform;
                        if (hierarchyIndex == hierarchy.Count - 1) {
                            desiredObject = parent;
                            return true;
                        } else {
                            return GetObjectFromHierarchy(ref desiredObject, hierarchy, hierarchyIndex + 1, parent);
                        }
                    }
                }
                return false;
            }

            static public bool GetObjectFromHierarchy(ref Transform desiredObject, List<string> hierarchy, int hierarchyIndex, Transform parent) {
                bool childFound = false;
                for (int childIndex = 0; childIndex < parent.childCount; childIndex++) {
                    if (parent.GetChild(childIndex).name == hierarchy[hierarchyIndex]) {
                        parent = parent.GetChild(childIndex);
                        childFound = true;
                    }
                }
                if (childFound) {
                    if (hierarchyIndex == hierarchy.Count - 1) {
                        desiredObject = parent;
                        return true;
                    } else {
                        return GetObjectFromHierarchy(ref desiredObject, hierarchy, hierarchyIndex + 1, parent);
                    }
                }
                return false;
            }
        }
    }
}
