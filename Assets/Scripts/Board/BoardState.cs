using Konane.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane
{
    //Purpose of this class is to maintain a persistent representation of the board space. Two will exist in a game - 1st represents current state, 2nd represents search board for AI
    public class BoardState
    {
        //public const int blackIdx = 0;
        //public const int whiteIdx = 1;

        //Representation of the 8x8 board space
        //public int[,] board;
        public string[,] board;

        public void Init(SpriteRenderer[,] spriteRenderers)
        {
            board = new string[8, 8];
            /*
             * Options for creating / updating board UI and board state
             * 1. Have initial sprite renderer drive the board set up - this means pieceRenderers fills up the 8x8 board according to init
             * 2. Initialize the board state here and call a function inside Board that will update the UI elements to reflect the 8x8 board variable
             * 
             * REGARDLESS - An update function will be needed inside Board to reflect the internal board space
             * Could have 3 states for a given square - Black / White / None - This will allow move logic to check the from/to move space to verify move validity
             * THE FLOW IS AS FOLLOWS -> PLAYER ATTEMPING MOVE CALLS BOARD STATE TO UPDATE -> BOARD STATE THEN CALLS BOARD TO UPDATE THE UI
             * *Aug 21 note - should it as follows? BoardState (Mainly used by AI) -> GameManager <- Board (UI element mainly used by player)
             * - This would mean that State and UI converge on GameManager, which maintains both for use with AI / Human Player
             */
            //string test = "";
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    board[file, rank] = spriteRenderers[file, rank].name;
                    //Debug.Log("DEBUG - Sprite color at rank: " + rank + " file: " + file + " - " + spriteRenderers[file, rank].name + " board value at file/rank: " + board[file, rank]);
                    //test += board[file, rank];
                }
                //Debug.Log("Row at rank: " + rank + ": " + test);
                //test += "\n";
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
            int d; //Distance being travelled

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

        public void PrintBoard()
        {
            string test = "";
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    //Debug.Log("DEBUG - Sprite color at rank: " + rank + " file: " + file + " - " + spriteRenderers[file, rank].name + " board value at file/rank: " + board[file, rank]);
                    test += board[file, rank] + ",";
                }
                //Debug.Log("Row at rank: " + rank + ": " + test);
                test += "\n";
            }
            Debug.Log("Board State:\n" + test);
        }

        //TODO: Create a utility method that will evaluate how 'good' a move is for the AI player.
        //This will likely be a calculation of how many moves maximizing player has vs how many moves minimizing player has
        //Also need to write a weighting function that will have AI prioritize keeping pieces at the outer edges since those are more advantageous

        //Using the current board configuration, determine how 'good' it is
        public int UtilityEvaluation(bool isBlack)
        {
            /*
             * First iteration considerations:
             * 1. Rather than pieces remaining, focus on available moves. Eg - Current player moves - opposing player moves. This will give a sense of what states are better. More moves should be 'good'
             * 2. Favor maintaining pieces at the edge of the board. Not sure if this should be a weighting factor or adding more points. I fear that if I make it multiplicitave then
             * it will favor being at the edge of the board too much
             * 3. Opposing player is out of moves - this is very good.
             * 4. Evaluating player is out of moves - this is very bad.
             */
            //Purely test code. Currently GeneratePlayerMoves takes a boardstate as an arg. Hoping that 'this' keyword will send the current boardstate over. No idea though
            //Goal is to invoke GeneratePlayerMoves twice and do the relevant comparisons
            MoveGenerator mg = new();
            Dictionary<string, List<string>> blackMoves = new();
            Dictionary<string, List<string>> whiteMoves = new();
            int moveDiff;
            
            //Doing too much here. Just have it return the static evaluation of the board for ONE player only. Have the actual mathy stuff done outside
            //Eg. UtilityEvaluation(black player) - UtilityEvaluation(white player)

            //Current player is black
            if (isBlack)
            {
                mg.GeneratePlayerMoves(this, ref blackMoves, isBlack); //I have no idea if 'this' is the way to do the solution.
                mg.GeneratePlayerMoves(this, ref whiteMoves, !isBlack); //I have no idea if 'this' is the way to do the solution.
                moveDiff = blackMoves.Keys.Count - whiteMoves.Keys.Count;
            }
            //Current player is white
            else
            {
                mg.GeneratePlayerMoves(this, ref blackMoves, !isBlack); //I have no idea if 'this' is the way to do the solution.
                mg.GeneratePlayerMoves(this, ref whiteMoves, isBlack); //I have no idea if 'this' is the way to do the solution.
                moveDiff = whiteMoves.Keys.Count - blackMoves.Keys.Count;
            }
            
            //Factor in number of edge moves

            return moveDiff;
        }
    }
}
