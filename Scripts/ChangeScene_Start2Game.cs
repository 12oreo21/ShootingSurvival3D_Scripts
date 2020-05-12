using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ChangeScene_Start2Game : MonoBehaviour, IPointerDownHandler
{
    Image image;
    float alfa;
    AudioSource[] audioSources;
    float audiotime;

    bool changeScene_Start2Game = false;
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        changeScene_Start2Game = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        image = transform.GetComponent<Image>();
        audioSources = GameObject.FindGameObjectWithTag("MainCamera").transform.GetComponents<AudioSource>();
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


    // Update is called once per frame
    void Update()
    {
        if (changeScene_Start2Game)
        {
            alfa += Time.deltaTime;
            image.color = new Color(image.color.r, image.color.g, image.color.b, alfa);
            if (alfa >= 1f)
            {
                audioSources[0].volume -= 0.05f * Time.deltaTime;
                if (audioSources[0].volume == 0f)
                {
                    audiotime += Time.deltaTime;
                    if (audiotime >= 0.5f)
                    {
                        SceneManager.LoadScene("GameScene_TPS");
                    }
                    AudioPlayOneShot(1);
                    audioSources[0].Stop();
                }
            }
        }
    }
}
