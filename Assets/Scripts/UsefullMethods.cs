using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class UsefullMethods : MonoBehaviour {

    [DllImport("__Internal")]
    static extern void alertWeb(string text);

    [DllImport("__Internal")]
    static extern void changeTitleWeb(string text);

    [DllImport("__Internal")]
    static extern void printLabelWeb();
    
    [DllImport("__Internal")]
    static extern string getCurrentURLPathWeb();
    
    [DllImport("__Internal")]
    static extern void changeCurrentURLPathWeb(string text);

    [DllImport("__Internal")]
    static extern void makeTicketLabelImageWeb(string subject, string password, string itemsLeft, string creationDate, string name, string ticketNumber, string phoneNumber, int width);

    [DllImport("__Internal")]
    static extern string getCompletedTicketImageWeb();

    [DllImport("__Internal")]
    static extern void getImageFromURLWeb();

    [DllImport("__Internal")]
    static extern string getCompletedImageFromURLWeb();

    [DllImport("__Internal")]
    static extern bool isDoneMakingTicketImageWeb();

    [DllImport("__Internal")]
    static extern bool isDoneGettingImageFromURLWeb();

    [DllImport("__Internal")]
    static extern void signInWeb();

    [DllImport("__Internal")]
    static extern bool isSignedInWeb();

    [DllImport("__Internal")]
    static extern string getAccessTokenWeb();

    [DllImport("__Internal")]
    static extern void logWeb(string text);

    [DllImport("__Internal")]
    static extern void getClipboardWeb();

    [DllImport("__Internal")]
    static extern string getCompletedClipboardTextWeb();

    [DllImport("__Internal")]
    static extern bool isDoneGettingClipboardWeb();

    [DllImport("__Internal")]
    static extern void goBackWeb();

    [DllImport("__Internal")]
    static extern void goForwardWeb();

    bool isSignedInInEditor = true;

    public static string URLPathInEditor = "/", perviousURLPath = "";

    public static readonly string[] statuses = { "Diagnosing", "Finding Price", "Approval Needed", "Waiting for Parts", "Waiting (Other)", "In Progress", "Ready", "Resolved" };

    public static readonly string[] legacyStatuses = { "New", "Scheduled", "Call Customer", "Waiting for Parts", "Waiting on Customer", "In Progress", "Customer Reply", "Resolved" };

    public static readonly string[] allLetters = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

    public static readonly string[] numbersUpToNinetyNine = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" };
    
    public readonly Color32[] statusColors = new Color32[] {
        new Color32(45, 180, 144, 255),
        new Color32(30, 121, 62, 255),
        new Color32(30, 70, 121, 255),
        new Color32(182, 183, 46, 255),
        new Color32(53, 178, 212, 255),
        new Color32(123, 31, 32, 255),
        new Color32(44, 176, 79, 255),
        new Color32(212, 175, 53, 255)
    };
#if UNITY_EDITOR
    public static string exampleImage = "";

    void Start() {
        if(exampleImage == "") {
            exampleImage = Resources.Load("Text/exampleImage").ToString();
        }
    }
#endif
    public string ConvertFromLegacyStatus(string status, bool isCallBy) {
        int index = ConvertFromLegacyStatusToStatusesIndex(status, isCallBy);
        if(index == -1) return status;
        return statuses[index];
    }

    public int ConvertFromLegacyStatusToStatusesIndex(string status, bool isCallBy) {
        if(status == "Ready!") return IndexOf(statuses, "Ready");
        if(IndexOf(statuses, status) != -1) {
            return IndexOf(statuses, status);
        } else if(IndexOf(legacyStatuses, status) != -1) {
            if(!isCallBy && IndexOf(legacyStatuses, status) == 0) return IndexOf(statuses, "In Progress");
            else return IndexOf(legacyStatuses, status);
        } else return -1;
    }

    public void PrintLabel() {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            printLabelWeb();
        } else {
            print("Label printed");
        }
    }

    public void MakeTicketLabelImage(string subject, string password, string itemsLeft, string creationDate, string name, string ticketNumber, string phoneNumber, int width, UnityAction<Sprite> then) {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            makeTicketLabelImageWeb(subject, password, itemsLeft, creationDate, name, ticketNumber, phoneNumber, width);
            StartCoroutine(ExecuteActionWhenTrue(delegate { return isDoneMakingTicketImageWeb(); }, delegate {
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(Convert.FromBase64String(getCompletedTicketImageWeb().Split(',')[1]));
                then(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero)); 
            }));
        } else {
#if UNITY_EDITOR
            StartCoroutine(ExecuteActionWhenTrue(delegate { return true; }, delegate {
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(Convert.FromBase64String(exampleImage.Split(',')[1]));
                then(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero));
            }));
#endif
        }
    }

    public void GetClipboard(UnityAction<string> then) {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            getClipboardWeb();
            StartCoroutine(ExecuteActionWhenTrue(delegate { return isDoneGettingClipboardWeb(); }, delegate {
                then(getCompletedClipboardTextWeb());
            }));
        } else {
            StartCoroutine(ExecuteActionWhenTrue(delegate { return true; }, delegate {
                then(GUIUtility.systemCopyBuffer);
            }));
        }
    }

    public void GoBack() {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            goBackWeb();
        }
    }

    public void GoForward() {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            goForwardWeb();
        }
    }

    public string[] StringsFromIndexes(string[] strings, int[] indexes) {
        List<string> onlyStringsFromIndexes = new List<string>();
        for(int i = 0; i < indexes.Length; i++) {
            onlyStringsFromIndexes.Add(strings[indexes[i]]);
        }
        return onlyStringsFromIndexes.ToArray();
    }

    public static string StringArrayToSingleStringWithPlusSignEveryOtherTime(string[] strings) {
        if(strings.Length == 0) return "";
        string s = "";
        for(int i = 0; i < strings.Length; i++) {
            s += strings[i] + (i / 2 == i / 2f ? " " : " + ");
        }
        return s[0..^3];
    }

    public int IndexOf<T>(T[] array, T item) {
        for(int i = 0; i < array.Length; i++) {
            if(EqualityComparer<T>.Default.Equals(array[i], item)) {
                return i;
            }
        }
        return -1;
    }

    public int IndexOf<T>(List<T> array, T item) {
        for(int i = 0; i < array.Count; i++) {
            if(EqualityComparer<T>.Default.Equals(array[i], item)) {
                return i;
            }
        }
        return -1;
    }

    public int IndexOf(string[] array, string item, bool ignoreCaps) {
        if(!ignoreCaps) return IndexOf(array, item);
        if(item == "" || item == null) return -1;
        for(int i = 0; i < array.Length; i++) {
            if(array[i].ToLower() == item.ToLower()) {
                return i;
            }
        }
        return -1;
    }

    public static T[] InterlaceArrays<T>(T[] one, T[] two) {
        T[] newArray = new T[one.Length * 2];
        for(int i = 0; i < one.Length; i++) {
            newArray[i * 2] = one[i];
        }
        for(int i = 0; i < one.Length; i++) {
            newArray[i * 2 + 1] = two[i];
        }
        return newArray;
    }

    T[] RemoveDifferenceOfArrays<T>(T[] array1, T[] array2) {
        List<T> newArray = new List<T>();
        if(SmallerArray(array1, array2) != array1) {
            T[] temp = array1;
            array1 = array2;
            array2 = temp;
        }
        for(int i = 0; i < array1.Length; i++) {
            if(Contains(array2, array1[i])) {
                newArray.Add(array1[i]);
            }
        }
        return newArray.ToArray();
    }

    T[] SmallerArray<T>(T[] array1, T[] array2) {
        return array1.Length > array2.Length ? array2 : array1;
    }

    public string GetWordBeforeIndex(string multipleWords, int index) {
        return multipleWords[..index].Split(' ')[^2];
    }

    public string GetWordAfterIndex(string multipleWords, int index) {
        return multipleWords.Split(' ')[multipleWords[..index].Split(' ').Length];
    }

    public string FormatToPhoneNumber(string number) {
        try {
            if(ParsePhoneNumber(number).Length != 10) return number;
            return string.Format("{0:###-###-####}", double.Parse(ParsePhoneNumber(number)));
        } catch {
            return number;
        }
    }

    public static string AddSuffix(int num) {
        string number = num.ToString();
        if(number.EndsWith("11")) return num + "th";
        if(number.EndsWith("12")) return num + "th";
        if(number.EndsWith("13")) return num + "th";
        if(number.EndsWith("1")) return num + "st";
        if(number.EndsWith("2")) return num + "nd";
        if(number.EndsWith("3")) return num + "rd";
        return num + "th";
    }

    public int CloseTime(DayOfWeek dayOfWeek) {
        int hour;
        if((int)dayOfWeek >= 1 && (int)dayOfWeek <= 5) {
            hour = 18;
        } else if((int)dayOfWeek == 6) {
            hour = 16;
        } else {
            hour = 0;
        }
        return hour;
    }

    public string DateFormatedToString(DateTime date) {
        if(date == new DateTime() || date == null) return "";
        return date.Month + "/" + date.Day + "/" + date.Year;
    }

    public string DateFormatedToString(string date) {
        return DateFormatedToString(DateTime.Parse(date));
    }

    public string TimeFormatedToString(DateTime date) {
        if(date == new DateTime() || date == null) return "";
        bool pm = false;
        string minute = date.Minute.ToString();
        string hour = date.Hour.ToString();
        if(date.Minute < 10) minute = "0" + minute;
        if(date.Hour > 12) {
            hour = (date.Hour - 12).ToString();
            pm = true;
        }
        if(hour == "0") hour = "12";
        return hour + ":" + minute + (pm ? " PM" : " AM");
    }

    public string TimeFormatedToString(string date) {
        return TimeFormatedToString(DateTime.Parse(date));
    }

    public static string DueDateFormatedToString(DateTime dueDate, DateTime createdAt) {
        if(dueDate == new DateTime() || dueDate == null) return "";
        if(createdAt == dueDate) return "";
        if(dueDate.Subtract(createdAt).Days >= 1) return AddSuffix(dueDate.Day);
        bool pm = false;
        string minute = dueDate.Minute.ToString();
        string hour = dueDate.Hour.ToString();
        if(dueDate.Minute < 10) minute = "0" + minute;
        if(dueDate.Hour > 12) {
            hour = (dueDate.Hour - 12).ToString();
            pm = true;
        }
        if(hour == "0") hour = "12";
        return hour + ":" + minute + (pm ? " PM" : " AM");
    }

    public static bool Contains<T>(T[] array, T target) {
        for(int i = 0; i < array.Length; i++) {
            if(array[i].Equals(target)) return true;
        }
        return false;
    }

    public static bool Contains<T>(List<T> array, T target) {
        for(int i = 0; i < array.Count; i++) {
            if(array[i].Equals(target)) return true;
        }
        return false;
    }

    public string[] GetLinksFromText(string text) {
        string newText = text.Replace("&amp;", "&");
        List<string> links = new List<string>();
        for(int i = 0; newText.Contains("http"); i++) {
            int endOfThisLink;
            if(newText[newText.IndexOf("http")..].Contains(" ")) {
                if(!newText[newText.IndexOf("http")..].Contains("\n")) {
                    endOfThisLink = newText[newText.IndexOf("http")..].IndexOf(" ");
                } else {
                    if(newText[newText.IndexOf("http")..].IndexOf("\n") < newText[newText.IndexOf("http")..].IndexOf(" ")) {
                        endOfThisLink = newText[newText.IndexOf("http")..].IndexOf("\n");
                    } else {
                        endOfThisLink = newText[newText.IndexOf("http")..].IndexOf(" ");
                    }
                }
            } else if(newText[newText.IndexOf("http")..].Contains("\n")) {
                endOfThisLink = newText[newText.IndexOf("http")..].IndexOf("\n");
            } else {
                endOfThisLink = newText[newText.IndexOf("http")..].Length;
            }
            links.Add(newText.Substring(newText.IndexOf("http"), endOfThisLink));
            newText = newText.Replace(links[i], "");
        }
        return links.ToArray();
    }

    public string ReplaceMultiple(string text, string[] targets, string replacementText) {
        for(int i = 0; i < targets.Length; i++) {
            text = text.Replace(targets[i], replacementText);
        }
        return text;
    }

    public static void Alert(string text) {
        if(text.Contains("Service Unavailable")) {
            text = "Repairshopr is having maintenance, redirecting to another website to give more information";
            Application.OpenURL("https://www.repairshoprstatus.com/");
        }
        if(text.Contains("429 Too Many Requests")) {
            text = "Repairshopr only allows 180 requests per minute. Please wait for five minutes and then try again.";
        }
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            alertWeb(text);
        } else {
            print(text);
        }
    }

    public static void ChangeTitle(string title) {
        if(title != "Cacell System") title += " | Cacell System";
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            changeTitleWeb(title);
        } else {
            //print("title changed to " + title);
        }
    }

    public void SignIn() {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            signInWeb();
        } else {
            isSignedInInEditor = true;
            print("Signing in");
        }
    }
    
    public bool IsSignedIn() {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            return isSignedInWeb();
        } else {
            return isSignedInInEditor;
        }
    }

    public string GetAccessToken() {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            return getAccessTokenWeb();
        } else {
            return "p";
        }
    }

    public static void Log(string text) {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            logWeb(text);
        } else {
            print(text);
        }
    }

    public static void ChangeCurrentURLPath(string path) {
        if(path == GetCurrentURLPath()) return;
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            perviousURLPath = path;
            changeCurrentURLPathWeb(path);
        } else {
            URLPathInEditor = path;
            perviousURLPath = path;
        }
    }

    public static string GetCurrentURLPath() {
        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            return getCurrentURLPathWeb();
        } else {
            return URLPathInEditor;
        }
    }

    string FormatSingleName(string name) {
        string newName = name.Replace("​", "");
        if(newName.Contains(" ")) return newName;
        if(newName.Length < 2) return newName;
        if(newName[1].ToString() == newName[1].ToString().ToLower() && newName[0].ToString() == newName[0].ToString().ToUpper()) return newName;
        return (newName[0].ToString().ToUpper() + newName[1..].ToLower()).Replace("​", "");
    }

    public string FormatName(string name) {
        if(!name.Contains(" ")) return FormatSingleName(name);
        name = name.Replace("  ", " ");
        string newName = "";
        for(int i = 0; name.Contains(" "); i++) {
            newName += FormatSingleName(name.Substring(0, name.IndexOf(" "))) + " ";
            name = name[(name.IndexOf(" ") + 1)..];
        }
        newName += FormatSingleName(name);
        return newName;
    }

    public string ParsePhoneNumber(string phoneNumber) {
        return phoneNumber.Replace("​", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");
    }

    public bool CanParseStringArray(string[] array, string[] ignoreIfItsTheSameAsThis, string removeFromEachStringWhenChecking) {
        foreach(string text in array) {
            if(Contains(ignoreIfItsTheSameAsThis, text)) continue;
            if(!CanParse(text.Replace(removeFromEachStringWhenChecking, ""))) return false;
        }
        return true;
    }

    public bool CanParseStringArray(string[] array) {
        foreach(string text in array) if(!CanParse(text)) return false;
        return true;
    }

    public bool CanParse(string text) {
        try {
            _ = long.Parse(text);
            return true;
        } catch {
            return false;
        }
    }

    public IEnumerator ExecuteActionWhenTrue(Func<bool> predicate, UnityAction action) {
        yield return new WaitUntil(predicate);
        action();
    }

    public IEnumerator ExecuteActionWhileTrue(Func<bool> predicate, UnityAction action) {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(predicate);
        action();
        StartCoroutine(ExecuteActionWhileTrue(predicate, action));
    }

    public IEnumerator ExecuteActionWhileTrueButOnlyStartAfterNotTrue(Func<bool> predicate, UnityAction action) {
        yield return new WaitWhile(predicate);
        yield return new WaitUntil(predicate);
        action();
        StartCoroutine(ExecuteActionWhileTrue(predicate, action));
    }

    public IEnumerator ExecuteActionEveryFrame(UnityAction action) {
        yield return new WaitForFixedUpdate();
        action();
        StartCoroutine(ExecuteActionEveryFrame(action));
    }

    public IEnumerator ExecuteActionWhileTrue(YieldInstruction timeBetweenEachCheck, Func<bool> predicate, UnityAction action) {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(predicate);
        action();
        yield return timeBetweenEachCheck;
        StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, predicate, action));
    }

    public IEnumerator ExecuteActionWhileTrue(float timeBetweenEachCheck, Func<bool> predicate, UnityAction action) {
        StartCoroutine(ExecuteActionWhileTrue(new WaitForSeconds(timeBetweenEachCheck), predicate, action));
        yield break;
    }

    public IEnumerator ExecuteActionWhileTrue(Func<float> timeBetweenEachCheck, Func<bool> overrideIfTrue, Func<bool> predicate, UnityAction action) {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(predicate);
        action();
        bool wasOverriden = false;
        Coroutine overrider = StartCoroutine(ExecuteActionWhenTrue(overrideIfTrue, delegate {
            wasOverriden = true;
            StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, overrideIfTrue, predicate, action));
        }));
        yield return new WaitForSeconds(timeBetweenEachCheck());
        if(!wasOverriden) {
            StopCoroutine(overrider);
            StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, overrideIfTrue, predicate, action));
        }
    }

    public IEnumerator ExecuteActionWhileTrue(Func<YieldInstruction> timeBetweenEachCheck, Func<bool> overrideIfTrue, Func<bool> predicate, UnityAction action) {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(predicate);
        action();
        bool wasOverriden = false;
        Coroutine overrider = StartCoroutine(ExecuteActionWhenTrue(overrideIfTrue, delegate {
            wasOverriden = true;
            StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, overrideIfTrue, predicate, action));
        }));
        yield return timeBetweenEachCheck();
        if(!wasOverriden) {
            StopCoroutine(overrider);
            StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, overrideIfTrue, predicate, action));
        }
    }

    public IEnumerator ExecuteActionWhileTrue(YieldInstruction timeBetweenEachCheck, Func<bool> overrideIfTrue, Func<bool> predicate, UnityAction action) {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(predicate);
        action();
        bool wasOverriden = false;
        Coroutine overrider = StartCoroutine(ExecuteActionWhenTrue(overrideIfTrue, delegate {
            wasOverriden = true;
            StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, overrideIfTrue, predicate, action));
        }));
        yield return timeBetweenEachCheck;
        if(!wasOverriden) {
            StopCoroutine(overrider);
            StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, overrideIfTrue, predicate, action));
        }
    }

    public IEnumerator ExecuteActionWhileTrue(Func<bool> dontStartCheckingAgainUntilThisIsTrue, Func<bool> predicate, UnityAction action) {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(predicate);
        action();
        yield return new WaitUntil(dontStartCheckingAgainUntilThisIsTrue);
        StartCoroutine(ExecuteActionWhileTrue(dontStartCheckingAgainUntilThisIsTrue, predicate, action));
    }

    public IEnumerator ExecuteActionWhileTrue(float timeBetweenEachCheck, Func<bool> overrideIfTrue, Func<bool> predicate, UnityAction action) {
        StartCoroutine(ExecuteActionWhileTrue(new WaitForSeconds(timeBetweenEachCheck), overrideIfTrue, predicate, action));
        yield break;
    }

    public IEnumerator ExecuteActionWhileTrue(Func<float> timeBetweenEachCheck, Func<bool> predicate, UnityAction action) {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(predicate);
        action();
        yield return new WaitForSeconds(timeBetweenEachCheck());
        StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, predicate, action));
    }

    public IEnumerator ExecuteActionWhileTrue(Func<YieldInstruction> timeBetweenEachCheck, Func<bool> predicate, UnityAction action) {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(predicate);
        action();
        yield return timeBetweenEachCheck();
        StartCoroutine(ExecuteActionWhileTrue(timeBetweenEachCheck, predicate, action));
    }

    public IEnumerator ExecuteActionIfTrueAfterTime(Func<bool> predicate, YieldInstruction time, UnityAction action) {
        yield return time;
        if(predicate()) action();
    }

    public IEnumerator ExecuteActionIfTrueAfterTime(Func<bool> predicate, float time, UnityAction action) {
        StartCoroutine(ExecuteActionIfTrueAfterTime(predicate, new WaitForSeconds(time), action));
        yield break;
    }

    public IEnumerator ExecuteActionInTheNextFrame(UnityAction action) {
        yield return new WaitForFixedUpdate();
        action();
    }

    public IEnumerator ExecuteActionAfterFrames(int frames, UnityAction action) {
        StartCoroutine(ExecuteActionInTheNextFrame(delegate {
            frames--;
            if(frames < 1) action();
            else StartCoroutine(ExecuteActionAfterFrames(frames, action));
        }));
        yield break;
    }

    public IEnumerator ExecuteActionAfterTime(YieldInstruction time, UnityAction action) {
        yield return time;
        action();
    }

}
