using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Konane.Game
{
    public class TutorialManager : MonoBehaviour
    {
        int idx;
        BoardState activeBoard;
        BoardState searchBoard;
        Board boardUI;
        public Button leftButton;
        public Button rightButton;
        public TextMeshProUGUI leftText;
        public TextMeshProUGUI rightText;

        public string[] firstBoard = new string[8];
        public string[] secondBoard = new string[8];
        public string[] thirdBoard = new string[8];
        public string[] fourthBoard = new string[8];
        public string[] fifthBoard = new string[8];

        // Start is called before the first frame update
        void Start()
        {
            activeBoard = new BoardState();
            searchBoard = new BoardState();
            boardUI = FindObjectOfType<Board>();
            boardUI.CreateBoard(activeBoard, searchBoard);
            idx = 0;
            DisplayFirstScreen();
        }

        public void PrevButton()
        {
            if (idx == 0)
                SceneManager.LoadScene(0);
            else
            {
                boardUI.ResetSquareColors();
                idx--;
                HandleUIDisplay();
            }
        }

        public void NextButton()
        {
            if (idx == 4)
                SceneManager.LoadScene(0);
            else
            {
                boardUI.ResetSquareColors();
                idx++;
                HandleUIDisplay();
            }
        }

        void HandleUIDisplay()
        {
            switch (idx)
            {
                case 0:
                    DisplayFirstScreen();
                    break;
                case 1:
                    DisplaySecondScreen();
                    break;
                case 2:
                    DisplayThirdScreen();
                    break;
                case 3:
                    DisplayFourthScreen();
                    break;
                case 4:
                    DisplayFifthScreen();
                    break;
            }
        }

        void DisplayFirstScreen()
        {
            leftButton.GetComponentInChildren<TextMeshProUGUI>().text = "Main Menu";
            rightButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
            leftText.text = "Hawaiian checkers, also known as Konane, is a game similar to checkers except the goal is to be the first to force your opponent to run out of moves.";
            rightText.text = "The game starts with the board completely filled with pieces.";
            boardUI.LoadBoard(activeBoard, searchBoard, firstBoard);
        }

        void DisplaySecondScreen()
        {
            leftButton.GetComponentInChildren<TextMeshProUGUI>().text = "Previous";
            rightButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
            leftText.text = "Black is always the first to move. The first move allowed is for the black player to remove one of their pieces from any of the highlighted squares.";
            rightText.text = "";
            boardUI.LoadBoard(activeBoard, searchBoard, secondBoard);
            boardUI.SetSelectedSquareColor(0, 7);
            boardUI.SetSelectedSquareColor(7, 0);
            boardUI.SetSelectedSquareColor(4, 3);
            boardUI.SetSelectedSquareColor(3, 4);
        }

        void DisplayThirdScreen()
        {
            leftButton.GetComponentInChildren<TextMeshProUGUI>().text = "Previous";
            rightButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
            leftText.text = "After black selects their piece to remove the white player will choose an adjacent piece to remove. Normal play begins after these opening moves.";
            rightText.text = "";
            boardUI.LoadBoard(activeBoard, searchBoard, thirdBoard);
            boardUI.SetSelectedSquareColor(3, 3);
            boardUI.SetSelectedSquareColor(5, 3);
            boardUI.SetSelectedSquareColor(4, 2);
            boardUI.SetSelectedSquareColor(4, 4);
        }

        void DisplayFourthScreen()
        {
            leftButton.GetComponentInChildren<TextMeshProUGUI>().text = "Previous";
            rightButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
            leftText.text = "Regular moves are similar to checkers in that players jump opposing pieces onto an empty square. Multiple jumps are allowed if available.";
            rightText.text = "";
            boardUI.LoadBoard(activeBoard, searchBoard, fourthBoard);
            boardUI.SetSelectedSquareColor(4, 1);
            boardUI.SetLegalTargetSquareColor(4, 3);
        }

        void DisplayFifthScreen()
        {
            leftButton.GetComponentInChildren<TextMeshProUGUI>().text = "Previous";
            rightButton.GetComponentInChildren<TextMeshProUGUI>().text = "Main Menu";
            leftText.text = "The game ends when a player makes a move resulting in their opponent running out of moves. The last player to move is the winner.";
            rightText.text = "In this example, white can make any of the highlighted moves to win. Black will have no moves in response and will lose the game.";
            boardUI.LoadBoard(activeBoard, searchBoard, fifthBoard);
            boardUI.SetSelectedSquareColor(0, 0);
            boardUI.SetLegalTargetSquareColor(0, 2);
            boardUI.SetLegalTargetSquareColor(2, 0);
            boardUI.SetSelectedSquareColor(7, 1);
            boardUI.SetLegalTargetSquareColor(5, 1);
        }
    }
}
