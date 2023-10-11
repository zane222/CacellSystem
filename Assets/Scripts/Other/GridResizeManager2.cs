using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridResizeManager2 : UsefullMethods {

    RectTransform canvas;

    GridLayoutGroup grid;

    float lastResX, lastResY, lastChildCount, width;

    public bool isBackground, isSubject, isComment;

    void Start() {
        canvas = (RectTransform)transform.parent.parent.parent.parent;
        grid = GetComponent<GridLayoutGroup>();
        SetGrid();
    }

    void SetGrid() {
        lastResX = canvas.rect.size.x;
        lastResY = canvas.rect.size.y;
        lastChildCount = transform.childCount;
        width = GetComponent<RectTransform>().anchorMax.x - GetComponent<RectTransform>().anchorMin.x;
        grid.cellSize = new Vector2(transform.parent.parent.parent.GetComponent<RectTransform>().rect.size.x * width - transform.parent.parent.parent.GetChild(1).GetComponent<RectTransform>().rect.size.x, isBackground ? 34.5384f : 22.14f /*lastResY * (isBackground ? .03198f : .0205f)*/);
        grid.spacing = new Vector2(0, isBackground ? 5.3136f : 17.712f /*lastResY * (isBackground ? .00492f : .0164f)*/);
        Canvas.ForceUpdateCanvases();
        SetScrollSize();
        StartCoroutine(CheckIfResolutionChanged());
    }

    void SetScrollSize() {
        if(!isBackground) return;
        float extraScrollSizeY = lastChildCount * (grid.cellSize.y + grid.spacing.y) - transform.parent.parent.GetComponent<RectTransform>().rect.size.y;
        if(extraScrollSizeY > 0) {
            transform.parent.GetComponent<RectTransform>().offsetMin = new Vector2(0, -1 * (extraScrollSizeY - transform.parent.GetComponent<RectTransform>().offsetMax.y));
        } else {
            transform.parent.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        }
    }

    IEnumerator CheckIfResolutionChanged() {
        if(lastResX != canvas.rect.size.x || lastResY != canvas.rect.size.y || lastChildCount != transform.childCount) {
            SetGrid();
        } else {
            yield return new WaitForSeconds(0f);
            StartCoroutine(CheckIfResolutionChanged());
        }
    }

}
