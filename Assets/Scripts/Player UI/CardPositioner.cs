using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardPositioner : MonoBehaviour
{
    private List<ICard> _loadedCards = new List<ICard>();
    private Dictionary<int, List<ICard>> _rankPairedLoadedCards = new Dictionary<int, List<ICard>>();

    //list contains on header Cards
    private List<ICard> _diffusedRankPairedLoadedCards = new List<ICard>();

    private CardPool _cardPool;
    private bool _isLocalPlayer;

    #region Card Positioning staff

    public CardPool CardPool { get => _cardPool; }

    [SerializeField, Header("X Axis Card Spacing STarter and Buffer ")]
    private float _xSpacing = 0.04f;

    [SerializeField, Header("Y Axis Card Spacing For First Card ON A Side ")]
    private float _ySpacing = 0.002f;

    [SerializeField, Header("Y Axis Card Spacing Buffer")]
    private float _ySpacingBuffer = 0.001f;

    [SerializeField, Header("Z Axis Card Spacing First Card On A Side")]
    private float _zSpacing = -0.004f;

    [SerializeField, Header("Z Axis Card Spacing Buffer before reaching 0.01f")]
    private float _zSpacingFirstBuffer = -0.002f;

    [SerializeField, Header("Z Axis Card Spacing Buffer after reaching 0.01f")]
    private float _zSpacingSecondBuffer = -0.005f;

    [SerializeField, Header("Y Axis Card Rotaion Buffer")]
    private float _yCardRotationBuffer = 2f;

    private Vector3 _layoutCardPosition = Vector3.zero;
    private Vector3 _cardPosition;
    private float _centerCardXPosition;
    private Quaternion _flipCard = Quaternion.Euler(-80f, 0f, 180f);

    #endregion Card Positioning staff

    public void LoadCards(CardInfo[] cards)
    {
        if (_cardPool == null)
        {
#if Log
            LogManager.LogError($" Loading Cards Canceled !CardPool is null !");
#endif
            return;
        }

        for (int index = 0; index < cards.Length; index++)
        {
            if (!cards[index].IsValid || IsCardLoaded(cards[index])) continue;
            ICard loadedCard = _cardPool.CreateACard(cards[index]);
            if (loadedCard == null)
            {
#if Log
                LogManager.LogError($"Pooling Card Failed ! cardinfo ={cards[index]}");
#endif
                return;
            }
            _loadedCards.Add(loadedCard);
        }
        //sync paired Dick
        SyncRankPairedDic();
        //position loaded cards here
        PositionLoadedCardsForLocalPlayer();
    }

    private void PositionLoadedCardsForLocalPlayer()
    {
        PresetCardsXPosition();
        PositionCardsOnX();
        PositionCardsOnY();
        PositionCardsOnZ();
        RotateCardsOnY();
    }

    private void PresetCardsXPosition()
    {
        //setting the first card position in center
        _cardPosition = Vector3.zero;
        //setting vars needed
        _centerCardXPosition = _cardPosition.x;

        //setting Card off the center to the right
        int cardCount = _rankPairedLoadedCards.Count;
        if (cardCount % 2 == 0)
            _cardPosition.x += (_xSpacing / 2);

        //presseting cards
        float currentXPos = 0;
        int index = 1;
        while (index <= cardCount)
        {
            //need whole position
            _layoutCardPosition = _cardPosition;
            //setting next card position
            if (_cardPosition.x == _centerCardXPosition)
                _cardPosition.x += _xSpacing;
            else
            {
                currentXPos = _cardPosition.x;
                _cardPosition.x = _centerCardXPosition;
                if (currentXPos > _centerCardXPosition)
                    _cardPosition.x += (_centerCardXPosition - currentXPos);
                else
                    _cardPosition.x += (_centerCardXPosition - currentXPos) + _xSpacing;
            }
            index++;
        }
    }

    private void DiffusePairedCards()
    {
        _diffusedRankPairedLoadedCards.Clear();
        foreach (var item in _rankPairedLoadedCards)
        {
            _diffusedRankPairedLoadedCards.Add(item.Value.First());
        }
    }

    private void RotateCardsOnY()
    {
        DiffusePairedCards();
        int cardsCount = _diffusedRankPairedLoadedCards.Count;
        bool isCardsCountEven = cardsCount % 2 == 0;

        int midleCardIndex = (cardsCount / 2);
        //starting with the card nex to the middle one
        int CurrentCardIndex = midleCardIndex + 1;
        int diffrence = 0;
        float yRotationStacker = _yCardRotationBuffer;
        int MirrorCardIndex = 0;
        do
        {
            //setting card z stacker
            var card = _diffusedRankPairedLoadedCards[CurrentCardIndex];
            //casually hard coding temporary shet 
            if (_isLocalPlayer)
                card.Transform.localRotation = Quaternion.Euler(0, yRotationStacker,0);
            else
                card.Transform.localRotation = Quaternion.Euler(-80f, yRotationStacker, 180f);

            //Mirror Card Index Calculation"Big Brain Mode"
            diffrence = CurrentCardIndex - midleCardIndex;
            MirrorCardIndex = midleCardIndex - diffrence;
            //if its even we have two middle cards
            if (isCardsCountEven)
                MirrorCardIndex -= 1;
            //setting mirror Card z stacker
            card = _diffusedRankPairedLoadedCards[MirrorCardIndex];
            if (_isLocalPlayer)
                card.Transform.localRotation = Quaternion.Euler(0, -yRotationStacker,0);
            else
                card.Transform.localRotation = Quaternion.Euler(-80f, yRotationStacker, 180f);
            //stacking y
            yRotationStacker += _yCardRotationBuffer;
            //advancing index I have a visible ptsd from Post Increment
            ++CurrentCardIndex;
        } while (CurrentCardIndex < cardsCount);
    }

    private void PositionCardsOnZ()
    {
        DiffusePairedCards();
        int cardsCount = _diffusedRankPairedLoadedCards.Count;
        bool isCardsCountEven = cardsCount % 2 == 0;

        int midleCardIndex = (cardsCount / 2);
        //starting with the card nex to the middle one
        int CurrentCardIndex = midleCardIndex + 1;
        int diffrence = 0;
        float zStacker = _zSpacing;
        int MirrorCardIndex = 0;
        do
        {
            //setting card z stacker
            var card = _diffusedRankPairedLoadedCards[CurrentCardIndex];
            card.Transform.localPosition = new Vector3(card.Transform.localPosition.x, card.Transform.localPosition.y, zStacker);
            //Mirror Card Index Calculation"Big Brain Mode"
            diffrence = CurrentCardIndex - midleCardIndex;
            MirrorCardIndex = midleCardIndex - diffrence;
            //if its even we have two middle cards
            if (isCardsCountEven)
                MirrorCardIndex -= 1;
            //setting mirror Card z stacker
            card = _diffusedRankPairedLoadedCards[MirrorCardIndex];
            card.Transform.localPosition = new Vector3(card.Transform.localPosition.x, card.Transform.localPosition.y, zStacker);
            //stacking z
            if (zStacker <= (-0.01f))
                zStacker += _zSpacingSecondBuffer;
            else
                zStacker += _zSpacingFirstBuffer;
            //advancing index I have a visible ptsd from Post Increment
            ++CurrentCardIndex;
        } while (CurrentCardIndex < cardsCount);
    }

    private void PositionCardsOnY()
    {
        //keeping y buffer low so that first position doesnt sink
        float yPos = (_rankPairedLoadedCards.Count * _ySpacingBuffer) * -1;

        foreach (var cardPair in _rankPairedLoadedCards)
        {
            var card = cardPair.Value.First();
            card.Transform.localPosition = new Vector3(card.Transform.localPosition.x, yPos, card.Transform.localPosition.z);
            yPos += _ySpacingBuffer;
        }
    }

    private void PositionCardsOnX()
    {
        bool needStackCards;
        foreach (var cardPair in _rankPairedLoadedCards)
        {
            //yStack = Math.Abs(_layoutCardPosition.y);
            needStackCards = false;
            foreach (var card in cardPair.Value)
            {
                card.Transform.SetParent(transform, false);
                card.Transform.localPosition = _layoutCardPosition;
                //all stacked cards are under table
                if (needStackCards)
                    card.Transform.position = Vector3.zero;
                needStackCards = true;
                //handling non local player cards
                if (!_isLocalPlayer)
                    card.Transform.localRotation = _flipCard;
            }

            //regular x spacing
            _layoutCardPosition.x += _xSpacing;
        }
    }

    public void Init(CardPool cardPool, bool islocalPlayer)
    {
        if (cardPool == null)
        {
#if Log
            LogManager.LogError($"Card Positioner Init Canceled !CardPool is null !");
#endif
            return;
        }
        _cardPool = cardPool;
        _isLocalPlayer = islocalPlayer;
    }

    private void SyncRankPairedDic()
    {
        if (_loadedCards.Count > 0)
        {
            foreach (var card in _loadedCards)
            {
                if (_rankPairedLoadedCards.TryGetValue(card.Rank, out var rankList))
                {
                    if (!rankList.ContainsCard(card.ID))
                        rankList.Add(card);
                }
                else
                {
                    var tempoAssignedList = new List<ICard>();
                    tempoAssignedList.Add(card);
                    _rankPairedLoadedCards.Add(card.Rank, tempoAssignedList);
                }
            }
        }
    }

    private bool IsCardLoaded(CardInfo card)
    {
        foreach (ICard icard in _loadedCards)
        {
            if (Extention.AreSameCard(icard.ToCardInfo(), card))
                return true;
        }
        return false;
    }
}