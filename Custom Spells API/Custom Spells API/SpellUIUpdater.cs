using Harmony;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Phedg1Studios.Shared;

namespace Phedg1Studios {
    namespace CustomSpellsAPI {
        public class SpellUIUpdater : MonoBehaviour {
            static private Color outlineColour = new Color(3f / 255f, 5f / 255f, 7f / 255f);
            static private Color inlineColour = new Color(16f / 255f, 25f / 255f, 32f / 255f);
            static private Color fillColour = new Color(31f / 255f, 48f / 255f, 61f / 255f);
            static private Color scrollbarBackgroundColour = new Color(2f / 255f, 6f / 255f, 9f / 255f);
            static private Color transparentColour = new Color(0, 0, 0, 0);
            static private Color redColour = new Color(1, 0, 0);
            static private float panelOffset = 25f;
            static private float inlineOffsetHorizontal = 1.5f;
            static private float inlineOffsetVertical = 1.5f;
            static private float fillOffsetHorizontal = 1.5f;
            static private float fillOffsetVertical = 3f;
            static private float scrollbarWidth = 14f;
            static private List<string> spellListDummyHierarchy = new List<string>() { "InGameUI", "WitchUICanvas", "WitchHut", "Window", "SpellContainer", "SpellList BG" };

            static public GameObject spellListDummy;
            static public GameObject spellList;
            static public ScrollRect scrollrect;
            static public List<GameObject> additionalSpellButtons = new List<GameObject>();

            // Upgrade spell list into a scroll
            [HarmonyPatch(typeof(GameUI))]
            [HarmonyPatch("Start")]
            public static class GameUIStart {
                [HarmonyPriority(200)]
                static void Postfix(GameUI __instance) {
                    Transform newObject = null;
                    if (Shared.Util.GetObjectFromHierarchy(ref newObject, spellListDummyHierarchy, 0, null)) {
                        spellListDummy = newObject.gameObject;
                    }
                    spellList = __instance.witchUI.spellList;

                    RectTransform spellListParentTransform = spellList.transform.parent.GetComponent<RectTransform>();
                    Image backgroundOutline = SpawnImageOffset(spellListParentTransform, null, outlineColour, new Vector2(0.5f, 1f), new Vector2(panelOffset, panelOffset), new Vector2(-panelOffset, -panelOffset * 2f));
                    RectTransform backgroundTransform = backgroundOutline.GetComponent<RectTransform>();
                    Image backgroundInline = SpawnImageOffset(backgroundOutline.transform, null, inlineColour, new Vector2(0.5f, 1f), new Vector2(inlineOffsetHorizontal, inlineOffsetVertical), new Vector2(-inlineOffsetHorizontal, -inlineOffsetHorizontal));
                    Image backgroundFill = SpawnImageOffset(backgroundInline.transform, null, fillColour, new Vector2(0.5f, 1f), new Vector2(fillOffsetHorizontal, fillOffsetVertical), new Vector2(-fillOffsetHorizontal, 0));
                    backgroundFill.gameObject.AddComponent<RectMask2D>();
                    RectTransform backgroundFillTransform = backgroundFill.gameObject.GetComponent<RectTransform>();
                    Image contentFill = SpawnImageSize(backgroundFill.transform, null, transparentColour, new Vector2(0.5f, 1f), new Vector2(backgroundFillTransform.rect.width, backgroundFillTransform.rect.height), new Vector2(0, 0));
                    RectTransform contentFillTransform = contentFill.gameObject.GetComponent<RectTransform>();

                    List<GameObject> containers = new List<GameObject>() { spellListDummy, spellList };
                    foreach (GameObject container in containers) {
                        GridLayoutGroup gridLayoutGroup = container.GetComponent<GridLayoutGroup>();
                        gridLayoutGroup.cellSize = new Vector2(contentFillTransform.rect.width - gridLayoutGroup.spacing.y * 2, gridLayoutGroup.cellSize.y);
                        gridLayoutGroup.constraintCount = 0;

                        RectTransform containerTransform = container.GetComponent<RectTransform>();
                        Vector3 localScale = containerTransform.localScale;
                        containerTransform.transform.parent = contentFillTransform;
                        containerTransform.localScale = localScale;

                        containerTransform.pivot = new Vector2(0.5f, 1f);
                        containerTransform.anchorMin = new Vector2(0.5f, 1f);
                        containerTransform.anchorMax = new Vector2(0.5f, 1f);
                        containerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, gridLayoutGroup.cellSize.x);
                        containerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentFillTransform.rect.height);
                        containerTransform.localPosition = new Vector2(0, -gridLayoutGroup.spacing.y);
                    }

                    scrollrect = backgroundOutline.gameObject.AddComponent<ScrollRect>();
                    scrollrect.horizontal = false;
                    scrollrect.vertical = true;
                    scrollrect.horizontalScrollbar = null;
                    scrollrect.verticalScrollbar = null;
                    scrollrect.movementType = ScrollRect.MovementType.Clamped;
                    scrollrect.inertia = false;
                    scrollrect.scrollSensitivity = 50;
                    scrollrect.viewport = backgroundFillTransform;
                    scrollrect.content = contentFillTransform;

                    Image scrollbarBackground = SpawnImageSize(spellListParentTransform, null, scrollbarBackgroundColour, new Vector2(0.5f, 1f), new Vector2(scrollbarWidth, backgroundOutline.GetComponent<RectTransform>().rect.height), new Vector2(spellListParentTransform.GetComponent<RectTransform>().rect.width / 2f - panelOffset / 2f, spellListParentTransform.GetComponent<RectTransform>().rect.height / 2f - panelOffset * 2f));
                    Image scrollbarOutline = SpawnImageSize(scrollbarBackground.transform, null, outlineColour, new Vector2(0.5f, 1f), new Vector2(scrollbarBackground.GetComponent<RectTransform>().rect.width, 100), new Vector2(0, 0));
                    Image scrollbarInline = SpawnImageOffset(scrollbarOutline.transform, null, inlineColour, new Vector2(0.5f, 1f), new Vector2(inlineOffsetHorizontal, inlineOffsetVertical), new Vector2(-inlineOffsetHorizontal, -inlineOffsetHorizontal));
                    Image scrollbarFill = SpawnImageOffset(scrollbarInline.transform, null, fillColour, new Vector2(0.5f, 1f), new Vector2(fillOffsetHorizontal, fillOffsetVertical), new Vector2(-fillOffsetHorizontal, 0));

                    Scrollbar scrollbar = scrollbarBackground.gameObject.AddComponent<Scrollbar>();
                    scrollbar.targetGraphic = scrollbarOutline;
                    scrollbar.handleRect = scrollbarOutline.GetComponent<RectTransform>();
                    scrollbar.direction = Scrollbar.Direction.BottomToTop;
                    scrollrect.verticalScrollbar = scrollbar;

                    RectTransform scrollbarOutlineTransform = scrollbarOutline.gameObject.GetComponent<RectTransform>();
                    scrollbarOutlineTransform.offsetMin = new Vector2(0, 0);
                    scrollbarOutlineTransform.offsetMax = new Vector2(0, 0);
                }
            }

            // Spawn an image with an absolute size
            static public Image SpawnImageSize(Transform parent, Sprite sprite, Color colour, Vector2 pivot, Vector2 size, Vector3 localPosition) {
                GameObject image = new GameObject();
                image.name = "Image";
                image.transform.parent = parent;
                RectTransform imageTransform = image.AddComponent<RectTransform>();
                imageTransform.pivot = pivot;
                imageTransform.anchorMin = new Vector2(0, 0);
                imageTransform.anchorMax = new Vector2(1, 1);
                imageTransform.localPosition = localPosition;
                imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                imageTransform.localScale = new Vector3(1, 1, 1);
                Image imageImage = image.AddComponent<Image>();
                imageImage.color = colour;
                imageImage.sprite = sprite;
                imageImage.type = Image.Type.Sliced;
                imageImage.raycastTarget = true;
                return imageImage;
            }

            // Spawn an image with a relative size
            static public Image SpawnImageOffset(Transform parent, Sprite sprite, Color colour, Vector2 pivot, Vector2 offsetMin, Vector2 offsetMax) {
                GameObject image = new GameObject();
                image.name = "Image";
                image.transform.parent = parent;
                RectTransform imageTransform = image.AddComponent<RectTransform>();
                imageTransform.pivot = pivot;
                imageTransform.anchorMin = new Vector2(0, 0f);
                imageTransform.anchorMax = new Vector2(1, 1);
                imageTransform.offsetMin = offsetMin;
                imageTransform.offsetMax = offsetMax;
                imageTransform.localScale = new Vector3(1, 1, 1);
                Image imageImage = image.AddComponent<Image>();
                imageImage.color = colour;
                imageImage.sprite = sprite;
                imageImage.type = Image.Type.Sliced;
                imageImage.raycastTarget = true;
                return imageImage;
            }

            // Add spell buttons for custom spells
            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("Start")]
            public static class WitchUIStart {
                static void Postfix(WitchUI __instance) {
                    additionalSpellButtons.Clear();
                    GameObject buttonOriginal = spellList.transform.GetChild(0).gameObject;
                    GameObject buttonOriginalDummy = spellListDummy.transform.GetChild(0).gameObject;
                    for (int spellIndex = 0; spellIndex < CustomSpellsAPI.spellData.Count; spellIndex++) {
                        GameObject buttonNew = GameObject.Instantiate(buttonOriginal, buttonOriginal.transform.parent);
                        buttonNew.transform.localScale = buttonOriginal.transform.localScale;
                        buttonNew.transform.localPosition = buttonOriginal.transform.localPosition;
                        Button button = buttonNew.GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        int customSpellIndex = spellIndex;
                        int globalSpellIndex = CustomSpellsAPI.spellDataOriginalCount + customSpellIndex;

                        Assembly assembly;
                        bool resultA = Porg.InteropClient.TryGetMod(CustomSpellsAPI.spellData[customSpellIndex].interopName, out assembly);
                        Type registerSpells = assembly.GetType("Phedg1Studios.Shared.RegisterSpells");
                        MethodInfo buttonClickedInfo = registerSpells.GetMethod("ButtonClicked", BindingFlags.NonPublic | BindingFlags.Static);
                        buttonClickedInfo.Invoke(null, new object[] { button.onClick, __instance, globalSpellIndex });

                        GameObject buttonNewDummy = GameObject.Instantiate(buttonOriginalDummy, buttonOriginalDummy.transform.parent);
                        buttonNewDummy.transform.localScale = buttonOriginalDummy.transform.localScale;
                        buttonNewDummy.transform.localPosition = buttonOriginalDummy.transform.localPosition;

                        additionalSpellButtons.Add(buttonNew);
                    }

                    int childCount = spellList.transform.childCount;
                    GridLayoutGroup gridLayoutGroup = spellList.GetComponent<GridLayoutGroup>();
                    float spellHeight = childCount * gridLayoutGroup.cellSize.y + (childCount + 1) * gridLayoutGroup.spacing.y;
                    float viewportHeight = spellList.transform.parent.parent.GetComponent<RectTransform>().rect.height;
                    spellList.transform.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(viewportHeight, spellHeight));
                    RefreshAvailableSpells(__instance);
                }
            }

            // Move the list back to the top each time the menu is open
            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("RefreshUI")]
            public static class WitchUIRefreshUI {
                static void Postfix(WitchUI __instance) {
                    if (scrollrect != null) {
                        scrollrect.verticalNormalizedPosition = 1;
                    }
                }
            }

            [HarmonyPatch(typeof(WitchUI))]
            [HarmonyPatch("RefreshAvailableSpells")]
            public static class WitchUIRefreshAvailableSpells {
                [HarmonyPriority(200)]
                static void Postfix(WitchUI __instance) {
                    RefreshAvailableSpells(__instance);
                }
            }

            // Only make spell buttons visible when a certain friendship is reached
            public static void RefreshAvailableSpells(WitchUI __instance) {
                System.Reflection.FieldInfo fieldInfo = typeof(WitchUI).GetField("witch", BindingFlags.NonPublic | BindingFlags.Instance);
                WitchHut witch = (WitchHut)fieldInfo.GetValue(__instance);
                fieldInfo = typeof(WitchHut).GetField("spellData", BindingFlags.NonPublic | BindingFlags.Instance);
                ICollection spellDataCollection = (ICollection)fieldInfo.GetValue(witch);
                int spellDataCount = 0;
                foreach (object spellData in spellDataCollection) {
                    spellDataCount += 1;
                }
                if (spellDataCount == spellList.transform.childCount) {
                    for (int spellIndex = 0; spellIndex < spellDataCount; spellIndex++) {
                        SpellDataCustom spellDataCustom = witch.GetSpellData(spellIndex) as SpellDataCustom;
                        if (spellDataCustom != null) {
                            bool spellButtonsActive = witch.relationship >= spellDataCustom.relationship;
                            spellList.transform.GetChild(spellIndex).gameObject.SetActive(spellButtonsActive);
                        }
                    }
                }
            }
        }
    }
}