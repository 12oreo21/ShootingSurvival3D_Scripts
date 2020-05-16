using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PlayerMove : MonoBehaviour
{
    PlayerHP phpScript;

    Quaternion curLocalRotation;

    GameObject Canvas;

    Transform Stick; //uGUI>Joystick>Stick
    Vector2 stickInitialPos; //uGUI>Joystick>Stickの開始時のローカル座標
    Vector2 stickCurrentPos; //uGUI>Joystick>Stickの現在のローカル座標
    float stickDis; //uGUI>Joystick>Stickの開始時のローカル座標から現在のローカル座標までの距離
    float stickRadian; //uGUI>Joystick>Stickの開始時と現在のローカル座標から求められる角度 = 孤度法（ラジアン）
    float stickAngle; //uGUI>Joystick>Stickの開始時と現在のローカル座標から求められる角度 = θ
    Quaternion stickRotation; //stickAngleをRotationに変換した値

    Transform JButton; //uGUI>JumpButton>JButton
    JumpButtonPress jbpScript; //"JumpButtonPress.cs"

    Transform CButton; //uGUI>CrouchButton>CButton
    CrouchButtonPress cbpScript; //"CrouchButtonPress.cs"

    Transform Panel_PlayerRotation; //uGUI>Panel_PlayerRotation
    PanelDrag pdScript; //"PanelDrag.cs"

    Transform PistolReadySlider; //uGUI>PistolShootButton>PistolReadySlider
    Slider prsSlider; //uGUI>PistolShootButton>PistolReadySliderのSliderコンポーネント

    Animator animator;

    AudioSource[] audioSources;

    //このrayは地面からの距離を図るため（接地 or 空中の判断）
    // rayの開始点
    public Transform rayPosition;
    //　rayの距離
    public float rayRange;

    [SerializeField]
    private Transform collisionRayPosition;
    [SerializeField]
    private float collisionRayRange;
    int layerMask = ~(1 << 9) & ~(1 << 2); //layer"Player"と"IgnoreRaycast"以外に当たる（"Playerには当たらない"）

    Vector3 jumpForce = new Vector3(0f, 200f, 0f);

    bool Idle; //Idle状態
    bool Walk; //Walk状態
    bool Run; //Run状態
    bool PistolReady; //Pistol状態

    GameObject PlayerAxis;
    Rigidbody paRigidbody;
    CapsuleCollider paCapsuleCollider;

    float translateX;
    float translateZ;
    float translateForward;

    GameObject Camera;

    float spineRotX = -3.63f;
    float spineRotY = -6.35f;
    float spineRotZ = -2.6f;

    // Start is called before the first frame update
    void Start()
    {
        phpScript = transform.GetComponent<PlayerHP>();

        Canvas = GameObject.FindGameObjectWithTag("Canvas");

        Stick = Canvas.transform.Find("Joystick").Find("Stick");
        stickInitialPos = Stick.localPosition;

        JButton = Canvas.transform.Find("JumpButton").Find("JButton");
        jbpScript = JButton.GetComponent<JumpButtonPress>();

        CButton = Canvas.transform.Find("CrouchButton").Find("CButton");
        cbpScript = CButton.GetComponent<CrouchButtonPress>();

        Panel_PlayerRotation = Canvas.transform.Find("Panel_PlayerRotation");
        pdScript = Panel_PlayerRotation.GetComponent<PanelDrag>();

        PistolReadySlider = Canvas.transform.Find("PistolShootButton").Find("PistolReadySlider");
        prsSlider = PistolReadySlider.GetComponent<Slider>();

        animator = transform.GetComponent<Animator>();

        Idle = true;
        Walk = false;
        Run = false;

        PlayerAxis = transform.parent.gameObject;
        paRigidbody = PlayerAxis.GetComponent<Rigidbody>();
        paCapsuleCollider = PlayerAxis.GetComponent<CapsuleCollider>();

        Camera = GameObject.FindGameObjectWithTag("MainCamera");

        audioSources = transform.GetComponents<AudioSource>();
    }


    public void StartSoundOfWalkRun(int index, float volume, float pitch)
    {
        if (audioSources[index].isPlaying)
        {
            return;
        }
        else if (!audioSources[index].isPlaying)
        {
            audioSources[index].volume = volume;
            audioSources[index].pitch = pitch;
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



    public void NeutralIdleMove()
    {
        if(stickDis <= 10f)
        {
            StopSoundOfWalkRun(0);
            StopSoundOfWalkRun(1);
            PlayerAxis.transform.Translate(0, 0, 0);
            paCapsuleCollider.height = 1.8f;
            paCapsuleCollider.center = new Vector3(0f, 0.9f, 0f);
            paCapsuleCollider.radius = 0.25f;
            animator.SetBool("NeutralIdle", true); //Animator "NeutrallIdle" をtrue
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
            animator.SetBool("CrouchPose", false);
            animator.SetBool("CrouchWalk", false);
            animator.SetBool("CrouchPistolPose", false);
            animator.SetBool("CrouchPistolWalk", false);
        }
    }



    //AnimationのJumpとJumpForwardにEventとして追加
    //その他のAnimation再生中にJumpの入力を受け付けないため / Jumpの再生中にその他のAnimationの入力を受け付けないため、Idleのtrue/falseで設定
    //Jump中はIdle==falseで、Jumpが終了時にIdle==trueに戻す。 
    public void IdleTrueEvent()
    {
        Idle = true;
    }


    //JumpのAnimationにEvent追加でPlayerに上向きの力を
    public void JumpForceEvent()
    {
        paRigidbody.AddForce(jumpForce);
    }


    //JumpForwardのAnimationにEvent追加でPlayerをジャンプさせる
    public void JumpForwardForceEvent()
    {
        paRigidbody.AddForce(jumpForce);
    }

    bool jumpForwardDoOnce = true;
    public void JumpForwardDoOnceEvent()
    {
        jumpForwardDoOnce = true;
    }


    public void JumpMove()
    {
        StopSoundOfWalkRun(0);
        StopSoundOfWalkRun(1);
        Idle = false; //Jump AnimationにEvent追加で、Animation終了時にIdleTrue()でIdle = trueを実行
        animator.SetTrigger("Jump");
    }



    public void CrouchPoseMove()
    {
        if (stickDis <= 10f)
        {
            StopSoundOfWalkRun(0);
            StopSoundOfWalkRun(1);
            PlayerAxis.transform.Translate(0, 0, 0);
            paCapsuleCollider.height = 1.2f;
            paCapsuleCollider.center = new Vector3(0f, 0.6f, 0f);
            paCapsuleCollider.radius = 0.35f;
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
            animator.SetBool("PistolWalk", false);
            animator.SetBool("PistolLeftStrafe", false);
            animator.SetBool("PistolWalkBackward", false);
            animator.SetBool("PistolRightStrafe", false);
            animator.SetBool("PistolRun", false);
            animator.SetBool("PistolRunBackward", false);
            animator.SetBool("CrouchPose", true); //Animator "CrouchPose" をtrue
            animator.SetBool("CrouchWalk", false);
            animator.SetBool("CrouchPistolPose", false);
            animator.SetBool("CrouchPistolWalk", false);
        }
    }

    public void CrouchWalkMove()
    {
        if (stickDis > 10f)
        {
            StopSoundOfWalkRun(0);
            StartSoundOfWalkRun(1, 0.02f, 1.077f);
            PlayerAxis.transform.position += this.transform.forward * stickDis * 0.025f * translateForward * Time.deltaTime;
            paCapsuleCollider.height = 1.4f;
            paCapsuleCollider.center = new Vector3(0f, 0.7f, 0f);
            paCapsuleCollider.radius = 0.35f;
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
            animator.SetBool("PistolWalk", false);
            animator.SetBool("PistolLeftStrafe", false);
            animator.SetBool("PistolWalkBackward", false);
            animator.SetBool("PistolRightStrafe", false);
            animator.SetBool("PistolRun", false);
            animator.SetBool("PistolRunBackward", false);
            animator.SetBool("CrouchPose", false); 
            animator.SetBool("CrouchWalk", true); //Animator"CrouchWalk"をtrue
            animator.SetBool("CrouchPistolPose", false);
            animator.SetBool("CrouchPistolWalk", false); 
        }
    }


    public void CrouchPistolPoseMove()
    {
        if (stickDis <= 10f)
        {
            StopSoundOfWalkRun(0);
            StopSoundOfWalkRun(1);
            PlayerAxis.transform.Translate(0, 0, 0);
            paCapsuleCollider.height = 1.3f;
            paCapsuleCollider.center = new Vector3(0f, 0.65f, 0f);
            paCapsuleCollider.radius = 0.3f;
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
            animator.SetBool("PistolWalk", false);
            animator.SetBool("PistolLeftStrafe", false);
            animator.SetBool("PistolWalkBackward", false);
            animator.SetBool("PistolRightStrafe", false);
            animator.SetBool("PistolRun", false);
            animator.SetBool("PistolRunBackward", false);
            animator.SetBool("CrouchPose", false); 
            animator.SetBool("CrouchWalk", false);
            animator.SetBool("CrouchPistolPose", true); //Animator "CrouchPistolPose" をtrue
            animator.SetBool("CrouchPistolWalk", false);
        }
    }

    public void CrouchPistolWalkMove()
    {
        if (stickDis > 10f)
        {
            StopSoundOfWalkRun(0);
            StartSoundOfWalkRun(1, 0.02f, 1.077f);
            PlayerAxis.transform.Translate(new Vector3(translateX, Stick.transform.localPosition.z, translateZ) / stickDis * 1.5f * Time.deltaTime);
            paCapsuleCollider.height = 1.5f;
            paCapsuleCollider.center = new Vector3(0f, 0.75f, 0f);
            paCapsuleCollider.radius = 0.3f;
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
            animator.SetBool("PistolWalk", false);
            animator.SetBool("PistolLeftStrafe", false);
            animator.SetBool("PistolWalkBackward", false);
            animator.SetBool("PistolRightStrafe", false);
            animator.SetBool("PistolRun", false);
            animator.SetBool("PistolRunBackward", false);
            animator.SetBool("CrouchPose", false);
            animator.SetBool("CrouchWalk", false); 
            animator.SetBool("CrouchPistolPose", false);
            animator.SetBool("CrouchPistolWalk", true); //Animator"CrouchPistolWalk"をtrue
        }
    }



    public void WalkMove()
    {
        if(stickDis > 10f && stickDis < 60f)
        {
            StopSoundOfWalkRun(0);
            StartSoundOfWalkRun(1, 0.03f, 1.077f);
            Walk = true;
            PlayerAxis.transform.position += this.transform.forward * stickDis * 0.06f * translateForward * Time.deltaTime;
            paCapsuleCollider.height = 1.8f;
            paCapsuleCollider.center = new Vector3(0f, 0.9f, 0f);
            paCapsuleCollider.radius = 0.25f;

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
            animator.SetBool("CrouchPose", false);
            animator.SetBool("CrouchWalk", false);
            animator.SetBool("CrouchPistolPose", false);
            animator.SetBool("CrouchPistolWalk", false);

        }
        
    }



    public void RunMove()
    {
        if (stickDis >= 60f)
        {
            StartSoundOfWalkRun(0, 0.06f, 0.965f);
            StopSoundOfWalkRun(1);
            Run = true;
            PlayerAxis.transform.position += this.transform.forward * stickDis * 0.06f * translateForward * Time.deltaTime;
            paCapsuleCollider.height = 1.8f;
            paCapsuleCollider.center = new Vector3(0f, 0.9f, 0f);
            paCapsuleCollider.radius = 0.25f;

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
            animator.SetBool("CrouchPose", false);
            animator.SetBool("CrouchWalk", false);
            animator.SetBool("CrouchPistolPose", false);
            animator.SetBool("CrouchPistolWalk", false);

        }
            
    }



    public void JumpForwardInWalkMove()
    {
        if (jumpForwardDoOnce)
        {
            StopSoundOfWalkRun(0);
            StopSoundOfWalkRun(1);
            animator.SetTrigger("JumpForwardInWalk");
            jumpForwardDoOnce = false;
        }
            
    }



    public void JumpForwardInRunMove()
    {
        if (jumpForwardDoOnce)
        {
            StopSoundOfWalkRun(0);
            StopSoundOfWalkRun(1);
            animator.SetTrigger("JumpForwardInRun");
            jumpForwardDoOnce = false;
        }
    }






    public void PistolIdleMove()
    {
        if (stickDis <= 10f)
        {
            StopSoundOfWalkRun(0);
            StopSoundOfWalkRun(1);
            PlayerAxis.transform.Translate(0, 0, 0);
            paCapsuleCollider.height = 1.8f;
            paCapsuleCollider.center = new Vector3(0f, 0.9f, 0f);
            paCapsuleCollider.radius = 0.25f;
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
            animator.SetBool("CrouchPose", false);
            animator.SetBool("CrouchWalk", false);
            animator.SetBool("CrouchPistolPose", false);
            animator.SetBool("CrouchPistolWalk", false);
        }
    }



    //PistolJumpのAnimationにEvent追加でPlayerに上向きの力を
    public void PistolJumpForceEvent()
    {
        paRigidbody.AddForce(jumpForce);
    }


    //PistolJumpForwardのAnimationにEvent追加でPlayerをジャンプさせる
    public void PistolJumpForwardForceEvent()
    {
        paRigidbody.AddForce(jumpForce);
    }


    public void PistolJumpMove()
    {
        StopSoundOfWalkRun(0);
        StopSoundOfWalkRun(1);
        Idle = false; //Jump AnimationにEvent追加で、Animation終了時にIdleTrue()でIdle = trueを実行
        animator.SetTrigger("PistolJump");
    }



    public void PistolWalkMove()
    {
        if (stickDis > 10f && stickDis < 60f)
        {
            StopSoundOfWalkRun(0);
            StartSoundOfWalkRun(1, 0.03f, 1.077f);
            Walk = true;
            PlayerAxis.transform.Translate(new Vector3(translateX, Stick.transform.localPosition.z, translateZ) / stickDis * 2.5f * Time.deltaTime);
            paCapsuleCollider.height = 1.8f;
            paCapsuleCollider.center = new Vector3(0f, 0.9f, 0f);
            paCapsuleCollider.radius = 0.25f;

            if (stickAngle >= 45f && stickAngle < 135f) //Joystickを前方向に傾けているとき
            {
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
                animator.SetBool("CrouchPose", false);
                animator.SetBool("CrouchWalk", false);
                animator.SetBool("CrouchPistolPose", false);
                animator.SetBool("CrouchPistolWalk", false);

            }
            else if ((stickAngle >= 135f && stickAngle <= 180f) || (stickAngle < -135f && stickAngle > -180f)) //Joystickを左方向に傾けているとき
            {
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
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", true); //Animator "PistolLeftStrafe" をtrue
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
                animator.SetBool("CrouchPose", false);
                animator.SetBool("CrouchWalk", false);
                animator.SetBool("CrouchPistolPose", false);
                animator.SetBool("CrouchPistolWalk", false);
            }
            else if (stickAngle >= -135f && stickAngle < -45f) //Joystickを後方向に傾けているとき
            {
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
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", true); //Animator "PistolWalkBackward" をtrue
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
                animator.SetBool("CrouchPose", false);
                animator.SetBool("CrouchWalk", false);
                animator.SetBool("CrouchPistolPose", false);
                animator.SetBool("CrouchPistolWalk", false);
            }
            else if (stickAngle >= -45f && stickAngle < 45f) //Joystickを右方向に傾けているとき
            {
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
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", true); //Animator "PistolRightStrafe" をtrue
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
                animator.SetBool("CrouchPose", false);
                animator.SetBool("CrouchWalk", false);
                animator.SetBool("CrouchPistolPose", false);
                animator.SetBool("CrouchPistolWalk", false);
            }
        }

    }



    public void PistolRunMove()
    {
        if (stickDis >= 60f)
        {
            StartSoundOfWalkRun(0, 0.06f, 0.965f);
            StopSoundOfWalkRun(1);
            Run = true;
            paCapsuleCollider.height = 1.8f;
            paCapsuleCollider.center = new Vector3(0f, 0.9f, 0f);
            paCapsuleCollider.radius = 0.25f;

            if (stickAngle >= 45f && stickAngle < 135f) //Joystickを前方向に傾けているとき
            {
                PlayerAxis.transform.Translate(new Vector3(translateX, Stick.transform.localPosition.z, translateZ) / stickDis * 4f * Time.deltaTime);
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
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", true); //Animator "PistolRun" をtrue
                animator.SetBool("PistolRunBackward", false);
                animator.SetBool("CrouchPose", false);
                animator.SetBool("CrouchWalk", false);
                animator.SetBool("CrouchPistolPose", false);
                animator.SetBool("CrouchPistolWalk", false);
            }
            else if ((stickAngle >= 135f && stickAngle <= 180f) || (stickAngle < -135f && stickAngle > -180f)) //Joystickを左方向に傾けているとき
            {
                PlayerAxis.transform.Translate(new Vector3(translateX, Stick.transform.localPosition.z, translateZ) / stickDis * 3f * Time.deltaTime);
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
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", true); //Animator "PistolLeftStrafe" をtrue
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
                animator.SetBool("CrouchPose", false);
                animator.SetBool("CrouchWalk", false);
                animator.SetBool("CrouchPistolPose", false);
                animator.SetBool("CrouchPistolWalk", false);
            }
            else if (stickAngle >= -135f && stickAngle < -45f) //Joystickを後方向に傾けているとき
            {
                PlayerAxis.transform.Translate(new Vector3(translateX, Stick.transform.localPosition.z, translateZ) / stickDis * 3f * Time.deltaTime);
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
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", false);
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", true); //Animator "PistolRunBackward" をtrue
                animator.SetBool("CrouchPose", false);
                animator.SetBool("CrouchWalk", false);
                animator.SetBool("CrouchPistolPose", false);
                animator.SetBool("CrouchPistolWalk", false);
            }
            else if (stickAngle >= -45f && stickAngle < 45f) //Joystickを右方向に傾けているとき
            {
                PlayerAxis.transform.Translate(new Vector3(translateX, Stick.transform.localPosition.z, translateZ) / stickDis * 3f * Time.deltaTime);
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
                animator.SetBool("PistolWalk", false);
                animator.SetBool("PistolLeftStrafe", false);
                animator.SetBool("PistolWalkBackward", false);
                animator.SetBool("PistolRightStrafe", true); //Animator "PistolRightStrafe" をtrue
                animator.SetBool("PistolRun", false);
                animator.SetBool("PistolRunBackward", false);
                animator.SetBool("CrouchPose", false);
                animator.SetBool("CrouchWalk", false);
                animator.SetBool("CrouchPistolPose", false);
                animator.SetBool("CrouchPistolWalk", false);
            }
        }

    }



    public void PistolJumpForwardInWalkMove()
    {
        if (jumpForwardDoOnce)
        {
            StopSoundOfWalkRun(0);
            StopSoundOfWalkRun(1);
            animator.SetTrigger("PistolJumpForwardInWalk");
            jumpForwardDoOnce = false;
        }
            
    }


    public void PistolJumpForwardInRunMove()
    {
        if (jumpForwardDoOnce)
        {
            StopSoundOfWalkRun(0);
            StopSoundOfWalkRun(1);
            animator.SetTrigger("PistolJumpForwardInRun");
            jumpForwardDoOnce = false;
        }
            
    }





    
    // Update is called once per frame
    void Update()
    {
        if (phpScript.playerHP <= 0f)
        {
            return;
        }

        //PlayerとPlayerAxisのy軸positionがずれる問題。
        transform.localPosition = new Vector3(0f, 0f, 0f);

        //JoystickとPlayerの動きを連動させるための変数を用意
        //実際の動き連動の実行は、JoystickDrag.OnDragを確認
        stickCurrentPos = Stick.transform.localPosition;
        stickDis = (stickCurrentPos - stickInitialPos).magnitude;
        stickRadian = Mathf.Atan2(stickCurrentPos.y, stickCurrentPos.x); //uGUI>Joystick>Stickの現在座標から弧度（ラジアン）を算出
        stickAngle = stickRadian * Mathf.Rad2Deg; //孤度（ラジアン）を角度（θ）に変換
        stickRotation = Quaternion.AngleAxis((450f - stickAngle), Vector3.up); //(450f - stickAngle)は、uGUI>Joystick>Stick と Player の動きを直感的に操作するための補正 == uGUI>Joystick>StickのθはX軸から反時計回り・PlayerのθはZ軸から時計回り



        //uGUI>PistolShootButton>PistolReadySliderのSliderコンポーネント のValueによってPistolを構えるかどうか
        if (prsSlider.value == 0)
        {
            PistolReady = false;
        }
        else if(prsSlider.value == 1)
        {
            PistolReady = true;
        }



        //Playerの向く方向
        //Pistol構えている時はPanelのDragでPlayerAxisをRotation（カメラも定位置を追従）
        if (PistolReady == true)
        {
            PlayerAxis.transform.rotation *= Quaternion.AngleAxis(pdScript.panelDragVector.x * 0.2f * Time.deltaTime, Vector3.up);
        }
        //Pistol構えていない時はJoystick傾けた方向にPlayerのみRotation
        else if(PistolReady == false)
        {
            if(stickDis >= 0.1f)
            {
                curLocalRotation　= Quaternion.Euler(transform.localRotation.x, (450f - stickAngle) + Camera.transform.localEulerAngles.y, transform.localRotation.z);
                transform.localRotation = curLocalRotation;
            }
        }



        //Pistolを構えていない && Crouch状態じゃない時
        if (PistolReady == false && cbpScript.Crouch == false)
        { 
            //NeutralIdle(Joystickを傾けた入力がない時)
            if (stickDis <= 10f && (Idle == true && Walk == false && Run == false))
            {
                NeutralIdleMove();
            }


            //Jump 
            if (jbpScript.Jump && Physics.Linecast(rayPosition.position, (rayPosition.position - transform.up * rayRange)) && (Idle == true && Walk == false && Run == false))
            {
                JumpMove();
            }


            //Walk ⇄ NeutralIdle 
            if (stickDis > 10f && stickDis < 60f && Idle == true)
            {
                WalkMove();

                //Walk ⇄ JumpForward 
                if (stickDis > 10f && stickDis < 60f && stickAngle >= 45f && stickAngle < 135f && jbpScript.Jump && Physics.Linecast(rayPosition.position, (rayPosition.position - transform.up * rayRange)) && (Idle == true && Walk == true && Run == false))
                {
                    JumpForwardInWalkMove();
                }
            }
            else if (stickDis <= 10f && (Idle == true && Walk == true && Run == false))
            {
                NeutralIdleMove();
                Walk = false;
            }


            //Run ⇄ Walk　Run ⇄ NeutralIdle 
            if ((stickDis >= 60f && (Idle == true && Walk == true)) || (stickDis >= 60f && (Idle == true && Walk == false)))
            {
                Idle = true;
                Walk = true;
                RunMove();

                if (stickDis >= 60f && stickAngle >= 45f && stickAngle < 135f && jbpScript.Jump && Physics.Linecast(rayPosition.position, (rayPosition.position - transform.up * rayRange)) && (Idle == true && Walk == true && Run == true))
                {
                    JumpForwardInRunMove();
                }
            }
            else if (stickDis > 10f && stickDis < 60f && (Idle == true && Walk == true && Run == true))
            {
                Walk = false;
                Run = false;
                WalkMove();
                

            }
            else if (stickDis <= 10f && (Idle == true && Walk == true && Run == true))
            {
                Walk = false;
                Run = false;
                NeutralIdleMove();
                
            }
        }



        //Pistolを構えている時  && Crouch状態じゃない時
        if (PistolReady == true && cbpScript.Crouch == false)
        {

            //PistolIdle(Joystickを傾けた入力がない時)
            if(stickDis <= 10f && (Idle == true && Walk == false && Run == false))
            {
                PistolIdleMove();
            }


            //Jump 
            if (jbpScript.Jump && Physics.Linecast(rayPosition.position, (rayPosition.position - transform.up * rayRange)) && (Idle == true && Walk == false && Run == false))
            {
                PistolJumpMove();
            }



            //PistolWalk ⇄ PistollIdle 
            if (stickDis > 10f && stickDis < 60f && Idle == true)
            {
                PistolWalkMove();

                //PistolWalk ⇄ PistolJumpForward 
                if (stickDis > 10f && stickDis < 60f && stickAngle >= 45f && stickAngle < 135f && jbpScript.Jump && Physics.Linecast(rayPosition.position, (rayPosition.position - transform.up * rayRange)) && (Idle == true && Walk == true && Run == false))
                {
                    PistolJumpForwardInWalkMove();
                }
            }
            else if (stickDis <= 10f && (Idle == true && Walk == true && Run == false))
            {
                PistolIdleMove();
                Walk = false;
            }


            //Run ⇄ Walk　Run ⇄ NeutralIdle 
            if ((stickDis >= 60f && (Idle == true && Walk == true)) || (stickDis >= 60f && (Idle == true && Walk == false)))
            {
                Idle = true;
                Walk = true;
                PistolRunMove();

                if (stickDis >= 60f && stickAngle >= 45f && stickAngle < 135f && jbpScript.Jump && Physics.Linecast(rayPosition.position, (rayPosition.position - transform.up * rayRange)) && (Idle == true && Walk == true && Run == true))
                {
                    PistolJumpForwardInRunMove();
                }
            }
            else if (stickDis > 10f && stickDis < 60f && (Idle == true && Walk == true && Run == true))
            {
                Walk = false;
                Run = false;
                PistolWalkMove();
            }
            else if (stickDis <= 10f && (Idle == true && Walk == true && Run == true))
            {
                Walk = false;
                Run = false;
                PistolIdleMove();
            }
        }


        //Pistolを構えていない && Crouch状態の時
        if (PistolReady == false && cbpScript.Crouch)
        {
            if(Physics.Linecast(rayPosition.position, (rayPosition.position - transform.up * rayRange)))
            {
                if (stickDis <= 10f)
                {
                    CrouchPoseMove();
                }
                else if (stickDis > 10f)
                {
                    CrouchWalkMove();
                }
            }
        }

        //Pistolを構えている時 && Crouch状態の時
        if (PistolReady && cbpScript.Crouch)
        {
            if(Physics.Linecast(rayPosition.position, (rayPosition.position - transform.up * rayRange)))
            {
                if (stickDis <= 10f)
                {
                    CrouchPistolPoseMove();
                }
                else if (stickDis > 10f)
                {
                    CrouchPistolWalkMove();
                }
            }
        }



        //Playerの擦り抜け防止
        bool collisionRaycastForward = Physics.Raycast(collisionRayPosition.position, transform.forward, collisionRayRange, layerMask);
        bool collisionRaycastBackward = Physics.Raycast(collisionRayPosition.position, -transform.forward, collisionRayRange, layerMask);
        bool collisionRaycastRight = Physics.Raycast(collisionRayPosition.position, transform.right, collisionRayRange, layerMask);
        bool collisionRaycastLeft = Physics.Raycast(collisionRayPosition.position, -transform.right, collisionRayRange, layerMask);
        bool collisionRaycastForwardRight = Physics.Raycast(collisionRayPosition.position, ((transform.forward * 2f) + transform.right), collisionRayRange, layerMask);
        bool collisionRaycastForwardLeft = Physics.Raycast(collisionRayPosition.position, ((transform.forward * 2f) - transform.right), collisionRayRange, layerMask);
        if ((collisionRaycastForward && Stick.transform.localPosition.y >= 0f) || (collisionRaycastBackward && Stick.transform.localPosition.y <= 0f)) 
        {
            translateZ = 0f;
        }
        else if ((collisionRaycastForward && Stick.transform.localPosition.y <= 0f) || (collisionRaycastBackward && Stick.transform.localPosition.y >= 0f)
            || (!collisionRaycastForward && !collisionRaycastBackward))
        {
            translateZ = Stick.transform.localPosition.y;
        }

        if ((collisionRaycastRight && Stick.transform.localPosition.x >= 0f) || (collisionRaycastLeft && Stick.transform.localPosition.x <= 0f))
        {
            translateX = 0f;
        }
        else if ((collisionRaycastRight && Stick.transform.localPosition.x <= 0f) || (collisionRaycastLeft && Stick.transform.localPosition.x >= 0f)
            || (!collisionRaycastRight && !collisionRaycastLeft))
        {
            translateX = Stick.transform.localPosition.x;
        }

        
        if (collisionRaycastForward || collisionRaycastForwardRight || collisionRaycastForwardLeft)
        {
            translateForward = 0f;
        }
        else if(!collisionRaycastForward && !collisionRaycastForwardRight && !collisionRaycastForwardLeft)
        {
            translateForward = 1f;
        }
    }




    //CameraのlocalRotationとSpineのRotationを、Pistol時に連動
    private void OnAnimatorIK(int layerIndex)
    {
        if (prsSlider.value == 1)
        {
            if (Camera.transform.localEulerAngles.x >= 0f && Camera.transform.localEulerAngles.x <= 180f)
            {
                spineRotX = Camera.transform.localEulerAngles.x * 0.85f - 8.7f;
                spineRotY = Camera.transform.localEulerAngles.x * 0.15f - 6f;
                spineRotZ = Camera.transform.localEulerAngles.x * 0.61f - 6.13f;
            }
            else if (Camera.transform.localEulerAngles.x > 180f && Camera.transform.localEulerAngles.x <= 360f)
            {
                spineRotX = (Camera.transform.localEulerAngles.x - 360f) * 0.75f - 8.7f;
                spineRotY = (Camera.transform.localEulerAngles.x -360f) * -0.5f - 6f;
                spineRotZ = (Camera.transform.localEulerAngles.x - 360f) * 0.54f - 6.13f;
            }
            animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Euler(spineRotX, spineRotY, spineRotZ));
            
            
            if (pdScript.cameraDrag)
            {
                animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Euler(spineRotX, spineRotY, spineRotZ));
            }
        }
        else if (prsSlider.value == 0)
        {
            spineRotX = -3.63f;
            spineRotY = -6.35f;
            spineRotZ = -2.6f;
        }
    }
}
