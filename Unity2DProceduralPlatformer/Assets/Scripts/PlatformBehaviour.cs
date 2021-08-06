using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBehaviour : MonoBehaviour
{
    private PlatformManager _platformGenerator;
    private SpriteRenderer spriteRenderer;

    public int platformID;
    public Vector2Int pos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        _platformGenerator = PlatformManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void setPlatform(int platformID, Vector2Int pos)
    {
        this.platformID = platformID;
        this.pos = pos;
        gameObject.name = "Platform (" + pos.x + "," + pos.y + "): " + platformID;

        spriteRenderer.sprite = _platformGenerator.platformTextures[platformID];
    }
}
