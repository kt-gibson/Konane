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
                        }
                        else
                            d += 10; //No multiple jumps, break the loop.
                    }
                }
            }
        }

        // Going to need a generate AI moves method that is very similar to player moves but bases itself on entire board states
        //TODO: Create a utility method that will evaluate how 'good' a move is for the AI player.
        //This will likely be a calculation of how many moves maximizing player has vs how many moves minimizing player has
        //Also need to write a weighting function that will have AI prioritize keeping pieces at the outer edges since those are more advantageous

        //Using the current board configuration, determine how 'good' it is
        public int UtilityEvaluation(BoardState board, bool isBlack)
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
            //MoveGenerator mg = new();
            Dictionary<string, List<string>> moves = new();
            

            //Doing too much here. Just have it return the static evaluation of the board for ONE player only. Have the actual mathy stuff done outside
            //Eg. UtilityEvaluation(black player) - UtilityEvaluation(white player)

            //Current player is black
            if (isBlack)
                GeneratePlayerMoves(board, ref moves, isBlack);
            //Current player is white
            else
                GeneratePlayerMoves(board, ref moves, !isBlack);
            
            //TODO: Factor in number of edge moves

            return moves.Keys.Count;
        }
    }
}
