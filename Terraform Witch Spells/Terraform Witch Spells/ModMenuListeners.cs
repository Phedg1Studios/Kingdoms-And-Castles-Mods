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
        public class ModMenuListeners : MonoBehaviour {
            static public void AddListeners(ModMenuSettings settings) {
                AddListenersPrices(settings);
                AddListenersCooldowns(settings);
                AddListenersRelationships(settings);
            }

            static private int MultipleOfFive(float givenFloat) {
                return Mathf.RoundToInt(givenFloat / 5f) * 5;
            }

            static private void AddListenersPrices(ModMenuSettings settings) {
                settings.prices.IncreaseFertility.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_IncreaseFertility)) {
                            int value = MultipleOfFive(setting.slider.value);
                            spellDataCustom.cost = value;
                            settings.prices.IncreaseFertility.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.prices.DecreaseFertility.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_DecreaseFertility)) {
                            int value = MultipleOfFive(setting.slider.value);
                            spellDataCustom.cost = value;
                            settings.prices.DecreaseFertility.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.prices.IncreaseElevation.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_IncreaseElevation)) {
                            int value = MultipleOfFive(setting.slider.value);
                            spellDataCustom.cost = value;
                            settings.prices.IncreaseElevation.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.prices.DecreaseElevation.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_DecreaseElevation)) {
                            int value = MultipleOfFive(setting.slider.value);
                            spellDataCustom.cost = value;
                            settings.prices.DecreaseElevation.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.prices.RelocateStone.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateStone)) {
                            int value = MultipleOfFive(setting.slider.value);
                            spellDataCustom.cost = value;
                            settings.prices.RelocateStone.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.prices.RelocateIron.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateIron)) {
                            int value = MultipleOfFive(setting.slider.value);
                            spellDataCustom.cost = value;
                            settings.prices.RelocateIron.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.prices.RelocateWitchHut.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateWitchHut)) {
                            int value = MultipleOfFive(setting.slider.value);
                            spellDataCustom.cost = value;
                            settings.prices.RelocateWitchHut.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.prices.RelocateKeep.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateKeep)) {
                            int value = MultipleOfFive(setting.slider.value);
                            spellDataCustom.cost = value;
                            settings.prices.RelocateKeep.Label = value.ToString();
                            break;
                        }
                    }
                });
            }

            // ----------------------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------

            static private void AddListenersCooldowns(ModMenuSettings settings) {
                settings.cooldowns.IncreaseFertility.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_IncreaseFertility)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.cooldown = value;
                            settings.cooldowns.IncreaseFertility.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.cooldowns.DecreaseFertility.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_DecreaseFertility)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.cooldown = value;
                            settings.cooldowns.DecreaseFertility.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.cooldowns.IncreaseElevation.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_IncreaseElevation)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.cooldown = value;
                            settings.cooldowns.IncreaseElevation.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.cooldowns.DecreaseElevation.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_DecreaseElevation)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.cooldown = value;
                            settings.cooldowns.DecreaseElevation.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.cooldowns.RelocateStone.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateStone)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.cooldown = value;
                            settings.cooldowns.RelocateStone.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.cooldowns.RelocateIron.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateIron)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.cooldown = value;
                            settings.cooldowns.RelocateIron.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.cooldowns.RelocateWitchHut.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateWitchHut)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.cooldown = value;
                            settings.cooldowns.RelocateWitchHut.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.cooldowns.RelocateKeep.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateKeep)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.cooldown = value;
                            settings.cooldowns.RelocateKeep.Label = value.ToString();
                            break;
                        }
                    }
                });
            }

            // ----------------------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------

            static private void AddListenersRelationships(ModMenuSettings settings) {
                settings.relationships.IncreaseFertility.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_IncreaseFertility)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.relationship = (WitchHut.Relationship)value;
                            settings.relationships.DecreaseFertility.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.relationships.DecreaseFertility.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_DecreaseFertility)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.relationship = (WitchHut.Relationship)value;
                            settings.relationships.DecreaseFertility.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.relationships.IncreaseElevation.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_IncreaseElevation)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.relationship = (WitchHut.Relationship)value;
                            settings.relationships.IncreaseElevation.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.relationships.DecreaseElevation.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_DecreaseElevation)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.relationship = (WitchHut.Relationship)value;
                            settings.relationships.DecreaseElevation.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.relationships.RelocateStone.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateStone)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.relationship = (WitchHut.Relationship)value;
                            settings.relationships.RelocateStone.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.relationships.RelocateIron.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateIron)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.relationship = (WitchHut.Relationship)value;
                            settings.relationships.RelocateIron.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.relationships.RelocateWitchHut.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateWitchHut)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.relationship = (WitchHut.Relationship)value;
                            settings.relationships.RelocateWitchHut.Label = value.ToString();
                            break;
                        }
                    }
                });

                settings.relationships.RelocateKeep.OnUpdate.AddListener((setting) => {
                    foreach (SpellDataCustom spellDataCustom in RegisterSpells.spellData) {
                        if (spellDataCustom.spellImpl == typeof(StreamerEffect_RelocateKeep)) {
                            int value = Mathf.RoundToInt(setting.slider.value);
                            spellDataCustom.relationship = (WitchHut.Relationship)value;
                            settings.relationships.RelocateKeep.Label = value.ToString();
                            break;
                        }
                    }
                });
            }

            static public void ForceUpdateSettings(ModMenuSettings settings) {
                settings.prices.IncreaseFertility.TriggerUpdate();
                settings.prices.DecreaseFertility.TriggerUpdate();
                settings.prices.IncreaseElevation.TriggerUpdate();
                settings.prices.DecreaseElevation.TriggerUpdate();
                settings.prices.RelocateStone.TriggerUpdate();
                settings.prices.RelocateIron.TriggerUpdate();
                settings.prices.RelocateWitchHut.TriggerUpdate();
                settings.prices.RelocateKeep.TriggerUpdate();

                settings.cooldowns.IncreaseFertility.TriggerUpdate();
                settings.cooldowns.DecreaseFertility.TriggerUpdate();
                settings.cooldowns.IncreaseElevation.TriggerUpdate();
                settings.cooldowns.DecreaseElevation.TriggerUpdate();
                settings.cooldowns.RelocateStone.TriggerUpdate();
                settings.cooldowns.RelocateIron.TriggerUpdate();
                settings.cooldowns.RelocateWitchHut.TriggerUpdate();
                settings.cooldowns.RelocateKeep.TriggerUpdate();

                settings.relationships.IncreaseFertility.TriggerUpdate();
                settings.relationships.DecreaseFertility.TriggerUpdate();
                settings.relationships.IncreaseElevation.TriggerUpdate();
                settings.relationships.DecreaseElevation.TriggerUpdate();
                settings.relationships.RelocateStone.TriggerUpdate();
                settings.relationships.RelocateIron.TriggerUpdate();
                settings.relationships.RelocateWitchHut.TriggerUpdate();
                settings.relationships.RelocateKeep.TriggerUpdate();
            }
        }
    }
}
