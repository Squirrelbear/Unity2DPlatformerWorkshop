using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCameraBehaviour : MonoBehaviour
{
	// Reference to the MainCamera to update its position and check 
    public Camera mainCamera;
	// Reference to the object to follow (normally a player).
    public Transform playerTransform;

    //mapX, mapY is size of background image
    public float mapX = 150f;
    public float mapY = 150f;
	
	// Calculated actual bounds based on camera size and map size combined with the offset.
    public float minX = -1;
    public float maxX = -1;
    public float minY;
    public float maxY;
	// Amount visible on the axis from the Camera used for calculating the above. Pulled from the camera.
    public float vertExtent;
    public float horzExtent;

    // An offset to the map if necessary.
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
