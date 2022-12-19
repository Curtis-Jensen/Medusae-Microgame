using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    #region Variables
    public int waveNumber;
    [Range(0, 1)] [Tooltip("The percent chance the enemy will be a medusa or a hoverbot")]
    public float medusaChance;
    public GameObject medusaPrefab;
    public GameObject hoverBotPrefab;
    public Text waveHud;
    [Tooltip("Whether or not the map is regenerated with each new wave")]
    public bool generatingChaotically;
    public ProceduralGeneration mapGenerator;

    string playerName;
    #endregion

    /* Keeps track of all the different groups that are being pooled
     * 
     * If the wave number is set to 0 that actually means that wave saving is on and it will automatically
     * chose a good wave
     * 
     * The reason the wave number is subtracted here is because EndWave() advances the wave,
     * even if it's the very first wave.  I used to have to have the starting wave always be 0 to
     * circummvent, but this is automatic.
     * 
     * And pauses the game in the beginning so the player knows how to play or that they've died
     */
    void Awake()
    {
        playerName = PlayerPrefs.GetString("playerName");
        if (waveNumber == 0)
            waveNumber = PlayerPrefs.GetInt(playerName + "waveNumber") / 2;

        EndWave();

        waveHud.text = waveNumber.ToString();
    }

    /* Advances the wave, calls spawning, and tells the appropriate scripts about the
     * increased threat
     */
    public void EndWave()
    {
        waveNumber++;

        PlayerPrefs.SetInt(playerName + "waveNumber", waveNumber);
        SpawnEnemies();

        waveHud.text = waveNumber.ToString();

        if(generatingChaotically) mapGenerator.CreateMap();
    }

    /* Spawns as many enemies as there are waves, and makes sure to leave active enemies alone.
     * 
     * Randomly determines what the spawn point will be based off the list of spawn points, 
     * which are determined by what the children are
     */
    void SpawnEnemies()
    {
        GameObject chosenEnemy;

        for (int i = 0; i < waveNumber; i++)
        {
            if (Random.value < medusaChance)
                chosenEnemy = medusaPrefab;
            else
                chosenEnemy = hoverBotPrefab;

            var nextSpawn = transform.GetChild(Random.Range(0, transform.childCount));

            Instantiate(chosenEnemy, nextSpawn.position, Quaternion.identity, gameObject.transform);
        }
    }
}