using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlexibleList : UsefullMethods {

    float lastSizeX, lastChildCount;

    float inputFieldPreferredHeight;

    RectTransform root;

    const float BACKGROUND_SPACING = 8f, BODY_SPACING = 48f;

    public void Start() {
        root = (RectTransform)transform.parent.parent.parent.parent;
    }

    void UpdateSize() {
        lastSizeX = root.rect.size.x;
        lastChildCount = transform.childCount;
        inputFieldPreferredHeight = transform.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>().preferredHeight;
        transform.parent.GetChild(0).GetComponent<VerticalLayoutGroup>().spacing = BACKGROUND_SPACING;
        transform.GetComponent<VerticalLayoutGroup>().spacing = BODY_SPACING;
        float fontSize = 19.8f;
        float totalHeightOfContent = FixInputFieldItemAndGetHeight(transform.parent.GetChild(0).GetChild(0).GetComponent<RectTransform>(), transform.GetChild(0).GetComponent<RectTransform>(), transform.parent.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>(), fontSize);
        for(int i = 1; i < lastChildCount; i++) {
            totalHeightOfContent += FixItemAndGetHeight(transform.parent.GetChild(0).GetChild(i).GetComponent<RectTransform>(), transform.GetChild(i).GetComponent<RectTransform>(), transform.parent.GetChild(0).GetChild(i).GetChild(0).GetComponent<RectTransform>(), fontSize);
        }
        SetScrollSize(transform.parent.GetComponent<RectTransform>(), totalHeightOfContent);
    }

    float FixItemAndGetHeight(RectTransform itemBackground, RectTransform itemBody, RectTransform itemOtherInformation, float fontSize) {
        FixOtherInformation(itemOtherInformation.GetComponent<TextMeshProUGUI>(), fontSize);
        return BACKGROUND_SPACING + FixBackground(itemBackground, FixBodyAndGetBodyHeight(itemBody.GetComponent<TextMeshProUGUI>(), fontSize), fontSize);
    }

    float FixInputFieldItemAndGetHeight(RectTransform itemBackground, RectTransform itemBody, RectTransform itemOtherInformation, float fontSize) {
        FixOtherInformation(itemOtherInformation.GetComponent<TextMeshProUGUI>(), fontSize);
        return BACKGROUND_SPACING + FixBackground(itemBackground, FixBodyAndGetBodyHeightForInputField(itemBody, fontSize), fontSize);
    }

    void SetScrollSize(RectTransform content, float totalHeightOfContent) {
        float extraScrollSizeY = totalHeightOfContent - content.parent.GetComponent<RectTransform>().rect.size.y;
        if(extraScrollSizeY > 0) {
            content.offsetMin = new Vector2(0, -1 * extraScrollSizeY);
        } else {
            content.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
            content.offsetMin = new Vector2(0, 0);
        }
    }

    float FixBodyAndGetBodyHeight(TextMeshProUGUI textObject, float fontSize) {
        textObject.enableAutoSizing = false;
        textObject.fontSize = fontSize;
        Canvas.ForceUpdateCanvases();
        textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, textObject.GetComponent<TextMeshProUGUI>().preferredHeight);
        return textObject.GetComponent<RectTransform>().rect.size.y;
    }

    float FixBodyAndGetBodyHeightForInputField(RectTransform fieldObject, float fontSize) {
        fieldObject.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().enableAutoSizing = false;
        fieldObject.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().fontSize = fontSize;
        fieldObject.GetComponent<TMP_InputField>().lineType = TMP_InputField.LineType.MultiLineNewline;
        fieldObject.GetComponent<Image>().color = new Color32(19, 19, 19, 255);
        Canvas.ForceUpdateCanvases();
        fieldObject.sizeDelta = new Vector2(0, fieldObject.GetComponent<TMP_InputField>().preferredHeight);
        return fieldObject.rect.size.y;
    }

    float FixBackground(RectTransform background, float bodyHeight, float fontSize) {
        background.sizeDelta = new Vector2(0, fontSize * 2 + bodyHeight);
        return background.rect.size.y;
    }

    void FixOtherInformation(TextMeshProUGUI text, float fontSize) {
        text.enableAutoSizing = false;
        text.enableWordWrapping = false;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.TopRight;
        text.color = new Color32(175, 175, 175, 255);
    }

    public void FixedUpdate() {
        if(transform.childCount == 0) return;
        if(lastSizeX != root.rect.size.x || lastChildCount != transform.childCount || inputFieldPreferredHeight != transform.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>().preferredHeight) {
            UpdateSize();
            return;
        }
        //if this isn't here the text will go above the field
        transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
    }

}
