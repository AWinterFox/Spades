using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class openPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;

    [SerializeField]
    GameObject notice;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void onClic()
    {
        panel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void clickandRick(string status)
    {
        notice.GetComponent<UnityEngine.UI.Text>().text = status;
        panel.SetActive(true);
    }
}
