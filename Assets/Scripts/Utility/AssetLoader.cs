using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AssetLoader 
{
    #region Deck Loader
    private const string CartoonishSkinAddr = "CartoonishCD";
    private const string WhiteDeckAddr = "White_deck.asset";
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
        var opCard = Addressables.LoadAssetAsync<DeckContainer>(WhiteDeckAddr);
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
    #region RunTimeData
    private const string RunTimeDataHolderaddr = "RunTimeData";
    private static RunTimeDataHolder _runTimeDataHolder;
    public static RunTimeDataHolder RunTimeDataHolder
    {
        get
        {
            if (_runTimeDataHolder == null)
                LoadCardContainer();
            return _runTimeDataHolder;
        }
    }
    private static void LoadRunTimeDataHolder()
    {
        var opUirefs = Addressables.LoadAssetAsync<RunTimeDataHolder>(RunTimeDataHolderaddr);
        opUirefs.WaitForCompletion();
        _runTimeDataHolder = opUirefs.Result;
    }
    #endregion
    #region Icons

    private static List<Sprite> _allIcons = null;
    private const string Icon = "Icon";
    public static List<Sprite> AllIcons
    {
        get
        {
            if (_allIcons == null)
                LoadIcons();
            return _allIcons;
        }
    }
    private static void LoadIcons()
    {
        _allIcons = new List<Sprite>();
        var op = Addressables.LoadAssetsAsync<Sprite>(Icon,null);
        op.WaitForCompletion();
        foreach (var icon in op.Result)
        {
            _allIcons.Add(icon);
        }

    }
    #endregion
   
}
