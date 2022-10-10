//using Konane;
//using Konane.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane.Game
{
    public class Search
    {
        //GameManager.AIDifficulty diff;
        BoardState board;
        MoveGenerator mg;
        Move bestMove;

        public Search(BoardState board/*, GameManager.AIDifficulty diff*/)
        {
            this.board = board;
            //this.diff = diff;
            mg = new();
        }

        public Move StartSearch(bool isBlack, int depth) //Might remove depth at some point
        {
            MiniMaxSearch(depth, 0, true, isBlack);//Call minimax as the maximizing player - this opens the search algorithm
            return bestMove;
        }

        //Naive implementation of MiniMax search agent. Enhance with alpha/beta, move ordering, and transposition tables
        int MiniMaxSearch(int depth, int plyFromRoot, bool maximizingPlayer, bool isBlack)
        {
            //Current player is out of moves - this is very bad and a loss - return a terrible score (-100)
            if (!mg.HasLegalMoves(board, isBlack))
                return -100;

            //Opposing player is out of moves - this is very good move and a win - return a great score (100)
            if (!mg.HasLegalMoves(board, !isBlack))
                return 100;
            
            //Reached - maximum depth, return evaluation
            if (depth == 0)
            {
                return mg.UtilityEvaluation(board, isBlack) - mg.UtilityEvaluation(board, !isBlack);
            }

            //If current player is out of moves then return a really bad score. Opposite if opposing player is out of moves

            //Need to get moves at a given state for use in the minimax move evaluation algorithm

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                List<Move> moves = mg.GenerateAIMovesList(board, isBlack);
                //For each possible move in the list of moves set an eval = MiniMaxSearch

                /* 1. Using the board - get all possible moves for a state and loop through them
                 * 
                 * foreach move in allMoves loop -> inside this loop do the following: a. Make the move - b. Feed the new board state into minimax - c. Unmake move
                 * maxEval = max(maxEval, minimaxsearch(depth - 1, !maximizingPlayer, !isBlack)) -> Note: Will need a reference to player perspective on each turn
                 * unmake the move
                 * 
                 */
                for (int i = 0; i < moves.Count; i++)
                {
                    board.MakeMove(moves[i]);
                    int eval = -MiniMaxSearch(depth - 1, plyFromRoot + 1, !maximizingPlayer, !isBlack);//Note - for max player, multiply by negative 1
                    //maxEval = Mathf.Max(eval, maxEval); //This would get used in alpha beta pruning - for naive minimax need to do an if statement
                    if (eval > maxEval)
                    {
                        maxEval = eval;
                        bestMove = moves[i];
                    }
                    board.UnMakeMove(moves[i]);
                }

                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                List<Move> moves = mg.GenerateAIMovesList(board, isBlack);
                //For each possible move in the list of moves set an eval = MiniMaxSearch
                for (int i = 0; i < moves.Count; i++)
                {
                    board.MakeMove(moves[i]);
                    int eval = MiniMaxSearch(depth - 1, plyFromRoot + 1, !maximizingPlayer, !isBlack);
                    minEval = Mathf.Min(eval, minEval);
                    board.UnMakeMove(moves[i]);
                }

                return minEval;
            }
        }
    }
}