using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Proyecto26;
using RepairShopRObjects;
using System.Linq;
using UnityEngine.Events;
using Newtonsoft.Json;

public class NewTicketManager : UsefullMethods {

    public readonly int[] INDEXES_OF_GRIDS_WITH_BOXES = new int[] { INDEX_OF_PROBLEM, INDEX_OF_ITEMS_LEFT },
        INDEXES_OF_GRIDS_WITH_PRICE_BOXES = new int[] { INDEX_OF_PROBLEM },
        INDEXES_OF_GRIDS_WITH_PLUS_BUTTON = new int[] { INDEX_OF_PROBLEM },
        INDEX_OF_GRIDS_WITH_BUTTONS = new int[] { INDEX_OF_DEVICE, INDEX_OF_BRAND, INDEX_OF_COLOR, INDEX_OF_HOW_LONG, INDEX_OF_DATA },
        INDEX_OF_GRIDS_WITH_OTHER_FIELDS = new int[] { INDEX_OF_DEVICE, INDEX_OF_BRAND, INDEX_OF_HOW_LONG, INDEX_OF_ITEMS_LEFT, INDEX_OF_DATA },
        INDEX_OF_GRIDS_WITH_ONLY_FIELD = new int[] { INDEX_OF_MODEL, INDEX_OF_OTHER, INDEX_OF_PASSWORD, INDEX_OF_LEGACY_SUBJECT };

    string[][] brands = { //can change based on the device
        new string[] { "iPhone", "Samsung", "Moto", "LG", "Pixel", "Revvl", "OnePlus", "" }, //phone
        new string[] { "iPad", "Samsung", ""}, //tablet
        new string[] { "MacBook", "HP", "Dell", "Lenovo", "Asus", "Acer", "Toshiba", "" }, //laptop
        new string[] { "Apple", "HP", "Dell", "Lenovo", "Asus", "Acer", "Toshiba", "" }, //desktop
        new string[] { "iMac", "HP", "Dell", "Lenovo", "Asus", "Acer", "Toshiba", "" }, //aio
        new string[] { "Apple", "" }, //smart watch
        new string[] { "PlayStation", "XBox", "Switch", "" }, //console
        new string[] { "" } //other
    };
    
    List<string>[] problems = { //can change based on the device
        new List<string>(new string[] { "LCD", "Battery", "Charge port", "Back glass", "Camera lens", "Camera issues", "Speaker issues", "Microphone", "No power", "No display", "Won't boot", "Liquid damage", "Reset" }), //phone
        new List<string>(new string[] { "LCD", "Battery", "Charge port", "Camera lens", "Camera issues", "Speaker issues", "No power", "No display", "Won't boot", "Liquid damage", "Reset" }), //tablet
        new List<string>(new string[] { "LCD", "No power", "No display", "Won't boot", "Install Kaspersky", "Install SSD", "Slow", "Hinges", "Keyboard", "Install Office", "Clean virus", "Liquid damage", "Reset" }), //laptop
        new List<string>(new string[] { "No power", "No display", "Won't boot", "Install Kaspersky", "Install SSD", "Slow", "Install Office", "Clean virus", "Reset" }), //desktop
        new List<string>(new string[] { "No power", "No display", "Won't boot", "Install Kaspersky", "Install SSD", "Slow", "Install Office", "Clean virus", "Reset" }), //aio
        new List<string>(new string[] { "LCD", "Battery", "Connection issues", "No power", "No display", "Won't boot", "Liquid damage", "Reset" }), //smart watch
        new List<string>(new string[] { "LCD", "Battery", "Charge port", "No power", "No display", "Won't boot", "Reset" }), //console
        new List<string>(new string[] { }) //other
    };

    string[] 
        devices = new string[] { "Phone", "Tablet", "Laptop", "Desktop", "All in one", "Watch", "Console", "" }, 
        howLong = new string[] { "30 min", "45 min", "2 hours", "4 hours", "1 day", "3 days", "" }, 
        itemsLeft = new string[] { "Charger", "Case", "" }, 
        needData = new string[] { "No data", "Save data", "Save data & programs", "" }, 
        colors = new string[] { "Purple", "Orange", "Black", "Gray", "White", "Yellow", "Pink", "Blue", "Brown", "Green", "Red", "Silver", "Gold", "Rose Gold" };

    long[] howManyMinutes = new long[] { 30, 45, 120, 240, 1440, 4320, 0 };

    readonly Color32[] colorsForColorButtons = new Color32[] { 
        new Color32(163, 44, 196, 255), 
        new Color32(255, 140, 0, 255), 
        new Color32(36, 36, 36, 255), 
        new Color32(128, 128, 128, 255), 
        new Color32(255, 255, 255, 255), 
        new Color32(255, 255, 0, 255), 
        new Color32(252, 142, 172, 255), 
        new Color32(0, 0, 195, 255), 
        new Color32(130, 65, 0, 255), 
        new Color32(0, 178, 0, 255), 
        new Color32(154, 0, 0, 255), 
        new Color32(255, 255, 255, 255), 
        new Color32(255, 215, 0, 255),
        new Color32(219, 154, 163, 255)
    };

    readonly bool[] colorIsShiny = {
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true
    };

    DateTime creationDate = DateTime.Now;

    GameObject button, shinyButton, box, selectedIconButton, selectedIconBox, field, plusButton, text, priceBox, doItIfBox, empty, keybindsObject;

    LargeTicket ticketBeingEdited = null;

    int customerID;

    const int INDEX_OF_DEVICE = 0, INDEX_OF_BRAND = 1, INDEX_OF_MODEL = 2, INDEX_OF_COLOR = 3, INDEX_OF_PROBLEM = 4, INDEX_OF_HOW_LONG = 5, INDEX_OF_PASSWORD = 6, INDEX_OF_ITEMS_LEFT = 7, INDEX_OF_DATA = 8, INDEX_OF_OTHER = 9, INDEX_OF_LEGACY_SUBJECT = 10, INDEX_OF_LEGACY_SUBJECT_PANNEL = 11, INDEX_OF_CACELL_SYSTEM_PANNEL = 12, INDEX_OF_VIEW_TICKET_BUTTON = 13, INDEX_OF_VIEW_CUSTOMER_BUTTON = 14, INDEX_OF_CREATE_OR_EDIT_BUTTON = 15, INDEX_OF_LOADING_PANNEL = 18, INDEX_OF_KEYBINDS_BUTTON = 17;

    int selectedGrid = -1;

    void Constructor() {
        button = Resources.Load("Prefabs/UI/Button") as GameObject;
        shinyButton = Resources.Load("Prefabs/UI/ShinyButton") as GameObject;
        box = Resources.Load("Prefabs/UI/Box") as GameObject;
        selectedIconButton = Resources.Load("Prefabs/UI/SelectedIconButton") as GameObject;
        selectedIconBox = Resources.Load("Prefabs/UI/SelectedIconBox") as GameObject;
        text = Resources.Load("Prefabs/UI/Text") as GameObject;
        priceBox = Resources.Load("Prefabs/UI/PriceBox") as GameObject;
        doItIfBox = Resources.Load("Prefabs/UI/DoItIfBox") as GameObject;
        empty = Resources.Load("Prefabs/UI/Empty") as GameObject;
        field = Resources.Load("Prefabs/UI/Field") as GameObject;
        plusButton = Resources.Load("Prefabs/UI/PlusButton") as GameObject;
        keybindsObject = Resources.Load("Prefabs/Keybinds/NewTicketKeybinds") as GameObject;
        transform.GetChild(INDEX_OF_KEYBINDS_BUTTON).GetComponent<Button>().onClick.AddListener(delegate {
            GameObject a = Instantiate(keybindsObject, transform);
            a.GetComponent<Button>().onClick.AddListener(delegate {
                Destroy(a);
            });
        });
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        transform.GetChild(INDEX_OF_LEGACY_SUBJECT).gameObject.SetActive(true);
        Instantiate(field, transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0));
        transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMin = new Vector2(.01f, .11f);
        transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(.99f, .89f);
        transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).GetComponent<Button>().onClick.AddListener(delegate {
            transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.SetActive(false);
            transform.GetChild(INDEX_OF_CACELL_SYSTEM_PANNEL).gameObject.SetActive(true);
            if(!transform.GetChild(INDEX_OF_BRAND).gameObject.activeSelf) ButtonPressed(transform.GetChild(INDEX_OF_DEVICE).GetChild(0).GetChild(devices.Length - 1));
            StartCoroutine(ExecuteActionInTheNextFrame(delegate { SelectGrid(INDEX_OF_LEGACY_SUBJECT); }));
        });
        transform.GetChild(INDEX_OF_CACELL_SYSTEM_PANNEL).GetChild(0).GetComponent<Button>().onClick.AddListener(delegate {
            transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.SetActive(true);
            transform.GetChild(INDEX_OF_CACELL_SYSTEM_PANNEL).gameObject.SetActive(false);
            SelectGrid(INDEX_OF_DEVICE);
        });
        transform.GetChild(INDEX_OF_CACELL_SYSTEM_PANNEL).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate {
            transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.SetActive(true);
            transform.GetChild(INDEX_OF_CACELL_SYSTEM_PANNEL).gameObject.SetActive(false);
            SelectGrid(INDEX_OF_DEVICE);
        });
        SetUpKeyCombinations();
    }

    public void Constructor(int customerID, string name) {
        if(name == "") {
            RestClient.Get(Main.URL + "/customers/" + customerID).Then(response => {
                string newName = JsonConvert.DeserializeObject<OneCustomer>(response.Text).customer.business_and_full_name;
                Constructor(customerID, (newName != "" && newName != null) ? newName : "(empty)");
            }).Catch(error => { Alert("Can't view customer because: " + error.Message); });
            return;
        }
        Constructor();
        this.customerID = customerID;
        ReplaceButtons(transform.GetChild(0), devices);
        SetUpButtonsInCorners(
            "New ticket", delegate { }, delegate { }, name, 
            delegate {
                Main.ViewCustomer(customerID);
                Destroy(transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).gameObject);
            },
            delegate {
                Main.ViewCustomerInNewWindow(customerID);
            },
            "Create",
            CreateButton
        );
        StartCoroutine(ExecuteActionAfterFrames(1, delegate { 
            SelectGrid(INDEX_OF_DEVICE);
            Destroy(transform.GetChild(INDEX_OF_LOADING_PANNEL).gameObject);        
        }));
    }

    public void Constructor(int ticketID) { //edit ticket
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        RestClient.Get(Main.URL + "/tickets/" + ticketID).Then(response => {
            Constructor(JsonConvert.DeserializeObject<OneLargeTicket>(response.Text).ticket);
        }).Catch(error => { Alert("Can't edit ticket because: " + error.Message); });
    }

    public void Constructor(LargeTicket ticket) { //edit ticket
        Constructor();
        FillInTicketWithPreviousValues(ticket);
        bool hasTicketChanged = false;
        StartCoroutine(ExecuteActionWhileTrue(30, delegate { return !hasTicketChanged; }, delegate {
            HasTicketChanged(delegate {
                hasTicketChanged = true;
                Alert("The ticket has just been edited, reload to see the changes. Pressing 'apply edit' may override the changes just now made to the ticket");
            });
        }));
    }

    void HasTicketChanged(UnityAction TicketHasChanged) {
        RestClient.Get(Main.URL + "/tickets/" + ticketBeingEdited.id).Then(response => {
            if(this == null) return;
            LargeTicket ticket = JsonConvert.DeserializeObject<OneLargeTicket>(response.Text).ticket;
            if(ticket.GetPassword() != ticketBeingEdited.GetPassword() || ticket.customer.phone != ticketBeingEdited.customer.phone || ticket.subject != ticketBeingEdited.subject || ticket.comments.Length != ticketBeingEdited.comments.Length) {
                TicketHasChanged();
                return;
            }
        }).Catch(error => { });
    }

    void SetUpKeyCombinations() {
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        }, delegate {
            transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate { return !Input.GetKey(KeyCode.Tab); }, delegate {
            return Input.GetKeyDown(KeyCode.Tab);
        }, delegate {
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                SelectPreviousGrid();
            } else {
                SelectNextGrid();
            }
        }));
        for(int i = 1; i < 10; i++) {
            int x = i;
            StartCoroutine(ExecuteActionWhileTrue(delegate {
                return Input.GetKeyDown((KeyCode)(x + 48));
            }, delegate {
                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift)) {
                    SelectOption(transform.GetChild(selectedGrid).GetChild(0).childCount - x);
                } else {
                    SelectOption(x - 1);
                }
            }));
        }
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.H)) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            for(int i = INDEX_OF_DEVICE; i < INDEX_OF_CACELL_SYSTEM_PANNEL; i++) {
                if(IsAnyFieldFocused(i)) return;
            }
            Main.TicketList();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.C) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            for(int i = INDEX_OF_DEVICE; i <= INDEX_OF_CACELL_SYSTEM_PANNEL; i++) {
                if(IsAnyFieldFocused(i)) return;
            }
            transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.T) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            for(int i = INDEX_OF_DEVICE; i <= INDEX_OF_CACELL_SYSTEM_PANNEL; i++) {
                if(IsAnyFieldFocused(i)) return;
            }
            transform.GetChild(INDEX_OF_VIEW_TICKET_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.L) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            for(int i = INDEX_OF_DEVICE; i <= INDEX_OF_CACELL_SYSTEM_PANNEL; i++) {
                if(IsAnyFieldFocused(i)) return;
            }
            if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.activeSelf) {
                transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).GetComponent<Button>().onClick.Invoke();
            } else {
                transform.GetChild(INDEX_OF_CACELL_SYSTEM_PANNEL).GetChild(0).GetComponent<Button>().onClick.Invoke();
            }
        }));
    }

    object ThisTicket() {
        if(ticketBeingEdited == null) {
            if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.activeSelf) {
                return new PostTicket(
                    brands[IndexOfSelectedButton(INDEX_OF_DEVICE)][IndexOfSelectedButton(INDEX_OF_BRAND)],
                    transform.GetChild(INDEX_OF_MODEL).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                    AnyBoxesOrButtonsSelected(INDEX_OF_COLOR) ? colors[IndexOfSelectedButton(INDEX_OF_COLOR)] : "",
                    devices[IndexOfSelectedButton(INDEX_OF_DEVICE)],
                    DoesTicketSubjectNeedDevice(IndexOfSelectedButton(INDEX_OF_DEVICE), IndexOfSelectedButton(INDEX_OF_BRAND)),
                    StringsFromIndexes(problems[IndexOfSelectedButton(INDEX_OF_DEVICE)].ToArray(), IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)),
                    GetDoItIf(IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)),
                    GetPrices(IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)),
                    transform.GetChild(INDEX_OF_OTHER).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                    customerID,
                    howManyMinutes[IndexOfSelectedButton(INDEX_OF_HOW_LONG)] != 0 ? creationDate.AddMinutes(howManyMinutes[IndexOfSelectedButton(INDEX_OF_HOW_LONG)]) : creationDate,
                    creationDate,
                    howLong[IndexOfSelectedButton(INDEX_OF_HOW_LONG)],
                    transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                    IndexesOfSelectedBoxes(INDEX_OF_ITEMS_LEFT).Contains(IndexOf(itemsLeft, "Charger")),
                    StringsFromIndexes(itemsLeft, IndexesOfSelectedBoxes(INDEX_OF_ITEMS_LEFT)),
                    needData[IndexOfSelectedButton(INDEX_OF_DATA)],
                    Contains(GetPrices(IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)), "CKCL")
                );
            } else {
                return new PostTicket(
                    transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                    customerID,
                    transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                    IndexesOfSelectedBoxes(INDEX_OF_ITEMS_LEFT).Contains(IndexOf(itemsLeft, "Charger"))
                );
            }
        } else {
            if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.activeSelf) {
                return new LargeTicket(
                    brands[IndexOfSelectedButton(INDEX_OF_DEVICE)][IndexOfSelectedButton(INDEX_OF_BRAND)],
                    transform.GetChild(INDEX_OF_MODEL).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                    AnyBoxesOrButtonsSelected(INDEX_OF_COLOR) ? colors[IndexOfSelectedButton(INDEX_OF_COLOR)] : "",
                    devices[IndexOfSelectedButton(INDEX_OF_DEVICE)],
                    DoesTicketSubjectNeedDevice(IndexOfSelectedButton(INDEX_OF_DEVICE), IndexOfSelectedButton(INDEX_OF_BRAND)),
                    StringsFromIndexes(problems[IndexOfSelectedButton(INDEX_OF_DEVICE)].ToArray(), IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)),
                    GetDoItIf(IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)),
                    GetPrices(IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)),
                    transform.GetChild(INDEX_OF_OTHER).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                    howManyMinutes[IndexOfSelectedButton(INDEX_OF_HOW_LONG)] != 0 ? creationDate.AddMinutes(howManyMinutes[IndexOfSelectedButton(INDEX_OF_HOW_LONG)]) : creationDate,
                    creationDate,
                    howLong[IndexOfSelectedButton(INDEX_OF_HOW_LONG)],
                    transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                    IndexesOfSelectedBoxes(INDEX_OF_ITEMS_LEFT).Contains(IndexOf(itemsLeft, "Charger")),
                    StringsFromIndexes(itemsLeft, IndexesOfSelectedBoxes(INDEX_OF_ITEMS_LEFT)),
                    needData[IndexOfSelectedButton(INDEX_OF_DATA)],
                    Contains(GetPrices(IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)), "CKCL"),
                    ticketBeingEdited
                ).ConvertToLargeTicketWithTicketTypeId().ChangeTicketTypeIdToComputer(transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text);
            } else {
                if(transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text.Trim() != "") {
                    return new LargeTicket(
                        transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                        transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                        IndexesOfSelectedBoxes(INDEX_OF_ITEMS_LEFT).Contains(IndexOf(itemsLeft, "Charger")),
                        ticketBeingEdited
                    ).ConvertToLargeTicketWithTicketTypeId().ChangeTicketTypeIdToComputer(transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text);
                } else {
                    return new LargeTicket(
                        transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                        transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text,
                        IndexesOfSelectedBoxes(INDEX_OF_ITEMS_LEFT).Contains(IndexOf(itemsLeft, "Charger")),
                        ticketBeingEdited
                    );
                }
            }
        }
    }

    void SetUpButtonsInCorners(string ticketNumber, UnityAction ticketNumberListener, UnityAction ticketNumberMiddleClickListener, string name, UnityAction nameListener, UnityAction nameMiddleClickListener, string apply, UnityAction applyListener) {
        transform.GetChild(INDEX_OF_VIEW_TICKET_BUTTON).GetChild(0).GetComponent<TextMeshProUGUI>().text = ticketNumber;
        transform.GetChild(INDEX_OF_VIEW_TICKET_BUTTON).GetComponent<Button>().onClick.RemoveAllListeners();
        transform.GetChild(INDEX_OF_VIEW_TICKET_BUTTON).GetComponent<Button>().onClick.AddListener(ticketNumberListener);
        transform.GetChild(INDEX_OF_VIEW_TICKET_BUTTON).gameObject.AddComponent<OnMiddleClick>().SetListener(ticketNumberMiddleClickListener);
        transform.GetChild(INDEX_OF_VIEW_TICKET_BUTTON).GetComponent<ChangeSizeToFirstChildsPreferedSize>().Start();
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<Button>().onClick.RemoveAllListeners();
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<Button>().onClick.AddListener(nameListener);
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).gameObject.AddComponent<OnMiddleClick>().SetListener(nameMiddleClickListener);
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<ChangeSizeToFirstChildsPreferedSize>().Start();
        transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).GetChild(0).GetComponent<TextMeshProUGUI>().text = apply;
        transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).GetComponent<Button>().onClick.RemoveAllListeners();
        transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).GetComponent<Button>().onClick.AddListener(applyListener);
        transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).GetComponent<ChangeSizeToFirstChildsPreferedSize>().Start();
    }

    void FillInTicketWithPreviousValues(LargeTicket ticket) {
        ticketBeingEdited = ticket;
        customerID = ticketBeingEdited.customer.id;
        ReplaceButtons(transform.GetChild(0), devices);
        creationDate = ticketBeingEdited.created_at;
        SetUpButtonsInCorners(
            "#" + ticketBeingEdited.number.ToString(), delegate {
                Main.TicketViewer(ticketBeingEdited);
                Destroy(transform.GetChild(INDEX_OF_VIEW_TICKET_BUTTON).gameObject);
            }, delegate {
                Main.TicketViewerInNewWindow(ticketBeingEdited);
            },
            FormatName(ticketBeingEdited.customer_business_then_name), 
            delegate {
                Main.ViewCustomer(customerID);
                Destroy(transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).gameObject);
            }, delegate {
                Main.ViewCustomerInNewWindow(customerID);
            },
            "Apply Edit",
            ApplyEditButton
        );
        if(ticketBeingEdited.GetTicketDetails() != null) {
            TicketDetailsV1 ticketDetails = ticketBeingEdited.GetTicketDetails();
            transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = ticketBeingEdited.subject;
            FillInObjectButtons(INDEX_OF_DEVICE, devices, ticketDetails.device);
            FillInProblems(ticketDetails, delegate {
                if(IndexOf(devices, ticketDetails.device) != -1) {
                    FillInObjectButtons(INDEX_OF_BRAND, brands[IndexOf(devices, ticketDetails.device)], ticketDetails.brand);
                } else {
                    FillInObjectButtons(INDEX_OF_BRAND, ticketDetails.brand);
                }
                if(ticketDetails.color != "" && IndexOf(colors, ticketDetails.color) != -1) ButtonPressed(transform.GetChild(INDEX_OF_COLOR).GetChild(0).GetChild(IndexOf(colors, ticketDetails.color)));
                transform.GetChild(INDEX_OF_MODEL).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = ticketDetails.model;
                FillInObjectButtons(INDEX_OF_HOW_LONG, howLong, ticketDetails.howLong);
                transform.GetChild(INDEX_OF_OTHER).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = ticketDetails.otherDetails;
                FillInObjectBoxes(INDEX_OF_ITEMS_LEFT, itemsLeft, ticketDetails.itemsLeft);
                FillInObjectButtons(INDEX_OF_DATA, needData, ticketDetails.needData);
                transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = ticketBeingEdited.GetPassword();
                StartCoroutine(ExecuteActionAfterFrames(8, delegate {
                    SelectGrid(INDEX_OF_DEVICE);
                    Destroy(transform.GetChild(INDEX_OF_LOADING_PANNEL).gameObject);
                }));
            });
        } else {
            transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).GetComponent<Button>().onClick.Invoke();
            transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = ticketBeingEdited.subject;
            transform.GetChild(INDEX_OF_PASSWORD).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = ticketBeingEdited.GetPassword();
            try {
                TicketDetailsV1 ticketDetails = EstimateTicketValuesFromLegacySubject(ticketBeingEdited.subject);
                StartCoroutine(ExecuteActionInTheNextFrame(delegate {
                    FillInObjectButtons(INDEX_OF_DEVICE, devices, ticketDetails.device);
                    StartCoroutine(ExecuteActionInTheNextFrame(delegate {
                        if(ticketBeingEdited.properties.ACCharger()) BoxPressed(transform.GetChild(INDEX_OF_ITEMS_LEFT).GetChild(0).GetChild(0));
                        transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = ticketBeingEdited.subject;
                        if(IndexOf(devices, ticketDetails.device, true) != -1) {
                            FillInObjectButtons(INDEX_OF_BRAND, brands[IndexOf(devices, ticketDetails.device)], ticketDetails.brand);
                        } else {
                            FillInObjectButtons(INDEX_OF_BRAND, ticketDetails.brand);
                        }
                        if(ticketDetails.color != "" && IndexOf(colors, ticketDetails.color) != -1) ButtonPressed(transform.GetChild(INDEX_OF_COLOR).GetChild(0).GetChild(IndexOf(colors, ticketDetails.color)));
                        if(ticketDetails.needData != null) FillInObjectButtons(INDEX_OF_DATA, needData, ticketDetails.needData);
                        transform.GetChild(INDEX_OF_MODEL).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = ticketDetails.model;
                        FillInProblems(ticketDetails, delegate {
                            StartCoroutine(ExecuteActionAfterFrames(3, delegate {
                                SelectGrid(INDEX_OF_LEGACY_SUBJECT);
                                if(transform.GetChild(INDEX_OF_DEVICE).GetComponent<TextMeshProUGUI>().fontStyle == FontStyles.Underline) {
                                    DeselectGrid(INDEX_OF_DEVICE);
                                    transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().Select();
                                    transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().caretPosition = transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text.Length;
                                }
                                Destroy(transform.GetChild(INDEX_OF_LOADING_PANNEL).gameObject);
                            }));
                        });
                    }));
                }));
            } catch {
                if(transform.GetChild(INDEX_OF_DEVICE).GetComponent<TextMeshProUGUI>().fontStyle == FontStyles.Underline) {
                    DeselectGrid(INDEX_OF_DEVICE);
                    transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().Select();
                    transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().caretPosition = transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text.Length;
                }
                Destroy(transform.GetChild(INDEX_OF_LOADING_PANNEL).gameObject);
            }
        }
    }

    TicketDetailsV1 EstimateTicketValuesFromLegacySubject(string subject) {
        subject = " " + subject + " ";
        subject = subject.ToLower().Replace(" iph ", " iphone ").Replace(" ip ", " iphone ").Replace(" ltop ", " laptop ").Replace(" aio ", " all in one ").Replace(" dtop ", " desktop ").Replace(" blk ", " black ").Replace(" gld ", " gold ").Replace(" f11 ", " no data ").Replace(" wht ", " white ").Replace(" white ", " white / silver ").Replace(" pink ", " pink / rose gold ").Replace(" hacked ", " clean virus ").Replace("  ", " ");
        TicketDetailsV1 newTicketDetails = new TicketDetailsV1();
        for(int i = 0; i < devices.Length - 1; i++) {
            if(subject.Contains(devices[i].ToLower())) {
                newTicketDetails.device = devices[i];
                break;
            }
        }
        if(newTicketDetails.device == null) {
            for(int i = 0; i < brands.Length - 1 && newTicketDetails.brand == null; i++) {
                for(int ii = 0; ii < brands[i].Length - 1; ii++) {
                    string brand = brands[i][ii];
                    if(subject.Contains(brand.ToLower())) {
                        newTicketDetails.brand = brand;
                        newTicketDetails.device = devices[i];
                        break;
                    }
                }
            }
        } else {
            for(int i = 0; i < brands[IndexOf(devices, newTicketDetails.device)].Length - 1; i++) {
                string brand = brands[IndexOf(devices, newTicketDetails.device)][i];
                if(subject.Contains(brand.ToLower())) {
                    newTicketDetails.brand = brand;
                    break;
                }
            }
        }
        for(int i = 0; i < allLetters.Length; i++) {
            for(int ii = 0; ii < numbersUpToNinetyNine.Length; ii++) {
                if(subject.Contains(" " + allLetters[i] + numbersUpToNinetyNine[ii] + " ")) {
                    newTicketDetails.model = allLetters[i].ToUpper() + numbersUpToNinetyNine[ii];
                    break;
                }
            }
        }
        for(int i = 0; i < numbersUpToNinetyNine.Length; i++) {
            if(subject.Contains(" note " + numbersUpToNinetyNine[i] + " ")) {
                newTicketDetails.model = "Note " + numbersUpToNinetyNine[i];
                break;
            }
        }
        string[] modelSuffixes = { "plus", "pro max", "pro", "ultra", "5g", "mini" }; //"+"
        for(int i = 0; i < numbersUpToNinetyNine.Length; i++) {
            if(subject.Contains(" " + numbersUpToNinetyNine[i] + " ")) {
                if(GetWordBeforeIndex(subject, subject.IndexOf(" " + numbersUpToNinetyNine[i] + " ") + 1) == newTicketDetails.brand.ToLower()) {
                    newTicketDetails.model = numbersUpToNinetyNine[i];
                    for(int ii = 0; ii < modelSuffixes.Length; ii++) {
                        if(GetWordAfterIndex(subject, subject.IndexOf(numbersUpToNinetyNine[i]) + 1) == modelSuffixes[ii]) {
                            newTicketDetails.model += " " + FormatName(modelSuffixes[ii]);
                            break;
                        }
                    }
                }
            }
        }
        List<string> probs = new List<string>();
        if(IndexOf(devices, newTicketDetails.device) != -1) {
            for(int ii = 0; ii < problems[IndexOf(devices, newTicketDetails.device)].Count - 1; ii++) {
                if(subject.Contains(problems[IndexOf(devices, newTicketDetails.device)][ii].ToLower())) {
                    probs.Add(problems[IndexOf(devices, newTicketDetails.device)][ii]);
                }
            }
        }
        newTicketDetails.problems = probs.ToArray();
        for(int i = 0; i < colors.Length; i++) {
            if(subject.Contains(colors[i].ToLower())) {
                newTicketDetails.color = colors[i];
                break;
            }
        }
        for(int i = 0; i < needData.Length; i++) {
            if(subject.Contains(needData[i].ToLower())) {
                newTicketDetails.needData = needData[i];
                break;
            }
        }
        newTicketDetails.prices = Enumerable.Repeat("CKCL", newTicketDetails.problems.Length).ToArray();
        return newTicketDetails;
    }

    void FillInObjectButtons(int grid, string[] listOfOptions, string target) {
        if(target == null) return;
        if(target.Trim() == "" && grid != INDEX_OF_DEVICE) return;
        if(IndexOf(listOfOptions, target.Trim()) == -1) {
            ButtonPressed(transform.GetChild(grid).GetChild(0).GetChild(transform.GetChild(grid).GetChild(0).childCount - 1));
            transform.GetChild(grid).GetChild(1).GetChild(transform.GetChild(grid).GetChild(0).childCount - 1).GetComponent<TMP_InputField>().text = target.Trim();
        } else {
            ButtonPressed(transform.GetChild(grid).GetChild(0).GetChild(IndexOf(listOfOptions, target.Trim())));
        }
    }

    void FillInObjectButtons(int grid, string target) {
        if(target == "") return;
        ButtonPressed(transform.GetChild(grid).GetChild(0).GetChild(transform.GetChild(grid).GetChild(0).childCount - 1));
        transform.GetChild(grid).GetChild(1).GetChild(transform.GetChild(grid).GetChild(0).childCount - 1).GetComponent<TMP_InputField>().text = target;
    }

    void FillInObjectBoxes(int grid, string[] listOfOptions, string[] targets) {
        if(targets.Length == 0) return;
        for(int i = 0; i < targets.Length; i++) {
            if(IndexOf(listOfOptions, targets[i]) == -1) {
                transform.GetChild(grid).GetChild(1).GetChild(transform.GetChild(grid).GetChild(0).childCount - 1).GetComponent<TMP_InputField>().text = targets[i];
                BoxPressed(transform.GetChild(grid).GetChild(0).GetChild(transform.GetChild(grid).GetChild(0).childCount - 1));
            } else {
                BoxPressed(transform.GetChild(grid).GetChild(0).GetChild(IndexOf(listOfOptions, targets[i])));
            }
        }
    }

    void FillInProblems(TicketDetailsV1 ticketDetails, UnityAction then) {
        if(ticketDetails.problems.Length == 0) {
            then();
            return;
        }
        if(ticketDetails.problems[0].Length == 0) {
            then();
            return;
        }
        for(int i = 0; i < ticketDetails.problems.Length; i++) {
            int x = i;
            int deviceIndex;
            if(IndexOf(devices, ticketDetails.device) == -1) {
                deviceIndex = devices.Length - 1;
            } else {
                deviceIndex = IndexOf(devices, ticketDetails.device);
            }
            StartCoroutine(ExecuteActionAfterFrames(i * 2, delegate {
                if(Contains(problems[deviceIndex], ticketDetails.problems[x])) {
                    BoxPressed(transform.GetChild(INDEX_OF_PROBLEM).GetChild(0).GetChild(IndexOf(problems[deviceIndex], ticketDetails.problems[x])));
                    if(ticketDetails.doItIf[x] != "" || ticketDetails.prices[x] == "") {
                        transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(IndexOf(problems[deviceIndex], ticketDetails.problems[x])).GetChild(1).GetComponent<Button>().onClick.Invoke();
                        transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(IndexOf(problems[deviceIndex], ticketDetails.problems[x])).GetChild(2).GetComponent<TMP_InputField>().text = ticketDetails.doItIf[x];
                        transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(IndexOf(problems[deviceIndex], ticketDetails.problems[x])).GetChild(3).GetComponent<TMP_InputField>().text = ticketDetails.prices[x];
                    } else {
                        if(ticketDetails.prices[x].ToUpper() != "CKCL") transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(IndexOf(problems[deviceIndex], ticketDetails.problems[x])).GetComponent<TMP_InputField>().text = ticketDetails.prices[x];
                    }
                } else {
                    TMP_InputField otherField = AddField(INDEX_OF_PROBLEM, true);
                    otherField.text = ticketDetails.problems[x];
                    if(ticketDetails.doItIf[x] != "" || ticketDetails.prices[x] == "") {
                        transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(otherField.transform.GetSiblingIndex()).GetChild(1).GetComponent<Button>().onClick.Invoke();
                        transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(otherField.transform.GetSiblingIndex()).GetChild(2).GetComponent<TMP_InputField>().text = ticketDetails.doItIf[x];
                        transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(otherField.transform.GetSiblingIndex()).GetChild(3).GetComponent<TMP_InputField>().text = ticketDetails.prices[x];
                    } else {
                        if(ticketDetails.prices[x].ToUpper() != "CKCL") transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(otherField.transform.GetSiblingIndex()).GetComponent<TMP_InputField>().text = ticketDetails.prices[x];
                    }
                }
                if(x == ticketDetails.problems.Length - 1) then();
            }));
        }
    }

    string[] GetDoItIf(int[] indexesOfSelectedBoxes) {
        List<string> arrayOfDoItIf = new List<string>();
        for(int i = 0; i < indexesOfSelectedBoxes.Length; i++) {
            if(transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(indexesOfSelectedBoxes[i]).childCount > 0) {
                arrayOfDoItIf.Add(transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(indexesOfSelectedBoxes[i]).GetChild(2).GetComponent<TMP_InputField>().text);
            } else {
                arrayOfDoItIf.Add("");
            }
        }
        return arrayOfDoItIf.ToArray();
    }

    int IndexOfSelectedButton(int indexOfGrid) {
        for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(0).childCount - 1; i++) {
            if(transform.GetChild(indexOfGrid).GetChild(0).GetChild(i).childCount > 0) return i;
        }
        return transform.GetChild(indexOfGrid).GetChild(0).childCount - 1;
    }

    int[] IndexesOfSelectedBoxes(int indexOfGrid) {
        List<int> selected = new List<int>();
        for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(0).childCount - 1; i++) {
            if(transform.GetChild(indexOfGrid).GetChild(0).GetChild(i).childCount > 0) {
                selected.Add(i);
            }
        }
        if(!Contains(INDEX_OF_GRIDS_WITH_OTHER_FIELDS, indexOfGrid)) return selected.ToArray();
        for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(1).childCount; i++) {
            if(transform.GetChild(indexOfGrid).GetChild(1).GetChild(i).name.Contains("Field")) {
                if(transform.GetChild(indexOfGrid).GetChild(0).GetChild(i).childCount > 0 && transform.GetChild(indexOfGrid).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().text != "") selected.Add(i);
            }
        }
        return selected.Distinct().ToArray();
    }

    int[] IndexesOfBoxesWithDoItIfEnabled(int indexOfGrid) {
        List<int> selected = new List<int>();
        for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(0).childCount - 1; i++) {
            if(transform.GetChild(indexOfGrid).GetChild(3).GetChild(i).childCount > 0) {
                selected.Add(i);
            }
        }
        return selected.ToArray();
    }

    bool AnyBoxesOrButtonsSelected(int indexOfGrid) {
        for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(0).childCount; i++) {
            if(transform.GetChild(indexOfGrid).GetChild(0).GetChild(i).childCount > 0) return true;
        }
        return false;
    }

    void HowLong() {
        string[] lables = new string[howLong.Length];
        Color32[] colors = new Color32[howLong.Length];
        for(int i = 0; i < howLong.Length; i++) {
            if(howLong[i] != "") {
                lables[i] = howLong[i] + " (" + DueDateFormatedToString(creationDate.AddMinutes(howManyMinutes[i]), creationDate) + ")";
                colors[i] = ColorsForDate(creationDate.AddMinutes(howManyMinutes[i]), i > 6);
            } else {
                lables[i] = "";
            }
        }
        ReplaceButtons(transform.GetChild(INDEX_OF_HOW_LONG), lables, colors);
    }

    Color32 ColorsForDate(DateTime time, bool oneDayOrMore) {
        if(oneDayOrMore) return new Color32(255, 255, 255, 255);
        if(time.Hour >= CloseTime(time.DayOfWeek) || time.Hour < 10) {
            return new Color32(255, 0, 0, 255);
        } else if(time.Hour >= CloseTime(time.DayOfWeek) - 1) {
            return new Color32(255, 255, 0, 255);
        } else {
            return new Color32(255, 255, 255, 255);
        }
    }

    void ReplaceButtons(Transform titleTransform, string[] buttons) {
        ReplaceButtons(titleTransform, buttons, Enumerable.Repeat(new Color32(255, 255, 255, 255), buttons.Length).ToArray());
    }

    void ReplaceButtons(Transform titleTransform, string[] buttons, Color32[] colors) {
        titleTransform.gameObject.SetActive(true);
        for(int i = 0; i < titleTransform.GetChild(0).childCount; i++) {
            Destroy(titleTransform.GetChild(0).GetChild(i).gameObject);
            Destroy(titleTransform.GetChild(1).GetChild(i).gameObject);
        }
        titleTransform.GetComponent<Button>().onClick.RemoveAllListeners();
        titleTransform.GetComponent<Button>().onClick.AddListener(delegate { ReplaceButtons(titleTransform, buttons, colors); });
        for(int i = 0; i < buttons.Length; i++) {
            Transform buttonObject = Instantiate(button, titleTransform.GetChild(0)).transform;
            buttonObject.GetComponent<Button>().onClick.AddListener(delegate { ButtonPressed(buttonObject); });
            if(buttons[i] != "") {
                Transform textObject = Instantiate(text, titleTransform.GetChild(1)).transform;
                textObject.GetComponent<TextMeshProUGUI>().text = buttons[i];
                textObject.GetComponent<TextMeshProUGUI>().color = colors[i];
                textObject.GetComponent<Button>().onClick.AddListener(delegate { ButtonPressed(buttonObject); });
            } else {
                Instantiate(field, titleTransform.GetChild(1)).GetComponent<TMP_InputField>().onSelect.AddListener(delegate { ButtonPressed(buttonObject); });
            }
        }
    }

    void ReplaceButtons(Transform titleTransform, Color32[] colors) {
        ReplaceButtons(titleTransform, colors, Enumerable.Repeat(false, colors.Length).ToArray());
    }

    void ReplaceButtons(Transform titleTransform, Color32[] colors, bool[] shouldButtonsBeShiny) {
        titleTransform.gameObject.SetActive(true);
        for(int i = 0; i < titleTransform.GetChild(0).childCount; i++) {
            Destroy(titleTransform.GetChild(0).GetChild(i).gameObject);
        }
        titleTransform.GetComponent<Button>().onClick.RemoveAllListeners();
        titleTransform.GetComponent<Button>().onClick.AddListener(delegate { ReplaceButtons(titleTransform, colors, shouldButtonsBeShiny); });
        for(int i = 0; i < colors.Length; i++) {
            Transform buttonObject = Instantiate(shouldButtonsBeShiny[i] ? shinyButton : button, titleTransform.GetChild(0)).transform;
            buttonObject.GetComponent<Button>().onClick.AddListener(delegate { ButtonPressed(buttonObject); });
            buttonObject.GetComponent<Image>().color = colors[i];
        }
    }

    void ReplaceBoxes(Transform titleTransform, string[] boxes) {
        titleTransform.gameObject.SetActive(true);
        for(int i = 0; i < titleTransform.GetChild(1).childCount; i++) {
            Destroy(titleTransform.GetChild(0).GetChild(i).gameObject);
            Destroy(titleTransform.GetChild(1).GetChild(i).gameObject);
            if(Contains(INDEXES_OF_GRIDS_WITH_PRICE_BOXES, titleTransform.GetSiblingIndex())) {
                Destroy(titleTransform.GetChild(2).GetChild(i).gameObject);
                Destroy(titleTransform.GetChild(3).GetChild(i).gameObject);
            }
        }
        if(Contains(INDEXES_OF_GRIDS_WITH_PLUS_BUTTON, titleTransform.GetSiblingIndex())) {
            for(int i = 0; i < titleTransform.GetChild(0).childCount; i++) {
                Destroy(titleTransform.GetChild(0).GetChild(titleTransform.GetChild(0).childCount - 1).gameObject);
            }
        }
        titleTransform.GetComponent<Button>().onClick.RemoveAllListeners();
        titleTransform.GetComponent<Button>().onClick.AddListener(delegate { ReplaceBoxes(titleTransform, boxes); });
        for(int i = 0; i < boxes.Length; i++) {
            Transform boxObject = Instantiate(box, titleTransform.GetChild(0)).transform;
            boxObject.GetComponent<Button>().onClick.AddListener(delegate { BoxPressed(boxObject); });
            if(boxes[i] != "") {
                Transform textObject = Instantiate(text, titleTransform.GetChild(1)).transform;
                textObject.GetComponent<TextMeshProUGUI>().text = boxes[i];
                textObject.GetComponent<Button>().onClick.AddListener(delegate { BoxPressed(boxObject); });
            } else {
                TMP_InputField inputField = Instantiate(field, titleTransform.GetChild(1)).GetComponent<TMP_InputField>();
                inputField.onSelect.AddListener(delegate { SelectGrid(titleTransform.GetSiblingIndex()); });
                inputField.onDeselect.AddListener(delegate { 
                    if(inputField.text != "" && boxObject.childCount == 0) BoxPressed(boxObject);
                    if(inputField.text == "" && boxObject.childCount > 0) BoxPressed(boxObject); 
                });
                inputField.onSubmit.AddListener(delegate {
                    if(inputField.text != "" && boxObject.childCount == 0) BoxPressed(boxObject);
                    if(inputField.text == "" && boxObject.childCount > 0) BoxPressed(boxObject);
                });
            }
            if(Contains(INDEXES_OF_GRIDS_WITH_PRICE_BOXES, titleTransform.GetSiblingIndex())) {
                Instantiate(empty, titleTransform.GetChild(2));
                Instantiate(empty, titleTransform.GetChild(3));
            }
        }
        if(Contains(INDEXES_OF_GRIDS_WITH_PLUS_BUTTON, titleTransform.GetSiblingIndex())) {
            AddPlusButtonForAddingAField(INDEX_OF_PROBLEM, true, delegate (string previousText) {
                return previousText;
            });
        }
    }

    public void ButtonPressed(Transform buttonPressed) {
        SelectGrid(buttonPressed.parent.parent.GetSiblingIndex());
        RemoveAllOtherSelectedIcons(buttonPressed.parent, buttonPressed.GetSiblingIndex());
        if(buttonPressed.childCount <= 0) Instantiate(selectedIconButton, buttonPressed);
        if(buttonPressed.parent.parent.GetSiblingIndex() == INDEX_OF_DEVICE) MakeEverythingAppearAfterChoosingADevice(buttonPressed.GetSiblingIndex());
        if(Contains(INDEX_OF_GRIDS_WITH_OTHER_FIELDS, buttonPressed.parent.parent.GetSiblingIndex())) {
            if(buttonPressed.parent.childCount - 1 == buttonPressed.GetSiblingIndex()) buttonPressed.parent.parent.GetChild(1).GetChild(buttonPressed.GetSiblingIndex()).GetComponent<TMP_InputField>().ActivateInputField();
        }
    }

    public void BoxPressed(Transform boxPressed) {
        SelectGrid(boxPressed.parent.parent.GetSiblingIndex());
        if(Contains(INDEXES_OF_GRIDS_WITH_PRICE_BOXES, boxPressed.parent.parent.GetSiblingIndex())) {
            Destroy(boxPressed.parent.parent.GetChild(2).GetChild(boxPressed.GetSiblingIndex()).gameObject);
        }
        if(boxPressed.childCount > 0) {
            Destroy(boxPressed.GetChild(0).gameObject);
            if(Contains(INDEXES_OF_GRIDS_WITH_PRICE_BOXES, boxPressed.parent.parent.GetSiblingIndex())) {
                Instantiate(empty, boxPressed.parent.parent.GetChild(2)).transform.SetSiblingIndex(boxPressed.GetSiblingIndex());
                if(boxPressed.parent.parent.GetChild(3).GetChild(boxPressed.GetSiblingIndex()).childCount > 0) {
                    Destroy(boxPressed.parent.parent.GetChild(3).GetChild(boxPressed.GetSiblingIndex()).gameObject);
                    Instantiate(empty, boxPressed.parent.parent.GetChild(3)).transform.SetSiblingIndex(boxPressed.GetSiblingIndex());
                }
            }
            return;
        }
        Instantiate(selectedIconBox, boxPressed);
        StartCoroutine(ExecuteActionAfterFrames(1, delegate { 
            if(boxPressed.parent.parent.GetChild(1).GetChild(boxPressed.GetSiblingIndex()).childCount > 0) boxPressed.parent.parent.GetChild(1).GetChild(boxPressed.GetSiblingIndex()).GetComponent<TMP_InputField>().ActivateInputField();
        }));
        if(Contains(INDEXES_OF_GRIDS_WITH_PRICE_BOXES, boxPressed.parent.parent.GetSiblingIndex())) {
            CreatePriceBox(boxPressed.parent.parent.GetChild(2), boxPressed.parent.parent.GetChild(3), boxPressed.GetSiblingIndex());
        }
    }

    Transform CreatePriceBox(Transform priceBoxGrid, Transform doItIfBoxGrid, int boxSiblingIndex) {
        Transform priceBoxObject = Instantiate(priceBox, priceBoxGrid).transform;
        priceBoxObject.SetSiblingIndex(boxSiblingIndex);
        TMP_InputField priceBoxField = priceBoxObject.GetComponent<TMP_InputField>();
        if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.activeSelf && !(transform.childCount - 2 == INDEX_OF_LOADING_PANNEL)) priceBoxField.ActivateInputField();
        priceBoxField.onSelect.AddListener(delegate { SelectGrid(priceBoxGrid.parent.GetSiblingIndex()); });
        priceBoxField.onDeselect.AddListener(delegate (string text) {
            if(text == "CKCL") priceBoxField.text = "";
            if(text != "" && text[0] != '$' && CanParse(text)) priceBoxField.text = "$" + text;
        });
        priceBoxField.onSubmit.AddListener(delegate (string text) {
            priceBoxField.onDeselect.Invoke(text);
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) priceBoxObject.GetChild(1).GetComponent<Button>().onClick.Invoke();
        });
        if(doItIfBoxGrid.childCount < (boxSiblingIndex + 1)) Instantiate(empty, doItIfBoxGrid).transform.SetSiblingIndex(boxSiblingIndex);
        priceBoxObject.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate {
            CreateDoItIfBox(priceBoxGrid, doItIfBoxGrid, boxSiblingIndex, priceBoxObject);
        });
        if(priceBoxGrid.parent.GetChild(1).GetChild(boxSiblingIndex).childCount > 0) {
            priceBoxGrid.parent.GetChild(1).GetChild(boxSiblingIndex).GetComponent<TMP_InputField>().onSubmit.AddListener(delegate (string text) {
                priceBoxField.ActivateInputField();
            });
        }
        return priceBoxObject;
    }

    Transform CreateDoItIfBox(Transform priceBoxGrid, Transform doItIfBoxGrid, int boxSiblingIndex, Transform priceBoxObject) {
        Destroy(doItIfBoxGrid.GetChild(boxSiblingIndex).gameObject);
        Transform doItIfBoxObject = Instantiate(doItIfBox, doItIfBoxGrid).transform;
        doItIfBoxObject.SetSiblingIndex(boxSiblingIndex);
        TMP_InputField fieldForDoItIfBox = doItIfBoxObject.GetChild(2).GetComponent<TMP_InputField>();
        fieldForDoItIfBox.ActivateInputField();
        TMP_InputField priceBoxFieldForDoItIfBox = doItIfBoxObject.GetChild(3).GetComponent<TMP_InputField>();
        fieldForDoItIfBox.onSelect.AddListener(delegate { SelectGrid(doItIfBoxGrid.parent.GetSiblingIndex()); });
        fieldForDoItIfBox.onSubmit.AddListener(delegate (string text) {
            priceBoxFieldForDoItIfBox.ActivateInputField();
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) doItIfBoxObject.GetChild(0).GetComponent<Button>().onClick.Invoke();
        });
        priceBoxFieldForDoItIfBox.onSelect.AddListener(delegate { SelectGrid(doItIfBoxGrid.parent.GetSiblingIndex()); });
        priceBoxFieldForDoItIfBox.onDeselect.AddListener(delegate (string text) {
            if(text != "" && text[0] != '$' && CanParse(text)) priceBoxFieldForDoItIfBox.text = "$" + text;
        });
        priceBoxFieldForDoItIfBox.onSubmit.AddListener(delegate {
            priceBoxFieldForDoItIfBox.onDeselect.Invoke(priceBoxFieldForDoItIfBox.text);
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) doItIfBoxObject.GetChild(0).GetComponent<Button>().onClick.Invoke();
        });
        doItIfBoxObject.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate {
            Transform newPriceBoxObject = CreatePriceBox(priceBoxGrid, doItIfBoxGrid, boxSiblingIndex);
            Destroy(newPriceBoxObject.parent.GetChild(newPriceBoxObject.GetSiblingIndex() + 1).gameObject);
            Instantiate(empty, doItIfBoxGrid).transform.SetSiblingIndex(boxSiblingIndex);
            Destroy(doItIfBoxObject.gameObject);
        });
        Instantiate(empty, priceBoxGrid).transform.SetSiblingIndex(boxSiblingIndex);
        Destroy(priceBoxObject.gameObject);
        return doItIfBoxObject;
    }

    bool DoesTicketSubjectNeedDevice(int indexOfSelectedDevice, int indexOfSelectedBrand) {
        return DoesTicketSubjectNeedDevice(devices[indexOfSelectedDevice], brands[indexOfSelectedDevice][indexOfSelectedBrand]);
    }

    bool DoesTicketSubjectNeedDevice(string device, string brand) {
        if(device.ToLower() == "phone" || device.ToLower() == "console") return false;
        if(device.ToLower() == "tablet" && brand.ToLower() == "ipad") return false;
        if(device.ToLower() == "laptop" && brand.ToLower() == "macbook") return false;
        if(device.ToLower() == "all in one" && brand.ToLower() == "imac") return false;
        return true;
    }

    string[] GetPrices(int[] indexesOfSelectedBoxes) {
        int[] indexesOfBoxesWithDoItIfEnabled = IndexesOfBoxesWithDoItIfEnabled(INDEX_OF_PROBLEM);
        string[] prices = new string[indexesOfSelectedBoxes.Length];
        for(int i = 0; i < indexesOfSelectedBoxes.Length; i++) {
            if(!Contains(indexesOfBoxesWithDoItIfEnabled, indexesOfSelectedBoxes[i])) {
                if(transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(indexesOfSelectedBoxes[i]).GetComponent<TMP_InputField>().text != "") {
                    prices[i] = transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(indexesOfSelectedBoxes[i]).GetComponent<TMP_InputField>().text;
                } else {
                    prices[i] = "CKCL";
                }
            } else {
                prices[i] = transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(indexesOfSelectedBoxes[i]).GetChild(3).GetComponent<TMP_InputField>().text;
            }
        }
        return prices;
    }

    void CreateButton() {
        if(!transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).gameObject.activeSelf) return;
        if(!IsEnoughStuffIsEnteredToFinishMakingTheTicket()) return;
        transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).gameObject.SetActive(false);
        FillInOthers();
        print(JsonConvert.SerializeObject(ThisTicket(), Formatting.Indented));
        RestClient.Post(Main.URL + "/tickets", JsonConvert.SerializeObject(ThisTicket())).Then(response => {
            Main.TicketViewer(JsonConvert.DeserializeObject<OneLargeTicket>(response.Text).ticket);
        }).Catch(error => { 
            transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).gameObject.SetActive(true);
            Alert("Ticket was not created because: " + error.Message);
        });
    }

    void ApplyEditButton() {
        if(!transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).gameObject.activeSelf) return;
        if(!IsEnoughStuffIsEnteredToFinishMakingTheTicket()) return;
        transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).gameObject.SetActive(false);
        FillInOthers();
        print(JsonConvert.SerializeObject(ThisTicket(), Formatting.Indented));
        GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(ThisTicket(), Formatting.Indented);
        RestClient.Put(Main.URL + "/tickets/" + ticketBeingEdited.id, JsonConvert.SerializeObject(ThisTicket())).Then(response => {
            Main.TicketViewer(JsonConvert.DeserializeObject<OneLargeTicket>(response.Text).ticket);
        }).Catch(error => { 
            Alert("Ticket edit was not applied because: " + error.Message); 
            transform.GetChild(INDEX_OF_CREATE_OR_EDIT_BUTTON).gameObject.SetActive(true);
        });
    }

    bool IsEnoughStuffIsEnteredToFinishMakingTheTicket() {
        if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.activeSelf) {
            if(!AnyBoxesOrButtonsSelected(INDEX_OF_PROBLEM)) {
                Alert("You must enter the device and problem");
                return false;
            }/*
            if(!CanParseStringArray(GetPrices(IndexesOfSelectedBoxes(INDEX_OF_PROBLEM)), new string[] { "CKCL", "" }, "$")) {
                Alert("The prices must be only numbers");
                return false;
            }*/
        } else {
            if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text == "") {
                Alert("You must enter something into the Legacy subject field");
                return false;
            }
        }
        return true;
    }

    void MakeEverythingAppearAfterChoosingADevice(int indexOfSelectedDevice) {
        if(transform.GetChild(INDEX_OF_BRAND).GetChild(0).childCount <= 0) {
            transform.GetChild(INDEX_OF_OTHER).gameObject.SetActive(true);
            Instantiate(field, transform.GetChild(INDEX_OF_OTHER).GetChild(0)).GetComponent<TMP_InputField>().onSelect.AddListener(delegate { SelectGrid(INDEX_OF_OTHER); });
            transform.GetChild(INDEX_OF_MODEL).gameObject.SetActive(true);
            Instantiate(field, transform.GetChild(INDEX_OF_MODEL).GetChild(0)).GetComponent<TMP_InputField>().onSelect.AddListener(delegate { SelectGrid(INDEX_OF_MODEL); });
            transform.GetChild(INDEX_OF_PASSWORD).gameObject.SetActive(true);
            Instantiate(field, transform.GetChild(INDEX_OF_PASSWORD).GetChild(0)).GetComponent<TMP_InputField>().onSelect.AddListener(delegate { SelectGrid(INDEX_OF_PASSWORD); });
            ReplaceButtons(transform.GetChild(INDEX_OF_COLOR), colorsForColorButtons, colorIsShiny);
            ReplaceBoxes(transform.GetChild(INDEX_OF_ITEMS_LEFT), itemsLeft);
            ReplaceButtons(transform.GetChild(INDEX_OF_DATA), needData);
        }
        ReplaceButtons(transform.GetChild(INDEX_OF_BRAND), brands[indexOfSelectedDevice]);
        ReplaceBoxes(transform.GetChild(INDEX_OF_PROBLEM), problems[indexOfSelectedDevice].ToArray());
        HowLong();
    }

    public void SelectOption(int option) {
        if(option >= transform.GetChild(selectedGrid).GetChild(0).childCount || option < 0) return;
        if(IsAnyFieldFocused(selectedGrid)) return;
        if(Contains(INDEXES_OF_GRIDS_WITH_PLUS_BUTTON, selectedGrid) && transform.GetChild(selectedGrid).GetChild(0).childCount - 1 == option) {
            transform.GetChild(selectedGrid).GetChild(0).GetChild(option).GetComponent<Button>().onClick.Invoke();
            return;
        }
        if(Contains(INDEX_OF_GRIDS_WITH_BUTTONS, selectedGrid)) {
            if(transform.GetChild(selectedGrid).GetChild(0).childCount > option) ButtonPressed(transform.GetChild(selectedGrid).GetChild(0).GetChild(option));
        } else {
            if(transform.GetChild(selectedGrid).GetChild(0).childCount > option) BoxPressed(transform.GetChild(selectedGrid).GetChild(0).GetChild(option));
        }
    }

    void FillInOthers() {
        devices[^1] = FillInOther(INDEX_OF_DEVICE);
        brands[IndexOfSelectedButton(0)][^1] = FillInOther(INDEX_OF_BRAND);
        for(int i = problems[IndexOfSelectedButton(INDEX_OF_DEVICE)].Count; i < transform.GetChild(INDEX_OF_PROBLEM).GetChild(1).childCount; i++) {
            problems[IndexOfSelectedButton(INDEX_OF_DEVICE)].Add(transform.GetChild(INDEX_OF_PROBLEM).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().text);
        }
        howLong[^1] = FillInOther(INDEX_OF_HOW_LONG);
        howManyMinutes[^1] = ConvertHowLongOtherToMinutes(howLong[^1]);
        itemsLeft[^1] = FillInOther(INDEX_OF_ITEMS_LEFT);
        needData[^1] = FillInOther(INDEX_OF_DATA);
    }

    string FillInOther(int indexOfGrid) {
        return transform.GetChild(indexOfGrid).GetChild(1).GetChild(transform.GetChild(indexOfGrid).GetChild(0).childCount - 1).GetComponent<TMP_InputField>().text;
    }

    TMP_InputField AddField(int grid, bool useBoxes) {
        return AddField(grid, useBoxes, delegate (string previousText) {
            return previousText;
        });
    }

    TMP_InputField AddField(int grid, bool useBoxes, Func<string, string> onDeselectChangeTextTo) {
        TMP_InputField newField;
        if(grid == INDEX_OF_PROBLEM) newField = Instantiate(field, transform.GetChild(grid).GetChild(1)).GetComponent<TMP_InputField>();
        else newField = Instantiate(field, transform.GetChild(grid).GetChild(0).transform).GetComponent<TMP_InputField>();
        newField.onSelect.AddListener(delegate {
            SelectGrid(grid);
        });
        newField.onDeselect.AddListener(delegate (string text) {
            newField.text = onDeselectChangeTextTo(text);
        });
        newField.onSubmit.AddListener(delegate (string text) {
            newField.onDeselect.Invoke(text);
        });
        if(!(Contains(INDEXES_OF_GRIDS_WITH_PLUS_BUTTON, grid) && Contains(INDEXES_OF_GRIDS_WITH_PRICE_BOXES, grid))) return newField;
        Instantiate(empty, transform.GetChild(grid).GetChild(2));
        Transform buttonObject = Instantiate(useBoxes ? box : button, transform.GetChild(grid).GetChild(0)).transform;
        buttonObject.SetSiblingIndex(transform.GetChild(grid).GetChild(0).childCount - 2);
        buttonObject.GetComponent<Button>().onClick.AddListener(delegate { if(useBoxes) { BoxPressed(buttonObject); } else { ButtonPressed(buttonObject); } });
        buttonObject.GetComponent<Button>().onClick.Invoke();
        return newField;
    }

    void AddPlusButtonForAddingAField(int grid, bool useBoxes, Func<string, string> onDeselectChangeTextTo) {
        GameObject plusButtonObject = Instantiate(plusButton, transform.GetChild(grid).GetChild(0));
        plusButtonObject.GetComponent<Button>().onClick.AddListener(delegate {
            AddField(grid, useBoxes, onDeselectChangeTextTo);
            AddPlusButtonForAddingAField(grid, useBoxes, onDeselectChangeTextTo);
            Destroy(plusButtonObject);
        });
    }

    int ConvertHowLongOtherToMinutes(string text) {
        if(text.Length == 0) return 0;
        for(int i = 0; i < text.Length; i++) {
            if(!"1234567890".Contains(text[i])) {
                if(text[i] == 'm') {
                    if(CanParse(text[..i])) return int.Parse(text[..i]);
                } else if(text[i] == 'h') {
                    if(CanParse(text[..i])) return int.Parse(text[..i]) * 60;
                } else if(text[i] == 'd') {
                    if(CanParse(text[..i])) return int.Parse(text[..i]) * 60 * 24;
                } else if(text[i] == 'w') {
                    if(CanParse(text[..i])) return int.Parse(text[..i]) * 60 * 24 * 7;
                }
            }
        }
        return 0;
    }

    bool IsAnyFieldFocused(int indexOfGrid) {
        if(Contains(INDEX_OF_GRIDS_WITH_ONLY_FIELD, indexOfGrid)) return transform.GetChild(indexOfGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().isFocused;
        if(Contains(INDEXES_OF_GRIDS_WITH_PLUS_BUTTON, indexOfGrid)) {
            for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(1).childCount; i++) {
                if(transform.GetChild(indexOfGrid).GetChild(1).GetChild(i).name.Contains("Field")) {
                    if(transform.GetChild(indexOfGrid).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().isFocused) return true;
                }
            }
        } 
        if(Contains(INDEX_OF_GRIDS_WITH_OTHER_FIELDS, indexOfGrid)) {
            if(transform.GetChild(indexOfGrid).GetChild(1).GetChild(transform.GetChild(indexOfGrid).GetChild(1).childCount - 1).GetComponent<TMP_InputField>().isFocused) return true;
        }
        if(Contains(INDEXES_OF_GRIDS_WITH_PRICE_BOXES, indexOfGrid)) {
            for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(2).childCount; i++) {
                if(transform.GetChild(indexOfGrid).GetChild(2).GetChild(i).childCount > 0) {
                    if(transform.GetChild(indexOfGrid).GetChild(2).GetChild(i).GetComponent<TMP_InputField>().isFocused) return true;
                }
                if(transform.GetChild(indexOfGrid).GetChild(3).GetChild(i).childCount > 0) {
                    if(transform.GetChild(indexOfGrid).GetChild(3).GetChild(i).GetChild(2).GetComponent<TMP_InputField>().isFocused) return true;
                    if(transform.GetChild(indexOfGrid).GetChild(3).GetChild(i).GetChild(3).GetComponent<TMP_InputField>().isFocused) return true;
                }
            }
        }
        return false;
    }

    public void SelectPreviousGrid() {
        if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.activeSelf) {
            SelectGrid(selectedGrid - 1);
        } else {
            if(selectedGrid == INDEX_OF_LEGACY_SUBJECT) {
                SelectGrid(INDEX_OF_ITEMS_LEFT);
                return;
            }
            if(selectedGrid == INDEX_OF_PASSWORD) {
                SelectGrid(INDEX_OF_LEGACY_SUBJECT);
                return;
            }
            if(selectedGrid == INDEX_OF_ITEMS_LEFT) {
                SelectGrid(INDEX_OF_PASSWORD);
                return;
            }
        }
    }

    public void SelectNextGrid() {
        if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.activeSelf) {
            SelectGrid(selectedGrid + 1);
        } else {
            if(selectedGrid == INDEX_OF_LEGACY_SUBJECT) {
                SelectGrid(INDEX_OF_PASSWORD);
                return;
            }
            if(selectedGrid == INDEX_OF_PASSWORD) {
                SelectGrid(INDEX_OF_ITEMS_LEFT);
                return;
            }
            if(selectedGrid == INDEX_OF_ITEMS_LEFT) {
                SelectGrid(INDEX_OF_LEGACY_SUBJECT);
                return;
            }
        }
    }

    void SelectGrid(int grid) {
        if(transform.GetChild(INDEX_OF_LEGACY_SUBJECT_PANNEL).gameObject.activeSelf) {
            if(transform.GetChild(INDEX_OF_BRAND).GetChild(0).childCount == 0 && grid != INDEX_OF_DEVICE) return;
            if(grid == -1) {
                SelectGrid(INDEX_OF_OTHER);
                return;
            } else if(grid > INDEX_OF_OTHER || grid < INDEX_OF_DEVICE) {
                SelectGrid(INDEX_OF_DEVICE);
                return;
            }
        } else {
            if(!(grid == INDEX_OF_LEGACY_SUBJECT || grid == INDEX_OF_PASSWORD || grid == INDEX_OF_ITEMS_LEFT)) {
                SelectGrid(INDEX_OF_LEGACY_SUBJECT);
                return;
            }
        }
        if(grid == selectedGrid) return;
        if(selectedGrid != -1) {
            if(transform.childCount == INDEX_OF_LOADING_PANNEL + 2) {
                int x = selectedGrid;
                StartCoroutine(ExecuteActionInTheNextFrame(delegate {
                    DeselectGrid(x);
                }));
            } else {
                DeselectGrid(selectedGrid);
            }
        }
        selectedGrid = grid;
        transform.GetChild(selectedGrid).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Underline;
        if(Contains(INDEX_OF_GRIDS_WITH_ONLY_FIELD, selectedGrid)) {
            transform.GetChild(selectedGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().ActivateInputField();
            transform.GetChild(selectedGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().caretPosition = transform.GetChild(selectedGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text.Length;
        }
    }

    void DeselectGrid(int grid) {
        transform.GetChild(grid).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        if(Contains(INDEX_OF_GRIDS_WITH_ONLY_FIELD, grid)) {
            transform.GetChild(grid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().DeactivateInputField();
            transform.GetChild(grid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().onDeselect.Invoke(transform.GetChild(grid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text);
        }
        if(Contains(INDEXES_OF_GRIDS_WITH_PRICE_BOXES, grid)) {
            for(int i = 0; i < transform.GetChild(INDEX_OF_PROBLEM).GetChild(0).childCount - 1; i++) {
                if(transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(i).childCount != 0) {
                    transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(i).GetComponent<TMP_InputField>().DeactivateInputField();
                    transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(i).GetComponent<TMP_InputField>().onDeselect.Invoke(transform.GetChild(INDEX_OF_PROBLEM).GetChild(2).GetChild(i).GetComponent<TMP_InputField>().text);
                }
                if(transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(i).childCount == 0) continue;
                transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(i).GetChild(2).GetComponent<TMP_InputField>().DeactivateInputField();
                transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(i).GetChild(2).GetComponent<TMP_InputField>().onDeselect.Invoke(transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(i).GetChild(2).GetComponent<TMP_InputField>().text);
                transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(i).GetChild(3).GetComponent<TMP_InputField>().DeactivateInputField();
                transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(i).GetChild(3).GetComponent<TMP_InputField>().onDeselect.Invoke(transform.GetChild(INDEX_OF_PROBLEM).GetChild(3).GetChild(i).GetChild(2).GetComponent<TMP_InputField>().text);
            }
        }
        if(Contains(INDEXES_OF_GRIDS_WITH_PLUS_BUTTON, grid)) {
            for(int i = 0; i < transform.GetChild(grid).GetChild(1).childCount; i++) {
                if(transform.GetChild(grid).GetChild(1).GetChild(i).name.Contains("Field")) {
                    transform.GetChild(grid).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().DeactivateInputField();
                    transform.GetChild(grid).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().onDeselect.Invoke(transform.GetChild(grid).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().text);
                }
            }
        }
        if(Contains(INDEX_OF_GRIDS_WITH_OTHER_FIELDS, grid)) {
            transform.GetChild(grid).GetChild(1).GetChild(transform.GetChild(grid).GetChild(1).childCount - 1).GetComponent<TMP_InputField>().DeactivateInputField();
            transform.GetChild(grid).GetChild(1).GetChild(transform.GetChild(grid).GetChild(1).childCount - 1).GetComponent<TMP_InputField>().onDeselect.Invoke(transform.GetChild(grid).GetChild(1).GetChild(transform.GetChild(grid).GetChild(1).childCount - 1).GetComponent<TMP_InputField>().text);
        }
    }

    void RemoveAllOtherSelectedIcons(Transform grid, int indexOfOneToKeep) {
        for(int i = 0; i < grid.childCount; i++) {
            if(grid.GetChild(i).childCount > 0 && i != indexOfOneToKeep) Destroy(grid.GetChild(i).GetChild(0).gameObject);
        }
    }

}
