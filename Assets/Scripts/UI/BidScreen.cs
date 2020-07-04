using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BidScreen : MonoBehaviour
{
    [SerializeField]
    private Button[] buttons;

    [SerializeField]
    private GameObject bidScreen;

    private GameManager manager;

    public UnityEvent<int> OnBid = new BidEvent();

    private void Awake()
    {
        manager = GetComponent<GameManager>();

        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            var x = i;
            button.onClick.AddListener(() => Bid(x));
        }
    }

    public void Show(Player player)
    {
        bidScreen.SetActive(true);
        foreach (var bid in buttons)
        {
            bid.gameObject.SetActive(true);
        }

        if (player.Position > 2)
        {
            if (player.Partner.Bid <= 2)
            {
                buttons[0].gameObject.SetActive(false);
            }
            if (player.Partner.Bid <= 1)
            {
                buttons[1].gameObject.SetActive(false);
            }
        }
        if (player.Position > 3)
        {
            if (manager.CombinedBid <= 9)
            {
                buttons[0].gameObject.SetActive(false);
            }
            if (manager.CombinedBid <= 8)
            {
                buttons[1].gameObject.SetActive(false);
            }
            if (manager.CombinedBid <= 7)
            {
                buttons[2].gameObject.SetActive(false);
            }
            if (manager.CombinedBid <= 5)
            {
                buttons[3].gameObject.SetActive(false);
            }
            if (manager.CombinedBid <= 4)
            {
                buttons[4].gameObject.SetActive(false);
            }
            if (manager.CombinedBid <= 3)
            {
                buttons[5].gameObject.SetActive(false);
            }
        }

        if (player.Blind)
        {
            buttons[0].gameObject.SetActive(false);
            buttons[1].gameObject.SetActive(false);
            buttons[2].gameObject.SetActive(false);
            buttons[3].gameObject.SetActive(false);
            buttons[4].gameObject.SetActive(false);
        }
    }

    private void Bid(int i)
    {
        OnBid.Invoke(i);
        bidScreen.SetActive(false);
    }

    public class BidEvent : UnityEvent<int> { }
}
