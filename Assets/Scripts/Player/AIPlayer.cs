using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        List<string> legalMoves;
        bool isBlack;

        public AIPlayer(BoardState boardState, bool isBlack)
        {
            this.boardState = boardState;
            this.isBlack = isBlack;
            boardUI = GameObject.FindObjectOfType<Board>();
            //Log initial set of legal moves based on board positions
            legalMoves = boardUI.GetStartMoves(this.isBlack);//hard coding this bool for testing purposes - will need to passed in as part of class instantiation
            Debug.Log("Count of legalMoves: " + legalMoves.Count);
            for (int i = 0; i < legalMoves.Count; i++)
                Debug.Log("Value of legalMove board name at position " + i + ": " + legalMoves[i]);
        }

        public override void NotifyTurnToMove()
        {

        }

        public override void Update()
        {

        }
    }
}
