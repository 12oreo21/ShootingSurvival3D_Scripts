using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;


public class EnemyController : MonoBehaviour
{
    GameObject Player;
    GameObject Enemy;

    float disBetweenPlayerAndEnemy;
    float currnetDot;
    float searchableAngle;
    Vector3 enemyToPlayerDir;

    Animator animator;

    GameObject Canvas;

    Transform PSButton;
    LiveBulletOnButton lbbScript;

    GameObject Camera;
    LiveBullet lbScript;

    bool viewableRange;
    bool hearableRange;
    
    Vector3 playerCurrentPos;
    Vector3 enemyCurrentPos;

    public bool foundPlayer;
    bool warningPlayer;

    [SerializeField]
    GameObject MuzzleOfGun;

    [SerializeField]
    GameObject BulletEffect_Pistol_Enemy;

    [SerializeField]
    GameObject BulletPassingSound;
    [SerializeField]
    GameObject PistolFiringSoundEnemy;

    private NavMeshAgent agent;

    AudioSource[] audioSources;

    GoalObjectController gocScript;


    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Enemy = transform.root.gameObject;

        animator = Enemy.GetComponent<Animator>();

        Canvas = GameObject.FindGameObjectWithTag("Canvas");

        PSButton = Canvas.transform.Find("PistolShootButton").Find("PSButton");
        lbbScript = PSButton.GetComponent<LiveBulletOnButton>();

        Camera = GameObject.FindGameObjectWithTag("MainCamera");
        lbScript = Camera.GetComponent<LiveBullet>();

        agent = Enemy.GetComponent<NavMeshAgent>();

        foundPlayer = false;
        warningPlayer = false;

        SetDestinationRandomly();

        randomCountD1 = Random.Range(2f, 4f); //Battle時に移動する時間
        randomCountD2 = Random.Range(2f, 5f); //Battle時にアイドルする時間

        randomCountF1 = Random.Range(2f, 4f); //発砲時のインターバル
        randomCountF2 = Random.Range(1f, 2.5f); //発砲時間

        audioSources = transform.GetComponents<AudioSource>();

        gocScript = GameObject.FindGameObjectWithTag("Goal").GetComponent<GoalObjectController>();
    }


    public void StartSoundOfWalkRun(int index)
    {
        if (audioSources[index].isPlaying)
        {
            return;
        }
        else if (!audioSources[index].isPlaying)
        {
            audioSources[index].Play();
        }
    }

    public void StopSoundOfWalkRun(int index)
    {
        if (audioSources[index].isPlaying)
        {
            audioSources[index].Stop();
        }
        else if (!audioSources[index].isPlaying)
        {
            return;
        }
    }



    public void SetCurrentRange()
    {
        if (disBetweenPlayerAndEnemy <= 20f)
        {
            viewableRange = true;
            hearableRange = false;
        }
        else if (disBetweenPlayerAndEnemy > 20f && disBetweenPlayerAndEnemy <= 30f)
        {
            viewableRange = false;
            hearableRange = true;
        }
        else
        {
            viewableRange = false;
            hearableRange = false;
        }
    }


    Vector3 origin;
    RaycastHit hitInfoDestination;
    int layerMaskA = ~(1 << 9) & ~(1 << 8) & ~(1 << 2);
    bool raycastDestination;
    public void SetDestinationRandomly()
    {
        if (warningPlayer == false && foundPlayer == false)
        {
            if (!Enemy.name.StartsWith("Enemy_Wherever"))
            {
                origin = Enemy.transform.position + new Vector3(Random.Range(-20f, 20f), 60f, Random.Range(-20f, 20f));
            }
            else if (Enemy.name.StartsWith("Enemy_Wherever"))
            {
                origin = Enemy.transform.position + new Vector3(Random.Range(-40f, 40f), 60f, Random.Range(-40f, 40f));
            }

            raycastDestination = Physics.Raycast(origin, Vector3.down, out hitInfoDestination, 100f, layerMaskA);
            if (raycastDestination)
            {
                agent.destination = hitInfoDestination.point;
                if (agent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    agent.ResetPath();
                }
            }
        }
        else if (warningPlayer && foundPlayer == false)
        {
            origin = Enemy.transform.position + new Vector3(Random.Range(-10f, 10f), 60f, Random.Range(-10f, 10f));
            raycastDestination = Physics.Raycast(origin, Vector3.down, out hitInfoDestination, 100f, layerMaskA);
            if (raycastDestination)
            {
                agent.destination = hitInfoDestination.point;
                if (agent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    agent.ResetPath();
                }
            }
        }
    }


    public void SetDestinationToPlayer()
    {
        origin = new Vector3(Player.transform.position.x, 60f, Player.transform.position.z);
        raycastDestination = Physics.Raycast(origin, Vector3.down, out hitInfoDestination, 100f, layerMaskA);
        if (raycastDestination)
        {
            agent.destination = hitInfoDestination.point;
            if (agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                agent.ResetPath();
            }
        }
        
    }

    

    //見回りをする。
    float counttimeA;
    public void SearchRandomly() 
    {
        if(foundPlayer == false && warningPlayer == false)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                StopSoundOfWalkRun(0);
                StopSoundOfWalkRun(1);
                animator.SetBool("NeutralIdle", true); //Animator "NeutralIdle" をtrue
                animator.SetBool("Walk", false); 
                animator.SetBool("WalkLeftStrafe", false);
                animator.SetBool("WalkBackward", false);
                animator.SetBool("WalkRightStrafe", false);
                animator.SetBool("Run", false);
                animator.SetBool("RunLeftStrafe", false);
                animator.SetBool("RunBackward", false);
                animator.SetBool("RunRightStrafe", false);
                animator.SetBool("PistolIdle", false);
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
                counttimeA += Time.deltaTime;
                if (counttimeA >= 5f)
                {
                    SetDestinationRandomly();
                    counttimeA = 0f;
                }
            }
            else if (agent.remainingDistance > agent.stoppingDistance || agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                StartSoundOfWalkRun(0);
                StopSoundOfWalkRun(1);
                agent.speed = 2.5f;
                animator.SetBool("NeutralIdle", false);
                animator.SetBool("Walk", true); //Animator "Walk" をtrue
                animator.SetBool("WalkLeftStrafe", false);
                animator.SetBool("WalkBackward", false);
                animator.SetBool("WalkRightStrafe", false);
                animator.SetBool("Run", false);
                animator.SetBool("RunLeftStrafe", false);
                animator.SetBool("RunBackward", false);
                animator.SetBool("RunRightStrafe", false);
                animator.SetBool("PistolIdle", false);
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
            }
        }
        else
        {
            counttimeA = 0f;
        }
    }


    //Player未発見 && Playerの銃声が聞こえている && 未警戒時、その方向に移動する。
    public void ForwardSoundOfGun()
    {
        if (foundPlayer == false && lbbScript.liveBulletShoot && lbScript.bulletRemain)
        {
            if (hearableRange)
            {
                StopSoundOfWalkRun(0);
                StartSoundOfWalkRun(1);
                warningPlayer = true;
                SetDestinationToPlayer();
                agent.speed = 5f;
                animator.SetBool("NeutralIdle", false); 
                animator.SetBool("Walk", false);
                animator.SetBool("WalkLeftStrafe", false);
                animator.SetBool("WalkBackward", false);
                animator.SetBool("WalkRightStrafe", false);
                animator.SetBool("Run", true); //Animator "Run" をtrue
                animator.SetBool("RunLeftStrafe", false);
                animator.SetBool("RunBackward", false);
                animator.SetBool("RunRightStrafe", false);
                animator.SetBool("PistolIdle", false);
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
            }
            else if (viewableRange)
            {
                StartSoundOfWalkRun(0);
                StopSoundOfWalkRun(1);
                warningPlayer = true;
                SetDestinationToPlayer();
                agent.speed = 2f;
                animator.SetBool("NeutralIdle", false);
                animator.SetBool("Walk", false);
                animator.SetBool("WalkLeftStrafe", false);
                animator.SetBool("WalkBackward", false);
                animator.SetBool("WalkRightStrafe", false);
                animator.SetBool("Run", false); 
                animator.SetBool("RunLeftStrafe", false);
                animator.SetBool("RunBackward", false);
                animator.SetBool("RunRightStrafe", false);
                animator.SetBool("PistolIdle", false);
                animator.SetBool("PistolWalk", true); //Animator "PistolWalk" をtrue
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
            }
        }
    }


    //Player未発見 && Playerの銃声聞こえない && 警戒中 にあたりをランダムに操作
    float counttimeE;
    public void SearchRandomlyInWarning()
    {
        if(warningPlayer && foundPlayer == false)
        {
            counttimeE += Time.deltaTime;
            if(agent.remainingDistance <= agent.stoppingDistance || agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                StopSoundOfWalkRun(0);
                StopSoundOfWalkRun(1);
                animator.SetBool("NeutralIdle", false); 
                animator.SetBool("Walk", false);
                animator.SetBool("WalkLeftStrafe", false);
                animator.SetBool("WalkBackward", false);
                animator.SetBool("WalkRightStrafe", false);
                animator.SetBool("Run", false);
                animator.SetBool("RunLeftStrafe", false);
                animator.SetBool("RunBackward", false);
                animator.SetBool("RunRightStrafe", false);
                animator.SetBool("PistolIdle", true); //Animator "PistolIdle" をtrue
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
                counttimeE += Time.deltaTime;
                if (counttimeE >= 3f)
                {
                    SetDestinationRandomly();
                    counttimeE = 0f;
                }
            }
            else if (agent.remainingDistance > agent.stoppingDistance)
            {
                StartSoundOfWalkRun(0);
                StopSoundOfWalkRun(1);
                agent.speed = 1.5f;
                animator.SetBool("NeutralIdle", false);
                animator.SetBool("Walk", false); 
                animator.SetBool("WalkLeftStrafe", false);
                animator.SetBool("WalkBackward", false);
                animator.SetBool("WalkRightStrafe", false);
                animator.SetBool("Run", false);
                animator.SetBool("RunLeftStrafe", false);
                animator.SetBool("RunBackward", false);
                animator.SetBool("RunRightStrafe", false);
                animator.SetBool("PistolIdle", false);
                animator.SetBool("PistolWalk", true); //Animator "PistolWalk" をtrue
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
            }
        }
        else
        {
            counttimeE = 0f;
        }
    }


    //Playerへの警戒を止める(warningPlayer == false)
    float counttimeC;
    public void StopWarningPlayer()
    {
        if(foundPlayer == false)
        {
            counttimeC += Time.deltaTime;
            if (counttimeC >= 20f)
            {
                counttimeC = 0f;
                warningPlayer = false;
            }
        }
        else if(foundPlayer || (foundPlayer == false && lbbScript.liveBulletShoot && lbScript.bulletRemain && (hearableRange || viewableRange)))
        {
            counttimeC = 0f;
        }
    }






    //Playerを目視状態（foundPlayerをtrueにする）。Playerを見失うまでのカウントダウンをリセットする。
    RaycastHit hitInfoToFind;
    bool raycastToFind;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 enemyForward = Enemy.transform.forward;
            enemyToPlayerDir = (playerCurrentPos - enemyCurrentPos).normalized;
            currnetDot = Vector3.Dot(enemyForward, enemyToPlayerDir); //Enemyを起点にした時の、Player座標への方向ベクトルと、Enemyのforwardベクトルの内積
            searchableAngle = Mathf.Cos(75f * Mathf.Deg2Rad);  //θが45度の時のCosの値
            //raycastToFind用のベクトル
            Vector3 enemy2PlayerDir = ((playerCurrentPos + new Vector3(0, 1.0f, 0)) - (enemyCurrentPos + new Vector3(0, 1.6f, 0))).normalized;

            if (currnetDot >= searchableAngle)
            {
                raycastToFind = Physics.Raycast(enemyCurrentPos + new Vector3(0, 1.6f, 0), enemy2PlayerDir, out hitInfoToFind, 20f, layerMaskB);
                if (raycastToFind && hitInfoToFind.collider.tag == Player.tag)
                {
                    agent.ResetPath();
                    warningPlayer = true;
                    foundPlayer = true;
                    counttimeA = 0f;
                }
            }
        }
    }



    //PlayerがEnemyからのBattle中の視線raycastToFindWhenBattleから逃れた直後にPlayerを見失うカウントダウンを始める。
    

    float counttimeB;
    public void LostPlayer()
    {
        counttimeB += Time.deltaTime;
        if (counttimeB >= 15f)
        {
            counttimeB = 0f;
            foundPlayer = false;
        }
    }


    //交戦時
    bool raycastToFindWhenBattle;
    RaycastHit hitInfoToFindWhenBattle;
    int layerMaskB = ~(1 << 8) & ~(1 << 2);
    float counttimeD;
    Vector3 enemyMoveDirection;
    float enemyMoveAngle;
    Vector3 lookPos;
    public Vector3 bulletLookPos;
    Vector3 bulletLookPosExactly;
    bool doOnceBattleMoveSetting;
    float randomCountD1;
    float randomCountD2;
    float counttimeF;
    float randomCountF1;
    float randomCountF2;
    float counttimeG;
    float firingSpan = 0.3f;
    public void Battle()
    {
        if (foundPlayer)
        {
            raycastToFindWhenBattle = Physics.Raycast(enemyCurrentPos + new Vector3(0, 1.0f, 0), enemyToPlayerDir, out hitInfoToFindWhenBattle, 30f, layerMaskB);

            //視線で捉えているうちはPlayerの方を向く。
            if (raycastToFindWhenBattle && hitInfoToFindWhenBattle.collider.tag == Player.tag)
            {
                bulletLookPos = hitInfoToFindWhenBattle.transform.position;
                bulletLookPosExactly = hitInfoToFindWhenBattle.point;
                lookPos = new Vector3(hitInfoToFindWhenBattle.point.x, Enemy.transform.position.y, hitInfoToFindWhenBattle.point.z);
                Enemy.transform.LookAt(lookPos);
            }
            //視線で捉えられなくなったら、最後にPlayerを直接見ていた地点を向く。
            else if (!raycastToFindWhenBattle || (raycastToFindWhenBattle && hitInfoToFindWhenBattle.collider.tag != Player.tag))
            {
                LostPlayer();
                Enemy.transform.LookAt(lookPos);
            }
            //視線で捉えられていない時に銃声が聞こえたら、銃声地点を向く。
            else if ((!raycastToFindWhenBattle || (raycastToFindWhenBattle && hitInfoToFindWhenBattle.collider.tag != Player.tag))
                && lbbScript.liveBulletShoot && lbScript.bulletRemain && (viewableRange || hearableRange))
            {
                counttimeB = 0f;
                Enemy.transform.LookAt(new Vector3(Player.transform.position.x, Enemy.transform.position.y, Player.transform.position.z));
            }

            //発砲
            counttimeF += Time.deltaTime;
            if (counttimeF >= randomCountF1 && counttimeF <= randomCountF1 + randomCountF2)
            {
                counttimeG += Time.deltaTime;
                if (counttimeG >= firingSpan)
                {
                    var bulletPrefab = Instantiate(BulletEffect_Pistol_Enemy) as GameObject;
                    bulletPrefab.transform.position = new Vector3((transform.position.x + enemyToPlayerDir.x), (transform.position.y + 1.4f + enemyToPlayerDir.y), (transform.position.z + enemyToPlayerDir.z));
                    bulletPrefab.transform.LookAt(bulletLookPosExactly);

                    var bulletPassingSound = Instantiate(BulletPassingSound) as GameObject;
                    bulletPassingSound.transform.position = bulletLookPosExactly;

                    var pistolFiringSoundEnemy = Instantiate(PistolFiringSoundEnemy) as GameObject;
                    pistolFiringSoundEnemy.transform.position = MuzzleOfGun.transform.position;
                    
                    counttimeG = 0f;
                }
            }
            else if (counttimeF > randomCountF1 + randomCountF2)
            {
                counttimeF = 0f;
                counttimeG = 0f;
            }


            //Battle時の移動
            if (doOnceBattleMoveSetting)
            {
                enemyMoveDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                float enemyMoveRadian = Mathf.Atan2(enemyMoveDirection.z, enemyMoveDirection.x);
                enemyMoveAngle = Mathf.Rad2Deg * enemyMoveRadian;
                doOnceBattleMoveSetting = false;
            }
            else if (!doOnceBattleMoveSetting)
            {
                counttimeD += Time.deltaTime;
                if (counttimeD < randomCountD1)
                {
                    StartSoundOfWalkRun(0);
                    StopSoundOfWalkRun(1);
                    Enemy.transform.Translate(enemyMoveDirection * 2f * Time.deltaTime, Enemy.transform);
                    if (enemyMoveAngle >= 45f && enemyMoveAngle < 135f)
                    {
                        animator.SetBool("NeutralIdle", false);
                        animator.SetBool("Walk", false);
                        animator.SetBool("Run", false);
                        animator.SetBool("PistolIdle", false);
                        animator.SetBool("PistolWalk", true); //Animator "PistolWalk" をtrue
                        animator.SetBool("PistolLeftStrafe", false);
                        animator.SetBool("PistolWalkBackward", false);
                        animator.SetBool("PistolRightStrafe", false);
                        animator.SetBool("PistolRun", false);
                        animator.SetBool("PistolRunBackward", false);

                    }
                    else if ((enemyMoveAngle >= 135f && enemyMoveAngle <= 180f) || (enemyMoveAngle < -135f && enemyMoveAngle > -180f))
                    {
                        animator.SetBool("NeutralIdle", false);
                        animator.SetBool("Walk", false);
                        animator.SetBool("Run", false);
                        animator.SetBool("PistolIdle", false);
                        animator.SetBool("PistolWalk", false);
                        animator.SetBool("PistolLeftStrafe", true); //Animator "PistolLeftStrafe" をtrue
                        animator.SetBool("PistolWalkBackward", false);
                        animator.SetBool("PistolRightStrafe", false);
                        animator.SetBool("PistolRun", false);
                        animator.SetBool("PistolRunBackward", false);
                    }
                    else if (enemyMoveAngle >= -135f && enemyMoveAngle < -45f)
                    {
                        animator.SetBool("NeutralIdle", false);
                        animator.SetBool("Walk", false);
                        animator.SetBool("Run", false);
                        animator.SetBool("PistolIdle", false);
                        animator.SetBool("PistolWalk", false);
                        animator.SetBool("PistolLeftStrafe", false);
                        animator.SetBool("PistolWalkBackward", true); //Animator "PistolWalkBackward" をtrue
                        animator.SetBool("PistolRightStrafe", false);
                        animator.SetBool("PistolRun", false);
                        animator.SetBool("PistolRunBackward", false);
                    }
                    else if (enemyMoveAngle >= -45f && enemyMoveAngle < 45f)
                    {
                        animator.SetBool("NeutralIdle", false);
                        animator.SetBool("Walk", false);
                        animator.SetBool("Run", false);
                        animator.SetBool("PistolIdle", false);
                        animator.SetBool("PistolWalk", false);
                        animator.SetBool("PistolLeftStrafe", false);
                        animator.SetBool("PistolWalkBackward", false);
                        animator.SetBool("PistolRightStrafe", true); //Animator "PistolRightStrafe" をtrue
                        animator.SetBool("PistolRun", false);
                        animator.SetBool("PistolRunBackward", false);
                    }
                }
                else if (counttimeD >= randomCountD1 && counttimeD <= randomCountD1 + randomCountD2)
                {
                    StopSoundOfWalkRun(0);
                    StopSoundOfWalkRun(1);
                    Enemy.transform.Translate(new Vector3(0, 0, 0), Enemy.transform);
                    animator.SetBool("NeutralIdle", false);
                    animator.SetBool("Walk", false);
                    animator.SetBool("WalkLeftStrafe", false);
                    animator.SetBool("WalkBackward", false);
                    animator.SetBool("WalkRightStrafe", false);
                    animator.SetBool("Run", false);
                    animator.SetBool("RunLeftStrafe", false);
                    animator.SetBool("RunBackward", false);
                    animator.SetBool("RunRightStrafe", false);
                    animator.SetBool("PistolIdle", true); //Animator "PistolIdle" をtrue
                    animator.SetBool("PistolWalk", false);
                    animator.SetBool("PistolLeftStrafe", false);
                    animator.SetBool("PistolWalkBackward", false);
                    animator.SetBool("PistolRightStrafe", false);
                    animator.SetBool("PistolRun", false);
                    animator.SetBool("PistolRunBackward", false);

                    
                }
                else if (counttimeD > randomCountD1 + randomCountD2)
                {
                    doOnceBattleMoveSetting = true;
                    counttimeD = 0f;
                }
            }
        }
        else
        {
            doOnceBattleMoveSetting = true;
            counttimeD = 0f;
            counttimeF = 0f;
            counttimeG = 0f;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (gocScript.bombNum == 5)
        {
            return;
        }

        playerCurrentPos = Player.transform.position;
        enemyCurrentPos = Enemy.transform.position;
        disBetweenPlayerAndEnemy = (playerCurrentPos - enemyCurrentPos).magnitude;
        
        SetCurrentRange();

        SearchRandomly();

        ForwardSoundOfGun();

        SearchRandomlyInWarning();

        StopWarningPlayer();

        Battle();
       
    }


    


}
