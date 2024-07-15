using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveMan.Tools
{
    public class ColorfulCard : MonoBehaviour, ICardUI
    {
        private SpriteRenderer _cardSprite;

        public Transform Transform => transform;

        private void Awake()
        {
            _cardSprite = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            SetRandomColor();
        }

        private void SetRandomColor()
        {
            Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);

            // Assign the random color to the sprite renderer
            _cardSprite.color = randomColor;
        }
    }
}