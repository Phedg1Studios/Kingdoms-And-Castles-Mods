using UnityEngine;
using UnityEngine.UI;

namespace Phedg1Studios {
    namespace CustomResearchAPI {
        public class ResearchUIUpdater : MonoBehaviour {
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
            static private float optionHeight = 0;
            static private int padding = 6;
            static private float offset = 40;

            static public ScrollRect scrollrect;

            static public void UpdateUI(GameObject optionContainer) {
                RectTransform optionContainerParentTransform = optionContainer.transform.parent.GetComponent<RectTransform>();
                RectTransform optionContainerTransform = optionContainer.GetComponent<RectTransform>();
                optionHeight = optionContainerTransform.rect.height / 5;
                int siblingIndexOld = optionContainerTransform.GetSiblingIndex();

                Image backgroundOutline = SpawnImageOffset(optionContainerParentTransform, null, outlineColour, new Vector2(0.5f, 1f), new Vector2(panelOffset, panelOffset), new Vector2(-panelOffset, -panelOffset * 2f));
                backgroundOutline.transform.SetSiblingIndex(siblingIndexOld);
                Image backgroundInline = SpawnImageOffset(backgroundOutline.transform, null, inlineColour, new Vector2(0.5f, 1f), new Vector2(inlineOffsetHorizontal, inlineOffsetVertical), new Vector2(-inlineOffsetHorizontal, -inlineOffsetHorizontal));
                Image backgroundFill = SpawnImageOffset(backgroundInline.transform, null, fillColour, new Vector2(0.5f, 1f), new Vector2(fillOffsetHorizontal, fillOffsetVertical), new Vector2(-fillOffsetHorizontal, 0));
                backgroundFill.gameObject.AddComponent<RectMask2D>();
                RectTransform backgroundFillTransform = backgroundFill.gameObject.GetComponent<RectTransform>();
                
                Image contentFill = SpawnImageSize(backgroundFill.transform, null, transparentColour, new Vector2(0.5f, 1f), new Vector2(backgroundFillTransform.rect.width, backgroundFillTransform.rect.height + padding * 2), new Vector2(0, 0));
                RectTransform contentFillTransform = contentFill.gameObject.GetComponent<RectTransform>();
                

                Vector3 localScaleOptionContainer = optionContainerTransform.localScale;
                optionContainerTransform.parent = contentFillTransform;
                optionContainerTransform.localScale = localScaleOptionContainer;
                optionContainerTransform.pivot = new Vector2(0.5f, 1f);
                optionContainerTransform.anchorMin = new Vector2(0.5f, 1f);
                optionContainerTransform.anchorMax = new Vector2(0.5f, 1f);
                optionContainerTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentFillTransform.rect.width + offset);
                optionContainerTransform.localPosition = new Vector2(-offset, -padding);

                for (int childIndex = 0; childIndex < optionContainerTransform.childCount; childIndex++) {
                    optionContainerTransform.GetChild(childIndex).GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, optionContainerTransform.rect.width);
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

                Image scrollbarBackground = SpawnImageSize(optionContainerParentTransform, null, scrollbarBackgroundColour, new Vector2(0.5f, 1f), new Vector2(scrollbarWidth, backgroundOutline.GetComponent<RectTransform>().rect.height), new Vector2(optionContainerParentTransform.GetComponent<RectTransform>().rect.width / 2f - panelOffset / 2f, optionContainerParentTransform.GetComponent<RectTransform>().rect.height / 2f - panelOffset * 2f));
                scrollbarBackground.transform.SetSiblingIndex(siblingIndexOld);
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

            static public void UpdateContentSize(GameObject researchContainer) {
                int activeChildCount = 0;
                for (int childIndex = 0; childIndex < researchContainer.transform.childCount; childIndex++) {
                    if (researchContainer.transform.GetChild(childIndex).gameObject.activeSelf) {
                        activeChildCount += 1;
                    }
                }
                float dynamicHeight = activeChildCount * optionHeight + padding * 2;
                float viewportHeight = researchContainer.transform.parent.parent.GetComponent<RectTransform>().rect.height;
                researchContainer.transform.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(viewportHeight, dynamicHeight));
            }

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

            static public Image SpawnImageOffset(Transform parent, Sprite sprite, Color colour, Vector2 pivot, Vector2 offsetMin, Vector2 offsetMax) {
                GameObject image = new GameObject();
                image.name = "Image";
                image.transform.parent = parent;
                RectTransform imageTransform = image.AddComponent<RectTransform>();
                imageTransform.pivot = pivot;
                imageTransform.anchorMin = new Vector2(0, 0);
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
        }
    }
}