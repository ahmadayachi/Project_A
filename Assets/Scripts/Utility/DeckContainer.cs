using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "DeckContainer ")]
public class DeckContainer : ScriptableObject, ISerializationCallbackReceiver
{

    public Dictionary<string, List<Sprite>> SpriteContainer;

    [SerializeField] private List<Sprite> HeartSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> DiamondSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> SpideSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> CloverSprites = new List<Sprite>();
    public void OnBeforeSerialize()
    {

    }
    public void OnAfterDeserialize()
    {
        SpriteContainer = new Dictionary<string, List<Sprite>>
        {
            { "S", SpideSprites },
            { "D", DiamondSprites },
            { "H", HeartSprites },
            { "C", CloverSprites }
        };
    }
}
