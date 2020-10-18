using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Zat.Shared.ModMenu.Interactive;
using Zat.Shared.ModMenu.API;

namespace Phedg1Studios {
    namespace NPCHotkeys {
        public class NPCHotkeys : MonoBehaviour {
            static public KCModHelper helper;
            private UnityAction<Exception> modMenuException;
            private InteractiveConfiguration<ModMenuSettings> config;
            static private Building keep;
            static private WitchHut witchHut;
            static private KeyCode advisersHotkey = KeyCode.LeftBracket;
            static private KeyCode witchHotkey = KeyCode.RightBracket;

            void Preload(KCModHelper helper) {
                NPCHotkeys.helper = helper;
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            void Start() {
                modMenuException += ModMenuException;
                config = new InteractiveConfiguration<ModMenuSettings>();
                ModSettingsBootstrapper.Register(config.ModConfig, SettingsRegistered, modMenuException);
            }

            [HarmonyPatch(typeof(WitchHut.WitchHutSaveData))]
            [HarmonyPatch("Unpack")]
            public static class WitchHutSaveDatatUnpack {
                static void Postfix(WitchHut.WitchHutSaveData __instance, ref WitchHut __result) {
                    witchHut = __result;
                }
            }

            void Update() {
                if (Input.GetKeyDown(advisersHotkey)) {
                    OpenAdvisers();
                }
                if (Input.GetKeyDown(witchHotkey)) {
                    OpenWitch();
                }
            }

            static public void OpenWitch() {
                if (witchHut == null) {
                    for (int resourceIndex = 0; resourceIndex < World.inst.caveContainer.transform.childCount; resourceIndex++) {
                        witchHut = World.inst.caveContainer.transform.GetChild(resourceIndex).GetComponent<WitchHut>();
                        if (witchHut != null) {
                            break;
                        }
                    }
                }
                if (witchHut != null) {
                    Cell cell = World.inst.GetCellData(Mathf.RoundToInt(witchHut.transform.position.x), Mathf.RoundToInt(witchHut.transform.position.z));
                    if (cell != null) {
                        GameUI.inst.SelectCell(cell);
                    }
                }
            }

            static public void OpenAdvisers() {
                if (keep == null) {
                    for (int landMassIndex = 0; landMassIndex < Player.inst.PlayerLandmassOwner.ownedLandMasses.Count; landMassIndex++) {
                        ArrayExt<Building> keeps = Player.inst.GetBuildingListForLandMass(Player.inst.PlayerLandmassOwner.ownedLandMasses.data[landMassIndex], World.keepHash);
                        if (keeps.Count > 0) {
                            keep = keeps.data[0];
                            break;
                        }
                    }
                }
                if (keep != null) {
                    GameUI.inst.SelectBuilding(keep);
                }
            }

            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("SelectBuilding")]
            public static class GameUISelectBuilding {
                static void Postfix(Building building) {
                    if (building.uniqueNameHash == World.keepHash) {
                        keep = building;
                    }
                }
            }

            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("OnEnable")]
            public static class WitchUIOnEnable {
                static void Postfix() {
                    Cell cellSelected = GameUI.inst.GetCellSelected();
                    witchHut = World.inst.GetWitchHutAt(cellSelected);
                }
            }

            [Mod("NPC Hotkeys", "v1.0", "Phedg1 Studios")]
            public class ModMenuSettings {
                [Setting("Open Advisers", "Open the advisers interface")]
                [Hotkey(KeyCode.LeftBracket)]
                public InteractiveHotkeySetting OpenAdvisers { get; private set; }

                [Setting("Open Witch", "Open the witch interface")]
                [Hotkey(KeyCode.RightBracket)]
                public InteractiveHotkeySetting OpenWitch { get; private set; }
            }

            void SettingsRegistered(ModSettingsProxy proxy, SettingsEntry[] saved) {
                config.Install(proxy, saved);
                config.Settings.OpenAdvisers.OnUpdate.AddListener((setting) => {
                    advisersHotkey = config.Settings.OpenAdvisers.Key;
                });
                config.Settings.OpenWitch.OnUpdate.AddListener((setting) => {
                    witchHotkey = config.Settings.OpenWitch.Key;
                });
                config.Settings.OpenAdvisers.TriggerUpdate();
                config.Settings.OpenWitch.TriggerUpdate();
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
