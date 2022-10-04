using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Konane.Game
{
    //Credit to Sebastian Lague for demonstrating how to create a board programmatically - https://www.youtube.com/watch?v=U4ogK0MIzqk
    //AI difficulty note: Could do the following for some difficulty settings - 1. Pure random choice from AI, 2. Limit search depth / time for AI, 3. No limits on the AI
    //Very important note - This class exists primarily to visualize information for the player. The AI could play the game without this class (minus some validation functions I placed here)
    public class Board : MonoBehaviour
    {
        [SerializeField] BoardColor boardColor;
        [SerializeField] Sprite black;
        [SerializeField] Sprite white;

        //Variables for board generation - using 2d arrays to represent 8x8 board
        MeshRenderer[,] squareRenderers;
        SpriteRenderer[,] pieceRenderers;
        //[SerializeField] bool invertSquareColors = true; // To be used later if players want to swap board colors - may just leave out for simplicity

        void Awake()
        {
            //CreateBoard();
        }

        //This function will drive the initial creation of the board. It will also serve to initialize the boardstate
        //After this though, the boardstate should be driving the actions of this class. Eg - When a piece is taken from boardstate it calls an update method in board to refresh UI elements
        public void CreateBoard(BoardState activeBoard, BoardState searchBoard)
        {
            Shader squareShader = Shader.Find("Unlit/Color");
            squareRenderers = new MeshRenderer[8, 8];
            pieceRenderers = new SpriteRenderer[8, 8];//For now just get the board to generate.
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
            //BoardState test = new BoardState(); // This needs to be removed. Have a board state sent in as an argument and it will call the init method
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

        //For the future - allow the board to be swapped - this would mildy change starting positions as black and white would have different layouts.
        //Also build in a function that will load in the piece of opposite color
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
    }
}
