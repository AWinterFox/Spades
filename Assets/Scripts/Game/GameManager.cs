using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviourPun
{
    private static System.Random rng = new System.Random();

    [Header("Game options")]
    [SerializeField]
    private GameOptions gameOptions; 

    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private List<Card> cards;

    private List<Card> activeCards;
    public List<Card> ActiveCards { get { return activeCards; } }

    [SerializeField]
    private Transform cardPosition;

    [SerializeField]
    private Transform cardStack;

    public Player[] Players { get; private set; }

    [SerializeField]
    private Transform[] playerSpawns;

    [Header("Screens")]
    [SerializeField]
    private GameObject dealScreen;

    [SerializeField]
    private GameObject endScreen;

    [SerializeField]
    private GameObject tournamentScreen;

    [Header("Scoring")]
    [SerializeField]
    private int nilBidBonus = 100;

    [SerializeField]
    private int pointsPerBidTrick = 10;
    public int PointsPerBidTrick { get { return pointsPerBidTrick; } }

    [SerializeField]
    private int pointPerBag = 1;

    [SerializeField]
    private int tenBagPenalty = 100;

    [SerializeField]
    private float afterTrickDelay = 1.2f;

    [SerializeField]
    private AudioSource startGame;

    [SerializeField]
    private AudioSource gameWon;

    

    public static int TokenBet { get; set; } = 500;
    public static int WinScore { get; set; } = 500;
    public static int StartsOn { get; set; } = 0;
    public static Tournament Tournament { get; set; }
    public static Bracket CurrentBracket { get; set; }

    private GameState state = GameState.ReadyToDeal;

    private float sortingTime = 0;

    public Dictionary<Player, Card> Pile { get; private set; }
    public Dictionary<Player, int> PlayerBids { get; private set; }

    public Result Team1 { get; private set; } = new Result();
    public Result Team2 { get; private set; } = new Result();

    private List<AiPlayer> ais;

    public int Books { get; private set; } = 0;

    private int rounds = 0;

    float viewportWidth = 1;
    float viewportHeight = 1;
    Vector3 centerPosition;

    public int BidPlayer { get; private set; }

    //State fields
    public int TrickPlayer { get; private set; }
    public Card TrickCard { get; private set; }
    private bool finishedHand;

    public int CombinedBid { get
        {
            int bid = 0;
            foreach (var item in PlayerBids)
            {
                bid += item.Value;
            }
            return bid;
        } }

    [HideInInspector]
    public UnityEvent OnCardPlayed = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnGameStart = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnGameLost = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnGameWon = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnSpadesBroken = new UnityEvent();
    [HideInInspector]
    public UnityEvent OnFinished = new UnityEvent();
    [HideInInspector]
    public UnityEvent<GameState> OnStateEnded = new UnityEventTyped<GameState>();
    [HideInInspector]
    public UnityEvent<GameState> OnStateChanged = new UnityEventTyped<GameState>();

    private ScoringScreen scoring;

    public bool SpadesBroken { get; private set; } = false;


    private SoundManager soundManager;

    //Prefabs
    public ActivePlayer activePlayerPrefab;
    public AiPlayer aiPlayerPrefab;
    public Player simulatedPlayerPrefab;
    public Player onlinePlayerPrefab;

    public void StartGame(PlayerInfo[] players)
    {

        startGame.Play();
        scoring = FindObjectOfType<ScoringScreen>();
        StartsOn = Random.Range(0, 4);

        if (Tournament != null)
        {
            tournamentScreen.SetActive(true);
        }

        Players = new Player[4];
        for (int i = 0; i < players.Length; i++)
        {
            switch (players[i].Type)
            {
                case PlayerType.ActivePlayer:
                    Players[i] = PhotonNetwork.Instantiate(activePlayerPrefab.name, playerSpawns[i].position, Quaternion.identity).GetComponent<Player>();
                    Players[i].GetComponent<PhotonView>().TransferOwnership(players[i].NetworkPlayer);
                    break;
                case PlayerType.OnlinePlayer:
                    Players[i] = Instantiate(onlinePlayerPrefab, playerSpawns[i].position, Quaternion.identity).GetComponent<Player>();
                    break;
                case PlayerType.Simulated:
                    Players[i] = Instantiate(simulatedPlayerPrefab, playerSpawns[i].position, Quaternion.identity).GetComponent<Player>();
                    break;
                case PlayerType.Ai:
                    Players[i] = PhotonNetwork.Instantiate(aiPlayerPrefab.name, playerSpawns[i].position, Quaternion.identity).GetComponent<Player>();
                    break;
            }
            Players[i].Setup(playerSpawns[i].GetComponent<PlayerSpawn>(), players[i].Name);
        }

        centerPosition = new Vector3(0, 0, 0);

        NewGame();
    }

    void RandomizeSort<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case GameState.Sorting:
                sortingTime += Time.deltaTime;
                if(sortingTime >= 1)
                {
                    SetState(GameState.Bid);
                }
                break;
            case GameState.Bid:
                if(PlayerBids.Count >= 4)
                {
                    SetState(GameState.Trick);
                }
                break;
            case GameState.Trick:
                if (Pile.Count == 4 && !finishedHand)
                {
                    finishedHand = true;
                    foreach (var item in Pile)
                    {
                        if (item.Value.IsMoving)
                        {
                            finishedHand = false;
                        }
                    }
                    if (finishedHand)
                    {
                        StartCoroutine(IFinishedHand());
                    }
                }
                break;
            case GameState.Calculating:
                bool finished = true;
                foreach (var item in Pile)
                {
                    if (item.Value.IsMoving)
                    {
                        finished = false;
                    }
                }
                if(finished)
                {
                    foreach (var item in Pile)
                    {
                        Destroy(item.Value.gameObject);
                    }
                    
                    if(Books == 13)
                    {
                        SetState(GameState.Finished);
                    }
                    else
                    {
                        SetState(GameState.Trick);
                    }
                    
                }
                break;
            case GameState.Finished:

                break;
            default:
                break;
        }
    }

    private void SetState(GameState state)
    {
        Debug.Log($"State: {state.ToString()}");
        OnStateEnded.Invoke(this.state);
        this.state = state;
        switch (state)
        {
            case GameState.ReadyToDeal:
                endScreen.SetActive(false);
                dealScreen.SetActive(true);
                break;
            case GameState.Dealing:
                dealScreen.SetActive(false);
                break;
            case GameState.Sorting:
                foreach (var player in Players)
                {
                    player.SortCards();
                }
                sortingTime = 0;
                break;
            case GameState.Bid:
                Players[TrickPlayer].CanBid(1);
                break;
            case GameState.Trick:
                finishedHand = false;
                Pile = new Dictionary<Player, Card>();
                Players[TrickPlayer].CanPlayCard(1);
                Books += 1;
                break;
            case GameState.Calculating:
                var winner = GetWinner(Pile);
                winner.WinRound();
                TrickPlayer = Players.ToList().IndexOf(winner);
                foreach (var item in Pile)
                {
                    item.Value.Flip(CardFace.Front);
                    item.Value.MoveTo(winner.transform.position, item.Value.transform.rotation);
                }
                break;
            case GameState.Finished:
                endScreen.SetActive(true);

                Debug.Log("Rounds before firing: " + rounds);
                rounds++;

                //Calculate team scores
                CalculateTeam(Team1, Players[0], Players[2]);
                CalculateTeam(Team2, Players[1], Players[3]);

                // Optional win on 13.
                if (gameOptions.TeamWinsOn13)
                {
                    if (Team1.Bid == 13 && Team1.Tricks == 13)
                    {
                        Team2.Lost = true;
                    }
                    if (Team2.Bid == 13 && Team2.Tricks == 13)
                    {
                        Team1.Lost = true;
                    }
                }

                if(Team1.ScoreTotal >= WinScore && Team2.ScoreTotal >= WinScore)
                {
                    if(Team1.ScoreTotal > Team2.ScoreTotal)
                    {
                        WinGame();
                    }
                    else if(Team1.ScoreTotal < Team2.ScoreTotal)
                    {
                        LoseGame();
                    }
                }
                else if (Team1.ScoreTotal >= GameManager.WinScore || Team2.Lost)
                {
                    WinGame();
                }
                else if (Team2.ScoreTotal >= GameManager.WinScore || Team1.Lost)
                {
                    LoseGame();
                }

                /*
                if(Team1.Bid < Team1.Tricks){
                    Team1.Stucks = Team1.Stucks + 1;
                }else{
                    Team1.Stucks = 0;
                }

                if(Team2.Bid < Team2.Tricks){
                    Team2.Stucks = Team2.Stucks + 1;
                }else{
                    Team2.Stucks = 0;
                }

                if(Team1.Stucks == 3){
                    LoseGame();
                }

                if(Team2.Stucks == 3){
                    WinGame();
                }*/

                Players[0].Score = Team1.ScoreTotal;
                Players[1].Score = Team2.ScoreTotal;
                Players[2].Score = Team1.ScoreTotal;
                Players[3].Score = Team2.ScoreTotal;

                OnFinished.Invoke();

                if (Tournament != null)
                {
                    //tournamentScreen.SetActive(true);
                }

                break;
        }
        OnStateChanged.Invoke(state);
    }

    private IEnumerator IFinishedHand()
    {
        yield return new WaitForSeconds(afterTrickDelay);
        SetState(GameState.Calculating);
    }

    public void ResetGame()
    {
        rounds = 0;
        Team1 = new Result();
        Team2 = new Result();
        NewGame();
    }

    public void NewGame()
    {
        //Setup AI
        ais = new List<AiPlayer>();
        foreach (var player in Players)
        {
            player.Reset();
            var ai = player.GetComponent<AiPlayer>();
            if (ai) ais.Add(ai);
        }

        Players[0].Partner = Players[2];
        Players[2].Partner = Players[0];
        Players[1].Partner = Players[3];
        Players[3].Partner = Players[1];

        PlayerBids = new Dictionary<Player, int>();
        activeCards = new List<Card>();
        foreach (var card in cards)
        {
            
            var newCard = PhotonNetwork.Instantiate(card.name, cardStack.position, Quaternion.identity).GetComponent<Card>();

            newCard.Flip(CardFace.Back);

            activeCards.Add(newCard);
        }
        RandomizeSort(activeCards);
        if (gameOptions.MinimumOneSpade)
        {
            var spades = activeCards.Where(c => c.Suit == CardSuit.Spades).ToList();
            RandomizeSort(spades);
            for (int i = 0; i < 4; i++)
            {
                var spade = spades[i];
                activeCards.Remove(spade);
                activeCards.Add(spade);
            }
        }

        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].transform.localPosition += new Vector3(0, 0.01f * i, 0);
            activeCards[i].SetPosition(i);
        }

        //Set first player
        

        if(StartsOn == 3){
            StartsOn = 0;
        }else{
            StartsOn++;
        }
        //reference
        BidPlayer = StartsOn;
        TrickPlayer = StartsOn;

        SpadesBroken = false;
        Books = 0;

        SetState(GameState.ReadyToDeal);
    }

    public void WinGame()
    {
        gameWon.Play();
        TokenManager.AddTokens(TokenBet);
    }

    public void LoseGame(bool menu = false)
    {
        if(!menu){
            OnGameLost.Invoke();
        }
        
        TokenManager.TakeTokens(TokenBet);
    }

    private IEnumerator dealCoroutine;

    public void Deal()
    {
        SetState(GameState.Dealing);
        dealCoroutine = IDeal();
        StartCoroutine(dealCoroutine);
    }

    private IEnumerator IDeal()
    {
        for (int i = activeCards.Count-1; i >= 0; i--)
        {
            var card = activeCards[i];
            var player = i % 4;
            switch (player)
            {
                case 0:
                    Players[0].AddCard(card);
                    break;
                case 1:
                    Players[1].AddCard(card);
                    break;
                case 2:
                    Players[2].AddCard(card);
                    break;
                case 3:
                    Players[3].AddCard(card);
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.04f);
        }

        yield return new WaitForSeconds(1.5f);

        SetState(GameState.Sorting);
    }

    private Player GetWinner(Dictionary<Player, Card> hand)
    {
        Card bestCard = hand.First().Value;
        Player player = hand.First().Key;

        foreach (var item in hand)
        {
            if (item.Value.Suit == CardSuit.Spades && bestCard.Suit != CardSuit.Spades)
            {
                bestCard = item.Value;
                player = item.Key;
            }
            else if ((int)item.Value.Suit == (int)bestCard.Suit && (int)item.Value.Value > (int)bestCard.Value)
            {
                bestCard = item.Value;
                player = item.Key;
            }
        }

        return player;
    }

    public void PlayCard(Card card, Player player)
    {
        if(player == Players[TrickPlayer])
        {
            TrickCard = card;
        }
        Pile.Add(player, card);
        StartCoroutine(IWaitToPlayCard(card, player));
        OnCardPlayed.Invoke();
    }

    private IEnumerator IWaitToPlayCard(Card card, Player player)
    {
        while(card.IsMoving)
        {
            yield return 0;
        }
        foreach (var otherPlayer in Players.Where(p => p != player))
        {
            otherPlayer.OnCardPlayed.Invoke(card);
        }
        if (!SpadesBroken && card.Suit == CardSuit.Spades)
        {
            SpadesBroken = true;
            NoticeBoard.ShowMessage("Spades Broken");
            OnSpadesBroken.Invoke();

        }
        if (Pile.Count != 4)
        {
            Players[(TrickPlayer + Pile.Count) % 4].CanPlayCard(Pile.Count + 1);
        }
    }

    public void PlaceBid(Player player, int bid)
    {
        PlayerBids.Add(player, bid);
        if(PlayerBids.Count != 4)
        {
            Players[(TrickPlayer + PlayerBids.Count) % 4].CanBid(PlayerBids.Count + 1);
        }
        
    }

    private void CalculateTeam(Result result, Player p1, Player p2)
    {

        result.ScoreLastRound = result.ScoreTotal;
        p1.ScoreLastRound = result.ScoreLastRound;
        p2.ScoreLastRound = result.ScoreLastRound;
        result.Reset();

        result.Bid = (p1.Bid ?? 0) + (p2.Bid ?? 0);
        CalculatePlayer(result, p1);
        CalculatePlayer(result, p2);

        

        if(p1.Bid != null)
        {
            if (result.Tricks >= result.Bid)
            {
                result.TricksScore += result.Bid * pointsPerBidTrick;

                if (p1.Blind)
                {
                    result.TricksScore += p1.Bid.Value * pointsPerBidTrick;
                    result.TricksScore += p2.Bid.Value * pointsPerBidTrick;

                    Debug.Log("P1 trickscore: " + p1.Bid.Value * pointsPerBidTrick);
                    Debug.Log("P2 trickscore: " + p2.Bid.Value * pointsPerBidTrick);
                    Debug.Log("P1 bid value: " + p1.Bid.Value);
                    Debug.Log("P2 bid value: " + p2.Bid.Value);
                }
                if (p2.Blind)
                {
                    result.TricksScore += p1.Bid.Value * pointsPerBidTrick;
                    result.TricksScore += p2.Bid.Value * pointsPerBidTrick;

                    Debug.Log("P1 trickscore: " + p1.Bid.Value * pointsPerBidTrick);
                    Debug.Log("P2 trickscore: " + p2.Bid.Value * pointsPerBidTrick);
                    Debug.Log("P1 bid value: " + p1.Bid.Value);
                    Debug.Log("P2 bid value: " + p2.Bid.Value);
                }
            }
            else if(p1.Blind || p2.Blind){
                result.TricksScore += result.Bid * pointsPerBidTrick;

                result.TricksScore += p1.Bid.Value * pointsPerBidTrick;
                result.TricksScore += p2.Bid.Value * pointsPerBidTrick;

                Debug.Log("P1 trickscore: " + p1.Bid.Value * pointsPerBidTrick);
                Debug.Log("P2 trickscore: " + p2.Bid.Value * pointsPerBidTrick);
                Debug.Log("P1 bid value: " + p1.Bid.Value);
                Debug.Log("P2 bid value: " + p2.Bid.Value);
            }
            else
            {
                if(p1.Blind || p2.Blind){

                    result.TricksScore += result.Bid * pointsPerBidTrick;

                    result.TricksScore += p1.Bid.Value * pointsPerBidTrick;
                    result.TricksScore += p2.Bid.Value * pointsPerBidTrick;

                    result.TricksPenalty -= result.TricksScore;
                }else{
                    result.TricksPenalty -= result.Bid * pointsPerBidTrick;
                }
                
            }
            result.Bags = Mathf.Max(0, result.Tricks - result.Bid);
            result.BagsTotal = result.Bags + result.BagsPreviousRound;
            result.BagsNextRound = result.BagsTotal;
            result.BagsScore = result.Bags;
        }
        else
        {
            result.TricksScore += result.Tricks * pointsPerBidTrick;
        }
        
        if(result.BagsNextRound >= 10)
        {
            result.BagsPenalty -= tenBagPenalty;
            result.BagsNextRound -= 10;
        }
        result.Score = result.TricksScore + result.TricksPenalty + result.BagsPenalty + result.NilBidScore + result.NilBidPenalty;

        if (gameOptions.DoublePointsOn10Books)
        {
            if(result.Bid >= 10 && result.Tricks >= 10 && result.TricksScore > 0)
            {
                result.Score += result.TricksScore;
            }
        }
        result.ScoreTotal = result.Score + result.ScoreLastRound;
        Debug.Log(result.ScoreTotal);
        p1.Bags = result.BagsTotal;
        p2.Bags = result.BagsTotal;

        Debug.Log("Stucks: "+result.GamesWithOutTricks);
        if( result.Bid > result.Tricks)
        {
            result.GamesWithOutTricks += 1;
        }
        if(result.GamesWithOutTricks >= 3)
        {
            result.Lost = true;
            LoseGame();
        }
    }

    private void CalculatePlayer(Result result, Player player)
    {
        if(player.Bid != null && player.Bid == 0 && player.Tricks == 0)
        {
            //result.NilBidScore += nilBidBonus;
        }
        else if(player.Bid != null && player.Bid == 0 && player.Tricks != 0)
        {
            //result.NilBidPenalty -= nilBidBonus;
        }
        else
        {
            result.Tricks += player.Tricks;
        }
    }
}

public enum GameState
{
    ReadyToDeal,
    Dealing,
    Sorting,
    Bid,
    Trick,
    Calculating,
    Finished
}

public enum PlayerType
{
    ActivePlayer,
    OnlinePlayer,
    Simulated,
    Ai,
}

[System.Serializable]
public class PlayerInfo
{
    public string Name;
    public PlayerType Type;
    public Photon.Realtime.Player NetworkPlayer;
}

public class Result
{
    public int Bid;
    public int Tricks;
    public int TricksScore;
    public int TricksPenalty;
    public int Bags;
    public int BagsScore;
    public int BagsPenalty;
    public int BagsPreviousRound;
    public int BagsNextRound;
    public int BagsTotal;
    public int NilBidScore;
    public int NilBidPenalty;
    public int Score;
    public int ScoreLastRound;
    public int ScoreTotal;
    public int Stucks;
    public bool Lost;
    public int GamesWithOutTricks;

    public void Reset()
    {
        BagsPreviousRound = BagsNextRound;
        Bid = 0;
        Tricks = 0;
        TricksScore = 0;
        TricksPenalty = 0;
        Bags = 0;
        BagsPenalty = 0;
        BagsScore = 0;
        NilBidScore = 0;
        NilBidPenalty = 0;
        Score = 0;
        Stucks = 0;
    }
}

[System.Serializable]
public class GameOptions
{
    public bool MinimumOneSpade = false;
    public bool TeamWinsOn13 = false;
    public bool DoublePointsOn10Books = false;
    [Range(0,13)]
    public int MininmumBid = 0;

}