using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCircle : UsefullMethods {

    RectTransform rectTransform;

    void Start() {
        rectTransform = GetComponent<RectTransform>();
        StartCoroutine(Rotate());
#if UNITY_EDITOR
        transform.parent.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
#endif
    }

    IEnumerator Rotate() {
        yield return new WaitForSeconds(.001f);
        rectTransform.Rotate(0, 0, 1, 0);
        StartCoroutine(Rotate());
    }

}
