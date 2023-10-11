using Proyecto26;
using RepairShopRObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Events;

public class TicketViewerManager : UsefullMethods {

    GameObject background, textGameObject, textWithTopRightAchorsGameObject, fieldGameObject, keybindsObject;

    LargeTicket ticketBeingViewed;

    bool currentlyCreatingAComment = false;

    int selectedTechnician;

    readonly string[] technicians = { "Other", "Zane", "Emad", "Abu Adam", "Rolando", "Yusuf" };
    
    readonly string[] statusComments = { "", "", "", "Ordered", "", "Part recieved", "Ready", "They picked it up" };

    const int INDEX_OF_TICKET_LABEL_IMAGE = 0, INDEX_OF_COMMENTS = 1, INDEX_OF_STATUS = 4, INDEX_OF_EDIT_TICKET_BUTTON = 5, INDEX_OF_PRINT_BUTTON = 6, INDEX_OF_VIEW_CUSTOMER_BUTTON = 7, INDEX_OF_LOADING_PANNEL = 10, INDEX_OF_KEYBINDS_BUTTON = 9;

    public void Constructor(LargeTicket ticket, bool alreadyTriedToGetRidOfTheHTML = false) {
        if(!alreadyTriedToGetRidOfTheHTML) {
            for(int i = 0; i < ticket.comments.Length; i++) {
                if(ticket.comments[i].body.Contains("<")) {
                    RestClient.Get(Main.URL + "/tickets/" + ticket.id).Then(response => {
                        if(this == null) return;
                        LargeTicket newTicket = JsonConvert.DeserializeObject<OneLargeTicket>(response.Text).ticket;
                        Constructor(newTicket, true);
                    }).Catch(error => { Alert("Can't view ticket because: " + error.Message); });
                    return;
                }
            }
        }
        background = Resources.Load("Prefabs/UI/Background") as GameObject;
        textGameObject = Resources.Load("Prefabs/UI/Text") as GameObject;
        textWithTopRightAchorsGameObject = Resources.Load("Prefabs/UI/TextWithTopRightAnchors") as GameObject;
        fieldGameObject = Resources.Load("Prefabs/UI/Field2") as GameObject;
        keybindsObject = Resources.Load("Prefabs/Keybinds/TicketViewerKeybinds") as GameObject;
        transform.GetChild(INDEX_OF_KEYBINDS_BUTTON).GetComponent<Button>().onClick.AddListener(delegate {
            GameObject a = Instantiate(keybindsObject, transform);
            a.GetComponent<Button>().onClick.AddListener(delegate {
                Destroy(a);
            });
        });
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        ticketBeingViewed = ticket;
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<Button>().onClick.AddListener(delegate {
            Main.ViewCustomer(ticket.customer_id);
            Destroy(transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).gameObject);
        });
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).gameObject.AddComponent<OnMiddleClick>().SetListener(delegate {
            Main.ViewCustomerInNewWindow(ticket.customer_id);
        });
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetChild(0).GetComponent<TextMeshProUGUI>().text = FormatName(ticket.customer_business_then_name);
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<ChangeSizeToFirstChildsPreferedSize>().Start();
        ListComments(ticket.comments);
        selectedTechnician = PlayerPrefs.GetInt("selectedTechnician");
        transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = technicians[selectedTechnician];
        transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(delegate {
            if(selectedTechnician != technicians.Length - 1) {
                selectedTechnician++;
            } else {
                selectedTechnician = 0;
            }
            transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = technicians[selectedTechnician];
            PlayerPrefs.SetInt("selectedTechnician", selectedTechnician);
        });
        transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.AddComponent<OnRightClick>().SetListener(delegate {
            if(selectedTechnician != 0) {
                selectedTechnician--;
            } else {
                selectedTechnician = technicians.Length - 1;
            }
            transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = technicians[selectedTechnician];
            PlayerPrefs.SetInt("selectedTechnician", selectedTechnician);
        });
        string itemsLeft = "";
        if(ticket.GetTicketDetails() != null) {
            if(ticket.GetTicketDetails().itemsLeft.Length != 0) {
                itemsLeft = "They left " + string.Join(", ", ticket.GetTicketDetails().itemsLeft);
            }
        } else if(ticket.properties.ACCharger()) itemsLeft = "They left AC Charger";
        MakeTicketLabelImage(
            ticket.subject,
            ticket.GetPassword(),
            itemsLeft,
            ticket.CreationDateFormatedToString() + " | " + ticket.CreationTimeFormatedToString(),
            FormatName(ticket.customer.business_and_full_name),
            "# " + ticket.number,
            FormatToPhoneNumber(ticket.customer.phone),
            (int) transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().rect.size.x,
            delegate (Sprite sprite) {
                transform.GetChild(INDEX_OF_TICKET_LABEL_IMAGE).GetChild(0).GetChild(0).GetComponent<Image>().sprite = sprite;
                transform.GetChild(INDEX_OF_TICKET_LABEL_IMAGE).GetChild(0).GetChild(0).GetComponent<Image>().preserveAspect = true;
                Destroy(transform.GetChild(INDEX_OF_LOADING_PANNEL).gameObject);
            }
        );
        SetUpStatus(ticket.status, ticket.WasPreDiagnosed());
        transform.GetChild(INDEX_OF_EDIT_TICKET_BUTTON).GetComponent<Button>().onClick.AddListener(delegate { Main.EditTicket(ticketBeingViewed); });
        transform.GetChild(INDEX_OF_EDIT_TICKET_BUTTON).gameObject.AddComponent<OnMiddleClick>().SetListener(delegate { Main.EditTicketInNewWindow(ticketBeingViewed); });
        SetUpKeyCombinations();
        bool hasTicketChanged = false;
        StartCoroutine(ExecuteActionWhileTrue(30, delegate { return !hasTicketChanged; }, delegate {
            HasTicketChanged(delegate {
                hasTicketChanged = true;
                Alert("The ticket has just been edited, reload to see the changes. Any changes to the status or made after pressing the edit button may override the changes just now made to the ticket");
            });
        }));
    }

    public void Constructor(int ticketID) {
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        RestClient.Get(Main.URL + "/tickets/" + ticketID).Then(response => {
            if(this == null) return;
            LargeTicket ticket = JsonConvert.DeserializeObject<OneLargeTicket>(response.Text).ticket;
            Constructor(ticket);
        }).Catch(error => { Alert("Can't view ticket because: " + error.Message); });
    }

    public void Constructor(string ticketNumber) {
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        RestClient.Get(Main.URL + "/tickets?number=" + ticketNumber).Then(response => {
            if(this == null) return;
            Constructor(JsonConvert.DeserializeObject<SmallTickets>(response.Text).tickets[0].id);
        }).Catch(error => { Alert("Can't view ticket because: " + error.Message); });
    }

    void HasTicketChanged(UnityAction TicketHasChanged) {
        RestClient.Get(Main.URL + "/tickets/" + ticketBeingViewed.id).Then(response => {
            if(this == null) return;
            LargeTicket ticket = JsonConvert.DeserializeObject<OneLargeTicket>(response.Text).ticket;
            if(ticket.GetPassword() != ticketBeingViewed.GetPassword() || ticket.customer.phone != ticketBeingViewed.customer.phone || ticket.subject != ticketBeingViewed.subject || ticket.comments.Length != ticketBeingViewed.comments.Length) {
                TicketHasChanged();
                return;
            }
        }).Catch(error => { });
    }

    void SetUpKeyCombinations() {
        StartCoroutine(ExecuteActionWhileTrue(delegate { return Input.GetKeyDown(KeyCode.UpArrow) ? .6f : .05f; }, delegate { return !Input.GetKey(KeyCode.UpArrow); }, delegate {
            return Input.GetKey(KeyCode.UpArrow);
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition = new Vector2(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.x, transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.y - 39.852f);
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate { return Input.GetKeyDown(KeyCode.DownArrow) ? new WaitForSeconds(.6f) : new WaitForSeconds(.05f); }, delegate { return !Input.GetKey(KeyCode.DownArrow); }, delegate {
            return Input.GetKey(KeyCode.DownArrow);
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition = new Vector2(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.x, transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetComponent<RectTransform>().localPosition.y + 39.852f);
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused && transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text[transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().caretPosition - 1] == '\n') {
                print("s");
                transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text.Remove(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().caretPosition - 1, 1);
            }
            CreateCommentButton();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.Tab) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().ActivateInputField();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.Tab) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) return;
            transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text.Remove(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().caretPosition - 1, 1);
            transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().DeactivateInputField();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.E) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_EDIT_TICKET_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.P) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_PRINT_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.D) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_STATUS).GetChild(0).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.F) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_STATUS).GetChild(1).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.A) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_STATUS).GetChild(2).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.W) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_STATUS).GetChild(3).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.O) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_STATUS).GetChild(4).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.I) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_STATUS).GetChild(5).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.R) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_STATUS).GetChild(6).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.X) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_STATUS).GetChild(7).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.U) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows)) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.U) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if (!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused)
                transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<OnRightClick>().action();
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.H);
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) Main.TicketList();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.C) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            if(!transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().isFocused) transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
    }

    void SetUpStatus(string status, bool isCallBy) {
        for(int i = 0; i < statuses.Length; i++) {
            if(i < 3 && !isCallBy) transform.GetChild(INDEX_OF_STATUS).GetChild(i).GetComponent<Image>().color = new Color32(44, 44, 44, 255);
            else {
                int x = i;
                transform.GetChild(INDEX_OF_STATUS).GetChild(i).GetComponent<Image>().color = new Color32(28, 28, 28, 255);
                transform.GetChild(INDEX_OF_STATUS).GetChild(i).GetComponent<Button>().onClick.AddListener(delegate {
                    ChangeStatus(x, isCallBy);
                });
            }
        }
        int index = ConvertFromLegacyStatusToStatusesIndex(status, isCallBy);
        if(index == -1) {
            transform.GetChild(INDEX_OF_STATUS).GetChild(statuses.Length).GetComponent<TextMeshProUGUI>().text = "Status: " + status;
            return;
        }
        transform.GetChild(INDEX_OF_STATUS).GetChild(index).GetComponent<Image>().color = statusColors[index];
    }

    void ChangeStatus(int index, bool isCallBy) {
        if(transform.GetChild(INDEX_OF_STATUS).GetChild(index).GetComponent<Image>().color == statusColors[index]) return;
        for(int i = 0; i < statuses.Length; i++) {
            if(i < 3 && !isCallBy) transform.GetChild(INDEX_OF_STATUS).GetChild(i).GetComponent<Image>().color = new Color32(44, 44, 44, 255);
            else {
                transform.GetChild(INDEX_OF_STATUS).GetChild(i).GetComponent<Image>().color = new Color32(28, 28, 28, 255);
            }
        }
        transform.GetChild(INDEX_OF_STATUS).GetChild(index).GetComponent<Image>().color = statusColors[index];
        if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            if(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text.Length < 5 || Contains(statusComments, transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text)) {
                transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = statusComments[index];
            }
        }
        ticketBeingViewed.status = legacyStatuses[index];
        transform.GetChild(INDEX_OF_STATUS).GetChild(statuses.Length).GetComponent<TextMeshProUGUI>().text = "Status updating...";
        RestClient.Put(Main.URL + "/tickets/" + ticketBeingViewed.id, JsonConvert.SerializeObject(ticketBeingViewed)).Then(response => {
            if(this == null) return;
            transform.GetChild(INDEX_OF_STATUS).GetChild(statuses.Length).GetComponent<TextMeshProUGUI>().text = "Status:";
        }).Catch(error => { Alert("Ticket status was not updated because: " + error.Message); });
    }

    void ListComments(Comment[] comments) {
        GameObject comment = Instantiate(background, transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0));
        comment.GetComponent<Image>().color = new Color32(43, 43, 43, 255);
        GameObject a = Instantiate(fieldGameObject, transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1));
        a.AddComponent<OnRightClick>().SetListener(delegate {
            GetClipboard(delegate (string text) {
                a.GetComponent<TMP_InputField>().text += text;
                a.GetComponent<TMP_InputField>().caretPosition = a.GetComponent<TMP_InputField>().text.Length;
            });
        });
        Instantiate(textWithTopRightAchorsGameObject, comment.transform).transform.GetComponent<TextMeshProUGUI>().text = "Cacell System";
        for(int i = 0; i < comments.Length; i++) {
            if(comments[i].user_id == 0) continue;
            string[] links = GetLinksFromText(comments[i].body);
            List(ReplaceMultiple(comments[i].body, links, "[link]"), links, (comments[i].IsSms() ? "Public so probably SMS - " : "") + FormatName(comments[i].tech) + " - " + comments[i].CreationDateFormatedToString() + " | " + comments[i].CreationTimeFormatedToString(),
                comments[i].IsSms() ?
                    comments[i].tech == "customer-reply" ?
                        i / 2 == i / 2f ? new Color32(20, 33, 50, 255) : new Color32(25, 36, 50, 255)
                    :
                        i / 2 == i / 2f ? new Color32(17, 43, 22, 255) : new Color32(22, 43, 26, 255)
                :
                    i / 2 == i / 2f ? new Color32(50, 50, 50, 255) : new Color32(43, 43, 43, 255)
            );
        }
    }

    void List(string body, string[] links, string otherInfo, Color32 backgroundColor) {
        GameObject a = Instantiate(background, transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(0));
        a.GetComponent<Image>().color = backgroundColor;
        a.GetComponent<Button>().onClick.AddListener(delegate { foreach(string link in links) Application.OpenURL(link); });
        GameObject b = Instantiate(textGameObject, transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1));
        b.transform.GetComponent<TextMeshProUGUI>().text = body;
        b.GetComponent<Button>().onClick.AddListener(delegate { foreach(string link in links) Application.OpenURL(link); });
        GameObject c = Instantiate(textWithTopRightAchorsGameObject, a.transform);
        c.transform.GetComponent<TextMeshProUGUI>().text = otherInfo;
        c.GetComponent<Button>().onClick.AddListener(delegate { foreach(string link in links) Application.OpenURL(link); });
    }

    public void PDFButton() {
        //if(!(Keyboard.current.leftShiftKey.IsActuated() || Keyboard.current.rightShiftKey.IsActuated())) {
            PrintLabel();
        /*    return;
        }
        string subjectWithoutPrices = ticketBeingViewed.subject;
        for(int i = 0; subjectWithoutPrices.Contains("$") && i < 10; i++) {
            subjectWithoutPrices = subjectWithoutPrices.Replace(GetWordAfterIndex(subjectWithoutPrices, subjectWithoutPrices.IndexOf("$") - 2), "");
        }
        print(subjectWithoutPrices.Replace("  ", " "));
        MakeTicketLabelImage(
            subjectWithoutPrices.Replace("  ", " "),
            "",
            "",
            "",
            "",
            "# " + ticketBeingViewed.number,
            "",
            336,
            delegate (Sprite sprite) {
                PrintLabel();
                string itemsLeft = "";
                if(ticketBeingViewed.GetTicketDetails() != null) {
                    if(ticketBeingViewed.GetTicketDetails().itemsLeft.Length != 0) {
                        itemsLeft = "They left " + StringArrayToSingleString(ticketBeingViewed.GetTicketDetails().itemsLeft);
                    }
                } else if(ticketBeingViewed.properties.ACCharger()) itemsLeft = "They left AC Charger";
                MakeTicketLabelImage(
                    ticketBeingViewed.subject,
                    ticketBeingViewed.GetPassword(),
                    itemsLeft,
                    ticketBeingViewed.CreationDateFormatedToString() + " | " + ticketBeingViewed.CreationTimeFormatedToString(),
                    FormatName(ticketBeingViewed.customer.business_and_full_name),
                    "# " + ticketBeingViewed.number,
                    FormatToPhoneNumber(ticketBeingViewed.customer.phone),
                    (int)(transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().rect.x * -2),
                    delegate (Sprite sprite) { }
                );
                print((int)(transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().rect.x * -2));
            }
        );
        print((int)(transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().rect.x * -2));
        */
    }

    public void CreateCommentButton() {
        if(currentlyCreatingAComment) return;
        if(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).childCount == 0) return;
        if(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text.Replace("\n", "").Length == 0) return;
        currentlyCreatingAComment = true;
        RestClient.Post(Main.URL + "/tickets/" + ticketBeingViewed.id + "/comment", new PostComment(transform.GetChild(INDEX_OF_COMMENTS).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text.Trim(), technicians[selectedTechnician])).Then(response => {
            Main.TicketViewer(ticketBeingViewed.id);
        }).Catch(error => {
            currentlyCreatingAComment = false;
            Alert("Can't post comment because: " + error.Message); 
        });
    }

}
