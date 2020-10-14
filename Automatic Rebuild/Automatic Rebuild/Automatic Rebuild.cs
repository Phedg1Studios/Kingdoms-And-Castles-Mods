using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Zat.Shared.ModMenu.Interactive;
using Zat.Shared.ModMenu.API;

namespace Phedg1Studios {
    namespace AutomaticRebuild {
        public class AutomaticRebuild : MonoBehaviour {
            static public KCModHelper helper;
            private UnityAction<Exception> modMenuException;
            private InteractiveConfiguration<ModMenuSettings> config;
            static private bool doAutomaticRebuild = true;

            void Preload(KCModHelper helper) {
                AutomaticRebuild.helper = helper;
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            /*
            void Update() {
                if (Input.GetKeyDown(KeyCode.F2)) {
                    Cell cell = World.inst.GetCellData(GameUI.inst.GridPointerIntersection());
                    if (cell != null) {
                        for (int buildingIndex = 0; buildingIndex < cell.OccupyingStructure.Count; buildingIndex++) {
                            World.inst.WreckBuilding(cell.OccupyingStructure[buildingIndex]);
                        }
                    }
                }
            }
            */

            static public void RebuildAll() {
                for (int x = 0; x < World.inst.GridWidth; x++) {
                    for (int z = 0; z < World.inst.GridHeight; z++) {
                        Cell cell = World.inst.GetCellData(x, z);
                        if (cell != null) {
                            for (int buildingIndex = 0; buildingIndex < cell.OccupyingStructure.Count; buildingIndex++) {
                                World.inst.AttemptRebuild(cell.OccupyingStructure[buildingIndex]);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(World))]
            [HarmonyPatch("WreckBuilding")]
            public static class WorldWreckBuilding {
                static void Postfix(Building b) {
                    if (doAutomaticRebuild) {
                        Cell cell = World.inst.GetCellData(Mathf.RoundToInt(b.transform.position.x), Mathf.RoundToInt(b.transform.position.z));
                        if (cell != null) {
                            for (int buildingIndex = 0; buildingIndex < cell.OccupyingStructure.Count; buildingIndex++) { 
                                World.inst.AttemptRebuild(cell.OccupyingStructure[buildingIndex]);
                            }
                        }
                    }
                }
            }

            [Mod("Automatic Rebuild", "v1.0", "Phedg1 Studios")]
            public class ModMenuSettings {
                [Setting("Automatic Rebuild", "Whether buildings should automatically rebuild")]
                [Toggle(true, "True")]
                public InteractiveToggleSetting AutomaticRebuild { get; private set; }

                [Setting("Rebuild All", "Rebuild all current rubble")]
                [Button("Press Here")]
                public InteractiveButtonSetting RebuildAll { get; private set; }
            }

            void Start() {
                modMenuException += ModMenuException;
                config = new InteractiveConfiguration<ModMenuSettings>();
                ModSettingsBootstrapper.Register(config.ModConfig, SettingsRegistered, modMenuException);
            }

            void SettingsRegistered(ModSettingsProxy proxy, SettingsEntry[] saved) {
                config.Install(proxy, saved);
                config.Settings.AutomaticRebuild.OnUpdate.AddListener((setting) => {
                    doAutomaticRebuild = setting.toggle.value;
                    config.Settings.AutomaticRebuild.Label = setting.toggle.value.ToString();
                });
                config.Settings.RebuildAll.OnButtonPressed.AddListener(() => {
                    RebuildAll();
                });
                config.Settings.AutomaticRebuild.TriggerUpdate();
            }

            void ModMenuException(Exception exception) {
                Log(exception);
                Log(exception.Message);
                Log(exception.StackTrace);
            }

            static public void Log(object givenObject, bool traceBack = false) {
                if (givenObject == null) {
                    helper.Log("null");
                } else {
                    helper.Log(givenObject.ToString());
                }
                if (traceBack) {
                    helper.Log(StackTraceUtility.ExtractStackTrace());
                }
            }
        }
    }
}
