using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DealerBehaviourBase : IDealerBehaviour
{
    public List<ICard> DeckOfCards { get; set; }
    private byte[] _beloteCardRanks;
    private NetworkPrefabRef _cardPrefab = AssetLoader.PrefabContainer.CardPrefab;
    protected NetworkRunner _runner;
    private void SetUpCardRanksArray()
    {
        _beloteCardRanks = new byte[]
        {
            7,8,9,11,12,13,10,1
        };
    }
    public void CreateDeck()
    {
        var deckSprites = AssetLoader.DeckContainerInstance;
        if(deckSprites == null )
        {
            Debug.LogError($" Deck Coontainer is Null ! cant create Deck");
            return;
        }
        byte cardID = 0;
        foreach( var spriteList in deckSprites.SpriteContainer)
        {
            for(int index=0;index<deckSprites.SpriteContainer.Count;index++)
            {
                CreateCard(_beloteCardRanks[index], cardID++,spriteList.Key);
            }
        }
    }
    private void CreateCard(byte rank,byte ID,string suite)
    {
        if (_cardPrefab == null)
        {
            Debug.LogError("Card Prefab is null ! Check Prefab Container");
            return;
        }
        NetworkObject cardObject = _runner.Spawn(_cardPrefab);
        ICard card = cardObject.GetComponent<ICard>();
        card.SetUpcard(rank,ID,suite);
    }
}
