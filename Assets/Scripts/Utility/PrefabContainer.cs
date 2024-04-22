using Fusion;
using UnityEngine;

[CreateAssetMenu(menuName = "PrefabContainer")]
public class PrefabContainer : ScriptableObject
{
    #region Network prefabs
    public NetworkPrefabRef PlayerPrefab;
    public NetworkPrefabRef CardPrefab;
    #endregion
    #region Log Manager Prefabs
    public GameObject LogPrefab;
    public GameObject LogButton;
    public GameObject LogManagerPanel;
    public Sprite LogErrorSprite;
    #endregion
}
