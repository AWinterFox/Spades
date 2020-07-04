using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class closePanel : MonoBehaviour
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
        panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
