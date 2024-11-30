using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCard : MonoBehaviour, ICardInfo
{
    [SerializeField] private byte _rank;
    public byte Rank { get => _rank;}

    [SerializeField] private byte _id;
    public byte ID { get => _id; }

    [SerializeField] private CardSuit _suit;
    public CardSuit Suit { get => _suit; }

    [SerializeField] DisplayCardUIRefs _uiRefs;
    public DisplayCardUIRefs UIRefs { get => _uiRefs; }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
