using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class ActivePlayer : MonoBehaviour
{
    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private BidScreen bidScreen;

    private Controls controls;

    private Player player;
    private GameManager manager;

    private bool canPlayCard = false;

    // Start is called before the first frame update
    void Awake()
    {
        manager = FindObjectOfType<GameManager>();
        player = GetComponent<Player>();
        
        controls = new Controls();   
        controls.Game.Select.performed += OnSelect;
        bidScreen.OnBid.AddListener(PlaceBid);

        if (!player.photonView.IsMine) {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        controls.Enable();
        player.OnBidRoundStarted.AddListener(BidRoundStart);
        player.OnCanPlayCard.AddListener(CanPlayCard);
        player.OnSortCards.AddListener(OnSortCards);
    }

    private void OnDisable()
    {
        controls.Disable();
        player.OnBidRoundStarted.RemoveListener(BidRoundStart);
        player.OnCanPlayCard.RemoveListener(CanPlayCard);
        player.OnSortCards.RemoveListener(OnSortCards);
    }

    private List<Card> GetPlayable()
    {
        var playable = new List<Card>();
        if (player.Position == 1)
        {
            if (manager.SpadesBroken)
            {
                playable = player.Cards;
            }
            else
            {
                playable = player.Cards.Where(c => c.Suit != CardSuit.Spades).ToList();
            }
        }
        else
        {
            if (player.Cards.Any(c => c.Suit == manager.TrickCard.Suit))
            {
                playable = player.Cards.Where(c => c.Suit == manager.TrickCard.Suit).ToList();
            }
            else
            {
                playable = player.Cards;
            }
        }
        return playable;
    }

    private void CanPlayCard(int position)
    {
        canPlayCard = true;

        var playable = GetPlayable();

        foreach (var card in player.Cards)
        {
            card.SetInactive(!playable.Contains(card));
        }
    }

    private void OnSortCards()
    {
        player.FlipCards(player.Blind ? CardFace.Back : CardFace.Front);
    }

    private void OnSelect(InputAction.CallbackContext obj)
    {
        var mousePosition = controls.Game.Position.ReadValue<Vector2>();
        var worldRay = camera.ScreenPointToRay(mousePosition);

        //Get closest card ot the screen.
        var colliders = Physics.RaycastAll(worldRay);
        var card = GetClosestCard(colliders);

        //Debug.Log($"Play card: {card?.name ?? "None"}");
        if (!canPlayCard) return;
        

        var playable = GetPlayable();
        if(card != null)
        {
            if (!player.Cards.Contains(card))
            {
                return;
            }
            //Spades rule
            if(card.Suit == CardSuit.Spades && !manager.SpadesBroken && player.Position == 1 && player.Cards.Any(c => c.Suit != CardSuit.Spades))
            {
                NoticeBoard.ShowMessage("Spades need to be broken before you can play one.");
            }else if (!playable.Contains(card))
            {
                NoticeBoard.ShowMessage($"You need to play a card with in the {manager.TrickCard.Suit.ToString()} suit.");
            }
            else
            {
                player.PlayCard(card);
                canPlayCard = false;
                foreach (var gCard in player.Cards)
                {
                    gCard.SetInactive(false);
                }
            }
            
        }
    }

    private Card GetClosestCard(RaycastHit[] hits)
    {
        Card highest = null;

        foreach (var hit in hits)
        {
            var card = hit.collider.GetComponent<Card>();
            if (card)
            {
                if(card.Order > (highest?.Order ?? -1))
                {
                    highest = card;
                }
            }
        }

        return highest;
    }

    private void BidBlind(bool active)
    {
        player.Blind = active;

        bidScreen.Show(player);
    }

    private void BidRoundStart(int position)
    {
        bidScreen.Show(player);
    }

    public void PlaceBid(int bid)
    {
        if (manager.Team1.ScoreTotal <= -100 && player.Position > 2)
        {
            var minBid = Mathf.Max(bid, 6 - player.Partner.Bid.Value);
            if (bid < minBid)
            {
                NoticeBoard.ShowMessage($"You need to make a minimum bid of {minBid}.");
                return;
            }
        }
        player.SetBid(bid, false);
        player.FlipCards(CardFace.Front);
    }

    [System.Serializable]
    public class CardEvent : UnityEvent<Card> { }
}
