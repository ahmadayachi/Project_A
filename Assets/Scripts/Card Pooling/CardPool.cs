
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardPool
{
#if OldCardPool
    [SerializeField] private GameObject cardPrefab;
    public const int DEFAULT_ARRAY_SIZE = 13;
    private int _arrayIndex = 0;
    private IDisplayCard[] _displayCards = new IDisplayCard[DEFAULT_ARRAY_SIZE];
    private Queue<int> emptyIndex = new Queue<int>();

    private IDisplayCard currentCard => _displayCards[_arrayIndex];
    private bool arrayFull => _displayCards.Length == _arrayIndex;

    public CardPool(GameObject cardPrefab)
    {
        this.cardPrefab = cardPrefab;
    }
    
    
    public IDisplayCard CreateACard(int Name,int Id)
    {
        if (TryGetUsedCard(out IDisplayCard displayCard))
        {
            //adjust it to needs and return it
            displayCard.Enable(name:Name,Id);
            return displayCard;
        }

        if (arrayFull )
        {
#if Log
            Debug.Log($"Expanding Array to {_displayCards.Length*2}");
#endif
            //allocate more
            ExpandArray();
        }
#if Log
        Debug.Log($"Creating Card from array index {_arrayIndex}");
#endif
        _displayCards[_arrayIndex] = InstantiateCard();
        IDisplayCard returnValue= currentCard;
        _arrayIndex++;
        returnValue.Enable(name:Name,Id);
        
        return returnValue;
    }

    public void DestroyCard(IDisplayCard card)
    {
        for (int i = 0; i < _displayCards.Length; i++)
        {
            if (_displayCards[i].ID == card.ID)
            {
                _displayCards[i].Disable();
                //no need for contain because if it does it should be an error
                if (emptyIndex.Contains(i))
                {
#if Log
                    Debug.LogError($"trying to add element that already exist element: {i}");
#endif
                    break;
                }
                //notify we have gap
                emptyIndex.Enqueue(i);
                break;
            }
        }
        
    }

    public void Reset()
    {
        _displayCards = new IDisplayCard[DEFAULT_ARRAY_SIZE];
        emptyIndex.Clear();
        _arrayIndex = 0;
    }

    private bool TryGetUsedCard(out IDisplayCard displayCard)
    {
        displayCard = null;
        if (emptyIndex.Count == 0)
            return false;
        int usedIndex = emptyIndex.Dequeue();
#if Log
        Debug.Log($"Found unused index at {usedIndex}");
#endif
        displayCard = _displayCards[usedIndex];
        return true;
    }

    public void ExpandArray()
    {
        IDisplayCard[] _newArray = new IDisplayCard[_displayCards.Length * 2];
        for (int i = 0; i < _displayCards.Length; i++)
        {
            _newArray[i] = _displayCards[i];
        }

        _displayCards = _newArray;
        // _arrayIndex++;
        //unsure but should ask for clean up specifically previous array
        GC.Collect();
    }
    private IDisplayCard InstantiateCard()
    {
        var gameobj = MonoBehaviour.Instantiate(cardPrefab);
        return gameobj.GetComponent<IDisplayCard>();
    }

#endif
    [SerializeField] private GameObject cardPrefab;
    public const int DEFAULT_ARRAY_SIZE = 13;
    private int _arrayIndex = 0;
    private ICard[] _cards = new ICard[DEFAULT_ARRAY_SIZE];
    private Queue<int> emptyIndex = new Queue<int>();

    private ICard currentCard => _cards[_arrayIndex];
    private bool arrayFull => _cards.Length == _arrayIndex;

    public CardPool(GameObject cardPrefab)
    {
        this.cardPrefab = cardPrefab;
    }


    public ICard CreateACard(CardInfo cardIdentity)
    {
        if (TryGetUsedCard(out ICard card))
        {
            //adjust it to needs and return it
            card.SetRank(cardIdentity.Rank);
            card.SetID(cardIdentity.ID);
            card.SetSuite(cardIdentity.Suit);
            card.Enable();
            return card;
        }

        if (arrayFull)
        {
#if Log
            Debug.Log($"Expanding Array to {_cards.Length * 2}");
#endif
            //allocate more
            ExpandArray();
        }
#if Log
        Debug.Log($"Creating Card from array index {_arrayIndex}");
#endif
        _cards[_arrayIndex] = InstantiateCard();
        ICard cardToReturn = currentCard;
        _arrayIndex++;
        cardToReturn.SetRank(cardIdentity.Rank);
        cardToReturn.SetID(cardIdentity.ID);
        cardToReturn.SetSuite(cardIdentity.Suit);
        cardToReturn.Enable();

        return cardToReturn;
    }

    public void DestroyCard(ICard card)
    {
        for (int index = 0; index < _cards.Length; index++)
        {
            if (_cards[index].ID == card.ID)
            {
                _cards[index].Disable();
                //no need for contain because if it does it should be an error
                if (emptyIndex.Contains(index))
                {
#if Log
                    Debug.LogError($"trying to add element that already exist element: {index}");
#endif
                    break;
                }
                //notify we have gap
                emptyIndex.Enqueue(index);
                break;
            }
        }

    }

    public void Reset()
    {
        _cards = new ICard[DEFAULT_ARRAY_SIZE];
        emptyIndex.Clear();
        _arrayIndex = 0;
    }

    private bool TryGetUsedCard(out ICard card)
    {
        card = null;
        if (emptyIndex.Count == 0)
            return false;
        int usedIndex = emptyIndex.Dequeue();
#if Log
        Debug.Log($"Found unused index at {usedIndex}");
#endif
        card = _cards[usedIndex];
        return true;
    }

    public void ExpandArray()
    {
        ICard[] _newArray = new ICard[_cards.Length * 2];
        for (int i = 0; i < _cards.Length; i++)
        {
            _newArray[i] = _cards[i];
        }

        _cards = _newArray;
        // _arrayIndex++;
        ////unsure but should ask for clean up specifically previous array
        //GC.Collect();
    }
    private ICard InstantiateCard()
    {
        var gameobj = MonoBehaviour.Instantiate(cardPrefab);
        return gameobj.GetComponent<ICard>();
    }
}
