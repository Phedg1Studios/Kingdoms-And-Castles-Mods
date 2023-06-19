using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class Util : MonoBehaviour {
            void Update() {
                if (Input.GetKeyDown(KeyCode.F3)) {
                    //Cell cell = World.inst.GetCellData(GameUI.inst.GridPointerIntersection());
                    //Log(cell.x.ToString() + " " + cell.z.ToString());
                }
                if (Input.GetKeyDown(KeyCode.F2)) {
                    //SaveSceneHierarchy();
                    //LogTrees();
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

            static public void LogTrees() {
                Log("-");
                int floatingCells = 0;
                List<Vector3> treeCoords = new List<Vector3>();
                for (int x = 0; x < World.inst.GridWidth; x++) {
                    for (int z = 0; z < World.inst.GridHeight; z++) {
                        Cell cell = World.inst.GetCellData(x, z);
                        if (cell != null) {
                            if (cell.Type == ResourceType.Water) {
                                foreach (int treeID in cell.TreeIds) {
                                    if (treeID != -1) {
                                        floatingCells += 1;
                                        treeCoords.Add(new Vector3(cell.x, 0, cell.z));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                Log(floatingCells);
                foreach (Vector3 treeCord in treeCoords) {
                    Log(treeCord.x + " " + treeCord.z);
                }
                Log("--");
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

            static public void OnLogMessageReceived(string condition, string stackTrace, LogType type) {
                if (type == LogType.Exception) {
                    Log("Unhandled Exception: " + condition + "\n" + stackTrace);
                }
            }

            static public string NullSafeToString(object givenObject) {
                if (givenObject != null) {
                    return givenObject.ToString();
                } else {
                    return "null";
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
                Log(workingString += givenTransform.gameObject.name);
                //givenTree += workingString;
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
