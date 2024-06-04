using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunTimePlayerUI : MonoBehaviour
{
    public Image PlayerIcon;
    public int IconIndex;
    public TMP_InputField PlayerNameInput;
    public string PlayerName;

    private void Awake()
    {
        PlayerNameInput.onValueChanged.AddListener(OnNameInput);
    }
    private void OnNameInput(string name)
    {
        PlayerName = name;
    }
 }
