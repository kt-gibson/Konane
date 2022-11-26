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

    //Note - need a hookup between options and optionsmanager to display the list selection based on current options values
    public void Awake()
    {
        options = FindObjectOfType<Options>();
        difficulty.SetValueWithoutNotify((int)options.GetDifficulty());
        playerColor.SetValueWithoutNotify((int)options.GetPlayerColor());
    }
    public void SetDiff()
    {
        //Options.Difficulty diff = (Options.Difficulty)difficulty.value;
        //Debug.Log("DEBUG - Diff option selected: " + difficulty.value + " diff val: " + diff);
        //Debug.Log("DEBUG - Before diff set: " + options.GetDifficulty());
        options.SetDifficulty((Options.Difficulty)difficulty.value);
        //Debug.Log("DEBUG - After diff set: " + options.GetDifficulty());
    }

    public void SetColor()
    {
        //Debug.Log("DEBUG - Before color set: " + options.GetPlayerColor());
        options.SetPlayerColor((Options.PlayerColor)playerColor.value);
        //Debug.Log("DEBUG - After color set: " + options.GetPlayerColor());
    }
}
