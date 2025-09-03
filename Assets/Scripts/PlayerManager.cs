using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // PlayerManager class to manage player instances and their interactions
    // This class can be expanded to include player management functionalities.

    private Player player1;

    private Player player2;

    public List<PlayerCard> Player1Cards = new List<PlayerCard>();

    public List<PlayerCard> Player2Cards = new List<PlayerCard>();

    public enum ActivePlayerIndices
    {
        None = 0,
        Player1 = 1,
        Player2 = 2,
        Both = 3
    }

    public ActivePlayerIndices activePlayerIndices = ActivePlayerIndices.None;
    
    // Singleton instance
    public static PlayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

  
    public void SetPlayer(string name, Sprite avatar, int id, int index)
    {
        Player newPlayer = new Player(name, avatar, id, index);
        
        if (index == 1)
        {
            player1 = newPlayer;
           
            foreach (PlayerCard card in Player1Cards)
            {
                card.PlayerNameText.text = name;
                card.PlayerAvatarImage.sprite = avatar;
                if(card.PlayerScoreText)
                    card.PlayerScoreText.text = "0";
                card.gameObject.SetActive(true);
            }
            if(activePlayerIndices == ActivePlayerIndices.None || activePlayerIndices == ActivePlayerIndices.Player1)
            {
                activePlayerIndices = ActivePlayerIndices.Player1;
            }
            else if (activePlayerIndices == ActivePlayerIndices.Player2)
            {
                activePlayerIndices = ActivePlayerIndices.Both;
            }
        }
        else if (index == 2)
        {
            player2 = newPlayer;
            foreach (PlayerCard card in Player2Cards)
            {
                card.PlayerNameText.text = name;
                card.PlayerAvatarImage.sprite = avatar;
                if (card.PlayerScoreText)
                    card.PlayerScoreText.text = "0";
                card.gameObject.SetActive(true);
            }
            if (activePlayerIndices == ActivePlayerIndices.None || activePlayerIndices == ActivePlayerIndices.Player2)
            {
                activePlayerIndices = ActivePlayerIndices.Player2;
            }
            else if (activePlayerIndices == ActivePlayerIndices.Player1)
            {
                activePlayerIndices = ActivePlayerIndices.Both;
            }
        }
    }

    public void RemovePlayer(int index)
    {
        if (index == 1)
        {
            player1 = null;
            foreach (PlayerCard card in Player1Cards)
            {
                card.PlayerNameText.text = "";
                card.PlayerAvatarImage.sprite = null;
                if (card.PlayerScoreText)
                    card.PlayerScoreText.text = "0";
                card.gameObject.SetActive(false);
            }
            if (activePlayerIndices == ActivePlayerIndices.Player1)
            {
                activePlayerIndices = ActivePlayerIndices.None;
            }
            else if (activePlayerIndices == ActivePlayerIndices.Both)
            {
                activePlayerIndices = ActivePlayerIndices.Player2;
            }
        }
        else if (index == 2)
        {
            player2 = null;
            foreach (PlayerCard card in Player2Cards)
            {
                card.PlayerNameText.text = "";
                card.PlayerAvatarImage.sprite = null;
                if (card.PlayerScoreText)
                    card.PlayerScoreText.text = "0";
                card.gameObject.SetActive(false);
            }
            if (activePlayerIndices == ActivePlayerIndices.Player2)
            {
                activePlayerIndices = ActivePlayerIndices.None;
            }
            else if (activePlayerIndices == ActivePlayerIndices.Both)
            {
                activePlayerIndices = ActivePlayerIndices.Player1;
            }
        }
        
    }

    public void AddScore(int index, int score)
    {
        if (index == 1)
        {
            player1.AddScore(score);
            foreach (PlayerCard card in Player1Cards)
            {
                card.PlayerScoreText.text = player1.PlayerID.ToString();
            }
        }
        else if (index == 2)
        {
            player2.AddScore(score);
            foreach (PlayerCard card in Player2Cards)
            {
                card.PlayerScoreText.text = player2.PlayerID.ToString();
            }
        }
    }

    public void ResetScores()
    {
        if (player1 != null)
        {
            player1.ResetScore();
            foreach (PlayerCard card in Player1Cards)
            {
                card.PlayerScoreText.text = "0";
            }
        }

        if (player2 != null)
        {
            player2.ResetScore();
            foreach (PlayerCard card in Player2Cards)
            {
                card.PlayerScoreText.text = "0";
            }
        }
    }
  
    
}
