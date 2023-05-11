using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RagdollController : MonoBehaviour
{
    private Rigidbody[] bones;
    private CharacterController characterController;
    private PlayerInput input;
    private Animator animator;
    private ThirdPersonController thirdPersonController;
    private Collider waterCollider;
    private Collider platformCollider;
    private GameObject cameraRoot;

    public GameObject cameraRootBone;

    public bool isRagdoll = false;
    private Vector3 cameraRootDelta;

    // Start is called before the first frame update
    void Awake()
    {
        bones = GameObject.Find("Armature.001").GetComponentsInChildren<Rigidbody>();
        characterController = gameObject.GetComponent<CharacterController>();
        input = gameObject.GetComponent<PlayerInput>();
        animator = gameObject.GetComponent<Animator>();
        thirdPersonController = gameObject.GetComponent<ThirdPersonController>();
        waterCollider = GameObject.Find("WaterCollisionDetector").GetComponent<Collider>();
        platformCollider = GameObject.Find("PlatformCollisionDetector").GetComponent<Collider>();
        cameraRoot = GameObject.Find("PlayerCameraRoot");
        cameraRootDelta = cameraRoot.transform.localPosition;

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (isRagdoll)
        {
            cameraRoot.transform.position = cameraRootBone.transform.position;
        }
    }
    void Update()
    {
   


        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isRagdoll) StartRagdoll();
            else StopRagdoll();
        }
    }

    void StartRagdoll()
    {
        isRagdoll = true;

        // Attivo i rigidbody di tutte le ossa
        characterController.enabled = false;
        input.enabled = false;
        animator.enabled = false;
        thirdPersonController.enabled = false;

        for (int i = 0; i < bones.Length; i++)
        {
            Rigidbody boneRigid = bones[i];
            if (bones[i].name != "ObstacleCollisionDetector")
            {
                boneRigid.detectCollisions = true;
                boneRigid.velocity = Vector3.zero;
                boneRigid.isKinematic = false;
                bones[i].GetComponent<Collider>().enabled = true;
            }
        }

        if (waterCollider) waterCollider.enabled = false;
        if (platformCollider) platformCollider.enabled = false;

    }



    public void StopRagdoll()
    {
        StartCoroutine(StopRagdollRoutine());
        
    }

    public IEnumerator StopRagdollRoutine()
    {
        // Resetto il camera root e la posizione del personaggio
        characterController.transform.position = new Vector3(bones[0].transform.position.x, -1f, bones[0].transform.position.z);
        ResetCameraRoot();

        // Disattivo i rigidbody delle ossa e faccio tornare l'animator
        for (int i = 0; i < bones.Length; i++)
        {
            Rigidbody boneRigid = bones[i];
            if (bones[i].name != "ObstacleCollisionDetector")
            {

                boneRigid.detectCollisions = false;
                boneRigid.velocity = Vector3.zero;
                boneRigid.isKinematic = true;
                bones[i].GetComponent<Collider>().enabled = false;
            }
        }

        animator.enabled = true;

        // Eseguo l'animazione che si rialza e aspetto
        GameManager.Instance.characterAnimator.Play("GetUp");

        float t = 0;
        Vector3 p1 = characterController.transform.position;
        Vector3 p2 = p1 + new Vector3(0f, 0.5f, 0f);

        while(t < 2f)
        {
            characterController.transform.position = Vector3.Lerp(p1, p2, t / 2f);
            t += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        isRagdoll = false;

        // Ora riattivo i controlli e dico che non è più ragdoll
      
        characterController.enabled = true;
        input.enabled = true;
        thirdPersonController.enabled = true;
    
        if (waterCollider) waterCollider.enabled = true;
        if (platformCollider) platformCollider.enabled = true;
    }
    public void ResetCameraRoot()
    {
        cameraRoot.transform.localPosition = cameraRootDelta;
    }

    public void Explode(Vector3 position)
    {
        if (!isRagdoll) StartRagdoll();

        foreach (Rigidbody bone in bones)
        {
            bone.AddExplosionForce(2400f, position, 4f);
        }
    }
}
