using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane.Game
{
    //Credit to Sebastian Lague for demonstrating how to create a board programmatically - https://www.youtube.com/watch?v=U4ogK0MIzqk
    //This class exists primarily to visualize information for the player. The AI could play the game without this class (minus some validation functions I placed here)
    public class Board : MonoBehaviour
    {
        [SerializeField] BoardColor boardColor;
        [SerializeField] Sprite black;
        [SerializeField] Sprite white;

        //Variables for board generation - using 2d arrays to represent 8x8 board
        MeshRenderer[,] squareRenderers;
        SpriteRenderer[,] pieceRenderers;

        //This function will drive the initial creation of the board. It will also serve to initialize the boardstate
        //After this though, the boardstate should be driving the actions of this class. Eg - When a piece is taken from boardstate it calls an update method in board to refresh UI elements
        public void CreateBoard(BoardState activeBoard, BoardState searchBoard)
        {
            Shader squareShader = Shader.Find("Unlit/Color");
            squareRenderers = new MeshRenderer[8, 8];
            pieceRenderers = new SpriteRenderer[8, 8];
            string pieceName;

            //rank = horizontal (1-8), file = vertical (a-h) - these loops generate the board column by column
            for (int rank = 0; rank < 8; rank++)
                for (int file = 0; file < 8; file++)
                {
                    //Create each square representing the board
                    Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                    square.parent = transform;
                    square.name = BoardRepresentation.GetSquareNameFromCoord(file, rank);
                    square.position = PositionFromCoord(rank, file);
                    Material squareMat = new Material(squareShader);

                    //Assign the squares to the 2d array
                    squareRenderers[file, rank] = square.gameObject.GetComponent<MeshRenderer>();
                    squareRenderers[file, rank].material = squareMat;

                    //Create the necessary SpriteRenderer for the piece in question. Color will be determined in a separate method.
                    pieceName = BoardRepresentation.LightSquare(file, rank) ? "black" : "white";
                    SpriteRenderer pieceRenderer = new GameObject(pieceName).AddComponent<SpriteRenderer>();
                    pieceRenderer.transform.parent = square;
                    pieceRenderer.transform.position = PositionFromCoord(rank, file); // Currently leaving depth defaulted to 0. This might cause rendering issues so keep this in mind!
                    pieceRenderer.transform.localScale = Vector3.one;
                    pieceRenderers[file, rank] = pieceRenderer;
                }
            SetupBoard();
            activeBoard.Init(pieceRenderers);
            searchBoard.Init(pieceRenderers);
        }

        //Function that will load a board from a given setup
        public void LoadBoard(BoardState activeBoard, BoardState searchBoard, string[] testBoard)
        {
            //Need to update a given square with a piece (or none) and update both the UI and board states
            for (int rank = 0; rank < 8; rank++)
                for (int file = 0; file < 8; file++)
                {
                    switch (testBoard[rank][file])//Inverted because the inspector view is inverted
                    {
                        case 'b':
                            pieceRenderers[file, rank].name = "black";
                            pieceRenderers[file, rank].sprite = black;
                            break;
                        case 'w':
                            pieceRenderers[file, rank].name = "white";
                            pieceRenderers[file, rank].sprite = white;
                            break;
                        default:
                            pieceRenderers[file, rank].name = "none";
                            pieceRenderers[file, rank].sprite = null;
                            break;
                    }
                }

            activeBoard.Init(pieceRenderers);
            searchBoard.Init(pieceRenderers);
        }

        void SetupBoard()
        {
            SetSquareColors();
            SetPieces();
        }

        void SetSquareColors()
        {
            for (int rank = 0; rank < 8; rank++)
                for (int file = 0; file < 8; file++)
                    SetSquareColor(boardColor.light.baseColor, boardColor.dark.baseColor, rank, file);
        }

        //Should refactor this in concert with SetSquareColor so that future implementation of pieces will be smoother. Eg - White piece must be on black square
        Vector3 PositionFromCoord(int rank, int file, float z = 0)
        {
            return new Vector3(-3.5f + file, -3.5f + rank, z);
        }

        void SetSquareColor(Color light, Color dark, int rank, int file)
        {
            squareRenderers[file, rank].material.color = BoardRepresentation.LightSquare(file, rank) ? light : dark;
        }

        public void SetSelectedSquareColor(int rank, int file)
        {
            squareRenderers[file, rank].material.color = BoardRepresentation.LightSquare(file, rank) ? boardColor.light.selectColor : boardColor.dark.selectColor;
        }

        public void SetLegalTargetSquareColor(int rank, int file)
        {
            squareRenderers[file, rank].material.color = BoardRepresentation.LightSquare(file, rank) ? boardColor.light.legalMoveColor : boardColor.dark.legalMoveColor;
        }

        public void ResetSquareColors()
        {
            SetSquareColors();
        }

        //Place the opposing color piece on the square
        void SetPieces()
        {
            for (int rank = 0; rank < 8; rank++)
                for (int file = 0; file < 8; file++)
                {
                    pieceRenderers[file, rank].sprite = BoardRepresentation.LightSquare(file, rank) ? black : white;
                    pieceRenderers[file, rank].transform.position = PositionFromCoord(rank, file); // This is duplicated at the moment. Might be needed later to render pieces after moves are made
                }
        }

        public string GetPieceNameAtPos(int rank, int file)
        {
            return pieceRenderers[file, rank].name;
        }

        public string GetSquareNameAtPos(int rank, int file)
        {
            return squareRenderers[file, rank].name;
        }

        public List<string> GetStartMoves(bool isBlack)
        {
            List<string> moves = new();
            if (isBlack)
            {
                moves.Add(squareRenderers[0, 7].name);
                moves.Add(squareRenderers[3, 4].name);
                moves.Add(squareRenderers[4, 3].name);
                moves.Add(squareRenderers[7, 0].name);
            }
            else
            {
                //White player can only pick a piece orthogonal to black's selection
                for (int rank = 0; rank < 8; rank++)
                    for (int file = 0; file < 8; file++)
                    {
                        if (pieceRenderers[file, rank].name == "none")
                        {
                            //Check up
                            if (rank + 1 < 8)
                                moves.Add(squareRenderers[file, rank + 1].name);
                            //Check down
                            if (rank - 1 > 0)
                                moves.Add(squareRenderers[file, rank - 1].name);
                            //Check left
                            if (file - 1 > 0)
                                moves.Add(squareRenderers[file - 1, rank].name);
                            //Check right
                            if (file + 1 < 8)
                                moves.Add(squareRenderers[file + 1, rank].name);
                        }
                    }
            }
            return moves;
        }

        public void UpdateBoard(BoardState state)
        {
            for (int rank = 0; rank < 8; rank++)
                for (int file = 0; file < 8; file++)
                {
                    switch (state.board[file, rank])
                    {
                        case "black":
                            pieceRenderers[file, rank].sprite = black;
                            pieceRenderers[file, rank].name = "black";
                            break;
                        case "white":
                            pieceRenderers[file, rank].sprite = white;
                            pieceRenderers[file, rank].name = "white";
                            break;
                        case "none":
                            pieceRenderers[file, rank].sprite = null;
                            pieceRenderers[file, rank].name = "none";
                            break;
                    }
                }
        }

        //This function evaluates whether a selected piece matches the incoming player color
        public bool CheckSelectionColorMatch(int rank, int file, bool isBlack)
        {
            return (isBlack && GetPieceNameAtPos(rank, file) == "black") || (!isBlack && GetPieceNameAtPos(rank, file) == "white");
        }

        public void AnimateUpdateBoard(BoardState state, Move move)
        {
            StartCoroutine(AnimateMove(state, move));
        }

        //Animate start and end positions for the AI - this will simply highlighting the start and end squares, waiting time t, then resetting and making the move
        IEnumerator AnimateMove(BoardState state, Move move)
        {
            float t = 0;
            //Start square color
            SetSelectedSquareColor(move.startPos.rankIdx, move.startPos.fileIdx);

            //Target square color
            SetLegalTargetSquareColor(move.targetPos.rankIdx, move.targetPos.fileIdx);

            while (t < 1.5)
            {
                yield return null;
                t += Time.deltaTime;
            }

            UpdateBoard(state);
            //Reset square colors before exiting
            ResetSquareColors();
        }

        public void AnimateStartUpdateBoard(BoardState state, Coord move)
        {
            StartCoroutine(AnimateStartMove(state, move));
        }

        //Animate start and end positions for the AI - this will simply highlighting the start and end squares, waiting time t, then resetting and making the move
        IEnumerator AnimateStartMove(BoardState state, Coord move) //For some reason this is bugged - it doesn't update the board before the human player tries to generate moves - only appears when player is white
        {
            float t = 0;

            //Target square color
            SetLegalTargetSquareColor(move.rankIdx, move.fileIdx);

            while (t < 1.5)
            {
                yield return null;
                t += Time.deltaTime;
            }

            UpdateBoard(state);
            //state.PrintBoard();
            //Reset square colors before exiting
            ResetSquareColors();
        }
    }
}
