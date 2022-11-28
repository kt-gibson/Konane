using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Konane.Game
{
    public class GameManager : MonoBehaviour
    {
        Player playerBlack;
        Player playerWhite;
        Player currentPlayer;
        BoardState activeBoard;
        BoardState searchBoard;
        Board boardUI;
        Options options;
        public Button playAgain;
        public Button mainMenu;
        public TextMeshProUGUI winner;
        bool blackToMove;
        int moveCount; //Used for debugging purposes

        //Test board will look inverted in the inspector because the board loads (0,0) as bottom left where the array will see (0,0) as top left
        public string[] testBoard = new string[8]; //Used to load a custom board state. Will be needed for minimax testing on simple configurations (w/b are pieces, x is empty space)
        public bool loadTestBoard = false; //Used for debugging purposes
        public bool blackMove = true; //Used for debugging purposes

        public enum PlayerType
        {
            Human,
            AI
        }
        public enum GameState 
        { 
            Playing,
            GameOver
        }
        GameState gs;
        public PlayerType whitePlayerType; //change from public to private
        public PlayerType blackPlayerType; //change from public to private

        MoveGenerator mg = new();

        void Start()
        {
            activeBoard = new BoardState();
            searchBoard = new BoardState();
            boardUI = FindObjectOfType<Board>();
            options = FindObjectOfType<Options>();
            boardUI.CreateBoard(activeBoard, searchBoard);
            blackToMove = true;
            moveCount = 0;

            //Hide the buttons and text
            winner.text = "";
            playAgain.gameObject.SetActive(false);
            mainMenu.gameObject.SetActive(false);

            if (loadTestBoard)
            {
                blackToMove = blackMove;
                moveCount = 5;
                boardUI.LoadBoard(activeBoard, searchBoard, testBoard);
            }
            whitePlayerType = options.GetPlayerColor() == Options.PlayerColor.White ? PlayerType.Human : PlayerType.AI;
            blackPlayerType = options.GetPlayerColor() == Options.PlayerColor.Black ? PlayerType.Human : PlayerType.AI;

            //Test block
            //DumpOptimizedBoardMoves(true);
            //DumpOptimizedBoardMoves(false);

            NewGame(whitePlayerType, blackPlayerType);
        }

        //Temp test to make sure my math is correct - this is testing halving the iterations done when generating moves.
        /*void DumpOptimizedBoardMoves(bool playerBlack)
        {
            if (playerBlack)
            {
                Debug.Log("DUMPING BLACK SQUARES");
                for (int rank = 0; rank < 8; rank++)
                {
                    //int idx = rank % 2 == 0 ? (rank % 2) + 1 : (rank % 2) - 1;
                    for (int file = rank % 2 == 0 ? (rank % 2) + 1 : (rank % 2) - 1; file < 8; file += 2)
                    {
                        if (activeBoard.board[file, rank] == "black")
                            Debug.Log("File: " + file + "; Rank: " + rank + "; Piece: " + activeBoard.board[file, rank]);
                    }
                }
            }
            else
            {
                Debug.Log("DUMPING WHITE SQUARES");
                for (int rank = 0; rank < 8; rank++)
                {
                    for (int file = rank % 2; file < 8; file += 2)
                    {
                        if (activeBoard.board[file, rank] == "white")
                            Debug.Log("File: " + file + "; Rank: " + rank + "; Piece: " + activeBoard.board[file, rank]);
                    }
                }
            }
        }*/

        void Update()
        {
            if (gs == GameState.Playing)
                currentPlayer.Update();
        }

        void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType)
        {
            CreatePlayer(ref playerWhite, whitePlayerType, false);
            CreatePlayer(ref playerBlack, blackPlayerType, true);

            gs = GameState.Playing;
            NotifyPlayerToMove();
        }

        void NotifyPlayerToMove()
        {
            //First player to run out of moves as it becomes their turn loses
            if (!mg.HasLegalMoves(activeBoard, blackToMove) && moveCount > 2)
            {
                //Reveal winner text and buttons
                if (blackToMove)
                    if (blackPlayerType == PlayerType.Human)
                        winner.text = "You lose!";
                    else
                        winner.text = "You win!";
                else
                    if (whitePlayerType == PlayerType.Human)
                        winner.text = "You lose!";
                    else
                        winner.text = "You win!";

                playAgain.gameObject.SetActive(true);
                mainMenu.gameObject.SetActive(true);
                gs = GameState.GameOver;
            }

            if (gs == GameState.Playing)
            {
                moveCount++;
                currentPlayer = blackToMove ? playerBlack : playerWhite;

                if (moveCount > 2)
                    currentPlayer.NotifyTurnToMove();
                else
                    currentPlayer.NotifyOpeningTurnToMove();
            }
        }

        void OnMoveChosen(Move move)
        {
            activeBoard.MakeMove(move);
            searchBoard.MakeMove(move);

            //Animate the AI moves
            if (currentPlayer is AIPlayer)
                boardUI.AnimateUpdateBoard(activeBoard, move);
            else
                boardUI.UpdateBoard(activeBoard);

            blackToMove = !blackToMove;
            NotifyPlayerToMove();
        }

        void OnStartMoveChosen(Coord move)
        {
            activeBoard.MakeStartMove(move);
            searchBoard.MakeStartMove(move);

            //Animate the AI moves
            if (currentPlayer is AIPlayer)
                boardUI.AnimateStartUpdateBoard(activeBoard, move);
            else
                boardUI.UpdateBoard(activeBoard);

            boardUI.UpdateBoard(activeBoard);
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
