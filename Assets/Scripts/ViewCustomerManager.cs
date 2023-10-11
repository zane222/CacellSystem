using RepairShopRObjects;
using Proyecto26;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;

public class ViewCustomerManager : UsefullMethods {

    GameObject background, textGameObject, keybindsObject;

    Customer customerBeingViewed;

    SmallTicket[] allTickets;

    Phone[] currentPhones;

    int selectedObject = -1;

    const int INDEX_OF_TICKETS = 2, INDEX_OF_CREATE_TICKET_BUTTON = 3, INDEX_OF_LABELS = 4, INDEX_OF_EDIT_CUSTOMER_BUTTON = 5, INDEX_OF_LOADING_PANNEL = 8, INDEX_OF_KEYBINDS_BUTTON = 7;

    void Constructor() {
        background = Resources.Load("Prefabs/UI/Background") as GameObject;
        textGameObject = Resources.Load("Prefabs/UI/Text") as GameObject;
        keybindsObject = Resources.Load("Prefabs/Keybinds/ViewCustomerKeybinds") as GameObject;
        transform.GetChild(INDEX_OF_KEYBINDS_BUTTON).GetComponent<Button>().onClick.AddListener(delegate {
            GameObject a = Instantiate(keybindsObject, transform);
            a.GetComponent<Button>().onClick.AddListener(delegate {
                Destroy(a);
            });
        });
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        transform.GetChild(INDEX_OF_CREATE_TICKET_BUTTON).GetComponent<Button>().onClick.AddListener(delegate {
            Main.NewTicket(customerBeingViewed.id, transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
        });
        transform.GetChild(INDEX_OF_CREATE_TICKET_BUTTON).gameObject.AddComponent<OnMiddleClick>().SetListener(delegate {
            Main.NewTicketInNewWindow(customerBeingViewed.id);
        });
        RestClient.Get(Main.URL + "/tickets?customer_id=" + customerBeingViewed.id).Then(response => {
            if(this == null) return;
            SmallTickets tickets = JsonConvert.DeserializeObject<SmallTickets>(response.Text);
            List<SmallTickets> responses = new List<SmallTickets> { tickets };
            for(int i = 2; i <= tickets.meta.total_pages; i++) {
                RestClient.Get(Main.URL + "/tickets?customer_id=" + customerBeingViewed.id + "&page=" + i).Then(response2 => {
                    if(this == null) return;
                    responses.Add(JsonConvert.DeserializeObject<SmallTickets>(response2.Text));
                }).Catch(error => { Alert("Can't view customer because: " + error.Message); });
            }
            StartCoroutine(ExecuteActionWhenTrue(delegate { return responses.Count == tickets.meta.total_pages; }, delegate {
                allTickets = new SmallTickets(responses).tickets;
                ListTickets(allTickets);
                List<string> passwords = new List<string>();
                for(int i = 0; i < allTickets.Length; i++) {
                    if(allTickets[i].GetPassword() != "") passwords.Add(allTickets[i].GetPassword());
                }
                if(passwords.Count > 0) {
                    StartCoroutine(ExecuteActionWhenTrue(delegate { return transform.GetChild(1).GetComponent<TextMeshProUGUI>().text.Contains("Phone numbers:"); }, delegate { 
                        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text += "\n\nPreviously used passwords:\n" + string.Join("\n", passwords.Distinct().ToArray());
                    }));
                }
            }));
        }).Catch(error => { Alert("Can't view customer's tickets because: " + error.Message); });
        RestClient.Get(Main.URL + "/customers/" + customerBeingViewed.id + "/phones").Then(response => {
            if(this == null) return;
            currentPhones = JsonConvert.DeserializeObject<Phones>(response.Text).phones;
            transform.GetChild(1).GetComponent<TextMeshProUGUI>().text += "\n\nPhone numbers: ";
            for(int i = 0; i < currentPhones.Length; i++) {
                transform.GetChild(1).GetComponent<TextMeshProUGUI>().text += "\n" + FormatToPhoneNumber(currentPhones[i].number);
            }
            bool customerHasChanged = false;
            StartCoroutine(ExecuteActionWhileTrue(30, delegate { return !customerHasChanged; }, delegate {
                HasCustomerChanged(delegate {
                    customerHasChanged = true;
                    Alert("The customer has just been edited, reload to see the changes. Any changes made after pressing the edit button may override changes just now made");
                });
            }));
        }).Catch(error => { Alert("Can't view customer's phone numbers because: " + error.Message); });
        transform.GetChild(INDEX_OF_EDIT_CUSTOMER_BUTTON).GetComponent<Button>().onClick.AddListener(delegate { Main.EditCustomer(customerBeingViewed); });
        transform.GetChild(INDEX_OF_EDIT_CUSTOMER_BUTTON).gameObject.AddComponent<OnMiddleClick>().SetListener(delegate { Main.EditCustomerInNewWindow(customerBeingViewed); });
        SetUpKeyCombinations();
    }

    public void Constructor(int customerID) {
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        RestClient.Get(Main.URL + "/customers/" + customerID).Then(response => {
            if(this == null) return;
            Constructor(JsonConvert.DeserializeObject<OneCustomer>(response.Text).customer);
        }).Catch(error => { Alert("Can't view customer because: " + error.Message); });
    }

    public void Constructor(Customer customer) {
        customerBeingViewed = customer;
        Constructor();
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = FormatName(customer.business_and_full_name);
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Created: " + customer.CreationDateFormatedToString() + transform.GetChild(1).GetComponent<TextMeshProUGUI>().text;
        Destroy(transform.GetChild(INDEX_OF_LOADING_PANNEL).gameObject);
    }

    void HasCustomerChanged(UnityAction CustomerHasChanged) {
        RestClient.Get(Main.URL + "/customers/" + customerBeingViewed.id).Then(response => {
            if(this == null) return;
            Customer customer = JsonConvert.DeserializeObject<OneCustomer>(response.Text).customer;
            if(customer.business_and_full_name != customerBeingViewed.business_and_full_name || customer.phone != customerBeingViewed.phone) {
                CustomerHasChanged();
                return;
            }
            RestClient.Get(Main.URL + "/customers/" + customerBeingViewed.id + "/phones").Then(response => {
                if(this == null) return;
                Phone[] phones = JsonConvert.DeserializeObject<Phones>(response.Text).phones;
                if(phones.Length != currentPhones.Length) {
                    CustomerHasChanged();
                    return;
                }
                for(int i = 0; i < phones.Length; i++) {
                    if(phones[i].number != currentPhones[i].number) {
                        CustomerHasChanged();
                        return;
                    }
                }
            }).Catch(error => { });
        }).Catch(error => { });
    }

    void SetUpKeyCombinations() {
        StartCoroutine(ExecuteActionWhileTrue(delegate { return Input.GetKeyDown(KeyCode.UpArrow) ? .6f : .05f; }, delegate { return !Input.GetKey(KeyCode.UpArrow); }, delegate {
            return Input.GetKey(KeyCode.UpArrow);
        }, delegate {
            if(selectedObject <= 0) return;
            selectedObject--;
            transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject).localScale = new Vector2(.99f, 1);
            if(selectedObject != transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).childCount - 1) transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject + 1).localScale = new Vector2(1, 1);
            if(selectedObject < Math.Round(PixelsScrolledDownForTicketsScrollRect() / 39.85196f) + 1) {
                transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition = new Vector2(transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.x, transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.y - 39.85196f);
            }
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate { return Input.GetKeyDown(KeyCode.DownArrow) ? new WaitForSeconds(.6f) : new WaitForSeconds(.05f); }, delegate { return !Input.GetKey(KeyCode.DownArrow); }, delegate {
            return Input.GetKey(KeyCode.DownArrow);
        }, delegate {
            if(transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).childCount - 1 <= selectedObject) return;
            selectedObject++;
            transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject).localScale = new Vector2(.99f, 1);
            if(selectedObject != 0) transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject - 1).localScale = new Vector2(1, 1);
            if(selectedObject > Math.Round(PixelsInViewForTicketsScrollRect() / 39.85196f + PixelsScrolledDownForTicketsScrollRect() / 39.85196f) - 2) {
                transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition = new Vector2(transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.x, transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.y + 39.85196f);
            }
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        }, delegate {
            if(transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).childCount > 0) {
                if(selectedObject == -1) transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Button>().onClick.Invoke();
                else transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject).GetComponent<Button>().onClick.Invoke();
            } else {
                StartCoroutine(ExecuteActionWhenTrue(delegate { return transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).childCount > 0; }, delegate {
                    if(selectedObject == -1) transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Button>().onClick.Invoke();
                    else transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0).GetChild(selectedObject).GetComponent<Button>().onClick.Invoke();
                }));
            }
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        }, delegate {
            transform.GetChild(INDEX_OF_CREATE_TICKET_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.E);
        }, delegate {
            transform.GetChild(INDEX_OF_EDIT_CUSTOMER_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.H);
        }, delegate {
            Main.TicketList();
        }));
    }

    float PixelsScrolledDownForTicketsScrollRect() {
        return transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetComponent<RectTransform>().offsetMax.y;
    }

    float PixelsInViewForTicketsScrollRect() {
        return transform.GetChild(INDEX_OF_TICKETS).GetComponent<RectTransform>().rect.size.y;
    }

    void ListTickets(SmallTicket[] tickets) {
        transform.GetChild(INDEX_OF_LABELS).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Tickets (" + tickets.Length + ")";
        for(int i = 0; i < tickets.Length; i++) {
            int x = i;
            List(new string[] { "", "#" + tickets[i].number.ToString(), tickets[i].subject.Trim(), ConvertFromLegacyStatus(tickets[i].status, tickets[i].WasPreDiagnosed()), tickets[i].CreationDateFormatedToString() }, 
            delegate {
                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) Main.EditTicket(tickets[x].id);
                else Main.TicketViewer(tickets[x].id); 
            }, 
            delegate {
                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) Main.EditTicketInNewWindow(tickets[x].id);
                else Main.TicketViewerInNewWindow(tickets[x].id);
            }, i / 2 == i / 2f ? new Color32(50, 50, 50, 255) : new Color32(43, 43, 43, 255));
        }
    }

    void List(string[] text, UnityAction pressed, UnityAction middleClicked, Color32 backgroundColor) {
        GameObject a = Instantiate(background, transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(0));
        a.GetComponent<Button>().onClick.AddListener(pressed);
        a.AddComponent<OnMiddleClick>().SetListener(middleClicked);
        a.GetComponent<Image>().color = backgroundColor;
        for(int i = 1; i < text.Length; i++) {
            GameObject b = Instantiate(textGameObject, transform.GetChild(INDEX_OF_TICKETS).GetChild(0).GetChild(0).GetChild(i));
            b.GetComponent<TextMeshProUGUI>().text = text[i];
            b.GetComponent<Button>().onClick.AddListener(pressed);
            b.AddComponent<OnMiddleClick>().SetListener(middleClicked);
        }
    }
    /*
    void MoveTicketsToAnotherCustomer(Customer newCustomer) {
        if(allTickets.Length == 0) {
            Alert("This customer doesn't have any tickets");
            return;
        }
    }
    */
}
