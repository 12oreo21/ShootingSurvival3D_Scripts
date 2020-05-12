using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalObject : MonoBehaviour
{
    GoalObjectController gocScript;
    GameObject GOCanvas;
    Slider disposalSlider;
    Image disposalBackBar;
    Image disposalFillBar;
    GameObject MainCamera;
    AudioSource[] audioSources;
    Text gocText;


    // Start is called before the first frame update
    void Start()
    {
        gocScript = GameObject.FindGameObjectWithTag("Goal").GetComponent<GoalObjectController>();
        GOCanvas = transform.parent.GetChild(1).gameObject;
        disposalSlider = GOCanvas.transform.GetChild(0).GetComponent<Slider>();
        disposalBackBar = GOCanvas.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        disposalFillBar = GOCanvas.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>();
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        audioSources = transform.GetComponents<AudioSource>();
        gocText = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild(12).GetComponent<Text>();
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


    float disposalTime;
    public void OnTriggerStay(Collider other)
    {
        if (transform.parent.gameObject == null)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (disposalTime <= 10f)
            {
                disposalTime += Time.deltaTime;
                disposalBackBar.color = new Color(disposalBackBar.color.r, disposalBackBar.color.g, disposalBackBar.color.b, 100f / 255f);
                disposalFillBar.color = new Color(disposalFillBar.color.r, disposalFillBar.color.g, disposalFillBar.color.b, 1f);
                disposalSlider.value = disposalTime;
                GOCanvas.transform.LookAt(MainCamera.transform.position);
                AudioPlayOneShot(0);
            }
        }
    }


    public void OnTriggerExit(Collider other)
    {
        if (transform.parent.gameObject == null)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            disposalTime = 0f;
            disposalSlider.value = 0f;
            audioSources[0].Stop();
        }
    }


    bool minusBombNumOnce = true;
    float audioTime;
    bool disposal = false;
    // Update is called once per frame
    void Update()
    {
        if (transform.parent.gameObject == null)
        {
            return;
        }

        if (disposalTime >= 10f)
        {
            disposal = true;
        }

        if (disposal)
        {
            audioSources[0].Stop();
            if (minusBombNumOnce)
            {
                gocScript.bombNum += 1;
                gocText.text = gocScript.bombNum.ToString() + " / 5";
                minusBombNumOnce = false;
            }
            audioTime += Time.deltaTime;
            if (audioTime <= 1.593f)
            {
                AudioPlayOneShot(1);
            }
            else if (audioTime > 1.593f)
            {
                Destroy(transform.parent.gameObject);
            }
        }

        if (disposalTime == 0f)
        {
            disposalBackBar.color = new Color(disposalBackBar.color.r, disposalBackBar.color.g, disposalBackBar.color.b, 0f/255f);
            disposalFillBar.color = new Color(disposalFillBar.color.r, disposalFillBar.color.g, disposalFillBar.color.b, 0f/255f);
        }
    }
}
