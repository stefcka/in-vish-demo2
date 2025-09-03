using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    // Player class to manage player properties and actions
    // This class can be expanded to include player stats.
    private int playerIndex;
    private string playerName;
    private Sprite playerAvatar;
    private int playerID;
    private int playerScore;
  
    public Player(string name, Sprite avatar, int id, int playerIndex)
    {
        playerName = name;
        playerAvatar = avatar;
        playerID = id;
        playerScore = 0;
        this.playerIndex = playerIndex;
    }



    public void AddScore(int score)
    {
        playerScore += score;
    }

    public void ResetScore()
    {
        playerScore = 0;
    }

    public int PlayerID
    {
        get { return playerID; }
        set { playerID = value; }
    }

}
