using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalSceneController : MonoBehaviour
{
    AudioSource[] audioSources;
    float audioTime;
    Text panelText;

    // Start is called before the first frame update
    void Start()
    {
        audioSources = transform.GetComponents<AudioSource>();
        panelText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
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
        audioTime += Time.deltaTime;
        if (audioTime >= 0.5f)
        {
            if (audioTime >= 0.6f && audioTime < 0.7f)
            {
                panelText.text = "M|";
            }
            else if (audioTime >= 0.7f && audioTime < 0.8f)
            {
                panelText.text = "Mi|";
            }
            else if (audioTime >= 0.8f && audioTime < 0.9f)
            {
                panelText.text = "Mis|";
            }
            else if (audioTime >= 0.9f && audioTime < 1.0f)
            {
                panelText.text = "Miss";
            }
            else if (audioTime >= 1.0f && audioTime < 1.1f)
            {
                panelText.text = "Missi|";
            }
            else if (audioTime >= 1.2f && audioTime < 1.3f)
            {
                panelText.text = "Missio|";
            }
            else if (audioTime >= 1.3f && audioTime < 1.4f)
            {
                panelText.text = "Mission|";
            }
            else if (audioTime >= 1.4f && audioTime < 1.5f)
            {
                panelText.text = "Mission |";
            }
            else if (audioTime >= 1.5f && audioTime < 1.6f)
            {
                panelText.text = "Mission ";
            }
            else if (audioTime >= 1.6f && audioTime < 1.7f)
            {
                panelText.text = "Mission A|";
            }
            else if (audioTime >= 1.7f && audioTime < 1.8f)
            {
                panelText.text = "Mission Ac|";
            }
            else if (audioTime >= 1.8f && audioTime < 1.9f)
            {
                panelText.text = "Mission Acc|";
            }
            else if (audioTime >= 1.9f && audioTime < 2.0f)
            {
                panelText.text = "Mission Acco";
            }
            else if (audioTime >= 2.0f && audioTime < 2.1f)
            {
                panelText.text = "Mission Accom|";
            }
            else if (audioTime >= 2.1f && audioTime < 2.2f)
            {
                panelText.text = "Mission Accomp|";
            }
            else if (audioTime >= 2.2f && audioTime < 2.3f)
            {
                panelText.text = "Mission Accompl|";
            }
            else if (audioTime >= 2.3f && audioTime < 2.4f)
            {
                panelText.text = "Mission Accompli";
            }
            else if (audioTime >= 2.4f && audioTime < 2.5f)
            {
                panelText.text = "Mission Accomplis|";
            }
            else if (audioTime >= 2.5f && audioTime < 2.6f)
            {
                panelText.text = "Mission Accomplish|";
            }
            else if (audioTime >= 2.6f && audioTime < 2.7f)
            {
                panelText.text = "Mission Accomplishe|";
            }
            else if (audioTime >= 2.7f && audioTime < 2.8f)
            {
                panelText.text = "Mission Accomplished";
            }
            else if (audioTime >= 2.8f && audioTime < 2.9f)
            {
                panelText.text = "Mission Accomplished.|";
                audioSources[1].Stop();
            }
            else if (audioTime >= 2.9f && audioTime < 3.0f)
            {
                panelText.text = "Mission Accomplished.|";
                audioSources[1].Stop();
            }
            else if (audioTime >= 3.0f && audioTime < 3.1f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 3.1f && audioTime < 3.2f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 3.2f && audioTime < 3.3f)
            {
                panelText.text = "Mission Accomplished.";
            }
            else if (audioTime >= 3.3f && audioTime < 3.4f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 3.4f && audioTime < 3.5f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 3.5f && audioTime < 3.6f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 3.6f && audioTime < 3.7f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 3.7f && audioTime < 3.8f)
            {
                panelText.text = "Mission Accomplished.";
            }
            else if (audioTime >= 3.8f && audioTime < 3.9f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 3.9f && audioTime < 4.0f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 4.0f && audioTime < 4.1f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 4.1f && audioTime < 4.2f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 4.2f && audioTime < 4.3f)
            {
                panelText.text = "Mission Accomplished.";
            }
            else if (audioTime >= 4.3f && audioTime < 4.4f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 4.4f && audioTime < 4.5f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 4.5f && audioTime < 4.6f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 4.6f && audioTime < 4.7f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 4.7f && audioTime < 4.8f)
            {
                panelText.text = "Mission Accomplished.";
            }
            else if (audioTime >= 4.8f && audioTime < 4.9f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 4.9f && audioTime < 5.0f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 5.0f && audioTime < 5.1f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 5.1f && audioTime < 5.2f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 5.2f && audioTime < 5.3f)
            {
                panelText.text = "Mission Accomplished.";
            }
            else if (audioTime >= 5.3f && audioTime < 5.4f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 5.4f && audioTime < 5.5f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 5.5f && audioTime < 5.6f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 5.6f && audioTime < 5.7f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 5.7f && audioTime < 5.8f)
            {
                panelText.text = "Mission Accomplished.";
            }
            else if (audioTime >= 5.8f && audioTime < 5.9f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 5.9f && audioTime < 6.0f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 6.0f && audioTime < 6.1f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 6.1f && audioTime < 6.2f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 6.2f && audioTime < 6.3f)
            {
                panelText.text = "Mission Accomplished.";
            }
            else if (audioTime >= 6.3f && audioTime < 6.4f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 6.4f && audioTime < 6.5f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 6.5f && audioTime < 6.6f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 6.6f && audioTime < 6.7f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 6.7f && audioTime < 6.8f)
            {
                panelText.text = "Mission Accomplished.";
            }
            else if (audioTime >= 6.8f && audioTime < 6.9f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 6.9f && audioTime < 7.0f)
            {
                panelText.text = "Mission Accomplished.|";
            }
            else if (audioTime >= 7.0f)
            {
                SceneManager.LoadScene("StartScene_TPS");
            }

            if (audioTime <= 2.0f)
            {
                AudioPlayOneShot(1);
            }
        }
    }
}
