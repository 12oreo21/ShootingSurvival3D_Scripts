using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class JumpButtonPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public bool Jump; //trueの時にPlayerがJumpする。"PlayerMove.cs"で利用。

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        Jump = true;
    }


    public void OnPointerUp(PointerEventData pointerEventData)
    {
        Jump = false;
    }




    // Start is called before the first frame update
    void Start()
    {
        Jump = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
