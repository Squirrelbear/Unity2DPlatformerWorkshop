using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform player;

    public float minX, minY, maxX, maxY;

    // Start is called before the first frame update
    void Start()
    {
        GameObject sceneCamObj = Camera.main.gameObject;
        Camera mainCam = sceneCamObj.GetComponent<Camera>();
        Transform camPos = mainCam.transform;

        float vertExtent = mainCam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        minY = -vertExtent + camPos.position.z + 0.5f;
        maxY = vertExtent + camPos.position.z - 0.5f;
        minX = -horzExtent + 0.5f;
        maxX = horzExtent - 0.5f;

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Mathf.Clamp(player.position.x, 0, 2.56f * 8),
                                         Mathf.Clamp(player.position.y, 0, 2.56f * 8), -10);
    }
}
