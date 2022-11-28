using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Konane.Game
{
    public class AIPlayer : Player
    {
        Board boardUI;
        BoardState boardState;
        int moves = 1;
        List<string> legalStartMoves;
        Dictionary<string, List<string>> legalMoves = new();
        bool isBlack;
        MoveGenerator mg = new();
        string startPosition;
        Options options;
        int searchDepth;

        public AIPlayer(BoardState boardState, bool isBlack)
        {
            this.boardState = boardState;
            this.isBlack = isBlack;
            boardUI = GameObject.FindObjectOfType<Board>();
            options = GameObject.FindObjectOfType<Options>();
        }

        public override void NotifyTurnToMove()
        {
            switch (options.GetDifficulty())
            {
                // Easy - AI plays at random
                case Options.Difficulty.Easy:
                    //Debug.Log("DEBUG - Playing random moves");
                    mg.GeneratePlayerMoves(this.boardState, ref legalMoves, this.isBlack); //Moved from outisde switch - this shouldn't be called if using minimax
                    FindRandomMove();
                    break;
                //Intermediate - Depth limit of 3 - 5
                case Options.Difficulty.Intermediate:
                    //Debug.Log("DEBUG - Playing intermediate moves");
                    searchDepth = 3;
                    StartMiniMaxSearch();
                    break;
                //Difficult - Depth limit of 5 - 7
                case Options.Difficulty.Difficult:
                    //Debug.Log("DEBUG - Playing hard moves");
                    searchDepth = 5;
                    StartMiniMaxSearch();
                    break;
            }
        }

        public override void NotifyOpeningTurnToMove()
        {
            legalStartMoves = boardUI.GetStartMoves(this.isBlack);
            FindRandomStartMove(); // Going to always open with random start moves. I suspect it might be too much to ask the computer to search all parameters to see what is optimal
        }

        public override void Update()
        {
            
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

        void StartMiniMaxSearch()
        {
            Search search = new(boardState);
            Move chosenMove = search.StartSearch(isBlack, searchDepth);
            //legalMoves.Clear(); //Empty the dictionary - NOTE THIS IS TEMPORARY - THE MOVES DICTIONARY SHOULD ONLY BE USED FOR RANDOM PLAY!!
            //Debug.Log("Chosen Move: " + BoardRepresentation.GetSquareNameFromCoord(chosenMove.startPos.fileIdx, chosenMove.startPos.rankIdx) + "-" + BoardRepresentation.GetSquareNameFromCoord(chosenMove.targetPos.fileIdx, chosenMove.targetPos.rankIdx));
            ChosenMove(chosenMove); //Invoke the action, which is handled by GameManager
        }
    }
}
