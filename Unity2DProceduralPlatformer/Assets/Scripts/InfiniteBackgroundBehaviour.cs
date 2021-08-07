using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBackgroundBehaviour : MonoBehaviour
{
	// Reference to the two Background objects.
    public Transform background1, background2;

	// When true, the current Background being used primarily is background1
    private bool isBG1 = true;
	// Reference to the Camera
    public Transform cam;
	// The horizontal offset that movement has reached.
    private float curWidth;
	// The width of each individual background element.
    public float elementWidth;

    private void Start()
    {
		// Calculate the width between backgrounds
        elementWidth = Mathf.Abs(background1.position.x - background2.position.x);
		// Initialise the next transition point
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
