using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderFill : MonoBehaviour, IPointerUpHandler
{
    GameObject PistolReadySlider;
    Slider prsSlider;

    GameObject PSButton;
    Image psbImage;

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        if (prsSlider.value == 1.0f)
        {
            psbImage.raycastTarget = true;
        }
        else if (prsSlider.value == 0f)
        {
            psbImage.raycastTarget = false;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        PistolReadySlider = GameObject.Find("PistolReadySlider");
        prsSlider = PistolReadySlider.GetComponent<Slider>();

        PSButton = GameObject.Find("PSButton");
        psbImage = PSButton.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
