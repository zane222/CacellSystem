using Proyecto26;
using RepairShopRObjects;
using TMPro;
using UnityEngine.Events;
using UnityEngine;

public class SignInManager : UsefullMethods {

    const int INDEX_OF_NOT_SIGNED_IN_TEXT = 2;

    string accessTokenThatDidntWork = "";

    UnityAction<string> then;

    public void Constructor(UnityAction<string> then) {
        this.then = then;
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        }, delegate {
            SignInButton();
        }));
        StartCoroutine(ExecuteActionWhenTrue(delegate { 
            return IsSignedIn(); 
        }, delegate { 
            if(GetAccessToken() != accessTokenThatDidntWork) GetRepairShopRAPIKey(); 
        }));
        if(IsSignedIn()) return;
        RestClient.DefaultRequestHeaders["Authorization"] = PlayerPrefs.GetString("accessToken");
        RestClient.Get<ApiKey>("https://cacellsystem-default-rtdb.firebaseio.com/key1.json").Then(response => {
            RestClient.ClearDefaultHeaders();
            then(response.key);
            return;
        });
    }

    public void SignInButton() {
        SignIn();
    }

    void GetRepairShopRAPIKey() {
        string accessToken = "Bearer " + GetAccessToken();
        RestClient.DefaultRequestHeaders["Authorization"] = accessToken;
        RestClient.Get<ApiKey>("https://cacellsystem-default-rtdb.firebaseio.com/key1.json").Then(response => {
            RestClient.ClearDefaultHeaders();
            PlayerPrefs.SetString("accessToken", accessToken);
            then(response.key);
        }).Catch(error => {
            if(error.Message.Contains("Unauthorized")) {
                transform.GetChild(INDEX_OF_NOT_SIGNED_IN_TEXT).GetComponent<TextMeshProUGUI>().text = "You have successfully signed into your Google account but it is not on the whitelist, try another one";
                accessTokenThatDidntWork = accessToken;
#if UNITY_EDITOR
                then("API_KEY");
#endif
            } else {
                Alert("Can't sign in because: " + error.Message);
            }
            StartCoroutine(ExecuteActionWhenTrue(delegate {
                return IsSignedIn();
            }, delegate {
                GetRepairShopRAPIKey();
            }));
        });
    }

}
