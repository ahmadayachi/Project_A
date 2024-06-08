using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RunTimeDataHolder ")]
public class RunTimeDataHolder : ScriptableObject
{
    public List<RunTimePlayerData> RunTimePlayersData = new List<RunTimePlayerData>();
    public DeckInfo DeckInfo;
}
