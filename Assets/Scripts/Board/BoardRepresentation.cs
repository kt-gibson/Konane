using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardRepresentation : MonoBehaviour
{
    //Standard chess file/rank names - credit to Sebastian Lague for demonstrating how to create a board programmatically - https://www.youtube.com/watch?v=U4ogK0MIzqk
    public const string fileNames = "abcdefgh";
    public const string rankNames = "12345678";

	//Assign a square name given a file / rank set of indices.
	public static string GetSquareNameFromCoord(int fileIdx, int rankIdx)
    {
		return fileNames[fileIdx] + "" + (rankIdx + 1);
    }

	//Need a function to retrieve the idx values from square name - could make this a pass by ref function so I can assign two values
	public static void GetIdxFromSquareName(out int fileIdx, out int rankIdx, string name)
    {
		fileIdx = fileNames.IndexOf(name[0]);
		rankIdx = rankNames.IndexOf(name[1]);
    }

	//Use modulus division to determine whether square should be light or dark.
	public static bool LightSquare(int fileIdx, int rankIdx)
    {
		return (fileIdx + rankIdx) % 2 != 0;
    }
}
