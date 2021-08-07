using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : Singleton<PlatformManager>
{
    //public List<GameObject> tilePrefabs;
    //public GameObject tilePrefabBottom, tilePrefabTop, tilePrefabLeft, tilePrefabRight;

    public List<Sprite> platformTextures;
    public GameObject platformPrefab;
	// References to the empty objects for organising objects spawned in.
    private Transform _platformParent;
    private Transform _objectParent;
    private Transform _enemyParent;

    public int width, height;
    public float tileWidth, tileHeight;

    private List<GameObject> activePlatforms;
    private PlatformGenerator _platformGenerator;

    public GameObject player;

    public List<GameObject> objectPrefabs;
    public List<GameObject> activeObjects;

    public bool showPlatforms = true;
    public bool applyIDMapping = true;
    public bool spawnObjects = true;
    public float spawnObjectChance = 0.4f;
    public bool randomObjects = false;

    // Start is called before the first frame update
    void Start()
    {
        activePlatforms = new List<GameObject>();
        activeObjects = new List<GameObject>();
        instance = this;

        _platformParent = transform.GetChild(0);
        _objectParent = transform.GetChild(1);
        _enemyParent = transform.GetChild(2);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            destroyLevel();
            generateLevel();
        }
    }

    private void generateLevel()
    {
        if (_platformGenerator == null)
        {
            _platformGenerator = PlatformGenerator.instance;
        }
        //_platformGenerator.createMap(width, height);

        _platformGenerator.populateRooms();
        _platformGenerator.createMapFromRooms();
        if (showPlatforms)
            _platformGenerator.createSolutionPath();

        _platformGenerator.printMap();
        if (applyIDMapping)
            _platformGenerator.applyIDMapping();
        _platformGenerator.printMap();
        _platformGenerator.spawnMap();
        if (spawnObjects)
        {
            if (randomObjects)
                _platformGenerator.spawnRandomObjectsInRooms(spawnObjectChance);
            else
                _platformGenerator.spawnObjectsInRooms(0, spawnObjectChance);
        }

        Camera.main.GetComponent<FollowCameraBehaviour>().setMap(new Vector2((width) * tileWidth, (height) * tileHeight), new Vector2(tileWidth / 2, tileHeight / 2));
        player.SetActive(true);
        Vector2Int playerSpawn = _platformGenerator.findPlayerSpawn();
        player.transform.position = new Vector2(playerSpawn.x * tileWidth, playerSpawn.y * tileHeight);

    }

    public void spawnPlatform(int platformID, int x, int y)
    {
        var newObj = Instantiate(platformPrefab, new Vector2(x * tileWidth, y * tileHeight), Quaternion.identity, _platformParent);
        newObj.GetComponent<PlatformBehaviour>().setPlatform(platformID, new Vector2Int(x, y));
        activePlatforms.Add(newObj);
    }

    public void spawnObject(int objectID, int x, int y)
    {
        var newObj = Instantiate(objectPrefabs[objectID], new Vector2(x * tileWidth, y * tileHeight), Quaternion.identity, _objectParent);
        activePlatforms.Add(newObj);
    }

    public void destroyLevel()
    {
        for (int i = 0; i < activePlatforms.Count; i++)
        {
            Destroy(activePlatforms[i]);
        }
        activePlatforms = new List<GameObject>();

        for (int i = 0; i < activeObjects.Count; i++)
        {
            Destroy(activeObjects[i]);
        }
        activeObjects = new List<GameObject>();
    }
}
