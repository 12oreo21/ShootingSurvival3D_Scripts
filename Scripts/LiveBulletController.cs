using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveBulletController : MonoBehaviour
{
    [SerializeField]
    private GameObject HitEffect;
    [SerializeField]
    private GameObject HitSomethingEffect;

    EnemyHP ehpScript;
    float bulletDamage = 2f;

    GameObject MainCamera;
    LiveBullet lbScript;

    Rigidbody rigidBody;

    private Vector3 bulletDirection;
    private Vector3 BulletDirection
    {
        get { return bulletDirection; }
        set { bulletDirection = value; }
    }
    float bulletSpeed = 100000f;
    Vector3 initialPos;
    private bool earlyHitRaycast;
    RaycastHit earlyHitInfo;
    float earlyHitRange = 10.0f;
    int layerMask = ~(1 << 9) & ~(1 << 2);//layer"Player"と"Ignoreraycast"以外に当たる（"Playerには当たらない"）
    // Start is called before the first frame update
    void Start()
    {
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        lbScript = MainCamera.transform.GetComponent<LiveBullet>();

        rigidBody = gameObject.transform.GetComponent<Rigidbody>();

        initialPos = transform.position;

        if (lbScript.bulletRaycast)
        {
            BulletDirection = (lbScript.bulletHitInfo.point - transform.position).normalized;
            
        }
        else if (!lbScript.bulletRaycast)
        {
            Vector3 endOfBulletRaycast = MainCamera.transform.position + (MainCamera.transform.forward * lbScript.bulletRange);
            BulletDirection = (endOfBulletRaycast - transform.position).normalized;
        }

        transform.LookAt(transform.position + BulletDirection);

        earlyHitRaycast = Physics.Raycast(transform.position, BulletDirection, out earlyHitInfo, earlyHitRange, layerMask);
    }

    
    // Update is called once per frame
    void Update()
    {
        rigidBody.AddForce(BulletDirection * (bulletSpeed * Time.deltaTime));

        float distance = (initialPos - transform.position).magnitude;
        if (distance >= lbScript.bulletRange)
        {
            Destroy(gameObject);
        }

        //衝突したら衝突用のパーティクル生成(OnTriggerEnterでは漏れるのでRaycastで実装)
        if (gameObject != null)
        {
            if (transform.GetChild(0).gameObject == null)
            {
                return;
            }
            earlyHitRaycast = Physics.Raycast(transform.GetChild(0).gameObject.transform.position, BulletDirection, out earlyHitInfo, earlyHitRange, layerMask);
            if (earlyHitRaycast)
            {

                if (earlyHitInfo.transform.CompareTag("Enemy"))
                {
                    var hitEffect = Instantiate(HitEffect) as GameObject;
                    hitEffect.transform.position = earlyHitInfo.point;
                    hitEffect.transform.rotation = Quaternion.FromToRotation(hitEffect.transform.forward, earlyHitInfo.normal);

                    ehpScript = earlyHitInfo.transform.GetComponent<EnemyHP>();
                    ehpScript.enemyHP -= bulletDamage;

                    if (gameObject == null)
                    {
                        return;
                    }
                    Destroy(gameObject);
                }
                else if(!earlyHitInfo.transform.CompareTag("Enemy"))
                {
                    var hitSomethingEffect = Instantiate(HitSomethingEffect) as GameObject;
                    hitSomethingEffect.transform.position = earlyHitInfo.point - (BulletDirection * 0.02f);
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
