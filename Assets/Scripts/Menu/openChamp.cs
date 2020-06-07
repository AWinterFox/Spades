using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class openChamp : MonoBehaviour
{
    [SerializeField]
    private GameObject panel;

    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(onClick);
    }

    private void onClick()
    {
        panel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
