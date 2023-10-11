using UnityEngine;
using UnityEngine.UI;

public class Top : MonoBehaviour {

    void Start() {
        transform.GetChild(0).GetComponent<Button>().onClick.AddListener(Main.TicketList);
        transform.GetChild(2).GetComponent<Button>().onClick.AddListener(Main.TicketList);
        transform.GetChild(3).GetComponent<Button>().onClick.AddListener(Main.NewCustomer);
    }

}
