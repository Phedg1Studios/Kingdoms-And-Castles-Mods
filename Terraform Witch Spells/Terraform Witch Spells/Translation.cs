using Harmony;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using static Phedg1Studios.TerraformWitchSpells.Util;
using Phedg1Studios.Shared;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        public class Translation : MonoBehaviour {
            static private Dictionary<string, Dictionary<string, string>> translation = new Dictionary<string, Dictionary<string, string>>() {
                { "VotableIncreaseFertilityTitle", new Dictionary<string, string>() {
                    { "en", "Increase Soil Fertility" },
                } },
                { "VotableIncreaseFertilityDescription", new Dictionary<string, string>() {
                    { "en", "The land has grown more fertile." },
                } },
                { "VotableDecreaseFertilityTitle", new Dictionary<string, string>() {
                    { "en", "Decrease Soil Fertility" },
                } },
                { "VotableDecreaseFertilityDescription", new Dictionary<string, string>() {
                    { "en", "The land has grown more barren." },
                } },
                { "VotableRelocateStoneTitle", new Dictionary<string, string>() {
                    { "en", "Relocate Stone" },
                } },
                { "VotableRelocateStoneDescription", new Dictionary<string, string>() {
                    { "en", "The earth shakes as mountains move." },
                } },
                { "VotableRelocateIronTitle", new Dictionary<string, string>() {
                    { "en", "Relocate Iron" },
                } },
                { "VotableRelocateIronDescription", new Dictionary<string, string>() {
                    { "en", "The earth shakes as mountains move." },
                } },
                { "VotableRelocateWitchHutTitle", new Dictionary<string, string>() {
                    { "en", "Relocate Witch Hut" },
                } },
                { "VotableRelocateWitchHutDescription", new Dictionary<string, string>() {
                    { "en", "Happy to do as you ask." },
                } },
                { "VotableRelocateKeepTitle", new Dictionary<string, string>() {
                    { "en", "Relocate Keep" },
                } },
                { "VotableRelocateKeepDescription", new Dictionary<string, string>() {
                    { "en", "Happy to do as you ask." },
                } },
                { "VotableIncreaseElevationTitle", new Dictionary<string, string>() {
                    { "en", "Increase Elevation" },
                } },
                { "VotableIncreaseElevationDescription", new Dictionary<string, string>() {
                    { "en", "Without a sound the earth rises." },
                } },
                { "VotableDecreaseElevationTitle", new Dictionary<string, string>() {
                    { "en", "Decrease Elevation" },
                } },
                { "VotableDecreaseElevationDescription", new Dictionary<string, string>() {
                    { "en", "Without a sound the earth lowers." },
                } },
                { "fertilityNotMax", new Dictionary<string, string>() {
                    { "en", "Fertility cannot be increased further, sire." },
                } },
                { "fertilityNotMin", new Dictionary<string, string>() {
                    { "en", "Fertility cannot be decreased further, sire." },
                } },
                { "elevationNotMax", new Dictionary<string, string>() {
                    { "en", "This land cannot be raised, sire." },
                } },
                { "elevationNotMin", new Dictionary<string, string>() {
                    { "en", "This land cannot be lowered, sire." },
                } },
                { "dontSplitLandmass", new Dictionary<string, string>() {
                    { "en", "Cannot split landmasses, sire." },
                } },
                { "dontJoinLandmass", new Dictionary<string, string>() {
                    { "en", "Cannot connect landmasses, sire." },
                } },
                { "dontEliminateLandmass", new Dictionary<string, string>() {
                    { "en", "Cannot eliminate landmasses, sire." },
                } },
                { "existingLandmass", new Dictionary<string, string>() {
                    { "en", "Cannot create new landmasses, sire." },
                } },
                { "sameLandmass", new Dictionary<string, string>() {
                    { "en", "Must remain on the same landmass, sire." },
                } },
                { "outOfBounds", new Dictionary<string, string>() {
                    { "en", "That is beyond your influence, sire." },
                } },
                { "noWater", new Dictionary<string, string>() {
                    { "en", "There is no land here, sire." },
                } },
                { "noResource", new Dictionary<string, string>() {
                    { "en", "Blocked by an existing resource, sire." },
                } },
                { "noStructure", new Dictionary<string, string>() {
                    { "en", "Blocked by an existing structure, sire." },
                } },
                { "stone", new Dictionary<string, string>() {
                    { "en", "There is no stone here, sire." },
                } },
                { "iron", new Dictionary<string, string>() {
                    { "en", "There is no iron here, sire." },
                } },
                { "witch", new Dictionary<string, string>() {
                    { "en", "No witches live here, sire." },
                } },
                { "keep", new Dictionary<string, string>() {
                    { "en", "This is not a keep, sire." },
                } },
            };

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

            // Introduce custom translations
            [HarmonyPatch(typeof(LocalizationManager))]
            [HarmonyPatch("GetTranslation")]
            public static class LocalizationManagerGetTranslation {
                static void Postfix(string Term, ref string __result) {
                    __result = GetTranslation(Term, __result);
                }
            }
        }
    }
}
