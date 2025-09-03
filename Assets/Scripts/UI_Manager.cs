using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    //Singleton
    public static UI_Manager Instance;

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

    public GameObject HomeScreen;
    public GameObject GameScreen;
    public GameObject GameOverScreen;

    public void ShowHomeScreen()
    {
        HomeScreen.SetActive(true);
        GameScreen.SetActive(false);
        GameOverScreen.SetActive(false);
    }

    public void ShowGameScreen()
    {
        HomeScreen.SetActive(false);
        GameScreen.SetActive(true);
        GameOverScreen.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        HomeScreen.SetActive(false);
        GameScreen.SetActive(false);
        GameOverScreen.SetActive(true);
    }
   
}
