using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Konane.Game
{
    public class MoveGenerator
    {
        List<string> moves = new();
        int cumulativeScore;

        //Assume that game over hasn't been reached. GameManager should be checking to see if all moves for a player are exhausted
        public void GeneratePlayerMoves(BoardState state, ref Dictionary<string, List<string>> legalMoves, bool isBlack)
        {
            //Loop through the board and check the four possible directions a player could make a move. Add them to the dictionary of possible moves.
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    if ((state.board[file, rank] == "black" && isBlack) || (state.board[file, rank] == "white" && !isBlack))
                    {
                        //Peek up
                        PeekUp(state, rank, file, isBlack);

                        //Peek down
                        PeekDown(state, rank, file, isBlack);

                        //Peek left
                        PeekLeft(state, rank, file, isBlack);

                        //Peek right
                        PeekRight(state, rank, file, isBlack);

                        //If any moves exist, create a key/value pair for this square
                        if (moves?.Count > 0)
                        {
                            legalMoves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank), new List<string>(moves)); //Clone the list to avoid shared memory location issues.
                            moves.Clear(); //Empty list after creating the dictionary entry for reuse on next iteration.
                        }
                    }
                }
            }
        }

        //This function will search for both current and opposing player moves. This halves the looping being done for a given utility evaluation
        void GenerateAISearchMoves(BoardState state, ref Dictionary<string, List<string>> legalMoves, bool isBlack)
        {
            //Use modulus division based on player perspective. Light squares always have black pieces and dark squares always have white pieces.
            if (isBlack)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    for (int file = rank % 2 == 0 ? (rank % 2) + 1 : (rank % 2) - 1; file < 8; file += 2)
                    {
                        if (state.board[file, rank] == "black")
                        {
                            //Peek up
                            PeekUp(state, rank, file, isBlack);

                            //Peek down
                            PeekDown(state, rank, file, isBlack);

                            //Peek left
                            PeekLeft(state, rank, file, isBlack);

                            //Peek right
                            PeekRight(state, rank, file, isBlack);

                            //If any moves exist, create a key/value pair for this square
                            if (moves?.Count > 0)
                            {
                                legalMoves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank), new List<string>(moves)); //Clone the list to avoid shared memory location issues.
                                moves.Clear(); //Empty list after creating the dictionary entry for reuse on next iteration.
                            }
                        }
                    }
                }
            }
            else
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    for (int file = rank % 2; file < 8; file += 2)
                    {
                        if (state.board[file, rank] == "white")
                        {
                            //Peek up
                            PeekUp(state, rank, file, isBlack);

                            //Peek down
                            PeekDown(state, rank, file, isBlack);

                            //Peek left
                            PeekLeft(state, rank, file, isBlack);

                            //Peek right
                            PeekRight(state, rank, file, isBlack);

                            //If any moves exist, create a key/value pair for this square
                            if (moves?.Count > 0)
                            {
                                legalMoves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank), new List<string>(moves)); //Clone the list to avoid shared memory location issues.
                                moves.Clear(); //Empty list after creating the dictionary entry for reuse on next iteration.
                            }
                        }
                    }
                }
            }
        }

        //Method that will generate a list of moves for the AI to loop through
        public List<Move> GenerateAIMovesList(BoardState state, bool isBlack)
        {
            List<Move> aiMoves = new();
            Dictionary<string, List<string>> legalMoves = new();
            Coord startPos;
            Coord endPos;
            Move move;

            GeneratePlayerMoves(state, ref legalMoves, isBlack);

            //For each key in the dictionary create a series of possible moves based on each key/value pair
            foreach(string key in legalMoves.Keys)
            {
                BoardRepresentation.GetIdxFromSquareName(out int startFile, out int startRank, key); //Start pos
                //Iterate through each value for a given key and append the move to the aiMoves list
                for (int i = 0; i < legalMoves[key].Count; i++)
                {
                    BoardRepresentation.GetIdxFromSquareName(out int endFile, out int endRank, legalMoves[key][i]); //End pos
                    startPos = new Coord(startRank, startFile);
                    endPos = new Coord(endRank, endFile);
                    move = new Move(startPos, endPos);
                    aiMoves.Add(move);
                }
            }

            return aiMoves;
        }

        public bool HasLegalMoves(BoardState state, bool isBlack)
        {
            moves.Clear();
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    if ((state.board[file, rank] == "black" && isBlack) || (state.board[file, rank] == "white" && !isBlack))
                    {
                        //Peek up
                        PeekUp(state, rank, file, isBlack);

                        //Peek down
                        PeekDown(state, rank, file, isBlack);

                        //Peek left
                        PeekLeft(state, rank, file, isBlack);

                        //Peek right
                        PeekRight(state, rank, file, isBlack);

                        //If any moves exist return true
                        if (moves?.Count > 0)
                        {
                            moves.Clear(); //Empty list after creating the dictionary entry for reuse on next iteration.
                            return true; //If even a single move is found then it's not game over
                        }
                    }
                }
            }

            return false;
        }

        void PeekUp(BoardState state, int rank, int file, bool isBlack)
        {
            //Start by peeking up 1 move to see if a single jump is possible. If not the method will return. Since moves is global can assign it within each method.
            //Limit evaluations to be within the acceptable board space. Peeking up is positive so the max value is 7.
            string playerColor = isBlack ? "black" : "white";
            string opponentColor = isBlack ? "white" : "black"; //Opposing color is inverse of player color
            int d = 2; //Distance d used for calculating multiple jumps - Distance d starts at 2 since the first jump moves two spaces

            if (rank + 2 <= 7)
            {
                //Will need to debug this and make sure it's rank + x rather than file + x -- can do this through move highlighting
                if (state.board[file, rank] == playerColor && state.board[file, rank + 1] == opponentColor && state.board[file, rank + 2] == "none")
                {
                    //Jump is possible, add it to the move list
                    moves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank + 2));

                    //Search for multiple jumps
                    while (rank + d <= 5) //5 is the max distance that can be checked before index exceptions are encountered when peeking
                    {
                        if (state.board[file, rank + (d + 1)] == opponentColor && state.board[file, rank + (d + 2)] == "none")
                        {
                            //Another jump is possible, add it to the move list
                            moves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank + (d + 2)));
                            d += 2; //Increment distance d two units. The loop will evaluate if another jump is possible after this one. Max should be 3 jumps on an 8x8 board.
                            cumulativeScore += 2;
                        }
                        else
                            d += 10; //No multiple jumps, break the loop.
                    }
                }
            }
        }

        void PeekDown(BoardState state, int rank, int file, bool isBlack)
        {
            //Start by peeking up 1 move to see if a single jump is possible. If not the method will return. Since moves is global can assign it within each method.
            //Limit evaluations to be within the acceptable board space. Peeking down is negative so the min value is 0.
            string playerColor = isBlack ? "black" : "white";
            string opponentColor = isBlack ? "white" : "black"; //Opposing color is inverse of player color
            int d = 2; //Distance d used for calculating multiple jumps - Distance d starts at 2 since the first jump moves two spaces

            if (rank - 2 >= 0)
            {
                //Will need to debug this and make sure it's rank + x rather than file + x -- can do this through move highlighting
                if (state.board[file, rank] == playerColor && state.board[file, rank - 1] == opponentColor && state.board[file, rank - 2] == "none")
                {
                    //Jump is possible, add it to the move list
                    moves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank - 2));

                    //Search for multiple jumps
                    while (rank - d >= 2) //2 is the min distance that can be checked before index exceptions are encountered when peeking
                    {
                        if (state.board[file, rank - (d + 1)] == opponentColor && state.board[file, rank - (d + 2)] == "none")
                        {
                            //Another jump is possible, add it to the move list
                            moves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank - (d + 2)));
                            d += 2; //Increment distance d two units. The loop will evaluate if another jump is possible after this one. Max should be 3 jumps on an 8x8 board.
                            cumulativeScore += 2;
                        }
                        else
                            d += 10; //No multiple jumps, break the loop.
                    }
                }
            }
        }

        void PeekLeft(BoardState state, int rank, int file, bool isBlack)
        {
            //Start by peeking up 1 move to see if a single jump is possible. If not the method will return. Since moves is global can assign it within each method.
            //Limit evaluations to be within the acceptable board space. Peeking left is negative so the min value is 0.
            string playerColor = isBlack ? "black" : "white";
            string opponentColor = isBlack ? "white" : "black"; //Opposing color is inverse of player color
            int d = 2; //Distance d used for calculating multiple jumps - Distance d starts at 2 since the first jump moves two spaces

            if (file - 2 >= 0)
            {
                //Will need to debug this and make sure it's rank + x rather than file + x -- can do this through move highlighting
                if (state.board[file, rank] == playerColor && state.board[file - 1, rank] == opponentColor && state.board[file - 2, rank] == "none")
                {
                    //Jump is possible, add it to the move list
                    moves.Add(BoardRepresentation.GetSquareNameFromCoord(file - 2, rank));

                    //Search for multiple jumps
                    while (file - d >= 2) //2 is the min distance that can be checked before index exceptions are encountered when peeking
                    {
                        if (state.board[file - (d + 1), rank] == opponentColor && state.board[file - (d + 2), rank] == "none")
                        {
                            //Another jump is possible, add it to the move list
                            moves.Add(BoardRepresentation.GetSquareNameFromCoord(file - (d + 2), rank));
                            d += 2; //Increment distance d two units. The loop will evaluate if another jump is possible after this one. Max should be 3 jumps on an 8x8 board.
                            cumulativeScore += 2;
                        }
                        else
                            d += 10; //No multiple jumps, break the loop.
                    }
                }
            }
        }

        void PeekRight(BoardState state, int rank, int file, bool isBlack)
        {
            //Start by peeking up 1 move to see if a single jump is possible. If not the method will return. Since moves is global can assign it within each method.
            //Limit evaluations to be within the acceptable board space. Peeking right is positive so the max value is 7.
            string playerColor = isBlack ? "black" : "white";
            string opponentColor = isBlack ? "white" : "black"; //Opposing color is inverse of player color
            int d = 2; //Distance d used for calculating multiple jumps - Distance d starts at 2 since the first jump moves two spaces

            if (file + 2 <= 7)
            {
                //Will need to debug this and make sure it's rank + x rather than file + x -- can do this through move highlighting
                if (state.board[file, rank] == playerColor && state.board[file + 1, rank] == opponentColor && state.board[file + 2, rank] == "none")
                {
                    //Jump is possible, add it to the move list
                    moves.Add(BoardRepresentation.GetSquareNameFromCoord(file + 2, rank));

                    //Search for multiple jumps
                    while (file + d <= 5) //5 is the max distance that can be checked before index exceptions are encountered when peeking
                    {
                        if (state.board[file + (d + 1), rank] == opponentColor && state.board[file + (d + 2), rank] == "none")
                        {
                            //Another jump is possible, add it to the move list
                            moves.Add(BoardRepresentation.GetSquareNameFromCoord(file + (d + 2), rank));
                            d += 2; //Increment distance d two units. The loop will evaluate if another jump is possible after this one. Max should be 3 jumps on an 8x8 board.
                            cumulativeScore += 2;
                        }
                        else
                            d += 10; //No multiple jumps, break the loop.
                    }
                }
            }
        }

        int NumEdgeMoves(Dictionary<string, List<string>> moves)
        {
            //Get all the moves and check if the first part of the square string is a or h or the second part of the string is 1 or 8 (I'd say start with landing on any of these possible squares)
            //Do 1.5f for an edge move and floor the end value for return - An edge move is likely to be better overall
            float eval = 0;

            foreach (string key in moves.Keys)
            {
                foreach (string val in moves[key])
                {
                    //Two scenarios - 1. Start and end on edge - this is an excellent move - 2. End on an edge - this is good
                    if ((key[0] == 'a' || key[0] == 'h' || key[1] == '1' || key[1] == '8') && (val[0] == 'a' || val[0] == 'h' || val[1] == '1' || val[1] == '8'))
                        eval += 1.5f;
                    //Start position isn't on an edge but ends on one
                    else if ((key[0] != 'a' && key[0] != 'h' && key[1] != '1' && key[1] != '8') && (val[0] == 'a' || val[0] == 'h' || val[1] == '1' || val[1] == '8'))
                        eval += 1.0f;
                }
            }

            return (int)eval;
        }

        //Change this to have one 0-8 loop - this can evaluate each rank/file for edge pieces without needing 64 iterations
        //Leaving out for now due to performance concerns. The AI is searching well enough at the moment
        /*int NumEdgePieces(BoardState board, bool isBlack)
        {
            float eval = 0;
            string color = isBlack ? "black" : "white";
            string square;

            //Loop through the board and check for pieces matching player color - then check whether the current coordinate is an edge square. If so, increment eval
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    if (board.board[file, rank] == color)
                    {
                        square = BoardRepresentation.GetSquareNameFromCoord(file, rank);
                        if (square[0] == 'a' || square[0] == 'h' || square[1] == '1' || square[1] == '8')
                            eval += 0.5f;
                    }
                }
            }

            Debug.Log("DEBUG - NumEdgePieces - Returning eval: " + eval);
            return (int)eval;
        }*/

        //Take a move and evaluate if the target position is an edge square - return a bonus score of 1 if so and 0 otherwise (tweak score adjustment as necessary)
        int MoveLandsOnEdge(Move move)
        {
            string targetSquare = BoardRepresentation.GetSquareNameFromCoord(move.targetPos.fileIdx, move.targetPos.rankIdx);
            if (targetSquare[0] == 'a' || targetSquare[0] == 'h' || targetSquare[1] == '1' || targetSquare[1] == '8')
                return 1;

            return 0;
        }

        //Using the current board configuration, determine how 'good' it is
        public int UtilityEvaluation(BoardState board, bool isBlack)
        {
            Dictionary<string, List<string>> moves = new();
            cumulativeScore = 0;

            //GeneratePlayerMoves(board, ref moves, isBlack); // Commented out this call in favor of an optimized search that halves the iterations.
            GenerateAISearchMoves(board, ref moves, isBlack);

            //Factor in number of edge moves
            cumulativeScore += NumEdgeMoves(moves);

            //Factor in number of edge pieces
            //cumulativeScore += NumEdgePieces(board, isBlack);

            return moves.Keys.Count + cumulativeScore;
        }

        //This function will take the board and moves list and make a move for each value. It will evaluate the new position and then unmake the move
        public void SortMoveList(BoardState board, List<Move> moves, bool isBlack)
        {
            //Create a new array that matches the length of the move list
            int[] scores = new int[moves.Count];

            for (int i = 0; i < moves.Count; i++)
            {
                board.MakeMove(moves[i]);
                //If after the turn is made the opposing player has no moves, this is a win for current player. Add a large score so this is ordered first
                if (!HasLegalMoves(board, !isBlack)) // Opposing player is out of moves, prioritize this in the sorting order
                    scores[i] = 100;
                else
                    scores[i] = (UtilityEvaluation(board, isBlack) + MoveLandsOnEdge(moves[i])) - UtilityEvaluation(board, !isBlack); // Assign the overall score for a given move (player moves - opponent moves)
                board.UnMakeMove(moves[i]);
            }
            Sort(moves, scores);
        }

        //Function that will actually sort the moves list - since objects are passed by reference, modifying it here should change it in the calling method
        void Sort(List<Move> moves, int[] scores)
        {
            //Use a standard sort algorithm to reposition items in the list - order is greatest to least
            //Using bubble sort - should be fine since move list isn't going to balloon to huge lengths
            int n = moves.Count;
            //string debug = "";

            /*for (int i = 0; i < n; i++)
            {
                debug += "Move: " + BoardRepresentation.GetSquareNameFromCoord(moves[i].startPos.fileIdx, moves[i].startPos.rankIdx) + "->" +
                    BoardRepresentation.GetSquareNameFromCoord(moves[i].targetPos.fileIdx, moves[i].targetPos.rankIdx) + " Eval: " + scores[i] + "; ";
            }
            Debug.Log("DEBUG ORDERING BEFORE - " + debug);*/

            //Sort the moves list based on accompanying scores in the array
            for (int i = 0; i < n - 1; i++)
                for (int j = 0; j < n - i - 1; j++)
                    if (scores[j] < scores[j+1])
                    {
                        //Swap moves and scores for consistency - can use tuples to accomplish this
                        (moves[j], moves[j+1]) = (moves[j+1], moves[j]);
                        (scores[j], scores[j+1]) = (scores[j+1], scores[j]);
                    }

            /*debug = "";
            for (int i = 0; i < n; i++)
            {
                debug += "Move: " + BoardRepresentation.GetSquareNameFromCoord(moves[i].startPos.fileIdx, moves[i].startPos.rankIdx) + "->" +
                    BoardRepresentation.GetSquareNameFromCoord(moves[i].targetPos.fileIdx, moves[i].targetPos.rankIdx) + " Eval: " + scores[i] + "; ";
            }
            Debug.Log("DEBUG ORDERING AFTER - " + debug);*/
            //for (int i = 0; i < moves.Count; i++)
            //    Debug.Log("DEBUG - Move: " + BoardRepresentation.GetSquareNameFromCoord(moves[i].startPos.fileIdx, moves[i].startPos.rankIdx) + "-" + BoardRepresentation.GetSquareNameFromCoord(moves[i].targetPos.fileIdx, moves[i].targetPos.rankIdx) + " Score eval: " + scores[i]);
        }
    }
}
