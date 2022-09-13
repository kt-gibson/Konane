using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane.Game
{
    public class GameManager : MonoBehaviour
    {
        /*
         * High level implementation requirements
         * --------------------------------------
         * 1. Must set up board using Board class - this will create the UI and game objects
         * 2. Must assign the pieces to a given player (black/white)
         * 3. Need to determine whether Human or AI player is a given color (will be done through an options menu but can serialize a field for now)
         * 4. Store both current board state and current search board state (used by AI) - board state becomes search board state result after AI picks a move
         * 
         * 
         * Current Goal: Implement basic player movement (click to move)
         * Subset - Program the opening 4 moves (2 pieces removed for black / 2 pieces removed for white)
         *  - Need to add in some flags to either the player or board classes that will handle determining opening moves allowed
         *  - Also need a move class that will validate moves - this would factor in the first 4 moves
         *  - This is the point I should probably start developing my own frameworks - I've got the basics from Lague, now need to form my own class structures
         */
        Player playerBlack;
        Player playerWhite;
        Player currentPlayer;
        BoardState activeBoard;
        BoardState searchBoard;
        Board boardUI;
        bool blackToMove;
        int moveCount;

        public enum PlayerType
        {
            Human,
            AI
        }
        public enum GameState 
        { 
            Playing,
            //Stalemate,
            GameOver
        }
        GameState gs;
        public PlayerType whitePlayerType;
        public PlayerType blackPlayerType;

        MoveGenerator mg = new();

        void Start()
        {
            activeBoard = new BoardState();
            searchBoard = new BoardState();
            boardUI = FindObjectOfType<Board>();
            boardUI.CreateBoard(activeBoard, searchBoard);
            blackToMove = true;
            moveCount = 0;
            //playerBlack = new HumanPlayer(activeBoard); //Temp for testing
            NewGame(whitePlayerType, blackPlayerType);
        }

        // Update is called once per frame
        void Update()
        {
            if (gs == GameState.Playing)
                currentPlayer.Update();
        }

        void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType)
        {
            //Consider telling the board to reset positions and square colors - might just reload the scene though so may not matter
            CreatePlayer(ref playerWhite, whitePlayerType, false);
            CreatePlayer(ref playerBlack, blackPlayerType, true);

            gs = GameState.Playing;
            NotifyPlayerToMove();
        }

        void NotifyPlayerToMove()
        {
            //gs = GetGameState() - This line will eventually be needed to check for gameover conditions - for now don't need it
            //First player to run out of moves as it becomes their turn loses
            //Bug - this will force a loss scenario for opening 4 moves
            if (!mg.HasLegalMoves(activeBoard, blackToMove) && moveCount > 3)
            {
                if (blackToMove)
                    Debug.Log("Black loses!");
                else
                    Debug.Log("White loses!");
                gs = GameState.GameOver;
            }

            if (gs == GameState.Playing)
            {
                if (blackToMove)
                    Debug.Log("Black's Turn!");
                else
                    Debug.Log("White's Turn!");

                moveCount++;
                currentPlayer = blackToMove ? playerBlack : playerWhite;
                currentPlayer.NotifyTurnToMove();
            }
        }

        void OnMoveChosen(Move move)
        {
            //Note: Will need an eval of the current board state after a move is chosen to verify player to move has legal moves available - might do this in the notify method

            activeBoard.MakeMove(move);
            searchBoard.MakeMove(move);

            boardUI.UpdateBoard(activeBoard);

            activeBoard.PrintBoard();

            blackToMove = !blackToMove;
            NotifyPlayerToMove();
        }

        void OnStartMoveChosen(Coord move)
        {
            /*
             * Do the following
             * 1. Apply the selected move by calling the board state and and the board UI methods to update the piece selection/removal
             * 2. Update the board
             * 3. Notify the opposing player to move after setting the blackToMove flag
             */

            activeBoard.MakeStartMove(move);
            searchBoard.MakeStartMove(move);

            boardUI.UpdateBoard(activeBoard);

            activeBoard.PrintBoard();

            blackToMove = !blackToMove;
            NotifyPlayerToMove();
        }

        void CreatePlayer(ref Player player, PlayerType playerType, bool isBlack)
        {
            if (player != null)
            {
                player.onMoveChosen -= OnMoveChosen;
                player.onStartMoveChosen -= OnStartMoveChosen;
            }

            if (playerType == PlayerType.Human)
                player = new HumanPlayer(activeBoard, isBlack);
            else
                player = new AIPlayer(searchBoard, isBlack);

            player.onMoveChosen += OnMoveChosen;
            player.onStartMoveChosen += OnStartMoveChosen; //+= is the same as Delegate.Combine -= is the same as Delegate.Remove
        }
    }
}
