using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCollisionDetector : MonoBehaviour
{
    public DataCollector dataCollector;

    private void OnTriggerEnter(Collider other)
    {
        if (dataCollector != null)
        {
            if (other.tag == "Platform")
            {
                if (!dataCollector.CheckSamePoint(other.transform.position))
                {
                    Debug.Log("Collisione con piattaforma");
                    dataCollector.AddSimplifiedPoint(other.transform.position, 0);
                }

            }
            if (other.tag == "GoalPlatform")
            {
                if (!dataCollector.CheckSamePoint(other.transform.position))
                    dataCollector.AddSimplifiedPoint(other.transform.position, 1);
            }
        }
    }
}
