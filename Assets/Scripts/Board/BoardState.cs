using Konane.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane
{
    //Purpose of this class is to maintain a persistent representation of the board space. Two will exist in a game - 1st represents current state, 2nd represents search board for AI
    public class BoardState
    {
        //Representation of the 8x8 board space
        public string[,] board;

        public void Init(SpriteRenderer[,] spriteRenderers)
        {
            board = new string[8, 8];

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    board[file, rank] = spriteRenderers[file, rank].name;
                }
            }
        }

        //This function simply removes a piece from a given position, setting its value to "none"
        public void MakeStartMove(Coord move)
        {
            board[move.fileIdx, move.rankIdx] = "none";
        }

        public void MakeMove(Move move)
        {
            //Determine if the move is horizontal or vertical
            //Determine number of captures (should be max of 3)
            //Start coord becomes "none" - start coord + 1 / end coord - 1 becomes "none" - end coord becomes player color

            string startSquare = BoardRepresentation.GetSquareNameFromCoord(move.startPos.fileIdx, move.startPos.rankIdx);
            string targetSquare = BoardRepresentation.GetSquareNameFromCoord(move.targetPos.fileIdx, move.targetPos.rankIdx);
            string color = board[move.startPos.fileIdx, move.startPos.rankIdx];
            int d; //Direction being travelled

            //Check whether this is vertical (file letters match)
            if (startSquare[0] == targetSquare[0])
            {
                d = move.targetPos.rankIdx - move.startPos.rankIdx; //Note - Positive means moving up, negative means moving down (Every piece until the final position gets "none")
                //Every move from the start position (inclusive) to the end position (not inclusive) gets set to "none" - the target position gets set to the color found from the start position
                if (d > 0)
                {
                    //Going up
                    for (int rank = move.startPos.rankIdx; rank < move.targetPos.rankIdx; rank++)
                        board[move.startPos.fileIdx, rank] = "none";
                }
                else
                {
                    //Going down
                    for (int rank = move.startPos.rankIdx; rank > move.targetPos.rankIdx; rank--)
                        board[move.startPos.fileIdx, rank] = "none";
                }
                board[move.startPos.fileIdx, move.targetPos.rankIdx] = color; //Landing on target position gets the player color
            }
            // Else - file letters don't match, this is horizontal movement
            else
            {
                d = move.targetPos.fileIdx - move.startPos.fileIdx; //Note - Positive means moving right, negative means moving left (Every piece until the final position gets "none")
                if (d > 0)
                {
                    //Going right
                    for (int file = move.startPos.fileIdx; file < move.targetPos.fileIdx; file++)
                        board[file, move.startPos.rankIdx] = "none";
                }
                else
                {
                    //Going left
                    for (int file = move.startPos.fileIdx; file > move.targetPos.fileIdx; file--)
                        board[file, move.startPos.rankIdx] = "none";
                }
                board[move.targetPos.fileIdx, move.startPos.rankIdx] = color; //Landing on target position gets the player color
            }
        }

        //This function serves as the inverse of MakeMove. Given a move it will revert it by using the end position as an start point and start position as an end point
        public void UnMakeMove(Move move)
        {
            string startSquare = BoardRepresentation.GetSquareNameFromCoord(move.targetPos.fileIdx, move.targetPos.rankIdx);
            string targetSquare = BoardRepresentation.GetSquareNameFromCoord(move.startPos.fileIdx, move.startPos.rankIdx);
            string color = board[move.targetPos.fileIdx, move.targetPos.rankIdx];
            int d; //Direction being travelled

            //Check whether this is vertical (file letters match)
            if (startSquare[0] == targetSquare[0])
            {
                d = move.startPos.rankIdx - move.targetPos.rankIdx; //Note - Positive means moving up, negative means moving down (Every piece until the final position gets "none")
                //Every move from the start position (inclusive) to the end position (not inclusive) gets set to "none" - the target position gets set to the color found from the start position
                if (d > 0)
                {
                    //Going up
                    for (int rank = move.targetPos.rankIdx + 1; rank <= move.startPos.rankIdx; rank++)
                    {
                        board[move.startPos.fileIdx, rank] = BoardRepresentation.LightSquare(move.startPos.fileIdx, rank) ? "black" : "white";

                        //If the reverted piece's color matches that found by the board representation function, set it to none since intervening spaces must be blank
                        if (board[move.startPos.fileIdx, rank] == color && rank != move.startPos.rankIdx)
                            board[move.startPos.fileIdx, rank] = "none";
                    }
                }
                else
                {
                    //Going down
                    for (int rank = move.targetPos.rankIdx - 1; rank >= move.startPos.rankIdx; rank--)
                    {
                        board[move.startPos.fileIdx, rank] = BoardRepresentation.LightSquare(move.startPos.fileIdx, rank) ? "black" : "white";

                        //If the reverted piece's color matches that found by the board representation function, set it to none since intervening spaces must be blank
                        if (board[move.startPos.fileIdx, rank] == color && rank != move.startPos.rankIdx)
                            board[move.startPos.fileIdx, rank] = "none";
                    }
                }
                board[move.startPos.fileIdx, move.targetPos.rankIdx] = "none"; //Target piece gets removed
            }
            // Else - file letters don't match, this is horizontal movement
            else
            {
                d = move.startPos.fileIdx - move.targetPos.fileIdx; //Note - Positive means moving right, negative means moving left (Every piece until the final position gets "none")
                if (d > 0)
                {
                    //Going right
                    for (int file = move.targetPos.fileIdx + 1; file <= move.startPos.fileIdx; file++)
                    {
                        board[file, move.startPos.rankIdx] = BoardRepresentation.LightSquare(file, move.startPos.rankIdx) ? "black" : "white";

                        //If the reverted piece's color matches that found by the board representation function, set it to none since intervening spaces must be blank
                        if (board[file, move.startPos.rankIdx] == color && file != move.startPos.fileIdx)
                            board[file, move.startPos.rankIdx] = "none";
                    }
                }
                else
                {
                    //Going left
                    for (int file = move.targetPos.fileIdx - 1; file >= move.startPos.fileIdx; file--)
                    {
                        board[file, move.startPos.rankIdx] = BoardRepresentation.LightSquare(file, move.startPos.rankIdx) ? "black" : "white";

                        //If the reverted piece's color matches that found by the board representation function, set it to none since intervening spaces must be blank
                        if (board[file, move.startPos.rankIdx] == color && file != move.startPos.fileIdx)
                            board[file, move.startPos.rankIdx] = "none";
                    }
                }
                board[move.targetPos.fileIdx, move.startPos.rankIdx] = "none"; //Target piece gets removed
            }
        }

        public void PrintBoard()
        {
            string test = "";
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    test += board[file, rank] + ",";
                }
                test += "\n";
            }
            Debug.Log("Board State:\n" + test);
        }
    }
}
