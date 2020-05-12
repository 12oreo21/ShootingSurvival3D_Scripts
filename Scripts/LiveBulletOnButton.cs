using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class LiveBulletOnButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool liveBulletShoot;

    bool countDown = false;
    float counttime;

    Image psbImage;

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        liveBulletShoot = true;
    }


    public void OnPointerUp(PointerEventData pointerEventData)
    {
        liveBulletShoot = false;
        countDown = true;
    }

    public void IntervalForButton()
    {
        if (countDown)
        {
            counttime += Time.deltaTime;
            psbImage.raycastTarget = false;
            if (counttime >= 0.5f)
            {
                psbImage.raycastTarget = true;
                counttime = 0f;
                countDown = false;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        psbImage = transform.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        IntervalForButton();
    }
}