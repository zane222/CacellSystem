using Proyecto26;
using UnityEngine;
using TMPro;
using RepairShopRObjects;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using Newtonsoft.Json;

public class TicketListManager : UsefullMethods {

    GameObject background, textGameObject, keybindsObject;

    TMP_InputField search;

    List<string> statusesToHide = new List<string>();

    readonly string[] ticketsLables = { "Number", "Subject", "Status", "Created at", "Customer name" }, customersLables = { "", "Phone number", "", "Created at", "Name" };

    int latestTicketNumber, earliestTicketNumber = 4203, selectedObject = -1, currentPageNumber = 1;

    const int INDEX_OF_TICKETS = 0, INDEX_OF_LABLES = 1, INDEX_OF_KEYBINDS_BUTTON = 3;

    bool gettingMoreAfterScrolling = false;

    public void Constructor() {
        background = Resources.Load("Prefabs/UI/Background") as GameObject;
        textGameObject = Resources.Load("Prefabs/UI/Text") as GameObject;
        keybindsObject = Resources.Load("Prefabs/Keybinds/TicketListKeybinds") as GameObject;
        transform.GetChild(INDEX_OF_KEYBINDS_BUTTON).GetComponent<Button>().onClick.AddListener(delegate {
            GameObject a = Instantiate(keybindsObject, transform);
            a.GetComponent<Button>().onClick.AddListener(delegate {
                Destroy(a);
            });
        });
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        search = transform.GetChild(2).GetChild(2).GetComponent<TMP_InputField>();
        search.Select();
        SetUpStatus();
        transform.GetChild(2).GetChild(2).GetComponent<TMP_InputField>().onValueChanged.AddListener(SearchBoxValueChanged);
        ListTheTickets();
        SetUpLables();
        SetUpKeyCombinations();
        GetMoreTicketsOrCustomersAfterScrolling();
    }

    void SetUpKeyCombinations() {
        StartCoroutine(ExecuteActionWhileTrue(delegate { return Input.GetKeyDown(KeyCode.UpArrow) ? .6f : .05f; }, delegate { return !Input.GetKey(KeyCode.UpArrow); }, delegate {
            return Input.GetKey(KeyCode.UpArrow);
        }, delegate {
            if(selectedObject <= 0) return;
            selectedObject--;
            transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject).localScale = new Vector2(.99f, 1);
            if(selectedObject != transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).childCount - 1) transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject + 1).localScale = new Vector2(1, 1);
            if(selectedObject < Math.Round(PixelsScrolledDownForTicketsScrollRect() / 39.85196f) + 1) {
                transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition = new Vector2(transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.x, transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.y - 39.85196f);
            }
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate { return Input.GetKeyDown(KeyCode.DownArrow) ? new WaitForSeconds(.6f) : new WaitForSeconds(.05f); }, delegate { return !Input.GetKey(KeyCode.DownArrow); }, delegate {
            return Input.GetKey(KeyCode.DownArrow);
        }, delegate {
            if(transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).childCount - 1 <= selectedObject) return;
            selectedObject++;
            transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject).localScale = new Vector2(.99f, 1);
            if(selectedObject != 0) transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject - 1).localScale = new Vector2(1, 1);
            if(selectedObject > Math.Round(PixelsInViewForTicketsScrollRect() / 39.85196f + PixelsScrolledDownForTicketsScrollRect() / 39.85196f) - 2) {
                transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition = new Vector2(transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.x, transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.y + 39.85196f);
            }
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Tab);
        }, delegate {
            if(!search.isFocused) search.Select();
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return Input.GetKeyDown(KeyCode.H);
        }, delegate {
            if(!search.isFocused) Main.TicketList();
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return Input.GetKeyDown(KeyCode.N);
        }, delegate {
            if(!search.isFocused) NewCustomerButton();
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return Input.GetKeyDown(KeyCode.Tab) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }, delegate {
            //search.text = search.text.Remove(search.caretPosition - 1, 1);
            search.DeactivateInputField();
        }));
        StartCoroutine(ExecuteActionWhenTrue(delegate {
            return (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return));
        }, delegate {
            if(transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).childCount > 0) {
                transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject == -1 ? 0 : selectedObject).GetComponent<Button>().onClick.Invoke();
            } else {
                string currentTextInSearchBox = search.text;
                StartCoroutine(ExecuteActionWhenTrue(delegate { return transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).childCount > 0; }, delegate {
                    if(search.text != currentTextInSearchBox) return;
                    transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject == -1 ? 0 : selectedObject).GetComponent<Button>().onClick.Invoke();
                }));
            }
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate { // switch between customers and tickets
            return Input.GetKeyDown(KeyCode.LeftArrow);
        }, delegate {
            if(!search.isFocused) transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return Input.GetKeyDown(KeyCode.RightArrow);
        }, delegate {
            if(!search.isFocused) transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Button>().onClick.Invoke();
        }));
    }

    float PixelsScrolledDownForTicketsScrollRect() => transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().offsetMax.y;

    float PixelsInViewForTicketsScrollRect() => transform.GetChild(INDEX_OF_TICKETS).GetComponent<RectTransform>().rect.size.y;

    void SetUpStatus() {
        transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetComponent<Button>().onClick.AddListener(delegate {
            transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetChild(0).gameObject.SetActive(true);
        });
        transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetChild(0).GetChild(statuses.Length).GetComponent<Button>().onClick.AddListener(delegate {
            transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetChild(0).gameObject.SetActive(false);
        });
        for(int i = 0; i < statuses.Length; i++) {
            int x = i;
            transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetChild(0).GetChild(i).GetComponent<Button>().onClick.AddListener(delegate {
                ChangeStatus(x);
            });
        }
        transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetChild(0).GetChild(statuses.Length - 1).GetComponent<Image>().color = new Color32(44, 44, 44, 255);
        statusesToHide.Add(statuses[^1]);
    }

    void ChangeStatus(int index) {
        if(transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetChild(0).GetChild(index).GetComponent<Image>().color == new Color32(44, 44, 44, 255)) {
            transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetChild(0).GetChild(index).GetComponent<Image>().color = statusColors[index];
            statusesToHide.Remove(statuses[index]);
        } else {
            transform.GetChild(INDEX_OF_LABLES).GetChild(3).GetChild(0).GetChild(index).GetComponent<Image>().color = new Color32(44, 44, 44, 255);
            statusesToHide.Add(statuses[index]);
        }
        ListTheTickets();
    }
    
    void SetUpLables() {
        for(int i = 0; i < ticketsLables.Length; i++) {
            transform.GetChild(INDEX_OF_LABLES).GetChild(i + 1).GetComponent<TextMeshProUGUI>().text = ticketsLables[i];
        }
        transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate {
            //if(transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Image>().enabled) return;
            //string query = search.text;
            RemoveEverythingFromList();
            transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled = false;
            transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Image>().enabled = true;
            for(int i = 0; i < customersLables.Length; i++) {
                transform.GetChild(INDEX_OF_LABLES).GetChild(i + 1).GetComponent<TextMeshProUGUI>().text = customersLables[i];
            }
            SearchBoxValueChanged(search.text);
            /*
            if(query == "") {
                RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
                RestClient.Get(Main.URL + "/customers").Then(response => {
                    if(this == null) return;
                    if(search.text == query) {
                        ListCustomers(JsonConvert.DeserializeObject<Customers>(response.Text).customers);
                    }
                }).Catch(error => { Alert("Customers were not listed because: " + error.Message); });
            } else {
                RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
                RestClient.Get(Main.URL + "/customers/autocomplete?query=" + query).Then(response => {
                    if(this == null) return;
                    if(search.text == query) {
                        ListCustomers(JsonConvert.DeserializeObject<Customers>(response.Text).customers);
                    }
                }).Catch(error => { Alert("Customers were not listed because: " + error.Message); });
            }
            */
        });
        transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(delegate {
            //if(transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled) return;
            //string query = search.text;
            RemoveEverythingFromList();
            transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
            transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
            for(int i = 0; i < ticketsLables.Length; i++) {
                transform.GetChild(INDEX_OF_LABLES).GetChild(i + 1).GetComponent<TextMeshProUGUI>().text = ticketsLables[i];
            }
            SearchBoxValueChanged(search.text);
            /*
            if(!(CanParse(query) && query.Length <= latestTicketNumber.ToString().Length) && CanParse(ParsePhoneNumber(query))) {
                RestClient.Get(Main.URL + "/tickets?query=" + query).Then(response => {
                    if(this == null) return;
                    if(search.text == query) ListSmallTickets(JsonConvert.DeserializeObject<Tickets>(response.Text).ToSmallTicketArray());
                }).Catch(error => { Alert("Tickets were not listed because: " + error.Message); });
            } else {
                SearchBoxValueChanged(search.text);
                //search.DeactivateInputField();
            }
            */
        });
    }

    public void NewCustomerButton() {
        if(CanParse(ParsePhoneNumber(search.text))) {
            Main.NewCustomer(long.Parse(ParsePhoneNumber(search.text)));
        } else if(search.text.Contains(" ")) {
            Main.NewCustomer(search.text[..search.text.LastIndexOf(" ")], search.text[(search.text.LastIndexOf(" ") + 1)..]);
        } else {
            Main.NewCustomer(search.text);
        }
    }

    public void SearchBoxValueChanged(string query) {
        query = query.Trim();
        RemoveEverythingFromList();
        if(transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Image>().enabled) {
            if(query == "") {
                RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
                RestClient.Get(Main.URL + "/customers").Then(response => {
                    if(this == null) return;
                    if(search.text.Trim() == query) ListCustomers(JsonConvert.DeserializeObject<Customers>(response.Text).customers);
                }).Catch(error => { Alert("Customers were not listed because: " + error.Message); });
                return;
            }
            StartCoroutine(ExecuteActionIfTrueAfterTime(delegate {
                return search.text.Trim() == query;
            }, new WaitForSeconds(.3f), delegate {
                RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
                RestClient.Get(Main.URL + "/customers/autocomplete?query=" + (CanParse(ParsePhoneNumber(query)) ? ParsePhoneNumber(query) : query)).Then(response => {
                    if(this == null) return;
                    if(search.text.Trim() == query) ListCustomers(JsonConvert.DeserializeObject<Customers>(response.Text).customers);
                }).Catch(error => { Alert("Customers were not listed because: " + error.Message); });
            }));
            return;
        }
        if(query == "") {
            ListTheTickets();
        } else if(CanParse(query) && query.Length <= latestTicketNumber.ToString().Length) {
            StartCoroutine(ExecuteActionIfTrueAfterTime(delegate {
                return search.text.Trim() == query;
            }, new WaitForSeconds(.3f), delegate {
                SearchTicketNumber(query, 7);
            }));
        } else if(CanParse(ParsePhoneNumber(query))) {
            StartCoroutine(ExecuteActionIfTrueAfterTime(delegate {
                return search.text.Trim() == query;
            }, new WaitForSeconds(.3f), delegate {
                RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
                RestClient.Get(Main.URL + "/customers/autocomplete?query=" + ParsePhoneNumber(query)).Then(response => {
                    if(this == null)
                        return;
                    if(search.text.Trim() == query)
                        ListCustomers(JsonConvert.DeserializeObject<Customers>(response.Text).customers);
                }).Catch(error => { Alert("Customers were not listed because: " + error.Message); });
            }));
        } else {
            StartCoroutine(ExecuteActionIfTrueAfterTime(delegate {
                return search.text.Trim() == query;
            }, new WaitForSeconds(.3f), delegate {
                RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
                RestClient.Get(Main.URL + "/tickets?query=" + query).Then(response => {
                    if(this == null)
                        return;
                    if(search.text.Trim() == query)
                        ListSmallTickets(JsonConvert.DeserializeObject<Tickets>(response.Text).ToSmallTicketArray());
                }).Catch(error => { Alert("Tickets were not listed because: " + error.Message); });
            }));
        }
    }

    void SearchTicketNumber(string query, int maximumAmountOfResponses) {
        if(query.Length > latestTicketNumber.ToString().Length) return;
        if(query.Length >= earliestTicketNumber.ToString().Length) {
            RestClient.Get(Main.URL + "/tickets?number=" + query).Then(response => {
                if(this == null) return;
                ListSmallTickets(JsonConvert.DeserializeObject<SmallTickets>(response.Text).tickets);
            }).Catch(error => { Alert("Tickets were not listed because: " + error.Message); });
            return;
        }
        List<SmallTickets> responses = new List<SmallTickets>();
        bool tooHigh = int.Parse(query) > int.Parse(latestTicketNumber.ToString()[^query.Length..] + "");
        for(int i = 0; i < maximumAmountOfResponses; i++) {
            int number = int.Parse(latestTicketNumber.ToString()[..^query.Length] + query) - (i *  (int) Math.Pow(10, query.Length));
            if(tooHigh) number -= (int) Math.Pow(10, query.Length);
            if(number < earliestTicketNumber) {
                maximumAmountOfResponses = i;
                break;
            }
            RestClient.Get(Main.URL + "/tickets?number=" + number).Then(response => {
                if(this == null) return;
                responses.Add(JsonConvert.DeserializeObject<SmallTickets>(response.Text));
            }).Catch(error => { Alert("Tickets were not listed because: " + error.Message); });
        }
        StartCoroutine(ExecuteActionWhenTrue(delegate { 
            return responses.Count == maximumAmountOfResponses;
        }, delegate { 
            if(search.text == query.ToString()) ListSmallTickets(new SmallTickets(responses).tickets); 
        }));
    }

    void ListTheTickets() {
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        RestClient.Get(Main.URL + "/tickets?").Then(response => {
            if(this == null) return;
            SmallTicket[] tickets = JsonConvert.DeserializeObject<SmallTickets>(response.Text).tickets;
            latestTicketNumber = tickets[0].number;
            ListSmallTickets(RemoveTicketsWithStatuses(tickets, statusesToHide));
        }).Catch(error => { Alert("Tickets were not listed because: " + error.Message); });
    }

    SmallTicket[] RemoveTicketsWithStatuses(SmallTicket[] tickets, List<string> statusesToHide) {
        List<SmallTicket> newTickets = new List<SmallTicket>();
        for(int i = 0; i < tickets.Length; i++) {
            if(!statusesToHide.Contains(ConvertFromLegacyStatus(tickets[i].status, tickets[i].WasPreDiagnosed()))) newTickets.Add(tickets[i]);
        }
        return newTickets.ToArray();
    }

    void ListSmallTickets(SmallTicket[] tickets, bool addBelowWhatsThere = false) {
        if(!addBelowWhatsThere) RemoveEverythingFromList();
        for(int i = 0; i < tickets.Length && i < 50; i++) {
            int x = i;
            List(new string[] { "#" + tickets[i].number, tickets[i].subject.Trim(), ConvertFromLegacyStatus(tickets[i].status, tickets[i].WasPreDiagnosed()), tickets[i].CreationDateFormatedToString(), FormatName(tickets[i].customer_business_then_name) }, 
            delegate {
                if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) Main.ViewCustomer(tickets[x].customer_id);
                else if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) Main.EditTicket(tickets[x].id);
                else Main.TicketViewer(tickets[x].id); 
            }, delegate {
                if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) Main.ViewCustomerInNewWindow(tickets[x].customer_id);
                else if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) Main.EditTicketInNewWindow(tickets[x].id);
                else Main.TicketViewerInNewWindow(tickets[x].id);
            }, i / 2 == i / 2f ? new Color32(50, 50, 50, 255) : new Color32(43, 43, 43, 255));
        }
        if(transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled) return;
        transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
        transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
        for(int i = 0; i < ticketsLables.Length; i++) {
            transform.GetChild(INDEX_OF_LABLES).GetChild(i + 1).GetComponent<TextMeshProUGUI>().text = ticketsLables[i];
        }
    }
    /*
    void ListSuperSmallTickets(SuperSmallTicket[] tickets) {
        RemoveEverythingFromList();
        for(int i = 0; i < tickets.Length && i < 50; i++) {
            int x = i;
            List(new string[] { "#" + tickets[i].number, tickets[i].subject.Trim(), "", "", "" }, delegate { 
                Main.TicketViewer(tickets[x].number.ToString());
            }, delegate {
                Main.TicketViewerInNewWindow(tickets[x].number.ToString());
            }, i / 2 == i / 2f ? new Color32(50, 50, 50, 255) : new Color32(43, 43, 43, 255));
        }
        if(transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled) return;
        transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
        transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
        for(int i = 0; i < ticketsLables.Length; i++) {
            transform.GetChild(INDEX_OF_LABLES).GetChild(i + 1).GetComponent<TextMeshProUGUI>().text = ticketsLables[i];
        }
    }
    */
    void ListCustomers(Customer[] customers, bool addBelowWhatsThere = false) {
        if(!addBelowWhatsThere) RemoveEverythingFromList();
        for(int i = 0; i < customers.Length && i < 50; i++) {
            int x = i;
            List(new string[] { "", FormatToPhoneNumber(customers[i].phone), "", customers[i].CreationDateFormatedToString(), FormatName(customers[i].business_then_name) }, delegate {
                if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) Main.EditCustomer(customers[x]);
                else Main.ViewCustomer(customers[x]);
            }, delegate {
                if((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) Main.EditCustomerInNewWindow(customers[x]);
                else Main.ViewCustomerInNewWindow(customers[x]);
            }, i / 2 == i / 2f ? new Color32(50, 50, 50, 255) : new Color32(43, 43, 43, 255));
        }
        if(transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Image>().enabled) return;
        transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled = false;
        transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(1).GetComponent<Image>().enabled = true;
        for(int i = 0; i < customersLables.Length; i++) {
            transform.GetChild(INDEX_OF_LABLES).GetChild(i + 1).GetComponent<TextMeshProUGUI>().text = customersLables[i];
        }
    }

    void List(string[] text, UnityAction pressed, UnityAction middleClicked, Color32 backgroundColor) {
        GameObject a = Instantiate(background, transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0));
        a.GetComponent<Button>().onClick.AddListener(pressed);
        a.AddComponent<OnMiddleClick>().SetListener(middleClicked);
        a.GetComponent<Image>().color = backgroundColor;
        for(int i = 0; i < text.Length; i++) {
            GameObject b = Instantiate(textGameObject, transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(i + 1));
            b.GetComponent<TextMeshProUGUI>().text = text[i];
            b.GetComponent<Button>().onClick.AddListener(pressed);
            b.AddComponent<OnMiddleClick>().SetListener(middleClicked);
        }
    }

    public void GetMoreTicketsOrCustomersAfterScrolling() {
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        StartCoroutine(ExecuteActionWhileTrue(.1f, delegate {
            return transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().offsetMin.y * -1 < 300 && transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().rect.size.y > transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetComponent<RectTransform>().rect.size.y; // if there are less than 300 pixels below where the user can see, and there are more tickets then can fit on screen return true to get more tickets
        }, delegate {
            RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
            if(search.text == "" && !gettingMoreAfterScrolling && transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).childCount != 0) {
                if(transform.GetChild(INDEX_OF_LABLES).GetChild(0).GetChild(0).GetComponent<Image>().enabled) {
                    gettingMoreAfterScrolling = true;
                    RestClient.Get(Main.URL + "/tickets?page=" + (currentPageNumber + 1)).Then(response => {
                        if(this == null) return;
                        ListSmallTickets(RemoveTicketsWithStatuses(JsonConvert.DeserializeObject<SmallTickets>(response.Text).tickets, statusesToHide), true);
                        currentPageNumber++;
                        gettingMoreAfterScrolling = false;
                    }).Catch(error => { Alert("Tickets were not listed because: " + error.Message); });
                } else {
                    gettingMoreAfterScrolling = true;
                    RestClient.Get(Main.URL + "/customers?page=" + (currentPageNumber + 1)).Then(response => {
                        if(this == null) return;
                        ListCustomers(JsonConvert.DeserializeObject<Customers>(response.Text).customers, true);
                        currentPageNumber++;
                        gettingMoreAfterScrolling = false;
                    }).Catch(error => { Alert("Customers were not listed because: " + error.Message); });
                }
            }
        }));
    }

    void RemoveEverythingFromList() {
        for(int i = transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).childCount - 1; i >= 0; i--) {
            for(int ii = 0; ii < ticketsLables.Length + 1; ii++) {
                Destroy(transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(ii).GetChild(i).gameObject);
            }
        }
        transform.GetChild(INDEX_OF_TICKETS).GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
        currentPageNumber = 1;
        selectedObject = -1;
    }

    public void HomeButton() => Main.TicketList();

}
