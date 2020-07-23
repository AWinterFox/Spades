using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Events;

public class NetworkManager : MonoBehaviour
{
    [SerializeField]
    private GameObject loadingScreen;

    private static string matchId { get; set; }

    private static string clientId { get; set; }

    public List<MessagePlayer> Players { get; private set; }

    #region Public events
    [SerializeField]
    private UnityEvent EventWelcome = new UnityEvent();

    [SerializeField]
    private UnityEvent EventGameStart = new UnityEvent();

    #endregion

    private socketManager sockets
    {
        get { return socketManager.Current; }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!string.IsNullOrEmpty(clientId))
        {
            EventWelcome.Invoke();
        }

        sockets.On("connected", OnConnected);
        sockets.On("welcome", OnWelcome);
        sockets.On("GameStart", OnGameStart);
        sockets.On("GetBid", OnGetBid);
        sockets.On("OfferBlind", OnOfferBlind);
        sockets.On("PlayCard", OnPlayCard);
    }

    private void OnDestroy()
    {
        sockets.Off("connected", OnConnected);
        sockets.Off("welcome", OnWelcome);
        sockets.Off("GameStart", OnGameStart);
        sockets.Off("GetBid", OnGetBid);
        sockets.Off("OfferBlind", OnOfferBlind);
        sockets.Off("PlayCard", OnPlayCard);
    }

    #region Receive

    private void OnConnected(SocketIO.SocketIOEvent e)
    {
        sockets.Emit("welcome", false);
        Debug.Log("Connected");
    }

    private void OnWelcome(SocketIO.SocketIOEvent e)
    {
        clientId = e.data["id"].str;
        Debug.Log($"Welcome {clientId}");
        EventWelcome.Invoke();
    }

    private void OnGameStart(SocketIO.SocketIOEvent e){
        
        Debug.Log(e.data.Print());

        matchId = e.data["matchid"].str;

        Master.LoadGame(null);
    }

    private void OnGetBid(SocketIO.SocketIOEvent e)
    {
        int.TryParse(e.data[1].ToString(), out var books);
    }

    private void OnOfferBlind(SocketIO.SocketIOEvent e)
    {
        bool.TryParse(e.data[1].ToString(), out var blind);
        int.TryParse(e.data[1].ToString(), out var bid);
    }

    private void OnPlayCard(SocketIO.SocketIOEvent e)
    {
        int.TryParse(e.data[2].ToString(), out var index);
        bool.TryParse(e.data[3].ToString(), out var lead);
    }

    #endregion

    public void StartGame(string gameType)
    {
        Debug.Log($"Starting game {gameType}");
        sockets.Emit("findRoom", new MessageStartGame
        {
            ClientId = clientId,
            GameType = gameType
        }, false);
        sockets.Emit("releaseTheKraken", new MessageReleaseKraken
        {
            GameType = gameType,
            Bots = "3"
        }, false);

        loadingScreen.SetActive(true);
    }

    public class Message {
        [JsonProperty("matchid")]
        public string MatchId { get; set; }
    }

    public class MessageStartGame : Message
    {
        [JsonProperty("clientid")]
        public string ClientId { get; set; }

        [JsonProperty("gametype")]
        public string GameType { get; set; }

        [JsonProperty("priv")]
        public bool PrivateLobby { get; set; }
    }

    public class MessageStartGameResponse : Message
    {
        [JsonProperty("playerPosition")]
        public int PlayerPosition { get; set; }

        [JsonProperty("players")]
        public List<object> Players { get; set; }
    }

    public class MessagePlayer
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Position")]
        public int Position { get; set; }
    }

    public class MessageReleaseKraken : Message
    {
        [JsonProperty("type")]
        public string GameType { get; set; }

        [JsonProperty("amount")]
        public string Bots { get; set; }
    }

    public class MessageHand : Message
    {

    }

    public class MessageGetBidRequest : Message
    {

    }

    public class MessageGetBidResponse : Message
    {
        [JsonProperty("books")]
        public int Books { get; set; }
    }

    public class MessageOfferBlindRequest : Message
    {
        
    }

    public class MessageOfferBlindResponse : Message
    {
        [JsonProperty("blind")]
        public bool Blind { get; set; }

        [JsonProperty("bid")]
        public int? Bid { get; set; }
    }

    public class MessagePlayCardRequest : Message
    {
        [JsonProperty("suite")]
        public string Suit { get; set; }

        [JsonProperty("lead")]
        public bool Lead { get; set; }

        [JsonProperty("spadesBroken")]
        public bool SpadesBroken { get; set; }
    }

    public class MessagePlayCardRespone : Message
    {
        [JsonProperty("card")]
        public object Card { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("lead")]
        public bool Lead { get; set; }
    }
}
