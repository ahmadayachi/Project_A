using System;
using System.Collections.Generic;
using UnityEngine;

public class CardPool
{
    public const int DEFAULT_ARRAY_SIZE = 13;

    [SerializeField] private GameObject cardPrefab;

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

   
}
