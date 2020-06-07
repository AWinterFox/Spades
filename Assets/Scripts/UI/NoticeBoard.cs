using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NoticeBoard : MonoBehaviour
{
    [SerializeField]
    private TMP_Text panelText;

    [SerializeField]
    private GameObject panel;

    private static NoticeBoard instance;

    private void Awake()
    {
        instance = this;
        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public static void ShowMessage(string text, float time = 1.5f)
    {
        instance.StartCoroutine(instance.IShowMessage(text, time));
    }

    private IEnumerator IShowMessage(string text, float time)
    {
        panelText.text = text;
        panel.SetActive(true);
        yield return new WaitForSeconds(time);
        panel.SetActive(false);
    }
}
