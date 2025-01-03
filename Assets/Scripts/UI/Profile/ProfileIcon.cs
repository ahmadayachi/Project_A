using System;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIcon : MonoBehaviour
{
    public int ID;
    public Action<int> OnIconClicked;

    [SerializeField]
    public ProfileIconRefs UIRefs;

    void Awake()
    {
        SetIconButton();
    }
    public void ToggleIconHighlight(bool Toggle)=>UIRefs.IconHighlight.SetActive(Toggle);

    public void SetIcon(Sprite sprite)
    {
        UIRefs.SpriteHolder.sprite = sprite;
    }

    public void ResetIcon()
    {
        ID = 0;
        OnIconClicked = null;
        UIRefs.SpriteHolder.sprite = null;
        UIRefs.IconButton.onClick.RemoveAllListeners();
    }
    private void SetIconButton()
    {
        UIRefs.IconButton.onClick.RemoveAllListeners();
        UIRefs.IconButton.onClick.AddListener(() =>
        {
            OnIconClicked?.Invoke(this.ID);
        });
    }
}
[Serializable]
public struct ProfileIconRefs
{
    public Image SpriteHolder;
    public GameObject IconHighlight;
    public Button IconButton;
}
