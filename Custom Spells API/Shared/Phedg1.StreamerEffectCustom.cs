using UnityEngine;
using Assets;
using System.Collections.Generic;
using Phedg1Studios.Shared;
using I2.Loc;

namespace Phedg1Studios {
    namespace Shared {
        public abstract class StreamerEffectCustom : StreamerEffect {
            public virtual WitchHut witchHut { get; set; }
            public virtual WitchUI witchUI { get; set; }
            public virtual SpellDataCustom spellData { get; set; }
            public virtual int spellIndex { get; set; }

            public abstract void Activate();

            // Trigger the spell activation banner
            public void ShowBanner() {
                Votable votable = new Votable() {
                    Implementation = spellData.spellImpl,
                    color = spellData.color
                };
                EffectBanner.inst.ShowBannerGeneric(votable, ScriptLocalization.TheWitch, witchUI.witchSpr, spellData.locKey);
            }

            public bool PurchaseSpell() {
                WitchHut.SpellData outSpell = null;
                int num = witchHut.TryActivate((WitchHut.Spells)spellIndex, out outSpell);
                if (num == 0) {
                    return true;
                }
                return false;
            }
        }
    }
}