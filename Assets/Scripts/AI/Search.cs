using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane.Game
{
    public class Search
    {
        BoardState board;
        MoveGenerator mg;
        Move bestMove;
        Move bestSearchedMove;
        //int nodeCount; //Used for testing

        public Search(BoardState board)
        {
            this.board = board;
            mg = new();
        }

        public Move StartSearch(bool isBlack, int depth)
        {
            //nodeCount = 0;
            //MiniMaxSearch(depth, 0, true, isBlack);//Call minimax as the maximizing player - this opens the search algorithm
            AlphaBetaSearch(depth, 0, -int.MaxValue, int.MaxValue, isBlack); // Need to use -int.MaxValue because using int.MinValue * -1 will overflow the integer range
            return bestSearchedMove;
        }

        //Naive implementation of MiniMax search agent. Enhance with alpha/beta, move ordering, and transposition tables
        /*int MiniMaxSearch(int depth, int plyFromRoot, bool maximizingPlayer, bool isBlack)
        {
            //Current player is out of moves - this is very bad and a loss - return a terrible score (-100)
            if (!mg.HasLegalMoves(board, isBlack))
                return -100;

            //Opposing player is out of moves - this is very good move and a win - return a great score (100)
            if (!mg.HasLegalMoves(board, !isBlack))
                return 100;
            
            //Reached maximum depth - return evaluation of current player's 'score' minus the opposing player's 'score'
            if (depth == 0)
            {
                return mg.UtilityEvaluation(board, isBlack) - mg.UtilityEvaluation(board, !isBlack);
            }
            nodeCount++;
            //If current player is out of moves then return a really bad score. Opposite if opposing player is out of moves

            //Need to get moves at a given state for use in the minimax move evaluation algorithm

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                List<Move> moves = mg.GenerateAIMovesList(board, isBlack);
                //For each possible move in the list of moves set an eval = MiniMaxSearch
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
        }*/

        //Implementation of negamax search framework. Keeping the original minimax framework as a commented historic method
        int AlphaBetaSearch(int depth, int plyFromRoot, int alpha, int beta, bool isBlack)
        {
            //Current player is out of moves - this is very bad and a loss - return a terrible score (-100)
            if (!mg.HasLegalMoves(board, isBlack))
                return -100;

            //Reached maximum depth - return evaluation of current player's 'score' minus the opposing player's 'score'
            if (depth == 0)
                return mg.UtilityEvaluation(board, isBlack) - mg.UtilityEvaluation(board, !isBlack);
            //nodeCount++;
            List<Move> moves = mg.GenerateAIMovesList(board, isBlack);
            mg.SortMoveList(board, moves, isBlack); //Sort the moves from best to worst scores

            for (int i = 0; i < moves.Count; i++)
            {
                board.MakeMove(moves[i]);
                int eval = -AlphaBetaSearch(depth - 1, plyFromRoot + 1, -beta, -alpha, !isBlack);
                board.UnMakeMove(moves[i]);
                if (eval >= beta) //Use the hard fail cutoff - move is too good to beta is the best possible score to be achieved
                    return beta;

                if (eval > alpha) //New best move found - update the best move value
                {
                    alpha = eval;
                    bestMove = moves[i];
                    if (plyFromRoot == 0)
                    {
                        bestSearchedMove = moves[i];
                    }
                }
            }

            return alpha;
        }
    }
}