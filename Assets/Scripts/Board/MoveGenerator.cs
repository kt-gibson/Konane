using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Konane.Game
{
    public class MoveGenerator
    {
        //BoardState state;
        List<string> moves = new();
        int cumulativeScore;
        //Board boardUI;

        //Assume that game over hasn't been reached. GameManager should be checking to see if all moves for a player are exhausted
        public void GeneratePlayerMoves(BoardState state, ref Dictionary<string, List<string>> legalMoves, bool isBlack)
        {
            //string square;
            //string color;
            //boardUI = new Board();
            //legalMoves = new Dictionary<string, List<string>>();
            //moves = new List<string>();
            //this.state = state;
            //Peek the 4 directions and see what captures are possible. Will need to account for double and triple jumps
            //Thinking of using a dictionary with a string / list data structure. This method would return a list of moves for a given position. If the list returned is empty then
            //The piece wouldn't be selected and nothing added to the dictionary
            //Not sure if I should generate the entire sequence of possible moves each turn? Might be simpler to generate everything then check if the selected square is in the keys portion of the dictionary
            // Eg. Key 'a8' would have two items in the list (c8 or a6) as possible moves (assuming validation allows these moves).
            // Caller might use a loop - eg. for rank / file -> get string repr of square -> Call generate moves and see what's available for that possible selection -> if not empty then add to dict

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
        void GenerateAISearchMoves(BoardState state, ref Dictionary<string, List<string>> curPlayerMoves, ref Dictionary<string, List<string>> oppPlayerMoves, string currColor, string oppColor)
        {
            bool isBlack;
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    if (state.board[file, rank] == currColor)
                    {
                        moves.Clear();
                        isBlack = currColor == "black";
                        //Start with the current player moves. Do the normal process, append to the dictionary, then repeat for the opposing player
                        //Even though this is basically duplicating code, it's faster than doing another 64 iterations for a separate player. This houses searches for moves inside one double loop
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
                            curPlayerMoves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank), new List<string>(moves)); //Clone the list to avoid shared memory location issues.
                            moves.Clear(); //Empty list after creating the dictionary entry for reuse on next iteration.
                        }
                    }
                    // if square = currPlayerColor then - use terniary to set isblack flag -> else if square = oppPlayerColor then
                    //moves.Clear();

                    else if (state.board[file, rank] == oppColor)
                    {
                        moves.Clear();
                        isBlack = oppColor == "black";

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
                            oppPlayerMoves.Add(BoardRepresentation.GetSquareNameFromCoord(file, rank), new List<string>(moves)); //Clone the list to avoid shared memory location issues.
                            moves.Clear(); //Empty list after creating the dictionary entry for reuse on next iteration.
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
            //Dictionary<string, List<string>> moves = new();
            //GeneratePlayerMoves(board, ref moves, isBlack);
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

            Debug.Log("DEBUG - NumEdgeMoves - Returning eval: " + eval);
            return (int)eval;
        }

        //Change this to have one 0-8 loop - this can evaluate each rank/file for edge pieces without needing 64 iterations
        int NumEdgePieces(BoardState board, bool isBlack)
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
        }

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

            GeneratePlayerMoves(board, ref moves, isBlack); //Nov 10 NOTE - Optimize this by creating a new function that will generate moves for both players - this will have the number of loops
            //Currently I'm doing millions of bloated loops covering the same stuff. Need to optimize this so the move ordering isn't eating a shitload of time

            //TODO: Factor in number of edge moves
            cumulativeScore += NumEdgeMoves(moves);

            //TODO: Factor in number of edge pieces
            //cumulativeScore += NumEdgePieces(board, isBlack);

            //Debug.Log("Cumulative score: " + cumulativeScore + " Move count: " + moves.Keys.Count); 
            return moves.Keys.Count + cumulativeScore; //This isn't really factoring in multi jumps (treats them as 1-3 'moves' but doesn't weight them any different) - could make an eval function specifically for multi jumps
        }

        //This function will take the board and moves list and make a move for each value. It will evaluate the new position and then unmake the move
        public void SortMoveList(BoardState board, List<Move> moves, bool isBlack)
        {
            //Create a new array that matches the length of the move list
            int[] scores = new int[moves.Count];
            //board.PrintBoard();
            for (int i = 0; i < moves.Count; i++)
            {
                //board.PrintBoard();
                board.MakeMove(moves[i]);
                //board.PrintBoard();
                //if (UtilityEvaluation(board, isBlack) - UtilityEvaluation(board, !isBlack) != 0)
                //    Debug.Log("DEBUG - Current player move score: " + UtilityEvaluation(board, isBlack) + " Opposing player move score: " + UtilityEvaluation(board, !isBlack));
                //If after the turn is made the opposing player has no moves, this is a win for current player. Add a large score so this is ordered first
                //Rationale - If the net score of moves is 0, that can cause paths to be explored that aren't an immediate win rather than simply picking the winning move
                if (!HasLegalMoves(board, !isBlack)) // Opposing player is out of moves, prioritize this in the sorting order
                    scores[i] = 100;
                else
                    //Make a separate UtilityEval function that takes different parameters to cut down on the needless move generation
                    //Could also do a movegenerator function that increments based on square color - eg. if player not black on look at dark squares - this should halve the iterations needed
                    //could do a simple mod function that would determine what square to start on rank % 2 == 0 or 1, which is also the square I'd want to start on
                    //if !black -> i = rank % 2 - if black -> i = rank % 2 == 0 ? (rank % 2) + 1 : (rank % 2) - 1
                    scores[i] = (UtilityEvaluation(board, isBlack) + MoveLandsOnEdge(moves[i])) - UtilityEvaluation(board, !isBlack); // Assign the overall score for a given move (player moves - opponent moves)
                board.UnMakeMove(moves[i]);
                //board.PrintBoard();
            }
            //board.PrintBoard();
            Sort(moves, scores);
        }

        //Function that will actually sort the moves list - since objects are passed by reference, modifying it here should change it in the calling method
        void Sort(List<Move> moves, int[] scores)
        {
            //Use a standard sort algorithm to reposition items in the list - order is greatest to least
            //Using bubble sort - should be fine since move list isn't going to balloon to huge lengths
            int n = moves.Count;
            string debug = "";

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
