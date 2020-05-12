using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class PlayerHP : MonoBehaviour
{
    public float playerHP = 10f;

    GameObject Canvas;

    Transform CButton; //uGUI>CrouchButton>CButton
    CrouchButtonPress cbpScript; //"CrouchButtonPress.cs"

    Transform DamageUIBlood;
    Image duiBImage;
    Transform DamageUIDark;
    Image duiDImage;
    Transform GameOverUINoise;
    Image gouiNImage;
    Transform GameOverUIText;
    Image gouiTImage;
    Image gcuiBImage;

    Transform PistolReadySlider; //uGUI>PistolShootButton>PistolReadySlider
    Slider prsSlider; //uGUI>PistolShootButton>PistolReadySliderのSliderコンポーネント

    [SerializeField]
    GameObject Player_Ragdoll;
    [SerializeField]
    GameObject Player_Crouch_Ragdoll;
    [SerializeField]
    GameObject Player_Pistol_Ragdoll;
    [SerializeField]
    GameObject Player_PistolCrouch_Ragdoll;

    GameObject Pistol;
    MeshRenderer pistolMesh;

    SkinnedMeshRenderer[] bodyPartsMesh;

    bool doOnceRagdoll = true;

    CapsuleCollider playerCollider;
    Rigidbody playerRigidbody;

    AudioSource[] audioSources;

    GoalObjectController gocScript;

    // Start is called before the first frame update
    void Start()
    { 
        Canvas = GameObject.FindGameObjectWithTag("Canvas");
        CButton = Canvas.transform.Find("CrouchButton").Find("CButton");
        cbpScript = CButton.GetComponent<CrouchButtonPress>();

        DamageUIBlood = Canvas.transform.Find("DamageUIBlood");
        duiBImage = DamageUIBlood.GetComponent<Image>();
        DamageUIDark = Canvas.transform.Find("DamageUIDark");
        duiDImage = DamageUIDark.GetComponent<Image>();

        GameOverUINoise = Canvas.transform.Find("GameOverUINoise");
        gouiNImage = GameOverUINoise.GetComponent<Image>();
        GameOverUIText = Canvas.transform.Find("GameOverUIText");
        gouiTImage = GameOverUIText.GetComponent<Image>();

        gcuiBImage = Canvas.transform.Find("GameClearUIBackground").GetComponent<Image>();

        PistolReadySlider = Canvas.transform.Find("PistolShootButton").Find("PistolReadySlider");
        prsSlider = PistolReadySlider.GetComponent<Slider>();

        Pistol = GameObject.FindGameObjectWithTag("Pistol");
        pistolMesh = Pistol.transform.GetComponent<MeshRenderer>();

        bodyPartsMesh = transform.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        playerCollider = transform.parent.GetComponent<CapsuleCollider>();
        playerRigidbody = transform.parent.GetComponent<Rigidbody>();

        audioSources = transform.GetComponents<AudioSource>();

        gocScript = GameObject.FindGameObjectWithTag("Goal").GetComponent<GoalObjectController>();
    }


    
    public void HPManagement()
    {
        //HPの自然回復
        if (playerHP > 7.9f)
        {
            playerHP = 8f;
        }
        else if(playerHP <= 7.9f && playerHP > 0f)
        {
            //秒数に合わせてHPが回復する
            if(cbpScript.Crouch == false)
            {
                playerHP += (Time.deltaTime * 0.1f);
            }
            //しゃがんでいる時は、秒数に合わせて1.5倍速でHPが回復する
            else if (cbpScript.Crouch)
            {
                playerHP += (Time.deltaTime * 0.2f);
            }
        }
    }



    //HPに比例してDamageUIの透過度を調整する。（HPが減るほど濃くなる）
    public void DamageUI()
    {
        if(playerHP >= 6f)
        {
            duiBImage.color = new Color(duiBImage.color.r, duiBImage.color.g, duiBImage.color.b, 0f / 255f);
            duiDImage.color = new Color(duiDImage.color.r, duiDImage.color.g, duiDImage.color.b, 0f / 255f);
        }
        else if(playerHP < 6f && playerHP >= 4f)
        {
            duiBImage.color = new Color(duiBImage.color.r, duiBImage.color.g, duiBImage.color.b, 70f / 255f);
            duiDImage.color = new Color(duiDImage.color.r, duiDImage.color.g, duiDImage.color.b, 80f / 255f);
        }
        else if(playerHP < 4f && playerHP >= 2f)
        {
            duiBImage.color = new Color(duiBImage.color.r, duiBImage.color.g, duiBImage.color.b, 140f / 255f);
            duiDImage.color = new Color(duiDImage.color.r, duiDImage.color.g, duiDImage.color.b, 200f / 255f);
        }
        else if(playerHP < 2f && playerHP > 0f)
        {
            duiBImage.color = new Color(duiBImage.color.r, duiBImage.color.g, duiBImage.color.b, 200f / 255f);
            duiDImage.color = new Color(duiDImage.color.r, duiDImage.color.g, duiDImage.color.b, 255f / 255f);
        }
        
    }


    public void AudioPlayOneShot(int index)
    {
        if (audioSources[index].isPlaying)
        {
            return;
        }
        else if (!audioSources[index].isPlaying)
        {
            audioSources[index].PlayOneShot(audioSources[index].clip);
        }
    }


    //HPが0になったらUIとAudio表示・再生、PlayerをRagdollに。
    int index = 0;
    float alfa = 0f;
    float alfaSpeed = 0.45f;
    bool audioOnce1 = true;
    bool audioOnce2 = true;
    float audioSpan1;
    float audioSpan2;
    float audioSpan3;
    public void GameOver()
    {
        //HPが0以下になった時
        if (playerHP <= 0f)
        {
            gouiNImage.color = new Color(gouiNImage.color.r, gouiNImage.color.g, gouiNImage.color.b, 200f / 255f);
            gouiTImage.color = new Color(gouiTImage.color.r, gouiTImage.color.g, gouiTImage.color.b, alfa);
            alfa += alfaSpeed * Time.deltaTime;
            playerCollider.enabled = false;
            playerRigidbody.isKinematic = true;
            if (audioOnce1)
            {
                audioSpan1 += Time.deltaTime;
                if (audioSpan1 >= 2.184f)
                {
                    audioOnce1 = false;
                }
                AudioPlayOneShot(3);
            }

            
            if (!audioOnce1 && audioOnce2)
            {
                audioSpan2 += Time.deltaTime;
                if (audioSpan2 >= 1.944f)
                {
                    gcuiBImage.color = new Color(gcuiBImage.color.r, gcuiBImage.color.g, gcuiBImage.color.b, 1f);
                    audioOnce2 = false;
                }
                AudioPlayOneShot(2);
            }

            if (!audioOnce2)
            {
                audioSpan3 += Time.deltaTime;
                if (audioSpan3 >= 0.6f && audioSpan3 < 1.0f)
                {
                    audioSources[4].Stop();
                }
                else if (audioSpan3 >= 1.0f)
                {
                    SceneManager.LoadScene("StartScene_TPS");
                    return;
                }
                
                if (audioSpan3 >= 0.1f && audioSpan3 < 0.6f)
                {
                    AudioPlayOneShot(4);
                }
            }


            if (doOnceRagdoll)
            {
                pistolMesh.enabled = false;
                while (index < bodyPartsMesh.Length)
                {
                    bodyPartsMesh[index].enabled = false;
                    index++;
                }

                if (prsSlider.value == 0 && cbpScript.Crouch == false)
                {
                    var Ragdoll = Instantiate(Player_Ragdoll) as GameObject;
                    Ragdoll.transform.position = transform.position;
                    Ragdoll.transform.rotation = transform.rotation;
                    doOnceRagdoll = false;
                }
                else if (prsSlider.value == 0 && cbpScript.Crouch == true)
                {
                    var Ragdoll = Instantiate(Player_Crouch_Ragdoll) as GameObject;
                    Ragdoll.transform.position = transform.position;
                    Ragdoll.transform.rotation = transform.rotation;
                    doOnceRagdoll = false;
                }
                else if (prsSlider.value == 1 && cbpScript.Crouch == false)
                {
                    var Ragdoll = Instantiate(Player_Pistol_Ragdoll) as GameObject;
                    Ragdoll.transform.position = transform.position;
                    Ragdoll.transform.rotation = transform.rotation;
                    doOnceRagdoll = false;
                }
                else if (prsSlider.value == 1 && cbpScript.Crouch == true)
                {
                    var Ragdoll = Instantiate(Player_PistolCrouch_Ragdoll) as GameObject;
                    Ragdoll.transform.position = transform.position;
                    Ragdoll.transform.rotation = transform.rotation;
                    doOnceRagdoll = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gocScript.bombNum == 5)
        {
            return;
        }

        HPManagement();

        DamageUI();

        GameOver();
    }

}
