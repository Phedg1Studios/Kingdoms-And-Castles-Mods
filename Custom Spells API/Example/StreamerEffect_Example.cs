using UnityEngine;
using Phedg1Studios.Shared;

namespace Example {
    namespace CustomSpellsAPIExample {

        // All spell effects in the vanilla game have the naming convention StreamerEffect_InternalSpellName
        public class StreamerEffect_Example : StreamerEffectCustom {

            // A uninique name that is used to fetch the translations for the spells title and description
            public new static string GetTermSegment() {
                return "UniqueName";
            }

            // Called when a custom spell is purchased
            // Does nothing by default
            // Calll ShowBanner for default behavior
            public override void Activate() {
                // The instance of WitchHut the spell was purchased from
                Debug.Log(witchHut);
                // The instance of WitchUI the spell was purchased from
                Debug.Log(witchUI);
                // The spell data of the purchased spell
                Debug.Log(spellData);
                // The index of the purchased spell
                Debug.Log(spellIndex);

                // How long the effect should continue for
                Debug.Log(Timer.Duration);

                // If this spell can be purchased the gold will be deducted it will return true
                // Otherwise it will return false
                if (PurchaseSpell()) {
                    // Will show the spell banner, which is the behavior of a vanilla spell
                    ShowBanner();
                }
            }

            // Called when the spell banner is closed
            // It is recommended you set Timer.Duration here
            // This is where vanilla spells are activated
            public override void OnStart() {
            }

            // Called every frame after OnStart until OnEnd
            public void Update() {
            }

            // Called after OnStart when the timer's remaining time reaches zero
            // The instance of this class and its gameobject are destroyed
            public override void OnEnd() {
            }
        }
    }
}