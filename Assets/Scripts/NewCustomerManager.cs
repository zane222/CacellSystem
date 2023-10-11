using UnityEngine;
using TMPro;
using RepairShopRObjects;
using Proyecto26;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class NewCustomerManager : UsefullMethods {

    GameObject button, plusButton, field, selectedIconButton, keybindsObject;

    Phones oldPhones;

    Customer customerBeingEdited;

    bool applyingCurrently = false;

    int selectedGrid = -1;

    const int INDEX_OF_FIRST_NAME = 0, INDEX_OF_LAST_NAME = 1, INDEX_OF_PHONE_NUMBER = 2, INDEX_OF_BUSINESS_NAME = 3, INDEX_OF_VIEW_CUSTOMER_BUTTON = 4, INDEX_OF_CREATE_BUTTON = 5, INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON = 6, INDEX_OF_KEYBINDS_BUTTON = 8;

    public readonly int[] INDEXES_OF_GRIDS_WITH_PLUS_BUTTON = new int[] { INDEX_OF_PHONE_NUMBER },
    INDEX_OF_GRIDS_WITH_ONLY_FIELD = new int[] { INDEX_OF_FIRST_NAME, INDEX_OF_LAST_NAME, INDEX_OF_BUSINESS_NAME };

    public void Constructor() {
        RestClient.DefaultRequestHeaders["Authorization"] = Main.apiKey;
        button = Resources.Load("Prefabs/UI/Button") as GameObject;
        plusButton = Resources.Load("Prefabs/UI/PlusButton") as GameObject;
        field = Resources.Load("Prefabs/UI/Field") as GameObject;
        selectedIconButton = Resources.Load("Prefabs/UI/SelectedIconButton") as GameObject;
        keybindsObject = Resources.Load("Prefabs/Keybinds/NewCustomerKeybinds") as GameObject;
        transform.GetChild(INDEX_OF_KEYBINDS_BUTTON).GetComponent<Button>().onClick.AddListener(delegate {
            GameObject a = Instantiate(keybindsObject, transform);
            a.GetComponent<Button>().onClick.AddListener(delegate {
                Destroy(a);
            });
        });
        transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).GetComponent<Button>().onClick.AddListener(delegate { Create(true); });
        transform.GetChild(INDEX_OF_CREATE_BUTTON).GetComponent<Button>().onClick.AddListener(delegate { Create(false); });
        AddField(INDEX_OF_FIRST_NAME, delegate (string previousText) {
            return FormatName(previousText);
        });
        AddField(INDEX_OF_LAST_NAME, delegate (string previousText) {
            return FormatName(previousText);
        });
        AddField(INDEX_OF_PHONE_NUMBER, delegate (string previousText) {
            return FormatToPhoneNumber(previousText);
        });
        AddPlusButtonForAddingAField(INDEX_OF_PHONE_NUMBER, delegate (string previousText) {
            return FormatToPhoneNumber(previousText);
        });
        AddField(INDEX_OF_BUSINESS_NAME, delegate (string previousText) {
            return FormatName(previousText);
        });
        SelectGrid(0);
        StartCoroutine(ExecuteActionInTheNextFrame(delegate { SelectGrid(INDEX_OF_FIRST_NAME); }));
        SetUpKeyCombinations();
    }

    public void Constructor(long phoneNumber) {
        Constructor();
        transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = FormatToPhoneNumber(phoneNumber.ToString());
    }

    public void Constructor(string firstName) {
        Constructor();
        transform.GetChild(INDEX_OF_FIRST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = firstName;
    }

    public void Constructor(string firstName, string lastName) {
        Constructor();
        transform.GetChild(INDEX_OF_FIRST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = firstName;
        transform.GetChild(INDEX_OF_LAST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = lastName;
    }

    public void Constructor(int customerID) { //edit
        RestClient.Get(Main.URL + "/customers/" + customerID).Then(response => {
            Constructor(JsonConvert.DeserializeObject<OneCustomer>(response.Text).customer);
        }).Catch(error => { Alert("Can't view customer because: " + error.Message); });
    }

    public void Constructor(Customer customer) { //edit
        Constructor();
        customerBeingEdited = customer;
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetChild(0).GetComponent<TextMeshProUGUI>().text = customerBeingEdited.business_and_full_name;
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<Button>().onClick.AddListener(delegate { Main.ViewCustomer(customerBeingEdited); });
        transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).gameObject.AddComponent<OnMiddleClick>().SetListener(delegate { Main.ViewCustomerInNewWindow(customerBeingEdited); });
        transform.GetChild(INDEX_OF_CREATE_BUTTON).gameObject.SetActive(false);
        transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).GetComponent<Button>().onClick.RemoveAllListeners();
        transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).GetComponent<Button>().onClick.AddListener(delegate { ApplyEdit(); });
        transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Apply Edit";
        transform.GetChild(INDEX_OF_FIRST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = customer.firstname;
        transform.GetChild(INDEX_OF_LAST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = customer.lastname;
        transform.GetChild(INDEX_OF_BUSINESS_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text = customer.business_name;
        if(customer.mobile == null || customer.mobile == "") transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = customer.phone;
        else transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = customer.mobile;
        RestClient.Get(Main.URL + "/customers/" + customer.id + "/phones").Then(response => {
            Phones phones = JsonConvert.DeserializeObject<Phones>(response.Text);
            if(phones.phones.Length == 0) return;
            transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text = phones.phones[0].number;
            transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().onDeselect.Invoke(phones.phones[0].number);
            for(int i = 1; i < phones.phones.Length; i++) {
                TMP_InputField newField = AddField(INDEX_OF_PHONE_NUMBER, delegate (string previousText) {
                    return FormatToPhoneNumber(previousText);
                });
                newField.text = phones.phones[i].number;
                newField.onDeselect.Invoke(phones.phones[0].number);
            }
            oldPhones = phones;
            StartCoroutine(ExecuteActionAfterFrames(1, delegate { 
                ButtonPressed(transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(0).GetChild(0)); 
            }));
            bool customerHasChanged = false;
            StartCoroutine(ExecuteActionWhileTrue(30, delegate { return !customerHasChanged; }, delegate {
                HasCustomerChanged(delegate {
                    customerHasChanged = true;
                    Alert("The customer has just been edited, reload to see the changes. Pressing 'apply edit' may override the changes just now made to the ticket"); 
                });
            }));
        }).Catch(error => { Alert("Could not find the other phone numbers because: " + error.Message); });
    }

    void HasCustomerChanged(UnityAction CustomerHasChanged) {
        RestClient.Get(Main.URL + "/customers/" + customerBeingEdited.id).Then(response => {
            if(this == null) return;
            Customer customer = JsonConvert.DeserializeObject<OneCustomer>(response.Text).customer;
            if(customer.business_and_full_name != customerBeingEdited.business_and_full_name || customer.phone != customerBeingEdited.phone) {
                CustomerHasChanged();
                return;
            }
            RestClient.Get(Main.URL + "/customers/" + customerBeingEdited.id + "/phones").Then(response => {
                if(this == null) return;
                Phone[] phones = JsonConvert.DeserializeObject<Phones>(response.Text).phones;
                if(phones.Length != oldPhones.phones.Length) {
                    CustomerHasChanged();
                    return;
                }
                for(int i = 0; i < phones.Length; i++) {
                    if(phones[i].number != oldPhones.phones[i].number) {
                        CustomerHasChanged();
                        return;
                    }
                }
            }).Catch(error => { });
        }).Catch(error => { });
    }

    void SetUpKeyCombinations() {
        StartCoroutine(ExecuteActionWhileTrue(delegate {
            return (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        }, delegate {
            transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
        StartCoroutine(ExecuteActionWhileTrue(delegate { return !Input.GetKey(KeyCode.Tab); }, delegate {
            return Input.GetKeyDown(KeyCode.Tab);
        }, delegate {
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift)) {
                SelectPreviousGrid();
            } else {
                SelectNextGrid();
            }
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.H)) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            for(int i = INDEX_OF_FIRST_NAME; i <= INDEX_OF_BUSINESS_NAME; i++) {
                if(IsAnyFieldFocusedInThisGrid(i)) return;
            }
            Main.TicketList();
        }));
        StartCoroutine(ExecuteActionWhileTrueButOnlyStartAfterNotTrue(delegate {
            return Input.GetKeyDown(KeyCode.C) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !(Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows));
        }, delegate {
            for(int i = INDEX_OF_FIRST_NAME; i <= INDEX_OF_BUSINESS_NAME; i++) {
                if(IsAnyFieldFocusedInThisGrid(i)) return;
            }
            transform.GetChild(INDEX_OF_VIEW_CUSTOMER_BUTTON).GetComponent<Button>().onClick.Invoke();
        }));
    }

    void Create(bool makeTicket) {
        if(applyingCurrently) return;
        if(transform.GetChild(INDEX_OF_FIRST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text.Replace("​", "").Trim() == "") {
            Alert("You may have not entered the first name");
            return;
        }
        if(ParsePhoneNumber(transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(IndexOfSelectedButton(INDEX_OF_PHONE_NUMBER)).GetComponent<TMP_InputField>().text).Length != 10) {
            Alert("You may have typed the phone number wrong");
            return;
        }
        transform.GetChild(INDEX_OF_CREATE_BUTTON).gameObject.SetActive(false);
        transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).gameObject.SetActive(false);
        applyingCurrently = true;
        RestClient.Post(Main.URL + "/customers", JsonConvert.SerializeObject(ThisCustomer())).Then(response => {
            Customer customer = JsonConvert.DeserializeObject<OneCustomer>(response.Text).customer;
            UpdatePhones(customer.id, delegate {
                if(makeTicket) Main.NewTicket(customer.id, customer.business_and_full_name);
                else Main.ViewCustomer(customer.id);
            });
        }).Catch(error => {
            Alert("Customer not created because: " + error.Message);
            transform.GetChild(INDEX_OF_CREATE_BUTTON).gameObject.SetActive(true);
            transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).gameObject.SetActive(true);
            applyingCurrently = false;
        });
    }

    void ApplyEdit() {
        if(applyingCurrently) return;
        if(transform.GetChild(INDEX_OF_FIRST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text.Replace("​", "").Trim() == "") {
            Alert("You may have not entered the first name");
            return;
        }
        if(ParsePhoneNumber(transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(IndexOfSelectedButton(INDEX_OF_PHONE_NUMBER)).GetComponent<TMP_InputField>().text).Length != 10) {
            Alert("You may have typed the phone number wrong");
            return;
        }
        transform.GetChild(INDEX_OF_CREATE_BUTTON).gameObject.SetActive(false);
        transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).gameObject.SetActive(false);
        applyingCurrently = true;
        RestClient.Put(Main.URL + "/customers/" + customerBeingEdited.id, JsonConvert.SerializeObject(ThisCustomer())).Then(response => {
            UpdatePhones(customerBeingEdited.id, delegate {
                Main.ViewCustomer(customerBeingEdited.id);
            });
        }).Catch(error => {
            Alert("Customer not edited because: " + error.Message);
            transform.GetChild(INDEX_OF_CREATE_BUTTON).gameObject.SetActive(true);
            transform.GetChild(INDEX_OF_CREATE_AND_MAKE_TICKET_BUTTON_AND_APPLY_EDIT_BUTTON).gameObject.SetActive(true);
            applyingCurrently = false;
        });
    }

    void UpdatePhones(int customerId, UnityAction then) {
        List<string> currentPhones = new List<string>();
        for(int i = 0; i < transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).childCount; i++) {
            if(ParsePhoneNumber(transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().text).Length != 10) continue;
            currentPhones.Add(ParsePhoneNumber(transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().text));
        }
        List<Phone> phonesToDelete = new List<Phone>();
        if(oldPhones != null) phonesToDelete = new List<Phone>(oldPhones.phones);
        DeletePhones(phonesToDelete, customerId, delegate {
            PostPhones(currentPhones.Distinct().ToList(), customerId, ParsePhoneNumber(transform.GetChild(INDEX_OF_PHONE_NUMBER).GetChild(1).GetChild(IndexOfSelectedButton(INDEX_OF_PHONE_NUMBER)).GetComponent<TMP_InputField>().text), delegate {
                then();
            });
        });
    }

    void DeletePhones(List<Phone> phones, int customerId, UnityAction then) {
        if(phones.Count == 0) {
            then();
            return;
        }
        int amoutDeleted = 0;
        for(int i = 0; i < phones.Count; i++) {
            RestClient.Delete(Main.URL + "/customers/" + customerId + "/phones/" + phones[i].id).Then(response => {
                amoutDeleted++;
                if(amoutDeleted == phones.Count) then();
            }).Catch(error => { Alert("Some phone numbers could not be added/edited because: " + error.Message); });
        }
    }

    void PostPhones(List<string> phones, int customerId, string selectedPhone, UnityAction then) {
        if(phones.Count == 0) {
            then();
            return;
        }
        int amoutPosted = 0;
        for(int i = 0; i < phones.Count; i++) {
            RestClient.Post(Main.URL + "/customers/" + customerId + "/phones", new PostPhone(phones[i], true)).Then(response => {
                amoutPosted++;
                if(amoutPosted == phones.Count) MakeTheCorrectPhoneBeFirst(customerId, selectedPhone, then);
            }).Catch(error => { Alert("Some phone numbers could not be added/edited because: " + error.Message); });
        }
    }

    void MakeTheCorrectPhoneBeFirst(int customerId, string selectedPhone, UnityAction then) {
        RestClient.Get(Main.URL + "/customers/" + customerId + "/phones").Then(response => {
            Phones phones = JsonConvert.DeserializeObject<Phones>(response.Text);
            if(phones.phones[0].number == selectedPhone) {
                then();
            } else {
                RestClient.Delete(Main.URL + "/customers/" + customerId + "/phones/" + phones.phones[0].id).Then(response => {
                    RestClient.Delete(Main.URL + "/customers/" + customerId + "/phones/" + phones.phones[IndexOf(phones.ToStringArray(), selectedPhone)].id).Then(response => {
                        RestClient.Post(Main.URL + "/customers/" + customerId + "/phones", new PostPhone(selectedPhone, true)).Then(response => {
                            RestClient.Post(Main.URL + "/customers/" + customerId + "/phones", new PostPhone(phones.phones[0].number, true)).Then(response => {
                                MakeTheCorrectPhoneBeFirst(customerId, selectedPhone, then);
                            }).Catch(error => { Alert("Some phone numbers could not be added/edited because: " + error.Message); });
                        }).Catch(error => { Alert("Some phone numbers could not be added/edited because: " + error.Message); });
                    }).Catch(error => { Alert("Some phone numbers could not be added/edited because: " + error.Message); });
                }).Catch(error => { Alert("Some phone numbers could not be added/edited because: " + error.Message); });
            }
        }).Catch(error => { Alert("Some phone numbers could put into the correct order because: " + error.Message); });
    }

    TMP_InputField AddField(int grid, Func<string, string> onDeselectChangeTextTo) {
        TMP_InputField newField;
        if(grid == INDEX_OF_PHONE_NUMBER) newField = Instantiate(field, transform.GetChild(grid).GetChild(1)).GetComponent<TMP_InputField>();
        else newField = Instantiate(field, transform.GetChild(grid).GetChild(0).transform).GetComponent<TMP_InputField>();
        newField.onSelect.AddListener(delegate { 
            SelectGrid(grid); 
        });
        newField.onDeselect.AddListener(delegate { 
            newField.text = onDeselectChangeTextTo(newField.text); 
        });
        newField.onSubmit.AddListener(delegate { 
            newField.text = onDeselectChangeTextTo(newField.text); 
        });
        newField.onSubmit.AddListener(delegate {
            newField.text = onDeselectChangeTextTo(newField.text);
        });
        if(grid != INDEX_OF_PHONE_NUMBER) return newField;
        Transform buttonObject = Instantiate(button, transform.GetChild(grid).GetChild(0)).transform;
        buttonObject.SetSiblingIndex(transform.GetChild(grid).GetChild(0).childCount - 2);
        buttonObject.GetComponent<Button>().onClick.AddListener(delegate { ButtonPressed(buttonObject); });
        ButtonPressed(buttonObject);
        return newField;
    }

    void AddPlusButtonForAddingAField(int grid, Func<string, string> onDeselectChangeTextTo) {
        GameObject plusButtonObject = Instantiate(plusButton, transform.GetChild(grid).GetChild(0));
        plusButtonObject.GetComponent<Button>().onClick.AddListener(delegate {
            AddField(grid, onDeselectChangeTextTo);
            AddPlusButtonForAddingAField(grid, onDeselectChangeTextTo);
            Destroy(plusButtonObject);
        });
    }

    public void ButtonPressed(Transform buttonPressed) {
        SelectGrid(buttonPressed.parent.parent.GetSiblingIndex());
        RemoveAllOtherSelectedIcons(buttonPressed.parent, buttonPressed.GetSiblingIndex());
        if(buttonPressed.childCount <= 0) Instantiate(selectedIconButton, buttonPressed);
    }

    void RemoveAllOtherSelectedIcons(Transform grid, int indexOfOneToKeep) {
        for(int i = 0; i < grid.childCount; i++) {
            if(grid.GetChild(i).childCount > 0 && i != indexOfOneToKeep && !grid.GetChild(i).gameObject.name.Contains("PlusButton")) {
                Destroy(grid.GetChild(i).GetChild(0).gameObject);
            }
        }
    }

    object ThisCustomer() {
        if(customerBeingEdited == null) {
            return new PostCustomer(
                FormatName(transform.GetChild(INDEX_OF_FIRST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text),
                FormatName(transform.GetChild(INDEX_OF_LAST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text),
                FormatName(transform.GetChild(INDEX_OF_BUSINESS_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text),
                ""
            );
        } else {
            return new Customer(
                FormatName(transform.GetChild(INDEX_OF_FIRST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text),
                FormatName(transform.GetChild(INDEX_OF_LAST_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text),
                FormatName(transform.GetChild(INDEX_OF_BUSINESS_NAME).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text),
                "",
                customerBeingEdited
            );
        }
    }

    bool IsAnyFieldFocusedInThisGrid(int indexOfGrid) {
        if(Contains(INDEX_OF_GRIDS_WITH_ONLY_FIELD, indexOfGrid)) return transform.GetChild(indexOfGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().isFocused;
        if(Contains(INDEXES_OF_GRIDS_WITH_PLUS_BUTTON, indexOfGrid)) {
            for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(1).childCount; i++) {
                if(transform.GetChild(indexOfGrid).GetChild(1).GetChild(i).name.Contains("Field")) {
                    if(transform.GetChild(indexOfGrid).GetChild(1).GetChild(i).GetComponent<TMP_InputField>().isFocused) return true;
                }
            }
        }
        return false;
    }

    int IndexOfSelectedButton(int indexOfGrid) {
        for(int i = 0; i < transform.GetChild(indexOfGrid).GetChild(0).childCount - 1; i++) {
            if(transform.GetChild(indexOfGrid).GetChild(0).GetChild(i).childCount > 0) return i;
        }
        return transform.GetChild(indexOfGrid).GetChild(0).childCount - 1;
    }

    void SelectPreviousGrid() {
        SelectGrid(selectedGrid - 1);
    }

    void SelectNextGrid() {
        SelectGrid(selectedGrid + 1);
    }

    void SelectGrid(int grid) {
        if(grid > INDEX_OF_BUSINESS_NAME || grid < INDEX_OF_FIRST_NAME) {
            SelectGrid(INDEX_OF_FIRST_NAME);
            return;
        }
        if(grid == selectedGrid) return;
        if(selectedGrid != -1) DeselectPreviousGrid();
        selectedGrid = grid;
        transform.GetChild(selectedGrid).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Underline;
        if(grid != INDEX_OF_PHONE_NUMBER) transform.GetChild(selectedGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().ActivateInputField();
        else transform.GetChild(selectedGrid).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().ActivateInputField();
    }

    void DeselectPreviousGrid() {
        transform.GetChild(selectedGrid).GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        if(selectedGrid != INDEX_OF_PHONE_NUMBER) transform.GetChild(selectedGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().DeactivateInputField();
        else transform.GetChild(selectedGrid).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().DeactivateInputField();
        if(selectedGrid != INDEX_OF_PHONE_NUMBER) transform.GetChild(selectedGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().onDeselect.Invoke(transform.GetChild(selectedGrid).GetChild(0).GetChild(0).GetComponent<TMP_InputField>().text);
        else transform.GetChild(selectedGrid).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().onDeselect.Invoke(transform.GetChild(selectedGrid).GetChild(1).GetChild(0).GetComponent<TMP_InputField>().text);
    }

}
