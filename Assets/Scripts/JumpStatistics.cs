using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpStatistics : MonoBehaviour
{
    public ThirdPersonController controller;
    bool grounded = false;
    bool jumping = false;
    Vector3 startPosition;
    Vector3 endPosition;
    float distance;


    // Update is called once per frame
    void Update()
    {
        grounded = controller.Grounded;

        if (!grounded && !jumping)
        {
            jumping = true;
            startPosition = controller.transform.position;
        }

        if (grounded && jumping)
        {
            jumping = false;
            endPosition = controller.transform.position;
            distance = (endPosition-startPosition).magnitude;
            Debug.Log(distance);
        }
    }
}
