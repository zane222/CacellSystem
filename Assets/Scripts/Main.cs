using RepairShopRObjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Main : UsefullMethods {

    static GameObject newTicket, ticketList, ticketViewer, newCustomer, viewCustomer, signIn;

    static Transform canvas;

    public static string URL = "https://cacell.repairshopr.com/api/v1", apiKey;

    void Start() {
        canvas = transform;
        newTicket = Resources.Load("Prefabs/NewTicket") as GameObject;
        ticketList = Resources.Load("Prefabs/TicketList") as GameObject;
        ticketViewer = Resources.Load("Prefabs/TicketViewer") as GameObject;
        newCustomer = Resources.Load("Prefabs/NewCustomer") as GameObject;
        viewCustomer = Resources.Load("Prefabs/ViewCustomer") as GameObject;
        signIn = Resources.Load("Prefabs/SignIn") as GameObject;
        GetAPIKey(delegate (string key) {
            apiKey = key;
            StartCoroutine(ExecuteActionEveryFrame(delegate {
                if(GetCurrentURLPath() == perviousURLPath) return;
                perviousURLPath = GetCurrentURLPath();
                RunSceneBasedOnURL(GetCurrentURLPath());
            }));
        });
        transform.GetComponent<CanvasScaler>().scaleFactor = PlayerPrefs.GetFloat("scaleFactor", 1);
        SetUpKeyCombinations();
    }

    void SetUpKeyCombinations() {
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Equals); //plus
        }, delegate {
            if(Input.GetKeyDown(KeyCode.Equals)) {
                if(!(transform.GetComponent<RectTransform>().rect.size.y < 768 * 1.1f || transform.GetComponent<RectTransform>().rect.size.x < 1366 * 1.1f)) GetComponent<CanvasScaler>().scaleFactor += .1f;
                PlayerPrefs.SetFloat("scaleFactor", GetComponent<CanvasScaler>().scaleFactor);
            }
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Minus);
        }, delegate {
            if(Input.GetKeyDown(KeyCode.Minus)) {
                GetComponent<CanvasScaler>().scaleFactor -= .1f;
                PlayerPrefs.SetFloat("scaleFactor", GetComponent<CanvasScaler>().scaleFactor);
            }
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Alpha0);
        }, delegate {
            if(Input.GetKeyDown(KeyCode.Alpha0)) {
                GetComponent<CanvasScaler>().scaleFactor = 1;
                PlayerPrefs.SetFloat("scaleFactor", GetComponent<CanvasScaler>().scaleFactor);
            }
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate { //for some reason Alt + LeftArrow doesn't work by default so I manually made it work
            return (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.LeftArrow);
        }, delegate {
            if(Input.GetKeyDown(KeyCode.LeftArrow)) {
                GoBack();
            }
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.RightArrow);
        }, delegate {
            if(Input.GetKeyDown(KeyCode.RightArrow)) {
                GoForward();
            }
        }));
    }

    public static void RunSceneBasedOnURL(string url) {
        if(url.Length < 2) {
            TicketList();
            return;
        }
        switch(url[1]) {
            case '$':
                if(char.IsDigit(url[^1])) {
                    ViewCustomer(int.Parse(url[2..]));
                    return;
                }
                if(url.Contains("?edit")) {
                    EditCustomer(int.Parse(url[2..^5]));
                    return;
                }
                if(url.Contains("?newticket")) {
                    NewTicket(int.Parse(url[2..^10]));
                    return;
                }
                break;
            case '&':
                if(char.IsDigit(url[^1])) {
                    TicketViewer(int.Parse(url[2..]));
                    return;
                }
                if(url.Contains("?edit")) {
                    EditTicket(int.Parse(url[2..^5]));
                    return;
                }
                break;
            case '#':
                if(char.IsDigit(url[^1])) {
                    TicketViewer(url[2..]);
                    return;
                }
                break;
            default:
                if(url == "/newcustomer") {
                    NewCustomer();
                } else {
                    TicketList();
                }
                break;
        }
    }

    public static void NewTicket(int customerID, string name) {
        ChangeTitle("New Ticket");
        ChangeCurrentURLPath("/$" + customerID + "?newticket");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newTicket, canvas).GetComponent<NewTicketManager>().Constructor(customerID, name);
    }

    public static void NewTicket(int customerID) {
        ChangeTitle("New Ticket");
        ChangeCurrentURLPath("/$" + customerID + "?newticket");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newTicket, canvas).GetComponent<NewTicketManager>().Constructor(customerID, "");
    }

    public static void NewTicketInNewWindow(int customerID) => Application.OpenURL("$" + customerID + "?newticket");

    public static void EditTicket(int ticketID) {
        ChangeTitle("Edit Ticket");
        ChangeCurrentURLPath("/&" + ticketID + "?edit");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newTicket, canvas).GetComponent<NewTicketManager>().Constructor(ticketID);
    }

    public static void EditTicketInNewWindow(int ticketID) => Application.OpenURL("&" + ticketID + "?edit");

    public static void EditTicket(LargeTicket ticket) {
        ChangeTitle("Edit Ticket");
        ChangeCurrentURLPath("/&" + ticket.id + "?edit");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newTicket, canvas).GetComponent<NewTicketManager>().Constructor(ticket);
    }

    public static void EditTicketInNewWindow(LargeTicket ticket) => Application.OpenURL("&" + ticket.id + "?edit");

    public static void TicketList() {
        ChangeTitle("Cacell System");
        ChangeCurrentURLPath("/");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(ticketList, canvas).GetComponent<TicketListManager>().Constructor();
    }

    public static void TicketListInNewWindow() => Application.OpenURL("");

    public static void TicketViewer(int ticketID) {
        ChangeTitle("Ticket");
        ChangeCurrentURLPath("/&" + ticketID);
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(ticketViewer, canvas).GetComponent<TicketViewerManager>().Constructor(ticketID);
    }

    public static void TicketViewerInNewWindow(int ticketID) => Application.OpenURL("&" + ticketID);

    public static void TicketViewer(string number) {
        ChangeTitle("Ticket");
        ChangeCurrentURLPath("/#" + number);
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(ticketViewer, canvas).GetComponent<TicketViewerManager>().Constructor(number);
    }

    public static void TicketViewerInNewWindow(string number) => Application.OpenURL("#" + number);

    public static void TicketViewer(LargeTicket ticket) {
        ChangeTitle("Ticket");
        ChangeCurrentURLPath("/&" + ticket.id);
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(ticketViewer, canvas).GetComponent<TicketViewerManager>().Constructor(ticket);
    }

    public static void TicketViewerInNewWindow(LargeTicket ticket) => Application.OpenURL("&" + ticket.id);

    public static void NewCustomer() {
        ChangeTitle("New Customer");
        ChangeCurrentURLPath("/newcustomer");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newCustomer, canvas).GetComponent<NewCustomerManager>().Constructor();
    }

    public static void NewCustomer(string firstName) {
        ChangeTitle("New Customer");
        ChangeCurrentURLPath("/newcustomer");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newCustomer, canvas).GetComponent<NewCustomerManager>().Constructor(firstName);
    }

    public static void NewCustomer(string firstName, string lastName) {
        ChangeTitle("New Customer");
        ChangeCurrentURLPath("/newcustomer");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newCustomer, canvas).GetComponent<NewCustomerManager>().Constructor(firstName, lastName);
    }

    public static void NewCustomer(long phoneNumber) {
        ChangeTitle("New Customer");
        ChangeCurrentURLPath("/newcustomer");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newCustomer, canvas).GetComponent<NewCustomerManager>().Constructor(phoneNumber);
    }

    public static void NewCustomerInNewWindow() => Application.OpenURL("newcustomer");

    public static void EditCustomer(int customerID) {
        ChangeTitle("Edit Customer");
        ChangeCurrentURLPath("/$" + customerID + "?edit");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newCustomer, canvas).GetComponent<NewCustomerManager>().Constructor(customerID);
    }

    public static void EditCustomerInNewWindow(int customerID) => Application.OpenURL("$" + customerID + "?edit");

    public static void EditCustomer(Customer customer) {
        ChangeTitle("Edit Customer");
        ChangeCurrentURLPath("/$" + customer.id + "?edit");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(newCustomer, canvas).GetComponent<NewCustomerManager>().Constructor(customer);
    }

    public static void EditCustomerInNewWindow(Customer customer) => Application.OpenURL("$" + customer.id + "?edit");

    public static void ViewCustomer(int customerID) {
        ChangeTitle("Customer");
        ChangeCurrentURLPath("/$" + customerID);
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(viewCustomer, canvas).GetComponent<ViewCustomerManager>().Constructor(customerID);
    }

    public static void ViewCustomerInNewWindow(int customerID) => Application.OpenURL("$" + customerID);

    public static void ViewCustomer(Customer customer) {
        ChangeTitle("Customer");
        ChangeCurrentURLPath("/$" + customer.id);
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(viewCustomer, canvas).GetComponent<ViewCustomerManager>().Constructor(customer);
    }

    public static void ViewCustomerInNewWindow(Customer customer) => Application.OpenURL("$" + customer.id);

    public static void GetAPIKey(UnityAction<string> then) {
        ChangeTitle("Cacell System");
        if(canvas.childCount > 0) Destroy(canvas.GetChild(0).gameObject);
        Instantiate(signIn, canvas).GetComponent<SignInManager>().Constructor(then);
    }

}
