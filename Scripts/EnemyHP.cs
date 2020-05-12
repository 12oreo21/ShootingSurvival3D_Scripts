using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyHP : MonoBehaviour
{
    public float enemyHP;

    NavMeshAgent navMeshAgent;
    EnemySpine esScript;
    Animator animator;
    CapsuleCollider capsuleCollider;
    GameObject SearchPlayer;

    [SerializeField]
    private BoxCollider HipsCollider;
    [SerializeField]
    private Rigidbody HipsRigidbody;
    [SerializeField]
    private CapsuleCollider LeftUpLegCollider;
    [SerializeField]
    private Rigidbody LeftUpLegRigidbody;
    [SerializeField]
    private CapsuleCollider LeftLegCollider;
    [SerializeField]
    private Rigidbody LeftLegRigidbody;
    [SerializeField]
    private CapsuleCollider RightUpLegCollider;
    [SerializeField]
    private Rigidbody RightUpLegRigidbody;
    [SerializeField]
    private CapsuleCollider RightLegCollider;
    [SerializeField]
    private Rigidbody RightLegRigidbody;
    [SerializeField]
    private BoxCollider SpineCollider;
    [SerializeField]
    private Rigidbody SpineRigidbody;
    [SerializeField]
    private CapsuleCollider LeftArmCollider;
    [SerializeField]
    private Rigidbody LeftArmRigidbody;
    [SerializeField]
    private CapsuleCollider LeftForeArmCollider;
    [SerializeField]
    private Rigidbody LeftForeArmRigidbody;
    [SerializeField]
    private SphereCollider HeadCollider;
    [SerializeField]
    private Rigidbody HeadRigidbody;
    [SerializeField]
    private CapsuleCollider RightArmCollider;
    [SerializeField]
    private Rigidbody RightArmRigidbody;
    [SerializeField]
    private CapsuleCollider RightForeArmCollider;
    [SerializeField]
    private Rigidbody RightForeArmRigidbody;


    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = transform.GetComponent<NavMeshAgent>();
        esScript = transform.GetComponent<EnemySpine>();
        animator = transform.GetComponent<Animator>();
        capsuleCollider = transform.GetComponent<CapsuleCollider>();
        SearchPlayer = transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyHP <= 0f)
        {
            esScript.enabled = false;
            animator.enabled = false;
            SearchPlayer.SetActive(false);
            navMeshAgent.enabled = false;

            HipsCollider.enabled = true;
            HipsRigidbody.isKinematic = false;
            LeftUpLegCollider.enabled = true;
            LeftUpLegRigidbody.isKinematic = false;
            LeftLegCollider.enabled = true;
            LeftLegRigidbody.isKinematic = false;
            RightUpLegCollider.enabled = true;
            RightUpLegRigidbody.isKinematic = false;
            RightLegCollider.enabled = true;
            RightLegRigidbody.isKinematic = false;
            SpineCollider.enabled = true;
            SpineRigidbody.isKinematic = false;
            LeftArmCollider.enabled = true;
            LeftArmRigidbody.isKinematic = false;
            LeftForeArmCollider.enabled = true;
            LeftForeArmRigidbody.isKinematic = false;
            HeadCollider.enabled = true;
            HeadRigidbody.isKinematic = false;
            RightArmCollider.enabled = true;
            RightArmRigidbody.isKinematic = false;
            RightForeArmCollider.enabled = true;
            RightForeArmRigidbody.isKinematic = false;

            capsuleCollider.enabled = false;
            Destroy(gameObject, 30f);
        }
    }
}
