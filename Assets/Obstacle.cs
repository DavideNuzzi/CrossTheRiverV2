using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private RagdollController ragdollController;
    private DataCollector dataCollector;
    private Animator animator;
    private Transform player;

    // Start is called before the first frame update
    void Awake()
    {
        ragdollController = GameObject.Find("PlayerArmature").GetComponent<RagdollController>();
        GameObject dC = GameObject.Find("DataCollector");
        if (dC != null) dataCollector = dC.GetComponent<DataCollector>();

        animator = GetComponent<Animator>();
        player = GameObject.Find("PlayerArmature").transform; 
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3(player.position.x, 0, player.position.z));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "ObstacleCollisionDetector")
        {
            animator.SetBool("Attack", true);

            if (ragdollController.isRagdoll == false)
            {
                StartCoroutine(PlayerDied());
                if (dataCollector != null) dataCollector.AddSimplifiedPoint(transform.position, 3);

            }
        }
    }

    IEnumerator PlayerDied()
    {
        yield return new WaitForSeconds(0.1f);
        ragdollController.Explode(transform.position);
        GameManager.Instance.levelRunning = false;
        yield return new WaitForSeconds(3f);
        ragdollController.StopRagdoll();
        GameManager.Instance.ResetLevel();


    }

    private void LateUpdate()
    {
        animator.SetBool("Attack", false);
    }
}
