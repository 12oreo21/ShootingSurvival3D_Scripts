using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoalObjectController : MonoBehaviour
{
    [SerializeField]
    GameObject Bomb;

    List<Transform> GoalObjectPoses = new List<Transform>();
    int index;
    List<int> numbers = new List<int>();
    int count = 5;
    GameObject[] bomb = new GameObject[5];

    public int bombNum;

    AudioSource[] audioSources;
    Image gcuiBImage;
    

    // Start is called before the first frame update
    void Start()
    {
        audioSources = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetComponents<AudioSource>();
        gcuiBImage = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild(13).GetComponent<Image>();

        while (index < transform.childCount)
        {
            GoalObjectPoses.Add(transform.GetChild(index));
            numbers.Add(index);
            index++;
        }

        
        while (count-- > 0)
        {
            int ransu = Random.Range(0, numbers.Count);
            int i = numbers[ransu];
            bomb[count] = Instantiate(Bomb) as GameObject;
            bomb[count].transform.position = new Vector3(GoalObjectPoses[i].position.x, GoalObjectPoses[i].position.y + 0.085f, GoalObjectPoses[i].position.z);
            bomb[count].transform.rotation = GoalObjectPoses[i].rotation;
            numbers.RemoveAt(ransu);
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


    float time1;
    float time2;
    // Update is called once per frame
    void Update()
    {
        if(bombNum == 5)
        {
            time1 += Time.deltaTime;
            gcuiBImage.color = new Color(gcuiBImage.color.r, gcuiBImage.color.g, gcuiBImage.color.b, time1 / 7f);
            if (time1 >= 5.5f)
            {
                time2 += Time.deltaTime;
                if (time2 >= 1.944f)
                {
                    audioSources[2].Stop();
                    SceneManager.LoadScene("GoalScene_TPS");
                    return;
                }
                AudioPlayOneShot(2);
            }
        }
    }
}
