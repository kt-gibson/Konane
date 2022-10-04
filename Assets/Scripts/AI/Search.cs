//using Konane;
//using Konane.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane.Game
{
    public class Search
    {
        GameManager.AIDifficulty diff;
        BoardState board;

        public Search(BoardState board, GameManager.AIDifficulty diff)
        {
            this.board = board;
            this.diff = diff;
        }

        public void StartSearch()
        {

        }

        //Naive implementation of MiniMax search agent. Enhance with alpha/beta, move ordering, and transposition tables
        int MiniMaxSearch(int depth, bool maximizingPlayer)
        {
            if (depth == 0)
                return 0;//Need to do the static evaluation of the state

            //If current player is out of moves then return a really bad score. Opposite if opposing player is out of moves

            //Need to get moves at a given state for use in the minimax move evaluation algorithm

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                //For each possible move in the list of moves set an eval = MiniMaxSearch

                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                //For each possible move in the list of moves set an eval = MiniMaxSearch

                return minEval;
            }
        }
    }
}