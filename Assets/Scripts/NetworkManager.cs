using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    [SerializeField]
    private GameObject loadingScreen;

    private string matchId { get; set; }

    public List<MessagePlayer> Players { get; private set; }

    private socketManager sockets
    {
        get { return socketManager.Current; }
    }

    // Start is called before the first frame update
    void Start()
    {
        sockets.On("GameStart", OnGameStart);
        sockets.On("GetBid", OnGetBid);
        sockets.On("OfferBlind", OnOfferBlind);
        sockets.On("PlayCard", OnPlayCard);
    }

    private void OnDestroy()
    {
        sockets.Off("GameStart", OnGameStart);
        sockets.Off("GetBid", OnGetBid);
        sockets.Off("OfferBlind", OnOfferBlind);
        sockets.Off("PlayCard", OnPlayCard);
    }

    #region Receive

    private void OnGameStart(SocketIO.SocketIOEvent e){
        loadingScreen.SetActive(false);

        matchId = e.data["matchId"].str;

        Debug.Log(e.data.Print());

        //var players = e.data["players"].list;

        //foreach (var player in players)
        //{
        //    player.
        //}
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
