using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI PlayerScoreText;
    public Image PlayerAvatarImage;

    public void SetPlayerName(string playerName)
    {
        PlayerNameText.text = playerName;
    }

    public void SetPlayerScore(int playerScore)
    {
        PlayerScoreText.text = playerScore.ToString();
    }

    public void SetPlayerAvatar(Sprite playerAvatar)
    {
        PlayerAvatarImage.sprite = playerAvatar;
    }

}
