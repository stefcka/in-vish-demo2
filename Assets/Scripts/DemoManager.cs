using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DemoManager : MonoBehaviour
{
    public Sprite UserImage;
    public string Player1Name;
    public string Player2Name;

    public List<GameObject> WelcomeScreenButtons;
    public List<GameObject> GameScreenButtons;
    public List<GameObject> GameOverScreenButtons;

    public TMP_InputField P1_X;
    public TMP_InputField P1_Y;

    public TMP_InputField P2_X;
    public TMP_InputField P2_Y;

    //Add Player 1
    public void AddPlayer1()
    {
        PlayerManager.Instance.SetPlayer(Player1Name, UserImage, 1, 1);
      
    }

    //Add Player 2
    public void AddPlayer2()
    {
        PlayerManager.Instance.SetPlayer(Player2Name, UserImage, 2, 2);
       
    }

    //Remove Player 1
    public void RemovePlayer1()
    {
        PlayerManager.Instance.RemovePlayer(1);
     
    }

    //Remove Player 2
    public void RemovePlayer2()
    {
        PlayerManager.Instance.RemovePlayer(2);
        
    }

    public void SwitchState(int Scenenumber)
    {
        GameStateManager.Instance.SetState((GameState)Scenenumber);
        if(Scenenumber == 0)
        {
            foreach (GameObject button in WelcomeScreenButtons)
            {
                button.SetActive(true);
            }
            foreach (GameObject button in GameScreenButtons)
            {
                button.SetActive(false);
            }
            foreach (GameObject button in GameOverScreenButtons)
            {
                button.SetActive(false);
            }
        }
        else if(Scenenumber == 1)
        {
            foreach (GameObject button in WelcomeScreenButtons)
            {
                button.SetActive(false);
            }
            foreach (GameObject button in GameScreenButtons)
            {
                button.SetActive(true);
            }
            foreach (GameObject button in GameOverScreenButtons)
            {
                button.SetActive(false);
            }
        }
        else if(Scenenumber == 2)
        {
            foreach (GameObject button in WelcomeScreenButtons)
            {
                button.SetActive(false);
            }
            foreach (GameObject button in GameScreenButtons)
            {
                button.SetActive(false);
            }
            foreach (GameObject button in GameOverScreenButtons)
            {
                button.SetActive(true);
            }
        }
          
       
    }

    public void Shoot_P1()
    {
        ShotManager.Instance.Shoot(1, int.Parse(P1_X.text),int.Parse(P1_Y.text));
       
    }

    public void Shoot_P2()
    {
        ShotManager.Instance.Shoot(2, int.Parse(P2_X.text),int.Parse(P2_Y.text));
       
    }
}
