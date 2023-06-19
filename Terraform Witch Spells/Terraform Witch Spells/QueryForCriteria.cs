using Harmony;
using UnityEngine;
using Assets;
using Assets.Code;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using cakeslice;
using Phedg1Studios.Shared;
using static Phedg1Studios.TerraformWitchSpells.Util;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class QueryForCriteria : MonoBehaviour {
            static public int criteriaIndex = -1;
            static private StreamerEffectQuery streamerEffectQuery;
            static private List<List<Cell>> cells = new List<List<Cell>>();
            static private List<int> previousCell;
            static private List<Cell> previousCells = new List<Cell>();
            static private bool spellUsed = false;
            static private bool cellValid = false;
            static private string cellInvalidReason = "";
            static private ResourceAmount resourceAmount = new ResourceAmount();

            static public GameObject cursorObject;
            static public string cursorColour = "red";
            static private System.Reflection.MethodInfo updateCellSelectorMethod;
            static private Vector3 leftMouseDownPos;
            static private Vector3 rightMouseDownPos;
            static private List<int> leftMouseDownCell;
            static private ObjectHighlighter highlighter;
            static private ObjectHighlighter hoverHighlighter;
            static public List<int> neighbourIdxs = new List<int>();
            static private List<int> nullPos = new List<int>() { -1000000, -1000000, -1000000 };
            static private Building nullBuilding;
            static private int previousLandmassIdx;
            static private bool cursorRotateable = false;
            static private bool buildTabClicked = false;
            static private System.Reflection.FieldInfo unitsInfo;
            static private System.Reflection.FieldInfo fishInfo;

            private void Awake() {
                unitsInfo = typeof(OrdersManager).GetField("units", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fishInfo = typeof(FishSystem).GetField("fish", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                GameObject buildingObject = new GameObject();
                buildingObject.transform.position = new Vector3(0, 0, 0);
                nullBuilding = buildingObject.AddComponent<Building>();
                GameObject.Instantiate(new GameObject(), buildingObject.transform);
                buildingObject.transform.GetChild(0).position = new Vector3(0, 0, 0);
                nullBuilding.dragPlacementMode = Building.DragPlacementMode.Rectangle;
                DontDestroyOnLoad(buildingObject);
                rightMouseDownPos = new Vector3(nullPos[0], nullPos[1], nullPos[2]);
            }

            static public void SetCriterias(StreamerEffectQuery givenEffectQuery) {
                updateCellSelectorMethod = typeof(GameUI).GetMethod("UpdateCellSelector", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                streamerEffectQuery = givenEffectQuery;
                criteriaIndex = 0;
                buildTabClicked = false;
                previousLandmassIdx = -1;
                previousCells = new List<Cell>();
                leftMouseDownPos = new Vector3(nullPos[0], nullPos[1], nullPos[2]);
                leftMouseDownCell = new List<int>() { nullPos[0], nullPos[2] };
                ResetTracking();
                SfxSystem.PlayUiSelect();
            }








            private void LateUpdate() {
                if (criteriaIndex != -1) {
                    if (PointingSystem.GetPointer().GetPrimaryDown()) {
                        if (!GameUI.inst.WasEventConsumed() && !buildTabClicked) {
                            Cell cell = World.inst.GetCellData(GameUI.inst.GridPointerIntersection());
                            if (cell != null) {
                                leftMouseDownCell = new List<int>() { cell.x, cell.z };
                            }
                            leftMouseDownPos = Input.mousePosition;
                        }
                    }
                    if (PointingSystem.GetPointer().GetSecondaryDown()) {
                        rightMouseDownPos = Input.mousePosition;
                    }
                    if (PointingSystem.GetPointer().GetSecondaryUp()) {
                        if (!Cam.inst.IsDragging() && (double)Vector3.Distance(rightMouseDownPos, Input.mousePosition) < 4.0) {
                            StopSpelling();
                        }
                    }
                    if (criteriaIndex != -1) {
                        if (ConfigurableControls.inst.GetInputActionKeyDown(InputActions.EscapeButtonBehavior)) {
                            StopSpelling();
                        } else {
                            EvaluateCell();
                        }
                    }

                    GameUI.inst.UpdateUIScaling();
                    //GameUI.inst.UpdateTooltips();

                    if (PointingSystem.GetPointer().GetPrimaryUp()) {
                        if (!LevelUpUI.IsShowing() && !Cam.inst.IsDragging() && leftMouseDownPos.x != nullPos[0] && ((double)Vector3.Distance(leftMouseDownPos, Input.mousePosition) < 4.0 || streamerEffectQuery.IsDragable())) {
                            DoPrimaryClick();
                        }
                        leftMouseDownPos = new Vector3(nullPos[0], nullPos[1], nullPos[2]);
                    }

                    if (ConfigurableControls.inst.GetInputActionKeyDown(InputActions.RotateBuilding)) {
                        GameUI.inst.TryRotateHeldBuilding(true);
                    }

                    updateCellSelectorMethod.Invoke(GameUI.inst, new object[0]);
                    if (GameUI.inst.workerUI.Visible || GameUI.inst.constructUI.Visible || (GameUI.inst.shipUI.Visible || GameUI.inst.merchantUI.Visible)) {
                        GameUI.inst.creativeModeOptions.GetComponent<CreativeModeOptions>().HideOptions();
                    } else {
                        GameUI.inst.creativeModeOptions.SetActive(Player.inst.creativeMode);
                    }
                    if (highlighter != null) {
                        highlighter.SetOutlineImmediate(1);
                    }
                    if (hoverHighlighter != null) {
                        hoverHighlighter.SetOutlineImmediate(0);
                    }
                    if (buildTabClicked) {
                        buildTabClicked = false;
                    }
                }
            }

            static public Vector3 GetBounds() {
                Vector3 vector3 = cursorObject.transform.GetChild(0).rotation * streamerEffectQuery.sizes[criteriaIndex];
                return new Vector3(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y), Mathf.Abs(vector3.z));
            }

            static public void RotateCursor(float angle) {
                if (Mathf.RoundToInt(streamerEffectQuery.sizes[criteriaIndex].x) == Mathf.RoundToInt(streamerEffectQuery.sizes[criteriaIndex].z)) {
                    cursorObject.transform.GetChild(0).RotateAround(cursorObject.transform.position + new Vector3(streamerEffectQuery.sizes[criteriaIndex].x / 2f, 0.0f, streamerEffectQuery.sizes[criteriaIndex].z / 2f), new Vector3(0.0f, 1f, 0.0f), angle);
                } else {
                    cursorObject.transform.GetChild(0).RotateAround(cursorObject.transform.position + new Vector3(0.5f, 0.0f, 0.5f), new Vector3(0.0f, 1f, 0.0f), angle);
                }
            }

            static public void EvaluateCell() {
                bool cellIsACell = false;
                bool cellValidNew = true;
                Cell cell = World.inst.GetCellData(GameUI.inst.GridPointerIntersection());
                if (cell != null) {
                    cellIsACell = true;
                    List<int> newCell = new List<int>() { cell.x, cell.z };
                    if (newCell[0] != previousCell[0] || newCell[1] != previousCell[1]) {
                        Vector3 position = new Vector3(cell.x, 0, cell.z);
                        if (cursorRotateable && cell.OccupyingStructure.Count > 0) {
                            position = cell.OccupyingStructure[cell.OccupyingStructure.Count - 1].transform.position;
                        }
                        Vector3 originalSize = streamerEffectQuery.sizes[criteriaIndex];
                        float rotation = 0;
                        if (cursorRotateable) {
                            rotation = cursorObject.transform.GetChild(0).eulerAngles.y;
                        }
                        Vector3 size = new Vector3();
                        List<int> corner = Util.GetCornerOfBuilding(position, originalSize, rotation, ref size);
                        List<int> direction = new List<int>() { 1, 1 };

                        if (streamerEffectQuery.IsDragable() && leftMouseDownCell[0] >= 0 && leftMouseDownCell[1] >= 0) {
                            corner = new List<int>() { leftMouseDownCell[0], leftMouseDownCell[1] };
                            size = new Vector3(Mathf.Abs(cell.x - leftMouseDownCell[0]) + 1, 0, Mathf.Abs(cell.z - leftMouseDownCell[1]) + 1);
                            if (size.x != 1) {
                                direction[0] = Mathf.Abs(cell.x - leftMouseDownCell[0]) / (cell.x - leftMouseDownCell[0]);
                            }
                            if (size.z != 1) {
                                direction[1] = Mathf.Abs(cell.z - leftMouseDownCell[1]) / (cell.z - leftMouseDownCell[1]);
                            }
                        }
                        int landmassGold = World.GetLandmassOwner(World.inst.GetCellData(streamerEffectQuery.witchHut.transform.position).landMassIdx).Gold;
                        int cost = 0;
                        List<Cell> validCells = new List<Cell>();
                        int validCellsCount = 0;
                        Dictionary<int, Dictionary<int, int>> elevationCells = new Dictionary<int, Dictionary<int, int>>();
                        neighbourIdxs.Clear();

                        Dictionary<int, List<int>> invalidUnitcells = new Dictionary<int, List<int>>();
                        System.Collections.ICollection units = (System.Collections.ICollection)unitsInfo.GetValue(OrdersManager.inst);
                        foreach (object unit in units) {
                            Vector3 unitPos = ((IMoveableUnit)unit).GetPos();
                            List<int> adjustedPos = new List<int>() {
                                Mathf.FloorToInt(unitPos.x),
                                Mathf.FloorToInt(unitPos.z),
                            };
                            if (!invalidUnitcells.ContainsKey(adjustedPos[0])) {
                                invalidUnitcells.Add(adjustedPos[0], new List<int>());
                            }
                            if (!invalidUnitcells[adjustedPos[0]].Contains(adjustedPos[1])) {
                                invalidUnitcells[adjustedPos[0]].Add(adjustedPos[1]);
                            }
                        }
                        if (streamerEffectQuery.criterias[criteriaIndex].Contains("noFish")) {
                            //List<FishSystem.Fish> fish = new List<FishSystem.Fish>();
                            //System.Collections.ICollection fishCollection = (System.Collections.ICollection)fishInfo.GetValue(FishSystem.inst);
                            //foreach (object fishObject in fishCollection) {
                            //    fish.Add((FishSystem.Fish)fishObject);
                            //}
                        }

                        Dictionary<int, Dictionary<int, List<int>>> neighbouringCells = new Dictionary<int, Dictionary<int, List<int>>>();

                        for (int x = 0; x < size.x; x++) {
                            for (int z = 0; z < size.z; z++) {
                                if (cellValidNew && landmassGold >= cost + streamerEffectQuery.spellData.cost && (streamerEffectQuery.IsDragable() == false || validCells.Count == 0 || streamerEffectQuery.spellData.cooldown == 0)) {
                                    bool thisCellValid = true;
                                    Cell selectionCell = World.inst.GetCellData(new Vector3(corner[0] + x * direction[0], 0, corner[1] + z * direction[1]));
                                    int neighbourLandmassIdx = selectionCell.landMassIdx;
                                    cellInvalidReason = "";
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("fertilityNotMax")) {
                                        if (selectionCell.fertile >= 2 || selectionCell.Type != ResourceType.None) {
                                            thisCellValid = false;
                                            cellInvalidReason = "fertilityNotMax";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("fertilityNotMin")) {
                                        if (selectionCell.fertile <= 0 || selectionCell.Type != ResourceType.None) {
                                            thisCellValid = false;
                                            cellInvalidReason = "fertilityNotMin";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("noUnits")) {
                                        //if (OrdersManager.inst.FindUnitAt(selectionCell.x, selectionCell.z) != null) {
                                        if (invalidUnitcells.ContainsKey(selectionCell.x) && invalidUnitcells[selectionCell.x].Contains(selectionCell.z)) {
                                            thisCellValid = false;
                                            cellInvalidReason = "noUnits";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("dontSplitLandmass")) {
                                        if (selectionCell.Type == ResourceType.Water && selectionCell.deepWater == false) {
                                            Cell[] neighbours = new Cell[8];
                                            World.inst.GetNeighborCellsExtended(selectionCell, ref neighbours);
                                            List<Cell> uniqueLandmasses = new List<Cell>();
                                            int landmassIndex = 0;
                                            int landmassSize = 0;
                                            for (int neighbourIndex = 0; neighbourIndex < neighbours.Length + 1; neighbourIndex++) {
                                                Cell neighbour = neighbours[neighbourIndex % neighbours.Length];
                                                if (neighbour != null && neighbour.landMassIdx != -1 && (!elevationCells.ContainsKey(neighbour.x) || !elevationCells[neighbour.x].ContainsKey(neighbour.z))) {
                                                    if (neighbourIndex == 8) {
                                                        if (landmassSize >= 2 && uniqueLandmasses.Count >= 2) {
                                                            uniqueLandmasses.RemoveAt(uniqueLandmasses.Count - 1);
                                                        }
                                                    } else {
                                                        if ((landmassSize == 0 && neighbourIndex % 2 == 0) || (landmassSize > 0)) {
                                                            landmassSize += 1;
                                                            if (landmassSize == 1) {
                                                                uniqueLandmasses.Add(neighbour);
                                                            }
                                                        }
                                                    }
                                                } else {
                                                    if (landmassSize > 0) {
                                                        landmassIndex += 1;
                                                    }
                                                    landmassSize = 0;
                                                }
                                            }
                                            Log(uniqueLandmasses.Count);
                                            if (uniqueLandmasses.Count > 1) {
                                                Dictionary<int, List<int>> uniqueLandmassesCoordinates = new Dictionary<int, List<int>>();
                                                for (int landmasxIndex = 0; landmasxIndex < uniqueLandmasses.Count - 1; landmasxIndex++) {
                                                    if (!uniqueLandmassesCoordinates.ContainsKey(uniqueLandmasses[landmasxIndex + 1].x)) {
                                                        uniqueLandmassesCoordinates.Add(uniqueLandmasses[landmasxIndex + 1].x, new List<int>());
                                                    }
                                                    uniqueLandmassesCoordinates[uniqueLandmasses[landmasxIndex + 1].x].Add(uniqueLandmasses[landmasxIndex + 1].z);
                                                }

                                                Dictionary<int, List<int>> newLandmassCells = new Dictionary<int, List<int>>();
                                                Dictionary<int, List<int>> cellsToCheckCoordinates = new Dictionary<int, List<int>>();
                                                List<Cell> cellsToCheck = new List<Cell>() { uniqueLandmasses[0] };
                                                bool unreachableCells = true;
                                                bool cellsRemaining = true;
                                                int newLandmassCellsCount = 0;
                                                while (cellsRemaining) {
                                                    List<Cell> cellsToCheckOld = new List<Cell>();
                                                    foreach (Cell oldCell in cellsToCheck) {
                                                        cellsToCheckOld.Add(oldCell);
                                                    }
                                                    cellsToCheck.Clear();
                                                    cellsToCheckCoordinates.Clear();
                                                    foreach (Cell landmassCell in cellsToCheckOld) {
                                                        if (!newLandmassCells.ContainsKey(landmassCell.x)) {
                                                            newLandmassCells.Add(landmassCell.x, new List<int>());
                                                        }
                                                        newLandmassCellsCount += 1;
                                                        newLandmassCells[landmassCell.x].Add(landmassCell.z);
                                                        Cell[] landmassCellNeighbours = new Cell[4];
                                                        World.inst.GetNeighborCells(landmassCell, ref landmassCellNeighbours);
                                                        foreach (Cell landmassCellNeighbour in landmassCellNeighbours) {
                                                            if (landmassCellNeighbour != null) {
                                                                if (landmassCellNeighbour.landMassIdx != -1) {
                                                                    if (!elevationCells.ContainsKey(landmassCellNeighbour.x) || !elevationCells[landmassCellNeighbour.x].ContainsKey(landmassCellNeighbour.z)) {
                                                                        if (selectionCell.x != landmassCellNeighbour.x || selectionCell.z != landmassCellNeighbour.z) {
                                                                            if (!newLandmassCells.ContainsKey(landmassCellNeighbour.x) || !newLandmassCells[landmassCellNeighbour.x].Contains(landmassCellNeighbour.z)) {
                                                                                if (!cellsToCheckCoordinates.ContainsKey(landmassCellNeighbour.x) || !cellsToCheckCoordinates[landmassCellNeighbour.x].Contains(landmassCellNeighbour.z)) {
                                                                                    if (!cellsToCheckCoordinates.ContainsKey(landmassCellNeighbour.x)) {
                                                                                        cellsToCheckCoordinates.Add(landmassCellNeighbour.x, new List<int>());
                                                                                    }
                                                                                    cellsToCheckCoordinates[landmassCellNeighbour.x].Add(landmassCellNeighbour.z);
                                                                                    cellsToCheck.Add(landmassCellNeighbour);

                                                                                    if (uniqueLandmassesCoordinates.ContainsKey(landmassCellNeighbour.x) && uniqueLandmassesCoordinates[landmassCellNeighbour.x].Contains(landmassCellNeighbour.z)) {
                                                                                        uniqueLandmassesCoordinates[landmassCellNeighbour.x].Remove(landmassCellNeighbour.z);
                                                                                        if (uniqueLandmassesCoordinates[landmassCellNeighbour.x].Count == 0) {
                                                                                            uniqueLandmassesCoordinates.Remove(landmassCellNeighbour.x);
                                                                                            if (uniqueLandmassesCoordinates.Keys.Count == 0) {
                                                                                                unreachableCells = false;
                                                                                                cellsRemaining = false;
                                                                                                break;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        if (!unreachableCells) {
                                                            break;
                                                        }
                                                    }
                                                    if (cellsToCheck.Count == 0) {
                                                        cellsRemaining = false;
                                                    }
                                                }
                                                if (unreachableCells) {
                                                    thisCellValid = false;
                                                    cellInvalidReason = "dontSplitLandmass";
                                                }
                                            } else if (uniqueLandmasses.Count == 0) {
                                                thisCellValid = false;
                                                cellInvalidReason = "dontEliminateLandmass";
                                            }
                                        }
                                    }
                                    
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("dontJoinLandmass")) {
                                        if (selectionCell.landMassIdx == -1) {
                                            Cell[] neighbours = new Cell[4];
                                            World.inst.GetNeighborCells(selectionCell, ref neighbours);
                                            List<int> uniqueLandmasses = new List<int>();
                                            foreach (Cell neighbour in neighbours) {
                                                if (neighbour != null) {
                                                    int neighbourLandmass = neighbour.landMassIdx;
                                                    if (elevationCells.ContainsKey(neighbour.x) && elevationCells[neighbour.x].ContainsKey(neighbour.z)) {
                                                        neighbourLandmass = elevationCells[neighbour.x][neighbour.z];
                                                    }
                                                    if (neighbourLandmass != -1 && !uniqueLandmasses.Contains(neighbourLandmass)) {
                                                        uniqueLandmasses.Add(neighbourLandmass);
                                                    }
                                                }
                                            }
                                            if (uniqueLandmasses.Count == 0) {
                                                thisCellValid = false;
                                                cellInvalidReason = "existingLandmass";
                                            } else if (uniqueLandmasses.Count >= 2) {
                                                thisCellValid = false;
                                                cellInvalidReason = "dontJoinLandmass";
                                            } else {
                                                if (!elevationCells.ContainsKey(selectionCell.x)) {
                                                    elevationCells.Add(selectionCell.x, new Dictionary<int, int>());
                                                }
                                                elevationCells[selectionCell.x].Add(selectionCell.z, uniqueLandmasses[0]);
                                                neighbourLandmassIdx = uniqueLandmasses[0];
                                            }
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("elevationNotMax")) {
                                        if (selectionCell.Type != ResourceType.Water) {
                                            thisCellValid = false;
                                            cellInvalidReason = "elevationNotMax";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("elevationNotMin")) {
                                        if (selectionCell.Type == ResourceType.Water && selectionCell.deepWater) {
                                            thisCellValid = false;
                                            cellInvalidReason = "elevationNotMin";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("playerLandmass")) {
                                        if (!Player.inst.LandMassIsAPlayerLandMass(selectionCell.landMassIdx) || !Player.inst.AnyBuildingsOnLandMass(selectionCell.landMassIdx)) {
                                            thisCellValid = false;
                                            cellInvalidReason = "playerLandmass";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("noResourseLand")) {
                                        if (!(new List<ResourceType>() { ResourceType.None }.Contains(selectionCell.Type))) {
                                            thisCellValid = false;
                                            if (selectionCell.Type == ResourceType.Water) {
                                                cellInvalidReason = "noWater";
                                            } else {
                                                cellInvalidReason = "noResource";
                                            }
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("noResource")) {
                                        if (!(new List<ResourceType>() { ResourceType.None, ResourceType.Water }.Contains(selectionCell.Type))) {
                                            thisCellValid = false;
                                            cellInvalidReason = "noResource";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("noStructure")) {
                                        if (selectionCell.OccupyingStructure.Count != 0 ||
                                            selectionCell.SubStructure.Count != 0) {
                                            if (selectionCell.OccupyingStructure.Count == 1 && cells.Count > 0 && cells[0][0].OccupyingStructure.Count == 1 && selectionCell.OccupyingStructure[0].guid == cells[0][0].OccupyingStructure[0].guid) {

                                            } else {
                                                thisCellValid = false;
                                                cellInvalidReason = "noStructure";
                                            }
                                        } else {
                                            foreach (int treeId in selectionCell.TreeIds) {
                                                if (treeId != -1) {
                                                    thisCellValid = false;
                                                    cellInvalidReason = "noResource";
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("deepWater")) {
                                        if (selectionCell.Type != ResourceType.Water || selectionCell.deepWater == false) {
                                            thisCellValid = false;
                                            cellInvalidReason = "deepWater";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("noFish")) {
                                        if (FishSystem.inst.fishCells[selectionCell.z * World.inst.GridWidth + selectionCell.x].fish.Count > 0) {
                                            thisCellValid = false;
                                            cellInvalidReason = "noFish";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("fishSurvive")) {
                                        if (FishSystem.inst.fishCells[selectionCell.z * World.inst.GridWidth + selectionCell.x].baseProbability < 0.5f) {
                                            thisCellValid = false;
                                            cellInvalidReason = "fishSurvive";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("witch")) {
                                        if (selectionCell.Type != ResourceType.WitchHut) {
                                            thisCellValid = false;
                                            cellInvalidReason = "witch";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("stone")) {
                                        if (selectionCell.Type != ResourceType.Stone) {
                                            thisCellValid = false;
                                            cellInvalidReason = "stone";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("iron")) {
                                        if (selectionCell.Type != ResourceType.IronDeposit) {
                                            thisCellValid = false;
                                            cellInvalidReason = "iron";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("rock")) {
                                        if (new List<ResourceType>() { ResourceType.UnusableStone, ResourceType.Stone, ResourceType.IronDeposit }.Contains(selectionCell.Type) == false) {
                                            thisCellValid = false;
                                            cellInvalidReason = "rock";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("keep")) {
                                        if (!(selectionCell.OccupyingStructure.Count == 1 && selectionCell.OccupyingStructure[0].UniqueName == "keep")) {
                                            thisCellValid = false;
                                            cellInvalidReason = "keep";
                                        }
                                    }
                                    if (streamerEffectQuery.criterias[criteriaIndex].Contains("sameLandmass")) {
                                        if (selectionCell.landMassIdx != previousLandmassIdx) {
                                            thisCellValid = false;
                                            cellInvalidReason = "sameLandmass";
                                        }
                                    }
                                    if (selectionCell.x < 1 || selectionCell.x > World.inst.GridWidth - 2 || selectionCell.z < 1 || selectionCell.z > World.inst.GridHeight - 2) {
                                        thisCellValid = false;
                                        cellInvalidReason = "outOfBounds";
                                    }

                                    if (!thisCellValid && !streamerEffectQuery.IsDragable()) {
                                        cellValidNew = false;
                                    }
                                    if (thisCellValid) {
                                        if (streamerEffectQuery.criterias[criteriaIndex].Contains("dontSplitLandmass")) {
                                            if (selectionCell.Type == ResourceType.Water && selectionCell.deepWater == false) {
                                                if (!elevationCells.ContainsKey(selectionCell.x)) {
                                                    elevationCells.Add(selectionCell.x, new Dictionary<int, int>());
                                                }
                                                elevationCells[selectionCell.x].Add(selectionCell.z, 0);
                                            }
                                        }
                                        validCells.Add(selectionCell);
                                        neighbourIdxs.Add(neighbourLandmassIdx);
                                        cost += streamerEffectQuery.spellData.cost;
                                        validCellsCount += 1;
                                    }
                                }
                            }
                        }
                        resourceAmount.Set(FreeResourceType.Gold, cost);
                        if (streamerEffectQuery.IsDragable()) {
                            cellValidNew = validCells.Count > 0;
                        }
                        if (cellValid) {
                            List<Cell> previousCellsAdjusted = new List<Cell>();
                            foreach (Cell previousCellCell in previousCells) {
                                bool cellPersists = false;
                                foreach (Cell validCellCell in validCells) {
                                    if (previousCellCell.x == validCellCell.x && previousCellCell.z == validCellCell.z) {
                                        cellPersists = true;
                                        break;
                                    }
                                }
                                if (!cellPersists) {
                                    previousCellsAdjusted.Add(previousCellCell);
                                }
                            }
                            streamerEffectQuery.UpdateDisplay(previousCellsAdjusted);
                        }
                        previousCell = newCell;
                        previousCells = validCells;
                        if (cellValidNew && cursorObject == null) {
                            cells.Add(validCells);
                            streamerEffectQuery.UpdateDataAndDisplay(cells, false);
                            cells.RemoveAt(cells.Count - 1);
                        }
                        
                        if (cursorObject != null) {
                            cursorObject.transform.position = GameUI.inst.GridPointerIntersection();
                        }
                        if (cellValidNew && cursorObject == null) {
                            streamerEffectQuery.RollbackData(validCells);
                        }

                        if (cellValidNew && !streamerEffectQuery.IsDragable() && cell.OccupyingStructure.Count > 0 && cells.Count == 0) {
                            Building building = cell.OccupyingStructure[cell.OccupyingStructure.Count - 1];
                            nullBuilding.transform.position = building.transform.position;
                            nullBuilding.size = building.size;
                            nullBuilding.transform.GetChild(0).localPosition = new Vector3(nullBuilding.size.x / 2f, 0, nullBuilding.size.z / 2f);
                            nullBuilding.transform.GetChild(0).eulerAngles = new Vector3(0, 0, 0);
                            GameUI.inst.RotateBuilding(nullBuilding, building.transform.GetChild(0).eulerAngles.y);
                        } else {
                            if (streamerEffectQuery.IsDragable() && leftMouseDownCell[0] >= 0 && leftMouseDownCell[1] >= 0) {
                                nullBuilding.transform.position = new Vector3(corner[0], 0, corner[1]);
                                nullBuilding.size = size;
                            } else {
                                nullBuilding.transform.position = new Vector3(cell.x, 0, cell.z);
                                nullBuilding.size = streamerEffectQuery.sizes[criteriaIndex];
                            }
                            
                            if (cursorRotateable) {
                                nullBuilding.transform.GetChild(0).rotation = cursorObject.transform.GetChild(0).rotation;
                                nullBuilding.transform.GetChild(0).localPosition = cursorObject.transform.GetChild(0).localPosition;
                            } else {
                                nullBuilding.transform.GetChild(0).eulerAngles = new Vector3(0, 0, 0);
                                Vector3 offset = new Vector3(0, 0, 0);
                                if (direction[0] == -1) {
                                    offset = new Vector3(1, offset.y, offset.z);
                                }
                                if (direction[1] == -1) {
                                    offset = new Vector3(offset.x, offset.y, 1);
                                }
                                nullBuilding.transform.GetChild(0).localPosition = new Vector3(nullBuilding.size.x / 2f * direction[0] + offset.x, 0, nullBuilding.size.z / 2f * direction[1] + offset.z);
                            }
                        }

                        /*
                        GameUI.inst.SelectCell(cell, true, false);
                        if (cellValidNew) {
                            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
                            List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
                            if (cell.OccupyingStructure.Count > 0) {
                                GameObject model = cell.OccupyingStructure[cell.OccupyingStructure.Count - 1].transform.GetChild(0).gameObject;
                                AppendRenderers(model, ref meshRenderers, ref skinnedMeshRenderers);
                                hoverHighlighter = ObjectHighlighter.SetupOutlines(null, meshRenderers, skinnedMeshRenderers);
                            } else if (cell.Models != null && cell.Models.Count > 0) {
                                foreach (GameObject model in cell.Models) {
                                    AppendRenderers(model, ref meshRenderers, ref skinnedMeshRenderers);
                                }
                                hoverHighlighter = ObjectHighlighter.SetupOutlines(null, meshRenderers, skinnedMeshRenderers);
                            }
                        } else {
                            hoverHighlighter = null;
                        }
                        */
                    } else {
                        cellValidNew = cellValid;
                    }
                } else {
                    cellValidNew = cellValid;
                }
                if (cellIsACell && cellValidNew != cellValid) {
                    cellValid = cellValidNew;
                    if (cellValid) {
                        cursorColour = "blue";
                    } else {
                        cursorColour = "red";
                    }
                }
            }

            static public void DoPrimaryClick() {
                //LogCellData();
                if (cellValid) {
                    cells.Add(previousCells);
                    previousCells = new List<Cell>();
                    previousLandmassIdx = cells[cells.Count - 1][0].landMassIdx;
                    streamerEffectQuery.OnClick(cells[cells.Count - 1], criteriaIndex);
                    QueryNext();
                } else {
                    if (streamerEffectQuery.IsDragable()) {
                        previousCell = new List<int>() { nullPos[0], nullPos[2] };
                        leftMouseDownCell = new List<int>() { nullPos[0], nullPos[2] };
                        nullBuilding.size = streamerEffectQuery.sizes[criteriaIndex];
                        nullBuilding.transform.GetChild(0).localPosition = new Vector3(nullBuilding.size.x / 2f, 0, nullBuilding.size.z / 2f);
                        EvaluateCell();
                    }
                }
            }

            static public void QueryNext() {
                if (criteriaIndex < streamerEffectQuery.criterias.Count - 1) {
                    criteriaIndex += 1;
                    ResetTracking();
                } else {
                    int activationCount = 0;
                    if (streamerEffectQuery.draggable) {
                        activationCount = cells[cells.Count - 1].Count;
                    }
                    if (RegisterSpells.TryActivate(streamerEffectQuery.witchHut, streamerEffectQuery.spellIndex, activationCount)) {
                        int num = 0;
                        WitchHut.SpellData outSpell = null;
                        num = streamerEffectQuery.witchHut.TryActivate((WitchHut.Spells)streamerEffectQuery.spellIndex, out outSpell);
                        if (num == 0) {
                            if (streamerEffectQuery.draggable) {
                                World.GetLandmassOwner(World.inst.GetCellData(streamerEffectQuery.witchHut.transform.position).landMassIdx).Gold -= (cells[cells.Count - 1].Count - 1) * streamerEffectQuery.spellData.cost;
                            }
                            spellUsed = true;
                            streamerEffectQuery.UpdateDataAndDisplay(cells);
                            if (RegisterSpells.TryActivate(streamerEffectQuery.witchHut, streamerEffectQuery.spellIndex)) {
                                criteriaIndex = 0;
                                cells.Clear();
                                previousLandmassIdx = -1;
                                ResetTracking();
                            } else {
                                StopSpelling();
                            }
                        }
                    }
                }
            }

            static public void ResetTracking() {
                SetCursorPrefab();
                previousCell = new List<int>() { nullPos[0], nullPos[2] };
                leftMouseDownCell = new List<int>() { nullPos[0], nullPos[2] };
                nullBuilding.size = streamerEffectQuery.sizes[criteriaIndex];
                EvaluateCell();
                SfxSystem.PlayUiSelect();
            }

            static public void StopSpelling(bool forceQuit = false) {
                if (criteriaIndex != -1) {
                    criteriaIndex = -1;
                    leftMouseDownCell = new List<int>() { nullPos[0], nullPos[2] };

                    if (!forceQuit) {
                        SpeedControlUI.inst.SetSpeed(TerraformWitchSpells.speedBackup);
                        if (spellUsed) {
                            streamerEffectQuery.ShowBanner();
                        } else {
                            SfxSystem.PlayUiCancel();
                        }
                        
                        TerrainGen.inst.UpdateTextures();
                        World.inst.CombineStone();
                        foreach (List<Cell> cellList in cells) {
                            foreach (Cell cell in cellList) {
                                for (int childIndex = 0; childIndex < World.inst.caveContainer.transform.childCount; childIndex++) {
                                    Vector3 position = World.inst.caveContainer.transform.GetChild(childIndex).position;
                                    if (Mathf.RoundToInt(position.x) == cell.x && Mathf.RoundToInt(position.z) == cell.z) {
                                        World.inst.caveContainer.transform.GetChild(childIndex).gameObject.SetActive(true);
                                    }
                                }
                                if (cell.OccupyingStructure.Count > 0) {
                                    cell.OccupyingStructure[cell.OccupyingStructure.Count - 1].transform.GetChild(0).gameObject.SetActive(true);
                                }
                            }
                        }
                        streamerEffectQuery.UpdateDisplay(previousCells);
                    }
                    spellUsed = false;
                    SpeedControlUI.inst.ButtonsInteractable(true);
                    streamerEffectQuery = null;
                    cells.Clear();
                    SetCursorPrefab(true);
                    if (GameUI.inst.CellSelector != null) {
                        GameUI.inst.SelectCell(null, true, false);
                    }
                }
            }








            static public void SetCursorPrefab(bool setAsNull = false) {
                if (cursorObject != null) {
                    highlighter.Release();
                    DestroyImmediate(cursorObject);
                    cursorRotateable = false;
                }
                if (!setAsNull) {
                    if (cells.Count > 0) {
                        Cell latestCell = cells[cells.Count - 1][0];
                        if (latestCell.OccupyingStructure.Count > 0 || latestCell.Models != null || latestCell.Type == ResourceType.WitchHut) {
                            cursorObject = new GameObject();
                            cursorObject.transform.parent = Player.inst.buildingContainer.transform;
                            cursorObject.transform.position = new Vector3(0, 0, 0);

                            if (latestCell.Type == ResourceType.WitchHut) {
                                GameObject witchHut = GameObject.Instantiate(World.inst.witchHutPrefab);
                                witchHut.transform.SetParent(cursorObject.transform);
                                witchHut.transform.position = new Vector3(0, 0, 0);
                            } else if (latestCell.OccupyingStructure.Count > 0) {
                                cursorRotateable = true;
                                Building building = latestCell.OccupyingStructure[latestCell.OccupyingStructure.Count - 1];
                                GameObject newModel = GameObject.Instantiate(building.transform.GetChild(0).gameObject, cursorObject.transform);
                                newModel.SetActive(true);
                                newModel.transform.position = new Vector3(streamerEffectQuery.sizes[criteriaIndex].x / 2f, 0, streamerEffectQuery.sizes[criteriaIndex].z / 2f);
                                newModel.transform.eulerAngles = new Vector3(0, 0, 0);
                                RotateCursor(building.transform.GetChild(0).eulerAngles.y);
                            } else {
                                foreach (GameObject model in latestCell.Models) {
                                    GameObject newModel = GameObject.Instantiate(model, cursorObject.transform);
                                    newModel.transform.localPosition = model.transform.localPosition - new Vector3(latestCell.x, 0, latestCell.z);
                                    newModel.transform.localScale = model.transform.localScale;
                                    newModel.SetActive(true);
                                }
                            }

                            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
                            List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
                            AppendRenderers(cursorObject, ref meshRenderers, ref skinnedMeshRenderers);
                            highlighter = ObjectHighlighter.SetupOutlines(cursorObject, meshRenderers, skinnedMeshRenderers);
                        }
                    }
                }
            }

            static public void AppendRenderers(GameObject gameObject, ref List<MeshRenderer> meshRenderers, ref List<SkinnedMeshRenderer> skinnedMeshRenderers) {
                MeshRenderer[] meshRenderersArray = gameObject.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer meshRenderer in meshRenderersArray) {
                    meshRenderers.Add(meshRenderer);
                }
                SkinnedMeshRenderer[] skinnedMeshRenderersArray = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer meshRenderer in skinnedMeshRenderersArray) {
                    skinnedMeshRenderers.Add(meshRenderer);
                }
            }

            static public void SetCellModel(Cell cell) {
                if (cursorObject != null) {
                    if (cell.Models == null) {
                        cell.Models = new List<GameObject>();
                    }
                    int childCountOld = cursorObject.transform.childCount;
                    for (int childIndex = 0; childIndex < childCountOld; childIndex++) {
                        cell.Models.Add(cursorObject.transform.GetChild(0).gameObject);
                        if (new List<ResourceType>() { ResourceType.UnusableStone, ResourceType.Stone, ResourceType.IronDeposit }.Contains(cell.Type)) {
                            cursorObject.transform.GetChild(0).SetParent(World.inst.resourceContainer.transform);
                        } else {
                            cursorObject.transform.GetChild(0).SetParent(World.inst.caveContainer.transform);
                        }
                    }
                }
            }

            static public void LogCellData() {
                Cell cell = World.inst.GetCellData(GameUI.inst.GridPointerIntersection());
                foreach (Building occupyingStructure in cell.OccupyingStructure) {
                    Log(occupyingStructure);
                }
                Log("-");
                foreach (Building occupyingStructure in cell.SubStructure) {
                    Log(occupyingStructure.UniqueName);
                }
                Log("-");
                Log(cell.Type);
                Log(cell.TreeAmount);
                Log(cell.fertile);
                Log(cell.saltWater);
                Log(cell.deepWater);
                Log("-------------------------------------------------------------------");
            }








            // Display the tooltip for spell query
            [HarmonyPatch(typeof(BuildInfoUI))]
            [HarmonyPatch("Update")]
            public static class BuildInfoUIUpdate {
                static void Postfix(BuildInfoUI __instance) {
                    bool selectionInvalid = false;
                    if (criteriaIndex != -1) {
                        if (cellValid == false) {
                            if (cellInvalidReason != "") {
                                selectionInvalid = true;
                            }
                        }
                    }
                    
                    if (criteriaIndex != -1) {
                        bool showCost = streamerEffectQuery.IsDragable() && previousCells.Count > 1 && streamerEffectQuery.spellData.cost > 0;
                        __instance.buildDescParent.gameObject.SetActive(cursorRotateable);
                        __instance.buildRuleParent.gameObject.SetActive(selectionInvalid);
                        __instance.buildCostParent.gameObject.SetActive(showCost);
                        __instance.costTextUI.SetText(resourceAmount.ToString(" "));
                        __instance.ruleTextUI.text = Translation.GetTranslation(cellInvalidReason);
                    }
                }
            }

            // Fix the colour of the border
            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("UpdateCellSelector")]
            public static class GameUIUpdateCellSelector {
                static void Postfix() {
                    if (criteriaIndex != -1) {
                        GameUI.inst.CellSelector.GetComponent<Selector>().SetColor(TerraformWitchSpells.colours[cursorColour]);
                        Camera.main.GetComponent<OutlineEffect>().lineColor1 = TerraformWitchSpells.colours[cursorColour];
                    }
                }
            }

            // Allow cell selection to run as intentended in UpdateCellSelector
            [HarmonyPatch(typeof(PlacementMode))]
            [HarmonyPatch("IsPlacing")]
            public static class PlacementModeIsPlacing {
                static void Postfix(ref bool __result) {
                    if (criteriaIndex != -1) {
                        __result = true;
                    }
                }
            }

            // Get the right border set in UpdateCellSelector and prevent cell tooltip from being opened
            [HarmonyPatch(typeof(PlacementMode))]
            [HarmonyPatch("GetHoverBuilding")]
            public static class PlacementModeGetHoverBuilding {
                static void Postfix(ref Building __result) {
                    if (criteriaIndex != -1) {
                        __result = nullBuilding;
                    }
                }
            }

            // Stop spelling when loading a save
            [HarmonyPatch(typeof(LoadSave))]
            [HarmonyPatch("LoadAtPath")]
            public static class LoadSaveLoadAtPath {
                static void Postfix() {
                    StopSpelling(true);
                }
            }

            // Stop spelling when opening the pause menu
            [HarmonyPatch(typeof(PlayingMode))]
            [HarmonyPatch("OnClickedMenu")]
            public static class PlayingModeOnClickMenu {
                static void Prefix() {
                    StopSpelling();
                }
            }

            // Stop spelling when returning to the main menu
            [HarmonyPatch(typeof(MainMenuMode))]
            [HarmonyPatch("BackToMainMenu")]
            public static class MainMenuModeBackToMainMenu {
                static void Postfix() {
                    StopSpelling(true);
                }
            }

            static private bool DontRunIfSpelling() {
                if (criteriaIndex != -1) {
                    return false;
                }
                return true;
            }

            // Prevent cursor mode buttons from working
            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("SetCursorMode")]
            public static class GameUISetCursorMode {
                static bool Prefix() {
                    return DontRunIfSpelling();
                }
            }

            // Prevent building tab buttons from working
            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("OnShowBuildTabClicked")]
            public static class GameUIOnShowBuildTabClicked {
                static bool Prefix() {
                    if (criteriaIndex != -1) {
                        buildTabClicked = true;
                    }
                    return DontRunIfSpelling();
                }
            }

            // Prevent standard click functionality from happening when spell is active
            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("Update")]
            public static class GameUIUpdate {
                static bool Prefix() {
                    return DontRunIfSpelling();
                }
            }

            // Prevent border from rotating
            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("RotateBuilding")]
            [HarmonyPatch(new System.Type[] { typeof(Building), typeof(bool) })]
            public static class GameUIRotateBuilding {
                static bool Prefix() {
                    if (criteriaIndex != -1) {
                        if (cursorRotateable) {
                            RotateCursor(90);
                            return false;
                        }
                    }
                    return true;
                }
            }

            public static class KeyboardControlUpdatePlaymodeKeys {
                static bool Prefix() {
                    return DontRunIfSpelling();
                }
            }

            // Prevent camera zooming when in witch menu
            [HarmonyPatch(typeof(Cam))]
            [HarmonyPatch("UpdateMouseZoom")]
            public static class CamUpdateMouseZoom {
                static bool Prefix() {
                    if (GameUI.inst.witchUI.spellList.activeInHierarchy) {
                        return false;
                    }
                    return true;
                }
            }

            static private List<Code> treeGrowthUpdateCode = new List<Code>() {
                new Code(OpCodes.Ldsfld, "World inst"),
                new Code(OpCodes.Callvirt, "Int32 get_GridWidth()"),
                new Code(OpCodes.Ldsfld, "World inst"),
                new Code(OpCodes.Callvirt, "Int32 get_GridHeight()"),
                new Code(OpCodes.Mul, "null"),
                new Code(OpCodes.Conv_R4, "null"),
                new Code(OpCodes.Stloc_1, "null"),
                new Code(OpCodes.Ldarg_0, "null"),
                new Code(OpCodes.Dup, "null"),
                new Code(OpCodes.Ldfld, "System.Single cellsPerTick"),
                new Code(OpCodes.Ldloc_1, "null"),
                new Code(OpCodes.Ldarg_0, "null"),
                new Code(OpCodes.Ldfld, "System.Single updateInterval"),
                new Code(OpCodes.Div, "null"),
                new Code(OpCodes.Ldloc_0, "null"),
                new Code(OpCodes.Mul, "null"),
                new Code(OpCodes.Ldc_R4, "1"),
            };

            [HarmonyPatch(typeof(TreeGrowth))]
            [HarmonyPatch("Update")]
            public static class TreeGrowthUpdate {
                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                    List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                    int startIndex = -1;


                    for (int codeIndex = 0; codeIndex < codes.Count - treeGrowthUpdateCode.Count + 1; codeIndex++) {
                        for (int keyIndex = 0; keyIndex < treeGrowthUpdateCode.Count; keyIndex++) {
                            if (codes[codeIndex + keyIndex].opcode == treeGrowthUpdateCode[keyIndex].opCode && NullSafeToString(codes[codeIndex + keyIndex].operand) == treeGrowthUpdateCode[keyIndex].operand) {
                                if (keyIndex == treeGrowthUpdateCode.Count - 1) {
                                    startIndex = codeIndex + keyIndex;
                                    break;
                                }
                            } else {
                                if (keyIndex > 1) {
                                    //Log(keyIndex);
                                }
                                break;
                            }
                        }
                        //Log(codes[codeIndex].opcode.ToString() + " " + NullSafeToString(codes[codeIndex].operand));
                    }
                    if (startIndex != -1) {
                        codes.RemoveAt(startIndex);
                        codes.Insert(startIndex, new CodeInstruction(OpCodes.Ldc_R4, 0));
                    }
                    return codes.AsEnumerable();
                }
            }

            class Code {
                public OpCode opCode;
                public string operand;

                public Code(OpCode givenOpCode, string givenOperand) {
                    opCode = givenOpCode;
                    operand = givenOperand;
                }
            }
        }
    }
}