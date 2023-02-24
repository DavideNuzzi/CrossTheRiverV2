using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollision : MonoBehaviour
{
    public DataCollector dataCollector;

    public GameObject waterSplash;
    public AudioClip waterSplashSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "RiverWater")
        {
            GameObject.Instantiate(waterSplash, new Vector3(transform.position.x, 0.01f, transform.position.z), waterSplash.transform.rotation);
            GameManager.Instance.ResetLevel();
          //  LevelManager.Instance.playerFellWater = true;
            GameObject audio = new GameObject();
            AudioSource audioSource = audio.AddComponent<AudioSource>();
            audioSource.clip = waterSplashSound;
            audioSource.Play();

            dataCollector.AddSimplifiedPoint(transform.position, 2);
        }
    }

}
