using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public TMP_Dropdown difficulty;
    public TMP_Dropdown playerColor;
    Options options;

    public void Awake()
    {
        options = FindObjectOfType<Options>();
        difficulty.SetValueWithoutNotify((int)options.GetDifficulty());
        playerColor.SetValueWithoutNotify((int)options.GetPlayerColor());
    }
    public void SetDiff()
    {
        options.SetDifficulty((Options.Difficulty)difficulty.value);
    }

    public void SetColor()
    {
        options.SetPlayerColor((Options.PlayerColor)playerColor.value);
    }
}
