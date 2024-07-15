using CaveMan.Tools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CaveMan.Tools
{
    public class CardSpawner : MonoBehaviour
    {
        [SerializeField] private CardFanning _cardfanning;
        [SerializeField] private Button _spawnCardNoParent;
        [SerializeField] private Button _injectCard;
        [SerializeField] private GameObject _colorfulCardPrefab;

        private void Awake()
        {
            SetUpSpawnCardWithNoParent();
            SetUpInjectCard();
        }

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void SetUpInjectCard()
        {
            _injectCard.onClick.RemoveAllListeners();
            _injectCard.onClick.AddListener(InjectCard);
        }

        private void InjectCard()
        {
            var card = SpawnCard(_cardfanning.gameObject.transform);
            _cardfanning.AddCard(card);
        }

        private void SetUpSpawnCardWithNoParent()
        {
            _spawnCardNoParent.onClick.RemoveAllListeners();
            _spawnCardNoParent.onClick.AddListener(() => { SpawnCard(null); });
        }

        private ColorfulCard SpawnCard(Transform parent)
        {
            var ColofulCardGO = Instantiate(_colorfulCardPrefab, parent);
            return ColofulCardGO.GetComponent<ColorfulCard>();
        }
    }
}