using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardRepresentation : MonoBehaviour
{
    //Standard chess file/rank names - credit to Sebastian Lague for demonstrating how to create a board programmatically - https://www.youtube.com/watch?v=U4ogK0MIzqk
    public const string fileNames = "abcdefgh";
    public const string rankNames = "12345678";

	//64 unit representation of the board space - a1 = idx 0, b1 = idx 1, etc
	public const int a1 = 0;
	public const int b1 = 1;
	public const int c1 = 2;
	public const int d1 = 3;
	public const int e1 = 4;
	public const int f1 = 5;
	public const int g1 = 6;
	public const int h1 = 7;

	public const int a8 = 56;
	public const int b8 = 57;
	public const int c8 = 58;
	public const int d8 = 59;
	public const int e8 = 60;
	public const int f8 = 61;
	public const int g8 = 62;
	public const int h8 = 63;

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
