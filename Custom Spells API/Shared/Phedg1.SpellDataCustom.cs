using UnityEngine;

namespace Phedg1Studios {
    namespace Shared {
        public class SpellDataCustom : WitchHut.SpellData {
            // The witch relationship required for this spell to become available
            public WitchHut.Relationship relationship { get; set; }
            // If true spell cost will scale with population size, which is how the vanilla spells work

            public bool twitchVotable { get; set; }

            public bool scaleCost { get; set; }
        }
    }
}