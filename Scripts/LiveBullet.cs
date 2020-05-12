using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LiveBullet : MonoBehaviour
{
    GameObject Canvas;

    Transform Reticle;
    Image ReticleImage;

    Transform PistolShootButton;
    Transform PSButton;
    LiveBulletOnButton lbbScript;
    Transform PistolReadySlider;
    Slider slider;

    [SerializeField]
    GameObject MuzzleOfGun;

    Transform ReloadButton;
    Transform RButton;
    ReloadLiveBulletOnButton reloadlivebbScript;

    [SerializeField]
    GameObject BulletPrefab_Pistol;

    [SerializeField]
    GameObject PistolFiringSound;

    AudioSource[] audioSources;

    PlayerHP phpScript;

    // Start is called before the first frame update
    void Start()
    {
        Canvas = GameObject.FindGameObjectWithTag("Canvas");

        Reticle = Canvas.transform.Find("Reticle");
        ReticleImage = Reticle.GetComponent<Image>();

        PistolShootButton = Canvas.transform.Find("PistolShootButton");
        PSButton = PistolShootButton.Find("PSButton");
        lbbScript = PSButton.GetComponent<LiveBulletOnButton>();

        PistolReadySlider = PistolShootButton.Find("PistolReadySlider");
        slider = PistolReadySlider.GetComponent<Slider>();

        ReloadButton = Canvas.transform.Find("ReloadButton");
        RButton = ReloadButton.Find("RButton");
        reloadlivebbScript = RButton.GetComponent<ReloadLiveBulletOnButton>();

        bulletNum = bulletNumMax;

        bulletCounttime = bulletshootInterval;

        audioSources = transform.GetComponents<AudioSource>();

        phpScript = transform.parent.GetChild(0).GetComponent<PlayerHP>();
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

    bool doOncePistolReadySound;

    public float bulletRange;
    public bool bulletRaycast;
    public RaycastHit bulletHitInfo;
    float bulletCounttime;
    float bulletshootInterval = 0.5f;
    public int bulletNum;
    public int bulletNumMax;
    public bool bulletRemain;
    // Update is called once per frame
    void Update()
    {
        if (phpScript.playerHP <= 0f)
        {
            return;
        }

        if (slider.value == 1f)
        {
            if (doOncePistolReadySound)
            {
                AudioPlayOneShot(3);
                doOncePistolReadySound = false;
            }
            bulletRaycast = Physics.Raycast(transform.position, transform.forward, out bulletHitInfo, bulletRange);

            if (bulletRaycast)
            {
                if (bulletHitInfo.transform.tag == "Enemy")
                {
                    ReticleImage.color = new Color(255f, 0f, 0f, 200f) / 255f;
                }
                else if (bulletHitInfo.transform.tag != "Enemy")
                {
                    ReticleImage.color = new Color(255f, 0f, 0f, 50f) / 255f;
                }
            }
            else if (!bulletRaycast)
            {
                ReticleImage.color = new Color(255f, 0f, 0f, 50f) / 255f;
            }

            
            if (lbbScript.liveBulletShoot)
            {
                if (reloadlivebbScript.finishReload)
                {
                    if (bulletRemain)
                    {
                        bulletCounttime += Time.deltaTime;
                        if (bulletCounttime >= bulletshootInterval)
                        {
                            var bulletPrefab = Instantiate(BulletPrefab_Pistol) as GameObject;
                            bulletPrefab.transform.position = MuzzleOfGun.transform.position;
                            bulletNum -= 1;
                            bulletCounttime = 0f;

                            var pistolFiringSound = Instantiate(PistolFiringSound) as GameObject;
                            pistolFiringSound.transform.position = MuzzleOfGun.transform.position;
                        }
                    }
                    else if (!bulletRemain)
                    {
                        AudioPlayOneShot(1);
                        bulletCounttime = bulletshootInterval;
                    }
                }
                else if (!reloadlivebbScript.finishReload)
                {
                    bulletCounttime = bulletshootInterval;
                }
                
            }
            else if (!lbbScript.liveBulletShoot)
            {
                bulletCounttime = bulletshootInterval;
            }
        }
        else if (slider.value == 0f)
        {
            doOncePistolReadySound = true;
        }


        if(bulletNum <= 0)
        {
            bulletRemain = false;
        }
        else if (bulletNum > 0)
        {
            bulletRemain = true;
        }

        if (!reloadlivebbScript.finishReload)
        {
            AudioPlayOneShot(2);
        }

    }
}
