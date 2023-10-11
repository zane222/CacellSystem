using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnRightClick : MonoBehaviour, IPointerClickHandler {

    public UnityAction action;

    public void SetListener(UnityAction action) {
        this.action = action;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(eventData.button == PointerEventData.InputButton.Right) {
            action();
        }
    }

}
