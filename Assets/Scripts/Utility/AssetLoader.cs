using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AssetLoader 
{
    #region Deck Loader
    private const string CartoonishSkinAddr = "CartoonishCD.asset";
    
    private static DeckContainer _deckContainer;
    public static DeckContainer DeckContainerInstance
    {
        get
        {
            if (_deckContainer == null)
                LoadCardContainer();
            return _deckContainer;

        }
    }
    public static void LoadCardContainer()
    {       
        var opCard = Addressables.LoadAssetAsync<DeckContainer>(CartoonishSkinAddr);
        opCard.WaitForCompletion();
        _deckContainer = opCard.Result;
    }
    #endregion

    #region PrefabContainer
    private const string PrefabContaineraddr = "Prefabs.asset";
    private static PrefabContainer _prefabcontainer;
    public static PrefabContainer PrefabContainer
    {
        get
        {
            if (!_prefabcontainer)
                LoadPrefabContainer();
            return _prefabcontainer;
        }
    }
    private static void LoadPrefabContainer()
    {
        var opUirefs = Addressables.LoadAssetAsync<PrefabContainer>(PrefabContaineraddr);
        opUirefs.WaitForCompletion();
        _prefabcontainer = opUirefs.Result;
    }
    #endregion
}
