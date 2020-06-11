using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;
using System.Linq;

public class Player : MonoBehaviourPun
{
    private Transform cardPosition;
    private Transform playPosition;
    private TMP_Text nameText;
    private string playerName;

    [SerializeField]
    private TMP_Text bidText;

    [SerializeField]
    private GameObject bidObject;

    private bool canPlayCard = false;

    public UnityEvent<int> OnCanPlayCard = new UnityEventTyped<int>();
    public UnityEvent<int> OnBidRoundStarted = new UnityEventTyped<int>();
    public UnityEvent OnSortCards = new UnityEvent();
    public UnityEvent OnNewGame = new UnityEvent();
    public UnityEvent<Card> OnCardPlayed = new UnityEventTyped<Card>();

    public int PlayerIndex { get; set; }
    public int Position { get; private set; }
    public Player Partner;

    [SerializeField]
    private CardDirection directionOfCards = CardDirection.North;

    private Vector3 cardVector { get {
        switch (directionOfCards)
        {
            case CardDirection.North:
                return new Vector3(1, 0, 0);
            case CardDirection.West:
                return new Vector3(0, 1, 0);
            case CardDirection.East:
                return new Vector3(0, -1, 0);
            case CardDirection.South:
                return new Vector3(-1, 0, 0);
        }
        return new Vector3(1, 0, 0);
    } }

    private Vector3 cardDirection { get {
        switch (directionOfCards)
        {
            case CardDirection.North:
                return new Vector3(0, 1, 0);
            case CardDirection.West:
                return new Vector3(-1, 0, 0);
            case CardDirection.East:
                return new Vector3(1, 0, 0);
            case CardDirection.South:
                return new Vector3(0, -1, 0);
        }
        return new Vector3(1, 0, 0);
    } }

    private float cardAngle {  get
        {
            switch (directionOfCards)
            {
                case CardDirection.North:
                    return 0;
                case CardDirection.West:
                    return 90;
                case CardDirection.East:
                    return 270;
                case CardDirection.South:
                    return 180;
            }
            return 0;
        } }

    [SerializeField]
    private float cardSpacing = 0.3f;

    private GameManager manager;
    private List<Card> cards = new List<Card>();

    public List<Card> Cards { get { return cards; } }

    public int? Bid { get; private set; }
    public int Tricks { get; private set; } = 0;
    public int Bags = 0;
    public int ScoreLastRound = 0;
    public int Score { get; set; } = 0;
    public bool Blind { get; set; } = false;

    #region Lifecycle

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>();
    }

    public void Reset()
    {
        Tricks = 0;
        Bid = null;
        nameText.text = playerName;
        OnNewGame.Invoke();
    }

    public void Setup(PlayerSpawn spawn, string name)
    {
        playerName = name;
        nameText = spawn.NamePlate;
        playPosition = spawn.PlayPosition;
        cardPosition = spawn.CardPosition;
        directionOfCards = spawn.CardDirection;
    }

    #endregion

    public void AddCard(Card card)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("AddCardById", RpcTarget.Others, card.photonView.ViewID);
            card.MoveToTransform(cardPosition);
        }
        cards.Add(card);
        card.transform.parent = cardPosition;
    }

    [PunRPC]
    public void AddCardById(int id)
    {
        var card = FindObjectsOfType<Card>().FirstOrDefault(c => c.photonView.ViewID == id);
        cards.Add(card);
        card.transform.parent = cardPosition;
    }

    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }

    public void SortCards()
    {
        OnSortCards.Invoke();
        cards = cards.OrderByDescending(c => (int)c.Suit).ThenByDescending(c => (int)c.Value).ToList();
        MoveCards(1);
    }

    private void MoveCards(float time)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            //cards[i].SetDirection(directionOfCards);
            cards[i].SetPosition(i);
            StartCoroutine(IMoveCard(cards[i], i, cards.Count, time));
        }
    }

    private IEnumerator IMoveCard(Card card, int position, int total, float time)
    {
        var t = (position / (Mathf.Max(1,(float)total-1)));
        var angleTotal = 3.85f * total;
        var angle = angleTotal/2 - (t * angleTotal);

        var curve = t * (1 - t) * 4;
        var offsetTotal = 0.03f * total;
        var offset = -offsetTotal + curve * offsetTotal;

        Vector3 line = cardPosition.position - cardVector * ((total - 1) * cardSpacing / 2);
        Vector3 finish = line + (cardDirection * offset) + cardVector * (cardSpacing * position);
        Vector3 start = card.transform.position;

        var startAngle = card.transform.localEulerAngles.z;
        if(startAngle > 180 && startAngle - angle > 180)
        {
            angle += 360;
        }
        var finishAngle = angle;

        var timer = 0f;
        while (timer < time)
        {
            var delta = timer / time;
            card.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(startAngle,finishAngle, delta));
            card.transform.position = start + ((finish - start) * delta);
            timer += Time.deltaTime;
            yield return 0;
        }

        card.transform.position = finish;
    }

    public void FlipCards(CardFace face)
    {
        foreach (var card in cards)
        {
            card.Flip(face);
        }
    }

    public void PlayCard(Card card)
    {
        if (cards.Contains(card) && canPlayCard)
        {
            if(Position != 1 && cards.Any(c => c.Suit == manager.TrickCard.Suit) && card.Suit != manager.TrickCard.Suit)
            {
                Debug.LogError($"Cannot play card {card.Value.ToString()} of {card.Suit.ToString()}.", gameObject);
            }
            else
            {
                canPlayCard = false;
                card.MoveToTransform(playPosition);
                card.SetPosition(60 + Position);
                card.Flip(CardFace.Front);
                card.PlayedBy = this;
                card.transform.parent = null;
                cards.Remove(card);
                manager.PlayCard(card, this);
                MoveCards(0.3f);
            }
        }
    }
    
    public void SetBid(int bid, bool partnerBlind)
    {
        if(partnerBlind){
            Bid = 0;
        }else{
            Bid = Mathf.Clamp(bid,1,13);
        }
        
        StartCoroutine(ISetBid(bid));
        nameText.text = $"{playerName} ({Bid}) {Tricks}";

        foreach (var card in cards)
        {
            //card.Flip(CardFace.Front);
        }
    }

    private IEnumerator ISetBid(int bid)
    {
        bidText.text = $"I bid {bid}";
        bidObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        manager.PlaceBid(this, bid);
        bidObject.SetActive(false);
    }

    public void CanBid(int position)
    {
        Position = position;
        OnBidRoundStarted.Invoke(position);
    }

    public void CanPlayCard(int position)
    {
        Position = position;
        canPlayCard = true;
        OnCanPlayCard.Invoke(position);
    }

    public List<Card> GetPlayable(int position)
    {
        if (position == 1)
        {
            if (manager.SpadesBroken)
            {
                return Cards;
            }
            else
            {
                return Cards.Where(c => c.Suit != CardSuit.Spades).ToList();
            }
        }
        if (Cards.Any(c => c.Suit == manager.TrickCard.Suit))
        {
            return Cards.Where(c => c.Suit == manager.TrickCard.Suit).ToList();
        }
        else
        {
            return Cards;
        }
    }

    [PunRPC]
    public void WinRound()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("GameWon", RpcTarget.Others);
        }
        Tricks++;
        nameText.text = $"{playerName} ({Bid}) {Tricks}";
    }

    [PunRPC]
    public void GameWon()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("GameWon", RpcTarget.Others);
        }

        if(photonView.IsMine)
        {
            //Game win screen.
        }
    }
}

public enum CardDirection
{
    North,
    West,
    East,
    South
}

public enum CardFace
{
    Front,
    Back
}