using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AiPlayer : MonoBehaviour
{
    private Player player;
    private GameManager manager;

    private List<Card> remainingCards = new List<Card>();

    private List<Player> opponents;

    [SerializeField]
    private float delayMin = 0.5f;
    [SerializeField]
    private float delayMax = 1;

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>();
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        player.OnBidRoundStarted.AddListener(BidRoundStart);
        player.OnCanPlayCard.AddListener(PlayHand);
        player.OnNewGame.AddListener(Setup);
        player.OnCardPlayed.AddListener(CardPlayed);
    }

    private void OnDisable()
    {
        player.OnBidRoundStarted.RemoveListener(BidRoundStart);
        player.OnCanPlayCard.RemoveListener(PlayHand);
        player.OnNewGame.AddListener(Setup);
        player.OnCardPlayed.RemoveListener(CardPlayed);
    }

    private void Setup()
    {
        opponents = manager.Players.Where(p => p != player && p != player.Partner).ToList();
    }

    private void CardPlayed(Card card)
    {
        remainingCards.Remove(card);
    }

    public void PlayHand(int position)
    {
        StartCoroutine(IPlayHand(position));
    }

    private IEnumerator IPlayHand(int position)
    {
        yield return new WaitForSeconds(Random.Range(delayMin, delayMax));

        var cards = player.GetPlayable(position).OrderByDescending(c => (int)c.Suit).ThenByDescending(c => (int)c.Value).ToList();
        var playable = new List<Card>();
        for (int i = 0; i < cards.Count(); i++)
        {
            var current = cards[i];
            var next = i == cards.Count()-1 ? null : cards[i + 1];
            if (next == null)
                playable.Add(current);
            else
            {
                if(!(current.Suit == next.Suit && (int)current.Value == (int)next.Value - 1))
                {
                    playable.Add(current);
                }
            }
        }

        //var goodNilBid = player.Bid == 0 && player.Tricks == 0;
        //var goodPartnerNilBid = player.Partner.Bid == 0 && player.Partner.Tricks == 0;
        //var protectingNilBid = !goodNilBid && goodPartnerNilBid;
        //var opponentGoodNilBidder = OpponentsWithGoodNilBid();
        var haveMadeBid = HaveMadeBid();
        var shouldGetBags = ShouldGetBags();
        var pile = manager.Pile.Select(c => c.Value).ToList();
        

        Card cardToPlay = null;

        if(position > 1)
        {
            var matchCard = pile[0];
            if (!playable.Any(c => c.Suit == matchCard.Suit))
            {
                if (matchCard.Suit != CardSuit.Spades && playable.Any(c => c.Suit == CardSuit.Spades))
                {
                    playable = playable.Where(c => c.Suit == matchCard.Suit || c.Suit == CardSuit.Spades).ToList();
                }
            }
        }

        if (player.Position == 1)
        {
            for (int i = 0; i < playable.Count(); i++)
            {
                var card = playable[i];

                card.Goodness = (int)card.Value;

                for (int j = 0; j < remainingCards.Count; j++)
                {
                    var jCard = remainingCards[j];
                    if (jCard.Suit == card.Suit && (int)jCard.Value > (int)card.Value)
                        card.Goodness--;
                }

                if(card.Suit == CardSuit.Spades)
                {
                    card.Goodness -= 1;
                }
                if(card.Value == CardValue.King && card.Suit != CardSuit.Spades)
                {
                    var iHaveTheHighCard = player.Cards.Any(c => c.Suit == card.Suit && c.Value == CardValue.Ace) || !remainingCards.Any(c => c.Suit == card.Suit && c.Value == CardValue.Ace);
                    if(!iHaveTheHighCard)
                    {
                        card.Goodness -= 13;
                    }
                }
                else if(card.Value >= CardValue.Ace && card.Value < CardValue.JokerHigh)
                {
                    var iHaveTheHighCard = player.Cards.Any(c => c.Suit == card.Suit && c.Value > card.Value && !remainingCards.Any(r => r.Value > c.Value));
                    if(!iHaveTheHighCard)
                    {
                        card.Goodness -= 13;
                    }
                }
            }

            //Get cards to play
            playable = playable.OrderBy(c => c.Goodness).ToList();
            var best = playable.LastOrDefault();
            var worst = playable.FirstOrDefault();


                var iHaveHighest = remainingCards.Any(r => r.Value > best.Value && r.Suit == best.Suit);


            if(haveMadeBid)
            {
                if(shouldGetBags && iHaveHighest)
                {
                    cardToPlay = best;
                }
                else
                {
                    cardToPlay = worst;
                }
            }
            else
            {
                cardToPlay = iHaveHighest ? best : worst;
            }
        }
        else if(position == 2)
        {
            var importantCards = GetImportantCards(playable);

            if (haveMadeBid)
            {
                if (shouldGetBags)
                {
                    cardToPlay = TryWin2(importantCards);
                }
                else
                {
                    cardToPlay = TryLose2(importantCards);
                }
            } else if (player.Bid == 0)
            {
                cardToPlay = TryLose2(importantCards);
            }
            else
            {
                cardToPlay = TryWin2(importantCards);
            }
        } else if(position == 3 || position == 4)
        {
            var importantCards = GetImportantCards(playable);
            var bestCard = GetBestCardFromPile();
            var partnerHasIt = bestCard == pile[position - 3] && bestCard.Value > CardValue.Jack;

            if (partnerHasIt)
            {
                if (importantCards.WorstThatCanLose)
                {
                    cardToPlay = importantCards.WorstThatCanLose;
                }
                else
                {
                    cardToPlay = importantCards.BestThatCanLose;
                }
            }
            else
            {
                if(playable.Any(c => c.Suit == CardSuit.Spades))
                {
                    cardToPlay = importantCards.WorstThatCanWin;
                }
                else if (importantCards.BestThatCanWin)
                {
                    cardToPlay = importantCards.BestThatCanWin;
                }
                else
                {
                    cardToPlay = importantCards.BestThatCanLose;
                }
            }
        }
        else
        {
            var playableCards = player.Cards.Where(c => c.Suit == manager.TrickCard.Suit).ToList();
            if(playableCards.Count() == 0)
            {
                cardToPlay = player.Cards.First();
            }
            else
            {
                cardToPlay = playableCards.First();
            }
        }

        if(cardToPlay == null) {
            player.PlayCard(playable.First());
            Debug.LogError("No card to play", gameObject);
        } else {
            player.PlayCard(cardToPlay);
        }
    }

    private Card TryLose2(ImportantCards cards)
    {
        if (cards.BestThatCanLose)
        {
            return cards.BestThatCanLose;
        }
        else
        {
            return cards.WorstThatCanLose;
        }
    }

    private Card TryWin2(ImportantCards cards)
    {
        if (cards.WorstThatCanWin)
        {
            return cards.WorstThatCanWin;
        }
        else
        {
            return cards.WorstThatCanLose;
        }
    }

    private Card TryLoseLate(ImportantCards cards, Card bestCard, bool partnerHasIt)
    {
        if (cards.BestThatCanLose)
        {
            return cards.BestThatCanLose;
        }
        else
        {
            return cards.BestThatCanWin;
        }
    }

    /// <summary>
    /// Calculate if the bid has been made by the player and partner
    /// </summary>
    private bool HaveMadeBid()
    {
        if (player.Bid == 0 || player.Partner.Bid == 0) return false;
        var bid = player.Bid + player.Partner.Bid;
        var tricks = player.Tricks + player.Partner.Tricks;
        return tricks >= bid;
    }

    /// <summary>
    /// Work out if opponents trying for a nil bid and they are still able too
    /// </summary>
    /// <returns></returns>
    private Player OpponentsWithGoodNilBid()
    {
        foreach (var opponent in opponents)
        {
            if (opponent.Bid == opponent.Tricks)
                return opponent;
        }
        return null;
    }

    private bool ShouldGetBags()
    {
        if (opponents[0].Bid == 0 && opponents[1].Bid == 0)
        {
            //Opponent is bidding nil, dont get bags;
            return false;
        }
        var opponentBid = opponents[0].Bid + opponents[1].Bid;
        var opponentTricks = opponents[0].Tricks + opponents[1].Tricks;
        if (opponentTricks >= opponentBid)
        {
            return false;
        }
        var opponentMissingTricks = opponentBid - opponentTricks;
        var tricksLeftInRound = player.Cards.Count;
        if (opponentMissingTricks > tricksLeftInRound)
        {
            return false;
        }
        var ourOldBags = player.Bags;
        var ourBid = player.Bid + player.Partner.Bid;
        var ourTricks = player.Tricks + player.Partner.Tricks;
        var ourCurrentRoundBags = ourTricks - ourBid;
        var ourTotalBags = ourOldBags + ourCurrentRoundBags;
        var canSetByTaking2bags = tricksLeftInRound - opponentMissingTricks <= 2;
        if ((ourTotalBags < 5 || ourTotalBags >= 10) && canSetByTaking2bags)
        {
            return true;
        }
        return false;
    }

    private ImportantCards GetImportantCards(List<Card> playable)
    {
        var cards = new ImportantCards();
        var bestCardInPile = GetBestCardFromPile();
        var sorted = playable.OrderBy(c => (int)c.Suit).ThenBy(c => (int)c.Value).ToList();

        cards.WorstThatCanLose = sorted.First();

        if(CanCardWin(sorted.Last(), bestCardInPile))
        {
            var card = sorted.Last();
            if(!remainingCards.Any(c => c.Suit == card.Suit && c.Value > card.Value))
                cards.BestThatCanWin = card;
        }
        for (int i = 0; i < sorted.Count; i++)
        {
            var card = sorted[i];
            if (CanCardWin(card, bestCardInPile))
            {
                if (!remainingCards.Any(c => c.Suit == card.Suit && c.Value > card.Value) || card.Suit == CardSuit.Spades)
                {
                    cards.WorstThatCanWin = card;
                    break;
                }
            }
        }
        for (int i = sorted.Count -1; i >= 0; i--)
        {
            var card = sorted[i];
            if(!CanCardWin(card, bestCardInPile))
            {
                cards.BestThatCanLose = card;
            }
        }

        return cards;
    }

    /// <summary>
    /// Get the best card currently on the pile
    /// </summary>
    /// <returns></returns>
    private Card GetBestCardFromPile()
    {
        var pile = manager.Pile.Select(c => c.Value).ToList();
        var card = pile[0];
        for (int i = 0; i < pile.Count; i++)
        {
            var newCard = pile[i];
            if(CanCardWin(newCard, card))
            {
                card = newCard;
            }
        }
        return card;
    }

    /// <summary>
    /// Compare if card a can beat card b
    /// </summary>
    private bool CanCardWin(Card a, Card b)
    {
        return (a.Suit == CardSuit.Spades && b.Suit != CardSuit.Spades) || (a.Suit == b.Suit && a.Value > b.Value);
    }

    #region Bid Logic

    private void BidRoundStart(int position)
    {
        StartCoroutine(BidRoundStartI(position));
    }

    private IEnumerator BidRoundStartI(int position)
    {
        yield return new WaitForSeconds(Random.Range(delayMin, delayMax));

        //Make remainnig cards list. adding each card not already in players hand.
        remainingCards = new List<Card>();
        foreach (var card in manager.ActiveCards)
        {
            if (player.Cards.Contains(card)) continue;
            remainingCards.Add(card);
        }

        if (blindBid.bblind)
        {
            player.SetBid(0, true);
            yield break;
        }

        if (player.Partner.Bid == 13)
        {
            player.SetBid(0, true);
            yield break;
        }

        var info = new Info();

        for (int i = 0; i < player.Cards.Count; i++)
        {
            var card = player.Cards[i];
            if (card.Suit == CardSuit.Joker) info.Jokers.Count++;
            if (card.Suit == CardSuit.Spades)
            {
                info.Spades.Count++;
                if (card.Value == CardValue.Ace) info.Spades.HasAce = true;
                if (card.Value == CardValue.King) info.Spades.HasKing = true;
                if (card.Value == CardValue.Two) info.Spades.HasTwo = true;
            }
            if (card.Suit == CardSuit.Diamonds)
            {
                info.Diamonds.Count++;
                if (card.Value == CardValue.Ace) info.Diamonds.HasAce = true;
                if (card.Value == CardValue.King) info.Diamonds.HasKing = true;
                if (card.Value == CardValue.Two) info.Diamonds.HasTwo = true;
            }
            if (card.Suit == CardSuit.Hearts)
            {
                info.Hearts.Count++;
                if (card.Value == CardValue.Ace) info.Hearts.HasAce = true;
                if (card.Value == CardValue.King) info.Hearts.HasKing = true;
                if (card.Value == CardValue.Two) info.Hearts.HasTwo = true;
            }
            if (card.Suit == CardSuit.Clubs)
            {
                info.Clubs.Count++;
                if (card.Value == CardValue.Ace) info.Clubs.HasAce = true;
                if (card.Value == CardValue.King) info.Clubs.HasKing = true;
                if (card.Value == CardValue.Two) info.Clubs.HasTwo = true;
            }


        }

        var hardBid = 0;
        var softBid = 0f;

        var highCount = 0;

        var jokers = player.Cards.Where(c => c.Suit == CardSuit.Joker).ToList();
        var spades = player.Cards.Where(c => c.Suit == CardSuit.Spades).ToList();

        hardBid += jokers.Count;

        for (int i = 14; i > 0; i--)
        {
            var highSpade = spades.FirstOrDefault(c => (int)c.Value == i);
            if (highSpade)
            {
                hardBid++;
                spades.Remove(highSpade);
                highCount++;
            }
            else
            {
                break;
            }
        }

        var spadeFaceCards = spades.Where(c => (int)c.Value >= 11).ToList();

        foreach (var card in spadeFaceCards)
        {
            softBid++;
            spades.Remove(card);
        }

        var lowSpadeBid = (int)Mathf.Ceil(spades.Count / 2.5f);
        softBid += lowSpadeBid;

        softBid += SoftBidCalculation(info.Diamonds);
        softBid += SoftBidCalculation(info.Hearts);
        softBid += SoftBidCalculation(info.Clubs);
        softBid += jokers.Count;

        var lowerSoftBid = Mathf.Min(1, softBid);

        softBid -= lowerSoftBid;

        var bid = hardBid + softBid;

        if (bid < 3 && player.Partner.Bid != 0 && highCount == 0)
        {
            var canBidNil = true;
            if (jokers.Count > 0) canBidNil = false;
            if (info.Spades.HasAce || info.Spades.HasTwo) canBidNil = false;
            if (info.Diamonds.HasAce || info.Diamonds.HasTwo) canBidNil = false;
            if (info.Clubs.HasAce || info.Clubs.HasTwo) canBidNil = false;
            if (info.Hearts.HasAce || info.Hearts.HasTwo) canBidNil = false;
            if (player.Partner.Bid != null && player.Bid < 4) canBidNil = false;
            if (info.Spades.Count > 3) canBidNil = false;
            if (canBidNil)
            {
                player.SetBid(0, false);
                yield break;
            }
        }

        bid = Mathf.Ceil(bid);
        bid = Mathf.Min(bid, 6);
        bid = Mathf.Max(bid, 1);

        if ((player.Partner.Bid ?? 0) >= 6)
        {
            bid = hardBid;
        }
        else if ((bid + player.Partner.Bid ?? 0) > 9)
        {
            bid = Mathf.Max(9 - player.Partner.Bid ?? 0, hardBid);
        }

        var neededScoreToWin = GameManager.WinScore - player.ScoreLastRound;
        if ((player.Partner.Bid ?? 0) > 0)
        {
            bid = ReducedBidForEndGame(neededScoreToWin, bid, (bid + player.Partner.Bid) ?? 0, player.Bags);
        }
        else
        {
            bid = ReducedBidForEndGame(neededScoreToWin, bid, bid, player.Bags);
        }

        if (bid < hardBid)
        {
            bid = hardBid;
        }

        bid = Mathf.Max(bid, 1);

        if (player.PlayerIndex == 0 || player.PlayerIndex == 2) {
            if(manager.Team1.ScoreTotal <= -100 && position > 2)
            {
                bid = Mathf.Max(bid, 6 - player.Partner.Bid.Value);
            }
        }
        else {
            if (manager.Team2.ScoreTotal <= -100 && position > 2)
            {
                bid = Mathf.Max(bid, 6 - player.Partner.Bid.Value);
            }
        }

        if(position > 3)
        {
            bid = Mathf.Max(11 - manager.CombinedBid, bid);
        }

        if(position > 2){
            if(bid < 4){
                bid = 4;
            }
        }

        if (player.Partner.Bid + (int)bid > 13)
        {
            bid = 13 - (int)player.Partner.Bid;
        }

        player.SetBid((int)bid, false);
    }

    private float ReducedBidForEndGame(int neededScoreToWin, float bid, float combinedBid, int bags)
    {
        var scoreForBid = combinedBid * manager.PointsPerBidTrick;
        if(scoreForBid > neededScoreToWin)
        {
            var diff = scoreForBid - neededScoreToWin;
            var extraTricks = Mathf.FloorToInt(diff / manager.PointsPerBidTrick);
            if(player.Bags + extraTricks >= 10)
            {
                return bid;
            }
            if(extraTricks >= 5)
            {
                return bid - 2;
            } else if(extraTricks >= 1)
            {
                return bid - 1;
            }
        }
        return bid;
    }

    private float SoftBidCalculation(CardInfo info)
    {
        if(info.HasTwo && info.HasAce)
        {
            return 2;
        }
        else if (info.HasTwo)
        {
            return 1;
        }
        else if (info.HasAce)
        {
            return 0.5f;
        }
        return 0;
    }

    #endregion

    public class Info
    {
        public CardInfo Jokers = new CardInfo();
        public CardInfo Spades = new CardInfo();
        public CardInfo Diamonds = new CardInfo();
        public CardInfo Clubs = new CardInfo();
        public CardInfo Hearts = new CardInfo();
    }

    public class CardInfo
    {
        public int Count;
        public bool HasTwo;
        public bool HasAce;
        public bool HasKing;
    }

    public class ImportantCards
    {
        public Card WorstThatCanWin;
        public Card WorstThatCanLose;
        public Card BestThatCanWin;
        public Card BestThatCanLose;
    }
}
