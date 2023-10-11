using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CtrlZForTMPInputField : MonoBehaviour {

    List<string> textBefore = new List<string>();

    void Start() {
        textBefore.Add("");
        transform.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate (string text) {
            try {
                if(text.Length > 0) {
                    if(text[transform.GetComponent<TMP_InputField>().caretPosition - 1] == ' ' || text[transform.GetComponent<TMP_InputField>().caretPosition - 1] == '\n' || text[transform.GetComponent<TMP_InputField>().caretPosition - 1] == '.' || text[transform.GetComponent<TMP_InputField>().caretPosition - 1] == ',') textBefore.Add(text[..^1]);
                }
            } catch { }
        });
    }

    void Update() {
        if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Z)) {
            if(textBefore.Count > 0 && transform.GetComponent<TMP_InputField>().isFocused) {
                transform.GetComponent<TMP_InputField>().text = textBefore[^1];
                textBefore.Remove(textBefore[^1]);
            }
        }
    }

}
