using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CaveMan.Tools
{
    public class CardFanning : MonoBehaviour
    {
        public List<GameObject> _childrenCards = new List<GameObject>();
        private List<ICardUI> _cards = new List<ICardUI>();

        #region Card Positioning staff

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

        //private Vector3 _cardPosition;
        private float _centerCardXPosition;

        #endregion Card Positioning staff

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            //if (CantPositionCards()) return;
            //PositionCards();
        }
        public void AddCard(ICardUI card)
        {
            _cards.Add(card);
            PositionCards();
        }
        private bool CantPositionCards()
        {
            if (_cards == null) return true;
            if (_cards.Count == 0) return true;
            return false;
        }
        private void PositionCards()
        {
            PresetCardsXPosition(_cards);
            PositionCardsOnX(_cards);
            if (_cards.Count <= 3) return;
            PositionCardsOnY(_cards);
            PositionCardsOnZ(_cards);
            RotateCardsOnY(_cards);
        }
        private void RotateCardsOnY<T>(List<T> cards) where T : ICardUI
        {
            int cardsCount = cards.Count;
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
                var card = cards[CurrentCardIndex];
                card.Transform.localRotation = Quaternion.Euler(0, yRotationStacker, 0);
                //Mirror Card Index Calculation"Big Brain Mode"
                diffrence = CurrentCardIndex - midleCardIndex;
                MirrorCardIndex = midleCardIndex - diffrence;
                //if its even we have two middle cards
                if (isCardsCountEven)
                    MirrorCardIndex -= 1;
                //setting mirror Card z stacker
                card = cards[MirrorCardIndex];
                card.Transform.localRotation = Quaternion.Euler(0, -yRotationStacker, 0);
                //stacking y
                yRotationStacker += _yCardRotationBuffer;
                //advancing index I have a visible ptsd from Post Increment
                ++CurrentCardIndex;
            } while (CurrentCardIndex < cardsCount);
        }
        private void PositionCardsOnZ<T>(List<T> cards) where T : ICardUI
        {
            int cardsCount = cards.Count;
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
                var card = cards[CurrentCardIndex];
                card.Transform.localPosition = new Vector3(card.Transform.localPosition.x, card.Transform.localPosition.y, zStacker);
                //Mirror Card Index Calculation"Big Brain Mode"
                diffrence = CurrentCardIndex - midleCardIndex;
                MirrorCardIndex = midleCardIndex - diffrence;
                //if its even we have two middle cards
                if (isCardsCountEven)
                    MirrorCardIndex -= 1;
                //setting mirror Card z stacker
                card = cards[MirrorCardIndex];
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
        private void PositionCardsOnY<T>(IEnumerable<T> cards) where T : ICardUI
        {
            //keeping y buffer low so that first position doesnt sink
            float yPos = (cards.Count() * _ySpacingBuffer) * -1;

            foreach (var card in cards)
            {
                card.Transform.localPosition = new Vector3(card.Transform.localPosition.x, yPos, card.Transform.localPosition.z);
                yPos += _ySpacingBuffer;
            }
        }
        private void PresetCardsXPosition<T>(IEnumerable<T> cards) where T : ICardUI
        {
            //setting the first card position in center
            var _cardPosition = Vector3.zero;
            //setting vars needed
            var _centerCardXPosition = _cardPosition.x;

            //setting Card off the center to the right
            int cardCount = cards.Count();
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

        private void PositionCardsOnX<T>(IEnumerable<T> cards) where T : ICardUI
        {
            foreach (var card in cards)
            {
                card.Transform.SetParent(transform, false);
                card.Transform.localPosition = _layoutCardPosition;
                _layoutCardPosition.x += _xSpacing;
            }
        }
    }

    public interface ICardUI
    {
        public Transform Transform { get;}
    }
}