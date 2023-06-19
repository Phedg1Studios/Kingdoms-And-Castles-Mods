using Harmony;
using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Phedg1Studios {
    namespace Shared {
        public class Util : MonoBehaviour {
            private static string splitChar = ",";
            private static string dictChar = ":";
            private static string profileChar = "#";
            public static KCModHelper helper;

            static public void Setup(KCModHelper givenHelper, GameObject gameObject) {
                helper = givenHelper;
                gameObject.AddComponent<Util>();
                helper.Log("GAME STARTED");
            }

            void Update() {
                if (Input.GetKeyDown(KeyCode.F2)) {
                    helper.Log("F2 Pressed");
                    /*
                    Transform newObject = null;
                    List<string> objectHierarchyB = new List<string>() { "MainMenu", "MENU: Title", "TitleMenu", "SafeZone", "GenericMenuButtonPanel", "JuicePanel", "GenericMenuButton (Logbook)" };
                    if (GetObjectFromHierarchy(ref newObject, objectHierarchyB, 0, null)) {
                        //newObject.gameObject;
                    }
                    */
                    //SaveSceneHierarchy(4);
                    //LogComponentsOfObject(GameObject.Find("ItemEntryIcon(Clone)"));
                    //LogComponentsOfType(typeof(TooltipController));
                }
            }

            
            static public string TrimString(string givenString) {
                if (givenString.Length > 0) {
                    return givenString.Substring(0, givenString.Length - 1);
                }
                return "";
            }

            static public string ListFloatToString(List<float> givenList) {
                string newString = "";
                foreach (float value in givenList) {
                    newString += value.ToString() + splitChar;
                }
                newString = TrimString(newString);
                return newString;
            }

            static public string ListToString(List<int> givenList) {
                string newString = "";
                foreach (int pickupID in givenList) {
                    newString += pickupID.ToString() + splitChar;
                }
                newString = TrimString(newString);
                return newString;
            }

            static public List<string> StringToList(string givenString, char listString) {
                List<string> data = new List<string>();
                string[] parts = givenString.Split(listString);
                foreach (string part in parts) {
                    data.Add(part);
                }
                return data;
            }

            static public Dictionary<string, string> StringToDict(string givenString, char newLineString, char dictString) {
                string[] lines = givenString.Split(newLineString);
                Dictionary<string, string> data = new Dictionary<string, string>();
                foreach (string line in lines) {
                    string[] parts = line.Split(dictString);
                    if (parts.Length == 2) {
                        data[parts[0]] = parts[1];
                    }
                }
                return data;
            }

            static public string DictToString(List<Dictionary<int, int>> givenList) {
                string newString = "";
                foreach (Dictionary<int, int> pickup in givenList) {
                    foreach (int key in pickup.Keys) {
                        newString += key.ToString() + dictChar + pickup[key].ToString() + splitChar;
                    }
                    newString += profileChar;
                }
                newString = TrimString(newString);
                return newString;
            }

            static public Type GetType(string interopName, string className) {
                Assembly assembly;
                Porg.InteropClient.TryGetMod(interopName, out assembly);
                Type type = assembly.GetType(className);
                return type;
            }
            

            static public void LogComponentsOfObject(GameObject givenObject) {
                if (givenObject != null) {
                    Component[] components = givenObject.GetComponents(typeof(Component));
                    foreach (Component component in components) {
                        helper.Log(component.ToString());
                        /*
                        GivePickupsOnStart castedComponent = component as GivePickupsOnStart;
                        if (castedComponent != null) {
                            helper.Log(castedComponent.enabled.toString());
                            helper.Log(castedComponent.equipmentString.toString());
                            
                        }
                        */
                    };
                }
            }

            static public void LogComponentsOfType(Type givenType) {
                UnityEngine.Object[] sceneObjects = GameObject.FindObjectsOfType(givenType);
                helper.Log(sceneObjects.Length.ToString());
                foreach (UnityEngine.Object sceneObject in sceneObjects) {
                    /*
                    TooltipController controller = sceneObject as TooltipController;
                    helper.Log(controller.GetComponent<Canvas>().toString());
                    helper.Log(controller.GetComponent<Canvas>().sortingOrder.toString());
                    */
                    helper.Log(sceneObject.name);
                }
            }

            static public void SaveSceneHierarchy(int maxDepth = -1) {
                GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                string sceneTree = "";
                foreach (GameObject rootObject in rootObjects) {
                    sceneTree = MapHierarchy(rootObject.transform, 0, sceneTree, maxDepth);
                }
                helper.Log("LOG SCENE START HERE");
                helper.Log(sceneTree);
                helper.Log("LOG SCENE END HERE");
            }

            static public void SaveObjectHierarchy(Transform givenTransform, int maxDepth = -1) {
                string objectTree = "";
                objectTree = MapHierarchy(givenTransform, 0, objectTree, maxDepth);
                helper.Log("LOG OBJECT START HERE");
                helper.Log(objectTree);
                helper.Log("LOG OBJECT END HERE");
            }

            static string MapHierarchy(Transform givenTransform, int givenDepth, string givenTree, int maxDepth = -1) {
                string workingString = "";
                for (int spaceNumber = 0; spaceNumber < givenDepth * 4; spaceNumber++) {
                    workingString += " ";
                }
                workingString += givenTransform.gameObject.name + "\n";
                givenTree += workingString;
                if (givenDepth == maxDepth) {
                    return givenTree;
                }
                for (int childIndex = 0; childIndex < givenTransform.childCount; childIndex++) {
                    givenTree = MapHierarchy(givenTransform.GetChild(childIndex), givenDepth + 1, givenTree, maxDepth);
                }
                return givenTree;
            }

            static public bool GetObjectFromHierarchy(ref Transform desiredObject, List<string> hierarchy, int hierarchyIndex, Transform parent) {
                bool childFound = false;
                if (hierarchyIndex == 0) {
                    GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                    for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++) {
                        if (rootObjects[rootIndex].name == hierarchy[hierarchyIndex]) {
                            parent = rootObjects[rootIndex].transform;
                            childFound = true;
                        }
                    }
                } else {
                    for (int childIndex = 0; childIndex < parent.childCount; childIndex++) {
                        if (parent.GetChild(childIndex).name == hierarchy[hierarchyIndex]) {
                            parent = parent.GetChild(childIndex);
                            childFound = true;
                        }
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
