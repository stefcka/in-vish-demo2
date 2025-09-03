using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class coinCollected : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] GameObject enemyHitPointsText; //move to gamemanager

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Car") && gameManager != null)
        {
            gameManager.points += gameManager.pointsPerCoinCollected;

            GameObject newHitPointsText = Instantiate(enemyHitPointsText, new Vector3(transform.position.x, transform.position.y + 1.55f, transform.position.z), Quaternion.identity);
            newHitPointsText.gameObject.GetComponent<TextMeshPro>().text = "+" + gameManager.pointsPerCoinCollected;
            Destroy(newHitPointsText, 0.4f);
            Destroy(gameObject);
        }
        
    }
}
