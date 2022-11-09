using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Konane.Game
{
    public class HumanPlayer : Player
    {
        public enum InputState
        {
            None,
            PieceSelected,
            MoveSelected
        }

        InputState currState;

        Board boardUI;
        BoardState boardState;
        Camera cam;

        int moves = 1; //Temp for testing - set back to 1
        List<string> legalStartMoves;
        Dictionary<string, List<string>> legalMoves = new();
        MoveGenerator mg = new();
        string startPosition;
        //string targetPosition;
        bool isBlack;
        //Move lastMove; //Used for testing revert move.
        /*
         * Need the following variables
         * 1. selected square coordinate
         * 2. target square coordinate
         * 3. A list that stores the set of legal starting moves - easier to log this at the beginning of the game and delete legal moves as a player selects them
         *      - Can have this use chess board notation (eg. a1, b7, etc) - can write a function to get the relevant index position from that
         * 
         * These variables used to check a few things:
         * 1. Does the selected square match the player color?
         * 2. Is the target square empty?
         * 3. Is there an enemy piece between the selected square and target square?
         * 
         * When a piece is clicked do the following:
         * 1. set the state to piece selected
         * 2. Have the transform follow the mouse position
         * 3. store the starting idx position
         * 
         * When a selected piece has a mouse 0 click do the following
         * 1. Check if the selected square is empty
         * 2. Check if the piece between the start and end coords have an enemy piece
         * 3. If so, move the piece to the new position, set the enemy piece to null at the enemy position, and notify game manager a move was chosen
         * 
         * Note: Special condtions for first four moves - Click pieces simply removes them. Player selects their own piece to remove (make sure to highlight legal moves)
         * After this special scenario the game proceeds as normal
         * 
         * Process for moving pieces
         * 1. When player clicks one of their pieces, highlight legal moves (if 0 don't select the piece)
         * 2. Don't do any dragging movement for now, just highlight the possible moves and if player then clicks a legal move position process that as their selected move
         * HandleInput - this method will determine what sub function to call based on piece state (none, selected, etc)
         * HandlePieceSelection - Will log the start position and change the state to selected - this will also need to calculate legal moves for that piece
         * HandlePiecePlacement - This will check if target position is one of the determined legal moves. If so, it will log the move
         * 
         */

        // Add in initialization of opening move set list here. This is used to evaluate possible moves
        public HumanPlayer(BoardState boardState, bool isBlack)
        {
            this.boardState = boardState;
            this.isBlack = isBlack;
            cam = Camera.main;
            boardUI = GameObject.FindObjectOfType<Board>();
            //Log initial set of legal moves based on board positions
            /*Debug.Log("Count of legalMoves: " + legalStartMoves.Count);
            for (int i = 0; i < legalStartMoves.Count; i++)
                Debug.Log("Value of legalMove board name at position " + i + ": " + legalStartMoves[i]);*/
        }

        public override void NotifyTurnToMove()
        {
            currState = InputState.None;
            mg.GeneratePlayerMoves(this.boardState, ref legalMoves, this.isBlack);
        }

        public override void NotifyOpeningTurnToMove()
        {
            currState = InputState.None;
            legalStartMoves = boardUI.GetStartMoves(this.isBlack);
        }

        public override void Update()
        {
            //Temp test logic for undoing a move
            /*if (Input.GetKeyDown(KeyCode.Space))
            {
                boardState.UnMakeMove(lastMove);
                boardUI.UpdateBoard(boardState);
            }*/

            //Konane has special start move logic - handle this separately for easy to read code
            if (moves <= 1)
                HandleStartInput();
            else
                HandleInput();
        }

        void HandleInput()
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            if (currState == InputState.None)
                HandlePieceSelection(mousePos);
            else if (currState == InputState.PieceSelected)
                HandleMovement(mousePos);
        }

        void HandleStartInput()
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log("DEBUG - Value of currState: " + currState + " Is Black Player? " + isBlack);

            if (currState == InputState.None)
                HandleStartPieceSelection(mousePos);
            else if (currState == InputState.PieceSelected)
                HandleStartMovement(mousePos);
        }

        void HandleMovement(Vector2 mousePos)
        {
            if (Input.GetMouseButtonDown(0))
                HandleMoveSelection(mousePos);
            else if (Input.GetMouseButtonDown(1))
            {
                startPosition = null;
                boardUI.ResetSquareColors();
                currState = InputState.None;
            }
        }

        void HandleStartMovement(Vector2 mousePos)
        {
            //If Inputstate == Piece selected - verify the mousePos selection matches the current selection. If so, change to Moveselected and perform the move
            //Cut/paste the actual movement code from HandleStartPieceSelection to here. HandleStartPieceSelection will just highlight squares and change the enum values
            if (Input.GetMouseButtonDown(0))
                HandleStartMoveSelection(mousePos);
            else if (Input.GetMouseButtonDown(1))
            {
                //Right mouse button will clear current selection
                startPosition = null;
                //targetPosition = null;
                boardUI.ResetSquareColors();
                currState = InputState.None;
            }
        }

        //Given a rank / file combination, verify this is a valid selection. For opening moves must be between 0 and 7, matching color, and contained within legal moves
        //Will handle generic move validation in else block
        bool IsValidStartSelection(int rank, int file)
        {
            return (rank >= 0 && rank < 8 && file >= 0 && file < 8) && (boardUI.CheckSelectionColorMatch(rank, file, this.isBlack)) && (legalStartMoves.Contains(boardUI.GetSquareNameAtPos(rank, file)));
        }

        bool IsValidSelection(int rank, int file, bool isStartPos)
        {
            if (isStartPos)
                return (rank >= 0 && rank < 8 && file >= 0 && file < 8) && (boardUI.CheckSelectionColorMatch(rank, file, this.isBlack)) && (legalMoves.ContainsKey(boardUI.GetSquareNameAtPos(rank, file)));

            return (rank >= 0 && rank < 8 && file >= 0 && file < 8) && (boardState.board[file, rank] == "none") && (legalMoves[startPosition].Contains(boardUI.GetSquareNameAtPos(rank, file)));
        }

        void HandleStartPieceSelection(Vector2 mousePos)
        {
            if (Input.GetMouseButtonDown(0)) //GetMouseButtonDown returns true ONCE, which is what I need to avoid unnecessary triggers of this code
            {
                //Convert these to a coordinate value
                int rank = (int)(mousePos.y + 4); //rank - y
                int file = (int)(mousePos.x + 4); //file - x
                //Coord chosenMove;

                if (IsValidStartSelection(rank, file))
                {
                    currState = InputState.PieceSelected;
                    boardUI.SetSelectedSquareColor(rank, file);
                    startPosition = boardUI.GetSquareNameAtPos(rank, file);
                }
            }
        }

        void HandleStartMoveSelection(Vector2 mousePos)
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
        }

        void HandlePieceSelection(Vector2 mousePos)
        {
            if (Input.GetMouseButtonDown(0))
            {
                int rank = (int)(mousePos.y + 4); //rank - y
                int file = (int)(mousePos.x + 4); //file - x

                if (IsValidSelection(rank, file, true))
                {
                    currState = InputState.PieceSelected;
                    boardUI.SetSelectedSquareColor(rank, file);
                    startPosition = boardUI.GetSquareNameAtPos(rank, file);

                    //Set all the target color squares
                    foreach (string target in legalMoves[boardUI.GetSquareNameAtPos(rank, file)])
                    {
                        BoardRepresentation.GetIdxFromSquareName(out int targetFile, out int targetRank, target);
                        boardUI.SetLegalTargetSquareColor(targetRank, targetFile);
                    }
                }
            }
        }

        void HandleMoveSelection(Vector2 mousePos)
        {
            int rank = (int)(mousePos.y + 4); //rank - y
            int file = (int)(mousePos.x + 4); //file - x
            Coord startPos;
            Coord targetPos;
            Move chosenMove;

            if (IsValidSelection(rank, file, false))
            {
                currState = InputState.MoveSelected;
                BoardRepresentation.GetIdxFromSquareName(out int startFile, out int startRank, startPosition);
                startPos = new Coord(startRank, startFile);
                targetPos = new Coord(rank, file);
                chosenMove = new Move(startPos, targetPos);
                //lastMove = chosenMove; //Temp for testing
                startPosition = null;
                boardUI.ResetSquareColors();
                legalMoves.Clear(); //Empty the dictionary
                ChosenMove(chosenMove); //Invoke the action, which is handled by GameManager
            }
        }
    }
}
