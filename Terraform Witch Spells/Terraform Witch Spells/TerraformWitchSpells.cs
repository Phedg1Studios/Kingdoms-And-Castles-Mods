using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Assets;
using Assets.Code;
using static Phedg1Studios.TerraformWitchSpells.Util;
using Phedg1Studios.Shared;
using Zat.Shared.ModMenu.API;
using Zat.Shared.ModMenu.Interactive;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class TerraformWitchSpells : MonoBehaviour {
            static public KCModHelper helper;
            public Util util;
            static public MapEdit mapEdit;
            public QueryForCriteria queryForCriteria;

            static public int speedBackup;
            static public Dictionary<string, UnityEngine.Color> colours = new Dictionary<string, UnityEngine.Color>();

            private InteractiveConfiguration<ModMenuSettings> config;
            private UnityAction<Exception> modMenuException;

            // Initialize systems
            void Preload(KCModHelper helper) {
                TerraformWitchSpells.helper = helper;
                Zat.Shared.Debugging.Helper = helper;
                Application.logMessageReceived += OnLogMessageReceived;

                //Log(LocalizationManager.CurrentLanguageCode);
                util = gameObject.AddComponent<Util>();
                queryForCriteria = gameObject.AddComponent<QueryForCriteria>();
                mapEdit = new MapEdit();

                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                SetSpells();

                Assembly assembly = Assembly.GetAssembly(typeof(Water));
                System.Type type = assembly.GetType("Assets.Code.KeyboardControl");
                MethodInfo methodInfoA = type.GetMethod("UpdatePlaymodeKeys", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo methodInfoB = typeof(QueryForCriteria.KeyboardControlUpdatePlaymodeKeys).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);
                harmony.Patch(methodInfoA.GetBaseDefinition(), new HarmonyMethod(methodInfoB), null, null);
            }

            // Initialize variables
            void SceneLoaded(KCModHelper helper) {
            }

            void Start() {
                modMenuException += ModMenuException;
                config = new InteractiveConfiguration<ModMenuSettings>();
                ModSettingsBootstrapper.Register(config.ModConfig, SettingsRegistered, modMenuException);
            }

            void SettingsRegistered(ModSettingsProxy proxy, SettingsEntry[] saved ) {
                config.Install(proxy, saved);
                ModMenuListeners.AddListeners(config.Settings);
                ModMenuListeners.ForceUpdateSettings(config.Settings);
            }

            void ModMenuException(Exception exception) {
                Log(exception);
                Log(exception.Message);
                Log(exception.StackTrace);
            }

            public void SetSpells() {
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 75,
                    cooldown = 0,
                    spellImpl = typeof(StreamerEffect_IncreaseFertility),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_IncreaseFertility.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.ReallyLIkesYou,
                    scaleCost = false,
                });
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 75,
                    cooldown = 0,
                    spellImpl = typeof(StreamerEffect_DecreaseFertility),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_DecreaseFertility.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.ReallyLIkesYou,
                    scaleCost = false,
                });
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 100,
                    cooldown = 0,
                    spellImpl = typeof(StreamerEffect_IncreaseElevation),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_IncreaseElevation.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.FriendsForever,
                    scaleCost = false,
                });
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 100,
                    cooldown = 0,
                    spellImpl = typeof(StreamerEffect_DecreaseElevation),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_DecreaseElevation.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.FriendsForever,
                    scaleCost = false,
                });
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 500,
                    cooldown = 0,
                    spellImpl = typeof(StreamerEffect_RelocateStone),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_RelocateStone.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.LovesYou,
                    scaleCost = false,
                });
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 500,
                    cooldown = 0,
                    spellImpl = typeof(StreamerEffect_RelocateIron),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_RelocateIron.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.LovesYou,
                    scaleCost = false,
                });
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 2000,
                    cooldown = 100,
                    spellImpl = typeof(StreamerEffect_RelocateWitchHut),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_RelocateWitchHut.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.FriendsForever,
                    scaleCost = false,
                });
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 4000,
                    cooldown = 100,
                    spellImpl = typeof(StreamerEffect_RelocateKeep),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_RelocateKeep.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.FriendsForever,
                    scaleCost = false,
                });
            }



            static public void BackupSpeed() {
                if (SpeedControlUI.inst.pauseButton.isOn) {
                    speedBackup = 0;
                } else if (SpeedControlUI.inst.playButton1.isOn) {
                    speedBackup = 1;
                } else if (SpeedControlUI.inst.playButton2.isOn) {
                    speedBackup = 2;
                } else if (SpeedControlUI.inst.playButton3.isOn) {
                    speedBackup = 3;
                }
            }

            /*
            // Modify loaded save data to be compatible with new custom spells
            [HarmonyPatch(typeof(WitchHut.WitchHutSaveData))]
            [HarmonyPatch("Unpack")]
            public static class WitchHutSaveDatatUnpack {
                static void Postfix(WitchUI __instance, ref WitchHut __result, WitchHut obj) {
                    __result.relationship = WitchHut.Relationship.FriendsForever;
                    __result.status = WitchHut.Status.Finished;
                    __result.yearRemainingToCompleteMission = 0;
                    __result.latestSpell = WitchHut.Spells.NumSpells;
                    __result.lastMissionSuccess = true;
                }
            }
            */

            // Get default colours
            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("Start")]
            public static class GameUIStart {
                static void Postfix(GameUI __instance) {
                    colours.Clear();
                    System.Reflection.FieldInfo fieldInfo = typeof(GameUI).GetField("blue", BindingFlags.NonPublic | BindingFlags.Instance);
                    colours.Add("blue", (UnityEngine.Color)fieldInfo.GetValue(__instance));
                    fieldInfo = typeof(GameUI).GetField("red", BindingFlags.NonPublic | BindingFlags.Instance);
                    colours.Add("red", (UnityEngine.Color)fieldInfo.GetValue(__instance));
                    colours.Add("white", UnityEngine.Color.white);
                }
            }
        }
    }
}
