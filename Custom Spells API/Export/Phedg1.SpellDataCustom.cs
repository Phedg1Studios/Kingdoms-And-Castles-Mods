using System.Collections.Generic;
using UnityEngine;
using System;

namespace Phedg1Studios {
    namespace Shared {
        public class SpellDataCustom : WitchHut.SpellData {
            public enum Data {
                Cooldown = 0,
                Cost = 1,
                SpellImpl = 2,
                Color = 3,
                LocKey = 4,
                Relationship = 5,
                TwitchVotable = 6,
                ScaleCost = 7,
                InteropName = 8,
            }

            public enum TryActivate {
                WitchHut = 0,
                SpellIndex = 1,
                Activations = 2
            }

            const char dictString = ':';
            const char newLineString = '\n';
            const char listString = ',';

            // Start of spell data custom properties

            public string interopName { get; set; }

            // The witch relationship required for this spell to become available
            public WitchHut.Relationship relationship { get; set; }

            public bool twitchVotable { get; set; }

            // If true spell cost will scale with population size, which is how the vanilla spells work
            public bool scaleCost { get; set; }

            public Dictionary<int, object> GetProperties() {
                Dictionary<int, object> properties = new Dictionary<int, object>();
                properties.Add((int)Data.Cooldown, cooldown);
                properties.Add((int)Data.Cost, cost);
                properties.Add((int)Data.Color, color);
                properties.Add((int)Data.LocKey, locKey);
                properties.Add((int)Data.Relationship, relationship);
                properties.Add((int)Data.TwitchVotable, twitchVotable);
                properties.Add((int)Data.ScaleCost, scaleCost);
                properties.Add((int)Data.SpellImpl, spellImpl);
                properties.Add((int)Data.InteropName, interopName);
                return properties;
            }

            static public SpellDataCustom GetSpellDataCustom(Dictionary<int, object> properties) {
                SpellDataCustom spellDataCustom = new SpellDataCustom();
                if (properties.ContainsKey((int)Data.Cooldown)) {
                    spellDataCustom.cooldown = (int)Convert.ChangeType(properties[(int)Data.Cooldown], typeof(int));
                }
                if (properties.ContainsKey((int)Data.Cost)) {
                    spellDataCustom.cost = (int)Convert.ChangeType(properties[(int)Data.Cost], typeof(int));
                }
                if (properties.ContainsKey((int)Data.Color)) {
                    spellDataCustom.color = (Color)Convert.ChangeType(properties[(int)Data.Color], typeof(Color));
                }
                if (properties.ContainsKey((int)Data.LocKey)) {
                    spellDataCustom.locKey = (string)Convert.ChangeType(properties[(int)Data.LocKey], typeof(string));
                }
                if (properties.ContainsKey((int)Data.Relationship)) {
                    spellDataCustom.relationship = (WitchHut.Relationship)Convert.ChangeType(properties[(int)Data.Relationship], typeof(WitchHut.Relationship));
                }
                if (properties.ContainsKey((int)Data.TwitchVotable)) {
                    spellDataCustom.twitchVotable = (bool)Convert.ChangeType(properties[(int)Data.TwitchVotable], typeof(bool));
                }
                if (properties.ContainsKey((int)Data.ScaleCost)) {
                    spellDataCustom.scaleCost = (bool)Convert.ChangeType(properties[(int)Data.ScaleCost], typeof(bool));
                }
                if (properties.ContainsKey((int)Data.SpellImpl)) {
                    spellDataCustom.spellImpl = (Type)properties[(int)Data.SpellImpl];
                }
                if (properties.ContainsKey((int)Data.InteropName)) {
                    spellDataCustom.interopName = (string)Convert.ChangeType(properties[(int)Data.InteropName], typeof(string));
                }
                return spellDataCustom;
            }
        }
    }
}