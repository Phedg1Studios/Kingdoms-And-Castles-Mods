using Harmony;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using I2.Loc;
using Phedg1Studios.Shared;

namespace Example {
    namespace CustomSpellsAPIExample {
        public class CustomSpellsModExample : MonoBehaviour {
            static public KCModHelper helper;
            // Initialize systems
            void Preload(KCModHelper helper) {
                CustomSpellsModExample.helper = helper;
                var harmony = HarmonyInstance.Create("harmony");
                // The line below is necessary but commented out
                // because having it appear multiple times in a single project causes issues
                //harmony.PatchAll(Assembly.GetExecutingAssembly());

                // Uncomment the line below to have this example spell added to the game
                //SetSpells();
            }

            public void SetSpells() {
                RegisterSpells.spellData.Add(new SpellDataCustom() {
                    cost = 0,
                    cooldown = 0,
                    spellImpl = typeof(StreamerEffect_Example),
                    color = TwitchBonus.buffColor,
                    locKey = "Votable" + StreamerEffect_Example.GetTermSegment() + "Title",
                    relationship = WitchHut.Relationship.Enemies,
                    scaleCost = false,
                });
            }

            // Introduce custom translations
            [HarmonyPatch(typeof(LocalizationManager))]
            [HarmonyPatch("GetTranslation")]
            public static class LocalizationManagerGetTranslation {
                static void Postfix(string Term, ref string __result) {
                    __result = GetTranslation(Term, __result);
                }
            }

            static public string GetTranslation(string givenKey, string givenTranslation = "") {
                if (translation.ContainsKey(givenKey)) {
                    string languageCode = LocalizationManager.CurrentLanguageCode;
                    if (translation[givenKey].ContainsKey(languageCode)) {
                        return translation[givenKey][languageCode];
                    } else {
                        return translation[givenKey]["en"];
                    }
                }
                return givenTranslation;
            }

            // You will need to provide your own translations for every spell
            // This is an example on how translations can work easily
            // You will need translations for:
            // "Votable" + YourStreamerEffect.GetTermSegment() + "Title"
            // "Votable" + YourStreamerEffect.GetTermSegment() + "Description"
            static private Dictionary<string, Dictionary<string, string>> translation = new Dictionary<string, Dictionary<string, string>>() {
                { "VotableUniqueNameTitle", new Dictionary<string, string>() {
                    { "en", "Title Text" },
                } },
                { "VotableUniqueNameDescription", new Dictionary<string, string>() {
                    { "en", "The banner message." },
                } },
            };
        }
    }
}
