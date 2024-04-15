using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager :NetworkBehaviour
{
    #region Player Propertys 
    [Networked, Capacity(8)] public NetworkArray<NetworkObject> Clients { get; }
    #endregion
    private Dealer _dealer;

    #region Dealer Setup
    
    #endregion
}
