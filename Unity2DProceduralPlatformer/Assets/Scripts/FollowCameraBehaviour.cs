using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCameraBehaviour : MonoBehaviour
{
    public Camera mainCamera;
    public Transform playerTransform;

    //mapX, mapY is size of background image
    public float mapX = 150f;
    public float mapY = 150f;
    public float minX = -1;
    public float maxX = -1;
    public float minY;
    public float maxY;
    public float vertExtent;
    public float horzExtent;

    public Vector2 offset;


    // Start is called before the first frame update
    void Start()
    {
        //playerTransform = GameObject.Find("Player").transform;
        //mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        vertExtent = mainCamera.orthographicSize;
        horzExtent = vertExtent * Screen.width / Screen.height;

        // Calculations assume map is position at the origin
        minX = horzExtent - offset.x;
        maxX = mapX - horzExtent - offset.x;
        minY = vertExtent - offset.y;
        maxY = mapY - vertExtent - offset.y;
    }

    void LateUpdate()
    {
        Vector3 viewPos = new Vector3(playerTransform.position.x, playerTransform.position.y, -10);
        viewPos.x = Mathf.Clamp(viewPos.x, minX, maxX);
        viewPos.y = Mathf.Clamp(viewPos.y, minY, maxY);
        transform.position = viewPos;
    }

    public void setMap(Vector2 mapSize, Vector2 offset)
    {
        mapX = mapSize.x;
        mapY = mapSize.y;
        this.offset = offset;
    }
}
