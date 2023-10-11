using UnityEngine;
using TMPro;

public class ChangeSizeToFirstChildsPreferedSize : UsefullMethods {

    public bool isOnLeftSideOfScreen;

    public void Start() {
        transform.GetComponent<RectTransform>().sizeDelta = new Vector2(
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().preferredWidth,
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().preferredHeight
        );
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            (isOnLeftSideOfScreen ? 1 : -1) * transform.GetChild(0).GetComponent<TextMeshProUGUI>().preferredWidth / 2f, 
            transform.GetComponent<RectTransform>().anchoredPosition.y
        );
    }

}
