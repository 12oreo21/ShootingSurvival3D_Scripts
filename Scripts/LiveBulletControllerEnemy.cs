using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveBulletControllerEnemy : MonoBehaviour
{
    [SerializeField]
    private GameObject HitEffect;
    [SerializeField]
    private GameObject HitSomethingEffect;

    Transform Player;
    PlayerHP phpScript;

    float bulletDamage = 2f;

    Rigidbody rigidBody;

    float bulletSpeed = 100000f;
    Vector3 initialPos;
    private bool earlyHitRaycast;
    RaycastHit earlyHitInfo;
    float earlyHitRange = 10.0f;

    Vector3 bulletDirection;
    float bulletRange = 50f;

    GoalObjectController gocScript;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0);
        phpScript = Player.GetComponent<PlayerHP>();

        rigidBody = transform.GetComponent<Rigidbody>();
        initialPos = transform.position;
        bulletDirection = transform.forward;
        earlyHitRaycast = Physics.Raycast(transform.position, bulletDirection, out earlyHitInfo, earlyHitRange);

        gocScript = GameObject.FindGameObjectWithTag("Goal").GetComponent<GoalObjectController>();
    }


    // Update is called once per frame
    void Update()
    {
        if (gocScript.bombNum == 5)
        {
            return;
        }

        rigidBody.AddForce(bulletDirection * bulletSpeed * Time.deltaTime);

        float distance = (initialPos - transform.position).magnitude;
        if (distance >= bulletRange)
        {
            if (gameObject == null)
            {
                return;
            }
            Destroy(gameObject);
        }

        //衝突したら衝突用のパーティクル生成(OnTriggerEnterでは漏れるのでRaycastで実装)
        if (gameObject != null)
        {
            if (transform.GetChild(0).gameObject == null)
            {
                return;
            }
            earlyHitRaycast = Physics.Raycast(transform.GetChild(0).position, bulletDirection, out earlyHitInfo, earlyHitRange);
            if (earlyHitRaycast)
            {

                if (earlyHitInfo.transform.CompareTag("Player"))
                {
                    var hitEffect = Instantiate(HitEffect) as GameObject;
                    hitEffect.transform.position = earlyHitInfo.point;
                    hitEffect.transform.rotation = Quaternion.FromToRotation(hitEffect.transform.forward, earlyHitInfo.normal);

                    phpScript.playerHP -= bulletDamage;


                    if (gameObject == null)
                    {
                        return;
                    }
                    Destroy(gameObject);
                }
                else if (!earlyHitInfo.transform.CompareTag("Player") || !earlyHitInfo.transform.CompareTag("Enemy"))
                {
                    var hitSomethingEffect = Instantiate(HitSomethingEffect) as GameObject;
                    hitSomethingEffect.transform.position = earlyHitInfo.point - (bulletDirection * 0.02f);
                    hitSomethingEffect.transform.rotation = Quaternion.FromToRotation(Vector3.up, earlyHitInfo.normal);
                    if (gameObject == null)
                    {
                        return;
                    }
                    Destroy(gameObject);
                }
            }
        }
    }
}
