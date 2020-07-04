using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class openPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;
    private Button button;
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
}
