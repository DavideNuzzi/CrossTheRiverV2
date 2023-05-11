using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollision : MonoBehaviour
{
    public DataCollector dataCollector;

    public GameObject waterSplash;
    public AudioClip waterSplashSound;

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "RiverWater")
        {
            if (GameManager.Instance.resettingLevel == false)
            {
                dataCollector.AddSimplifiedPoint(transform.position, 2);

                GameManager.Instance.ResetLevel();
                //  LevelManager.Instance.playerFellWater = true;
                GameObject audio = new GameObject();
                AudioSource audioSource = audio.AddComponent<AudioSource>();
                audioSource.clip = waterSplashSound;
                audioSource.Play();
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "RiverWater")
        {
            GameObject.Instantiate(waterSplash, new Vector3(transform.position.x, 0.01f, transform.position.z), waterSplash.transform.rotation);
        }
    }

}
