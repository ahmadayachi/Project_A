using Fusion;
using UnityEngine;

[CreateAssetMenu(menuName = "PrefabContainer")]
public class PrefabContainer : ScriptableObject
{
    #region Network prefabs
    public NetworkPrefabRef PlayerPrefab;
    public GameObject CardPrefab;
    public GameObject RunnerPrefab;
    #endregion
    #region Log Manager Prefabs
    public GameObject LogPrefab;
    #endregion
}
