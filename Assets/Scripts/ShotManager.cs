using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UDPMessageManager;

public class ShotManager : MonoBehaviour
{
    public static ShotManager Instance;
    public GameObject Player1_hitEffectPrefab; 
    public GameObject Player2_hitEffectPrefab;
    private AudioSource audioSource;
    public AudioClip shotSound;
    public AudioClip[] hitEnemySound;
    public ParticleSystem enemyKilledParticle;
    private GameManager gameManager;

    //using child search instead
    //public GameObject damagedEnemyModel;
    //public GameObject normalEnemyModel;

    [SerializeField] GameObject enemyHitPointsText;


    //singleton
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

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

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void HandleShoot(ShootData data)
    {
        Debug.Log($"Shoot Event - Player {data.player} at ({data.x}, {data.y})");

        // Implement shoot logic here

        //Raycast a shot from the camera to the target position
        //Check if event is "Shoot"
     
        Vector3 targetPosition = new Vector3(data.x, data.y, 0);
        Ray ray = Camera.main.ScreenPointToRay(targetPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Hit: {hit.collider.name}");

            audioSource.PlayOneShot(shotSound);

            //vish
            //enemy and game manager logic
            if(hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("Projectile"))
            {
                if(hit.collider.gameObject.CompareTag("Enemy") && hit.collider.gameObject.GetComponent<health>().healthValue > 1)
                {
                    //ParticleSystem newEnemyKilledParticle = Instantiate(enemyKilledParticle, hit.collider.gameObject.transform.position, Quaternion.identity);
                    Debug.Log("child 0: " + hit.collider.gameObject.transform.GetChild(0).gameObject.name);
                    Debug.Log("child 1: " + hit.collider.gameObject.transform.GetChild(1).gameObject.name);

                    hit.collider.gameObject.GetComponent<health>().reduceHealthAndDamageModel();

                }
                else if(hit.collider.gameObject.CompareTag("Enemy") &&  hit.collider.gameObject.GetComponent<health>().healthValue <= 1)
                {
                    gameManager.points += gameManager.pointsPerEnemy;
                    GameObject.Find("GameManager").GetComponent<GameManager>().decreaseEnemyCount();
                    ParticleSystem newEnemyKilledParticle = Instantiate(enemyKilledParticle, hit.collider.gameObject.transform.position, Quaternion.identity);
                    Destroy(newEnemyKilledParticle, 10);

                    GameObject newHitPointsText = Instantiate(enemyHitPointsText, hit.point, Quaternion.identity);
                    newHitPointsText.gameObject.GetComponent<TextMeshPro>().text = "+" + gameManager.pointsPerEnemy;
                    Destroy(newHitPointsText, 0.4f);
                    Destroy(hit.collider.gameObject);
                }
                else
                {
                    gameManager.points += gameManager.pointsPerProjectileDestroyed;

                    GameObject newHitPointsText = Instantiate(enemyHitPointsText, hit.point, Quaternion.identity);
                    newHitPointsText.gameObject.GetComponent<TextMeshPro>().text = "+" + gameManager.pointsPerProjectileDestroyed;
                    Destroy(newHitPointsText, 0.4f);
                    Destroy(hit.collider.gameObject);
                }



                foreach (AudioClip clip in hitEnemySound)
                {

                    audioSource.PlayOneShot(clip);
                }
            }

            // Implement hit logic here
            // Create a hit effect.
            if (data.player == 1)
            {
                GameObject hitEffect = Instantiate(Player1_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
            else if (data.player == 2)
            {
                GameObject hitEffect = Instantiate(Player2_hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
           

        }
        else
        {
            Debug.Log("Missed!");
        }
    }

    public void Shoot(int PlayerNumber, int x, int y)
    {
        // Create a new ShootData object
        ShootData shootData = new ShootData
        {
            player = PlayerNumber,
            x = x,
            y = y
        };

        HandleShoot(shootData);

    }
}
