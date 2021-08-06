using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBackgroundBehaviour : MonoBehaviour
{
    public Transform background1, background2;

    private bool isBG1 = true;
    public Transform cam;
    private float curWidth;
    public float elementWidth;

    private void Start()
    {
        elementWidth = Mathf.Abs(background1.position.x - background2.position.x);
        curWidth = elementWidth;
    }

    private void Update()
    {
        if (curWidth < cam.position.x)
        {
            if (isBG1)
                background1.localPosition = new Vector2(background1.localPosition.x + elementWidth * 2, 0);
            else
                background2.localPosition = new Vector2(background2.localPosition.x + elementWidth * 2, 0);

            curWidth += elementWidth;
            isBG1 = !isBG1;
        }
        if (curWidth > cam.position.x + elementWidth)
        {
            if (isBG1)
                background2.localPosition = new Vector2(background2.localPosition.x - elementWidth * 2, 0);
            else
                background1.localPosition = new Vector2(background1.localPosition.x - elementWidth * 2, 0);

            curWidth -= elementWidth;
            isBG1 = !isBG1;
        }
    }
}
