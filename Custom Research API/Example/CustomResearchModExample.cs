using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using I2.Loc;
using Phedg1Studios.Shared;

namespace Example {
    public class CustomResearchAPIExample : MonoBehaviour {
        static public KCModHelper helper;

        // A reference to the UpgradeType that is registered, for use later
        // Pick a unique number, if multilple mods add research with the same number
        // Then it may cause problems
        static private Player.UpgradeType cheapSpells = (Player.UpgradeType)99;

        void Preload(KCModHelper helper) {
            CustomResearchAPIExample.helper = helper;
            var harmony = HarmonyInstance.Create("harmony");
            // The line below is necessary but commented out
            // because having it appear multiple times in a single project causes issues
            //harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Uncomment the line below to have this example research added to the game
            //SetResearch();
        }


        // Here is where the research definitions are set in RegisterSpells
        // A researchTime of 150 is equal to one standard year
        private void SetResearch() {
            RegisterResearch.OnGetResearchTranslation.AddListener(GetResearchTranslation);
            RegisterResearch.researchDefs.Add(new GreatLibrary.ResearchDef() {
                upgrade = cheapSpells,
                goldCost = 100,
                researchTime = 150,
            });
        }



        // Here is where the research's effect is implemented
        // Setup hooks that query if the player has the upgrade
        // and implement your changes if true
        [HarmonyPatch(typeof(WitchHut))]
        [HarmonyPatch("GetSpellCost")]
        public static class WitchHutGetSpellCost {
            [HarmonyPriority(200)]
            static void Postfix(ref int __result) {
                if (Player.inst.HasUpgrade(cheapSpells)) {
                    __result = Mathf.Max(0, Mathf.FloorToInt(__result / 10f));
                }
            }
        }



        // This is that was added to the RegisterResearch.OnGetResearchTranslation event
        static public void GetResearchTranslation(Player.UpgradeType upgradeType) {
            if (upgradeTranslationKeys.ContainsKey(upgradeType)) {
                RegisterResearch.researchTranslation = GetTranslation(upgradeTranslationKeys[upgradeType]);
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

        // You will need to provide your own translations for each research
        // This is an example on how translations can work easily
        static private Dictionary<string, Dictionary<string, string>> translation = new Dictionary<string, Dictionary<string, string>>() {
            { "ModName_CheapSpells", new Dictionary<string, string>() {
                { "en", "Bribe The Witch - Spells are 90% cheaper." },
            } },
        };

        // This key to convert an UpgradeType to translation name was made
        // so that other translations that may be necessary can all use the same system 
        static private Dictionary<Player.UpgradeType, string> upgradeTranslationKeys = new Dictionary<Player.UpgradeType, string>() {
            { cheapSpells, "ModName_CheapSpells" }
        };
    }
}