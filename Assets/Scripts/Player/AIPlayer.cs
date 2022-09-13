using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Konane.Game.HumanPlayer;

namespace Konane.Game
{
    public class AIPlayer : Player
    {
        /*
         * Initial todo
         * 1. Have the AI player randomly pick one of the four legal moves it has for its first two opening moves
         * This will require game manager to correctly shuffle the moves back and forth between human and AI players
         * This step is very important to get the first instantiation of the core game loop working. Once this aspect is complete can start fleshing out the AIs search algorithm
         */
        Board boardUI;
        BoardState boardState;
        int moves = 1;
        List<string> legalStartMoves;
        Dictionary<string, List<string>> legalMoves = new();
        bool isBlack;
        MoveGenerator mg = new();
        string startPosition;
        GameManager.AIDifficulty diff;

        public AIPlayer(BoardState boardState, bool isBlack, GameManager.AIDifficulty diff)
        {
            this.boardState = boardState;
            this.isBlack = isBlack;
            boardUI = GameObject.FindObjectOfType<Board>();
            legalStartMoves = boardUI.GetStartMoves(this.isBlack);
            this.diff = diff;
        }

        public override void NotifyTurnToMove()
        {
            if (moves > 2)
                mg.GeneratePlayerMoves(this.boardState, ref legalMoves, this.isBlack);
        }

        public override void Update()
        {
            //Konane has special start move logic - handle this separately for easy to read code
            if (moves <= 2)
                FindRandomStartMove(); // Going to always open with random start moves. I suspect it might be too much to ask the computer to search all parameters to see what is optimal
            else
            {
                switch (diff)
                {
                    case GameManager.AIDifficulty.Random:
                        FindRandomMove();
                        break;
                }
            }
        }

        void FindRandomStartMove()
        {
            Coord chosenMove;
            int selection = Random.Range(0, legalStartMoves.Count);
            BoardRepresentation.GetIdxFromSquareName(out int file, out int rank, legalStartMoves[selection]);
            chosenMove = new Coord(rank, file);
            moves += 1;
            ChosenStartMove(chosenMove);
        }

        void FindRandomMove()
        {
            Coord startPos;
            Coord targetPos;
            Move chosenMove;

            //1. Get a random key (key is also the start position)
            string key = legalMoves.ElementAt(Random.Range(0, legalMoves.Keys.Count)).Key;

            //2. Get a random target position in the key's value set (value is also the target position)
            string val = legalMoves[key][Random.Range(0, legalMoves[key].Count)];

            //Create the coords based off key/value pair
            BoardRepresentation.GetIdxFromSquareName(out int startFile, out int startRank, key);
            BoardRepresentation.GetIdxFromSquareName(out int targetFile, out int targetRank, val);

            startPos = new Coord(startRank, startFile);
            targetPos = new Coord(targetRank, targetFile);
            chosenMove = new Move(startPos, targetPos);
            legalMoves.Clear(); //Empty the dictionary
            ChosenMove(chosenMove); //Invoke the action, which is handled by GameManager
        }

        /*void HandleStartMoveSelection(Vector2 mousePos)
        {
            int rank = (int)(mousePos.y + 4); //rank - y
            int file = (int)(mousePos.x + 4); //file - x
            Coord chosenMove;

            if (IsValidStartSelection(rank, file))
            {
                if (startPosition == boardUI.GetSquareNameAtPos(rank, file))
                {
                    //Update the internal move state and reset the board UI colors
                    moves += 1;
                    currState = InputState.MoveSelected;
                    chosenMove = new Coord(rank, file);
                    startPosition = null;
                    boardUI.ResetSquareColors();
                    ChosenStartMove(chosenMove); //Invoke the action, which is handled by GameManager
                }
            }
        }*/
    }
}
