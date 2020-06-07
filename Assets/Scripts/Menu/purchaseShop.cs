using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

// Placing the Purchaser class in the CompleteProject namespace allows it to interact with ScoreManager, 
// one of the existing Survival Shooter scripts.
namespace CompleteProject
{
    // Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
    public class purchaseShop : MonoBehaviour
    {


        public void purchaseItems(int Tokens)
        {
            TokenManager.AddTokens(Tokens);
        }
        
    }
}
