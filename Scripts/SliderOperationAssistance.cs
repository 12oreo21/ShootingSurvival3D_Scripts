using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderOperationAssistance : MonoBehaviour
{
    Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        slider = transform.GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(slider.value <= 0.5f)
        {
            slider.value = 0f;
        }
        else if(slider.value > 0.5f)
        {
            slider.value = 1.0f;
        }
    }
}
