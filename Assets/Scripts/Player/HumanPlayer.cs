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
        bool isBlack;

        public HumanPlayer(BoardState boardState, bool isBlack)
        {
            this.boardState = boardState;
            this.isBlack = isBlack;
            cam = Camera.main;
            boardUI = GameObject.FindObjectOfType<Board>();
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
