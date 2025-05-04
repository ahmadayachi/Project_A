using Unity.Collections;
using UnityEngine;

public class EndGamePlayerDisplay : MonoBehaviour
{
    [SerializeField]
    public EndGamePlayerDisplayUIRefs PlayerDisplayUIRefs;
    public FixedString64Bytes PlayerID;

    public void SetPlayerDisplayData(EndGamePlayerDisplayData playerData)
    {
        PlayerID = playerData.PlayerID;
        PlayerDisplayUIRefs.PlayerName.text = playerData.PlayerName.ToString();
        PlayerDisplayUIRefs.playerRank.text = playerData.PlayerRank.ToString();
        PlayerDisplayUIRefs.PlayerIcon.sprite = playerData.PlayerIcon;
    }

}

