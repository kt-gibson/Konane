using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Options : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Intermediate,
        Difficult
    }

    public enum PlayerColor
    {
        Black,
        White
    }

    private Difficulty difficulty;
    private PlayerColor playerColor;
    static Options instance;

    public void Awake()
    {
        ManageSingleton();
    }

    //Using a simple singleton pattern for transferring options data between scenes. This will be used by GameManager during the play game scene.
    void ManageSingleton()
    {
        if (instance != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            difficulty = Difficulty.Easy;
            playerColor = PlayerColor.Black;
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public Difficulty GetDifficulty()
    {
        return difficulty;
    }

    public PlayerColor GetPlayerColor()
    {
        return playerColor;
    }

    public void SetDifficulty(Difficulty diff)
    {
        difficulty = diff;
    }

    public void SetPlayerColor(PlayerColor pc)
    {
        playerColor = pc;
    }
}
