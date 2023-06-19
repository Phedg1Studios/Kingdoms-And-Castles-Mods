using Harmony;
using System.Collections.Generic;
using UnityEngine;
using static Phedg1Studios.TerraformWitchSpells.Util;
using Phedg1Studios.Shared;
using Zat.Shared.ModMenu.Interactive;
using Zat.Shared.ModMenu.API;
using Zat;

namespace Phedg1Studios {
    namespace TerraformWitchSpells {
        [Mod("Terraform Witch Spells", "v1.0", "Phedg1Studios")]
        public class ModMenuSettings {
            [Setting("")]
            public InteractiveSetting EmptySetting { get; private set; }

            [Category("Prices")]
            public ModMenuSettingsPrices prices { get; private set; }

            [Category("Cooldowns")]
            public ModMenuSettingsCooldowns cooldowns { get; private set; }

            [Category("Relationship")]
            public ModMenuSettingsRelationships relationships { get; private set; }
        }

        public class ModMenuSettingsPrices : MonoBehaviour {
            [Setting("Increase Fertility", "How much the spell costs")]
            [Slider(0, 10000, 75, "75", true)]
            public InteractiveSliderSetting IncreaseFertility { get; private set; }

            [Setting("Decrease Fertility", "How much the spell costs")]
            [Slider(0, 10000, 75, "75", true)]
            public InteractiveSliderSetting DecreaseFertility { get; private set; }

            [Setting("Increase Elevation", "How much the spell costs")]
            [Slider(0, 10000, 100, "100", true)]
            public InteractiveSliderSetting IncreaseElevation { get; private set; }

            [Setting("Decrease Elevation", "How much the spell costs")]
            [Slider(0, 10000, 100, "100", true)]
            public InteractiveSliderSetting DecreaseElevation { get; private set; }

            [Setting("Relocate Stone", "How much the spell costs")]
            [Slider(0, 10000, 2000, "2000", true)]
            public InteractiveSliderSetting RelocateStone { get; private set; }

            [Setting("Relocate Iron", "How much the spell costs")]
            [Slider(0, 10000, 2000, "2000", true)]
            public InteractiveSliderSetting RelocateIron { get; private set; }

            [Setting("Transmute Rock", "How much the spell costs")]
            [Slider(0, 10000, 4000, "4000", true)]
            public InteractiveSliderSetting TransmuteRock { get; private set; }

            [Setting("Relocate Witch Hut", "How much the spell costs")]
            [Slider(0, 10000, 2000, "2000", true)]
            public InteractiveSliderSetting RelocateWitchHut { get; private set; }

            [Setting("Relocate Keep", "How much the spell costs")]
            [Slider(0, 10000, 4000, "4000", true)]
            public InteractiveSliderSetting RelocateKeep { get; private set; }
        }

        public class ModMenuSettingsCooldowns : MonoBehaviour {
            [Setting("Increase Fertility", "How many years between uses")]
            [Slider(0, 100, 0, "0", true)]
            public InteractiveSliderSetting IncreaseFertility { get; private set; }

            [Setting("Decrease Fertility", "How many years between uses")]
            [Slider(0, 100, 0, "0", true)]
            public InteractiveSliderSetting DecreaseFertility { get; private set; }

            [Setting("Increase Elevation", "How many years between uses")]
            [Slider(0, 100, 0, "0", true)]
            public InteractiveSliderSetting IncreaseElevation { get; private set; }

            [Setting("Decrease Elevation", "How many years between uses")]
            [Slider(0, 100, 0, "0", true)]
            public InteractiveSliderSetting DecreaseElevation { get; private set; }

            [Setting("Relocate Stone", "How many years between uses")]
            [Slider(0, 100, 10, "10", true)]
            public InteractiveSliderSetting RelocateStone { get; private set; }

            [Setting("Relocate Iron", "How many years between uses")]
            [Slider(0, 100, 10, "10", true)]
            public InteractiveSliderSetting RelocateIron { get; private set; }

            [Setting("Transmute Rock", "How many years between uses")]
            [Slider(0, 100, 25, "25", true)]
            public InteractiveSliderSetting TransmuteRock { get; private set; }

            [Setting("Relocate Witch Hut", "How many years between uses")]
            [Slider(0, 100, 100, "100", true)]
            public InteractiveSliderSetting RelocateWitchHut { get; private set; }

            [Setting("Relocate Keep", "How many years between uses")]
            [Slider(0, 100, 100, "100", true)]
            public InteractiveSliderSetting RelocateKeep { get; private set; }
        }

        public class ModMenuSettingsRelationships : MonoBehaviour {
            [Setting("Increase Fertility", "Relationship required to unlock")]
            [Slider(0, 10, 6, "6", true)]
            public InteractiveSliderSetting IncreaseFertility { get; private set; }

            [Setting("Decrease Fertility", "Relationship required to unlock")]
            [Slider(0, 10, 6, "6", true)]
            public InteractiveSliderSetting DecreaseFertility { get; private set; }

            [Setting("Increase Elevation", "Relationship required to unlock")]
            [Slider(0, 10, 8, "8", true)]
            public InteractiveSliderSetting IncreaseElevation { get; private set; }

            [Setting("Decrease Elevation", "Relationship required to unlock")]
            [Slider(0, 10, 8, "8", true)]
            public InteractiveSliderSetting DecreaseElevation { get; private set; }

            [Setting("Relocate Stone", "Relationship required to unlock")]
            [Slider(0, 10, 7, "7", true)]
            public InteractiveSliderSetting RelocateStone { get; private set; }

            [Setting("Relocate Iron", "Relationship required to unlock")]
            [Slider(0, 10, 7, "7", true)]
            public InteractiveSliderSetting RelocateIron { get; private set; }

            [Setting("Transmute Rock", "Relationship required to unlock")]
            [Slider(0, 10, 8, "8", true)]
            public InteractiveSliderSetting TransmuteRock { get; private set; }

            [Setting("Relocate Witch Hut", "Relationship required to unlock")]
            [Slider(0, 10, 8, "8", true)]
            public InteractiveSliderSetting RelocateWitchHut { get; private set; }

            [Setting("Relocate Keep", "Relationship required to unlock")]
            [Slider(0, 10, 8, "8", true)]
            public InteractiveSliderSetting RelocateKeep { get; private set; }
        }
    }
}
