using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGoal : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.collider.name);
        if (collision.collider.GetComponent<CharacterController>())
        {
            Debug.Log("True");
        }
    }



}
