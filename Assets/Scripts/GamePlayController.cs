using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GamePlayController : MonoBehaviour {

    public GameObject[] PlayerCarPrefabs;
    public GameObject[] PoliceCarPrefabs;
    public GameObject[] TownBuilderPrefabs;

    public int PoliceCount;
    public static GamePlayController Instance;
    [HideInInspector]public bool Loaded = false;
    MapGenerator mapGenerator;
    static bool _started;
    public static bool started
    {
        get
        {
            return _started;
        }
        set
        {
            Debug.Log("FIX ME!!!");
            _started = value;
        }
    }

    void Start()
    {
        Instance = this;
        mapGenerator = MapGenerator.Instance;
        if(mapGenerator == null)
        {
            Debug.Log("ITS ALL YOUR FAULT");
        }
        else
            StartCoroutine(StartGamePlay());
    }
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.P))
    //    {
    //        Application.CaptureScreenshot("Screenshot_" + System.DateTime.UtcNow + ".png", 5);
    //    }
    //}
    IEnumerator StartGamePlay()
    {
        while (true)
        {
            Debug.Log("Waiting To Load");
            yield return new WaitForSeconds(0.5f);
            if (mapGenerator.isTileMapReady)
            {
                break;
            }
        }
        // ***********************************************************
        // Use the <Rooms> array created earlier to get open locations
        // ***********************************************************


        // Spawn the player at random road tile
        TileMap tileMap = mapGenerator.tileMap;
        Tile startTile = tileMap.GetRoadTileNear(new Index(20, 20));
        int playerCarIndex = Random.Range(0, PlayerCarPrefabs.Length);
        Vector3 spawnPos = mapGenerator.IndexToWorldLocation(startTile.index);
        spawnPos.x += mapGenerator.tileWidth / 2;
        spawnPos.z += mapGenerator.tileHeight / 2;
        GameObject playerGo = (GameObject)Instantiate(PlayerCarPrefabs[playerCarIndex], spawnPos, Quaternion.identity);
        CameraController camScript = Camera.main.GetComponent<CameraController>();
        camScript.target = playerGo;
        camScript.DelayedStart();

        // Spawn the cops far away from each other
        startTile = tileMap.GetRoadTileNear(new Index(mapGenerator.tileSizeZ/2, mapGenerator.tileSizeX/2));
        int policeCarIndex = Random.Range(0, PoliceCarPrefabs.Length);
        spawnPos = mapGenerator.IndexToWorldLocation(startTile.index);
        spawnPos.x += mapGenerator.tileWidth / 2;
        spawnPos.z += mapGenerator.tileHeight / 2;
        GameObject policeGO = (GameObject)Instantiate(PoliceCarPrefabs[policeCarIndex], spawnPos, Quaternion.identity);

        yield return null; // Return in the next frame

        Loaded = true;
    }
    public void GameOver()
    {
        CarUserController.instance.Kill();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
