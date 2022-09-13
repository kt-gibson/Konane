using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Board/Theme")]
public class BoardColor : ScriptableObject
{
    public SquareColors light;
    public SquareColors dark;

    [System.Serializable]
    public struct SquareColors
    {
        public Color baseColor;
        public Color selectColor;
        public Color legalMoveColor;
    }
}
