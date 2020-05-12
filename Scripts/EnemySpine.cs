using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpine : MonoBehaviour
{
    GameObject Player;

    GameObject SearchPlayer;
    EnemyController ecScript;

    GoalObjectController gocScript;
    PlayerHP phpScript;


    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        SearchPlayer = transform.GetChild(2).gameObject;
        ecScript = SearchPlayer.GetComponent<EnemyController>();

        animator = GetComponent<Animator>();

        gocScript = GameObject.FindGameObjectWithTag("Goal").GetComponent<GoalObjectController>();
        phpScript = Player.transform.GetChild(0).GetComponent<PlayerHP>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    Animator animator;
    float spineX;
    float spineY;
    float spineZ;
    float dot;
    Vector3 enemy2player;
    float cos;
    float acos;
    float angle;
    private void OnAnimatorIK(int layerIndex)
    {
        if (gocScript.bombNum == 5)
        {
            return;
        }

        if (phpScript.playerHP <= 0f)
        {
            return;
        }

        if (ecScript.foundPlayer)
        {
            enemy2player = (ecScript.bulletLookPos - transform.position).normalized;
            cos = Vector3.Dot(transform.forward, enemy2player);
            acos = Mathf.Acos(cos);
            angle = acos * 180 / 3;
            spineX = Mathf.Clamp(spineX, -45f, 45f);
            spineY = Mathf.Clamp(spineY, -2f, 2f);
            spineZ = Mathf.Clamp(spineZ, -45f, 45f);

            if (ecScript.bulletLookPos.y >= transform.position.y)
            {
                animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Euler(-angle, -6.35f, -angle));
            }
            else if (ecScript.bulletLookPos.y < transform.position.y)
            {
                animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Euler(angle, -6.35f, angle));
            }
            
        }

    }
}
