using System;
using UnityEngine;

public class DisplayCard : MonoBehaviour, ICardInfo
{
    [SerializeField] private byte _rank;
    public byte Rank { get => _rank; }

    [SerializeField] private byte _id;
    public byte ID { get => _id; }

    [SerializeField] private CardSuit _suit;
    public CardSuit Suit { get => _suit; }

    [SerializeField] DisplayCardUIRefs _uiRefs;
    public DisplayCardUIRefs UIRefs { get => _uiRefs; }

    public Action<byte> OnCardSelected;
    public Action<byte> OnCardDeSelected;

    public const string X = "X";
    private int _cardCounter;
    

    private void Awake()
    {
        //setting up card Button 
        SetUpButton();
    }

    public void SetRank(byte rank)
    {
        _rank = rank;
    }

    public void SetHighlighColor(Color color)
    {
        _uiRefs.CardPlateHighlight.color = color;
        _uiRefs.CardCounterHighlight.color = color;
    }
    public void SetSuit(CardSuit suit)
    {
        _suit = suit;

        //grabing the Suit 
        _uiRefs.CardSuit.sprite = AssetLoader.DeckContainerInstance.GetSuitSprite(_rank, _suit);
    }
    public void DisbaleButton()
    {
        _uiRefs.CardButton.enabled = false;
    }
    public void SetIdleState()
    {
        //hiding the card counter 
        HideAndResetCardCounter();

        //hiding card Highlight 
        _uiRefs.CardPlateHighlight.gameObject.SetActive(false);

        OnCardDeSelected?.Invoke(_rank);
    }

    public void CustomSelectState(int CardNumber)
    {
        //setting up the card counter 
        SetCardCounterNumber(CardNumber);

        //setting up hight light 
        _uiRefs.CardPlateHighlight.gameObject.SetActive(true);

        OnCardSelected?.Invoke(_rank);
    }
    /// <summary>
    /// idk but thats is the only name i could thing of right now 
    /// </summary>
    public void PumpSelect()
    {
        if (_cardCounter < CardManager.MaxRankCounter)
            CustomSelectState(_cardCounter + 1);
        //reseting if max Pump is Reached
        else
            SetIdleState();

    }
    #region private swamp
    private void HideAndResetCardCounter()
    {
        _uiRefs.CardCounterGameObject.SetActive(false);
        _uiRefs.CardCounterHighlight.gameObject.SetActive(false);
        _uiRefs.CardCounterText.text = string.Empty;
        _cardCounter = 0;
    }
    private void SetCardCounterNumber(int number)
    {
        _cardCounter = number;
        _uiRefs.CardCounterText.text = X + number.ToString();
        _uiRefs.CardCounterHighlight.gameObject.SetActive(true);
        _uiRefs.CardCounterGameObject.SetActive(true);
    }
    private void SetUpButton()
    {
        _uiRefs.CardButton.onClick.RemoveAllListeners();
        _uiRefs.CardButton.onClick.AddListener(PumpSelect);
    }
    #endregion
}
