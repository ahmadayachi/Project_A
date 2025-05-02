#define USE_FIXED_ARRAY_SIZE
//#define BYPASS_CARDPOOLING
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
    private GameObject _cardPrefab;
    private int _arraySize;
    private int _arrayIndex = 0;
    private ICard[] _cards;
    private Queue<int> emptyIndex = new Queue<int>();
    private ICard currentCard => _cards[_arrayIndex];
    private bool arrayFull => _cards.Length == _arrayIndex;
    private Transform _cardsHolder;
    public CardPool(CardPoolArguments poolArgs)
    {
        _cardPrefab = poolArgs.CardPrefab;
        _cardsHolder = poolArgs.CardsHolder;
#if USE_FIXED_ARRAY_SIZE
        InitArray(poolArgs.MaxPlayerCards, poolArgs.ActivePlayerCount);
#endif
    }

    public ICard CreateACard(CardInfo cardInfo)
    {
#if !BYPASS_CARDPOOLING
        if (TryGetUsedCard(out ICard card))
        {
            //adjust it to needs and return it
            card.Enable(cardInfo);
            return card;
        }
#endif

        if (arrayFull)
        {
#if Log
            LogManager.Log($"Expanding Array to {_cards.Length * 2},", Color.yellow, LogManager.CardPoolLog);
#endif
            //allocate more
            ExpandArray();
        }
#if Log
        LogManager.Log($"Creating Card from array index {_arrayIndex}", Color.yellow, LogManager.CardPoolLog);
#endif
        var spawnedCard = InstantiateCard();
        if (spawnedCard == null)
        {
#if Log
            LogManager.LogError("Card Creation Failed ! Spawned Card is Null");
#endif
        }
        _cards[_arrayIndex] = spawnedCard;
        ICard cardToReturn = currentCard;
        _arrayIndex++;
        cardToReturn.Enable(cardInfo);

        return cardToReturn;
    }

    public void DestroyCard(ICard card)
    {
        int startingIndex = emptyIndex.Count> 0 ? emptyIndex.Count : 0;
        for (int index = 0; index < _cards.Length; index++)
        {
            if (_cards[index].ID == card.ID)
            {
                var cardToDestroy = _cards[index];

                cardToDestroy.Disable();

                cardToDestroy.Transform.rotation = Quaternion.identity;
                //playcing the card out of the screen 
                cardToDestroy.Transform.SetParent(_cardsHolder);

                cardToDestroy.Transform.position = Vector3.zero;
                cardToDestroy.Transform.localRotation = Quaternion.identity;

                //no need for contain because if it does it should be an error
                if (emptyIndex.Contains(startingIndex))
                {
#if Log
                    LogManager.LogError($"trying to add element that already exist element: {index}");
#endif
                    break;
                }
                emptyIndex.Enqueue(startingIndex++);
                break;
            }
        }
    }

    public void DestroyAll()
    {
        if (_cards == null || _cards.Length <= 0)
        {
#if Log
            LogManager.Log("Ignoring Deastroying Pooled Cards! Card Pool is Empty or Null !", Color.yellow, LogManager.ValueInformationLog);
            return;
#endif
        }
        for (int index = 0; index < _cards.Length; index++)
        {
            if (_cards[index] == null) continue;
#if Log
            LogManager.Log($"destroying this card ! {_cards[index]}", Color.yellow, LogManager.ValueInformationLog);
#endif
            _cards[index].Disable();
            //playcing the card out of the screen 
            _cards[index].Transform.SetParent(_cardsHolder);
            //no need for contain because if it does it should be an error
            if (emptyIndex.Contains(index))
            {
#if Log
                LogManager.LogError($"trying to add element that already exist element: {index}");
#endif
                break;
            }
            //notify we have gap
            emptyIndex.Enqueue(index);
        }
    }

    public void Reset()
    {
        _cards = new ICard[_arraySize];
        emptyIndex.Clear();
        _arrayIndex = 0;
    }

#if USE_FIXED_ARRAY_SIZE
    private void InitArray(byte MaxPlayerCards, byte playerNumber)
    {
        //determined maximum cards that can be played in a game
        _arraySize = MaxPlayerCards * playerNumber;
        _cards = new ICard[_arraySize];
    }
#endif

    private bool TryGetUsedCard(out ICard card)
    {
        card = null;
        if (emptyIndex.Count == 0)
            return false;
        int usedIndex = emptyIndex.Dequeue();
#if Log
        LogManager.Log($"Found unused index at {usedIndex}", Color.cyan, LogManager.CardPoolLog);
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
        var gameobj = GameObject.Instantiate(_cardPrefab, _cardsHolder);
        return gameobj.GetComponent<ICard>();
    }
}