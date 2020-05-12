using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CrouchButtonPress : MonoBehaviour, IPointerDownHandler
{

    public bool Crouch; //trueの時にPlayerがCrouchする。"PlayerMove.cs"で利用。

    GameObject Player;
    PlayerMove pmScript;
    
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (Physics.Linecast(pmScript.rayPosition.position, (pmScript.rayPosition.position - Player.transform.up * pmScript.rayRange)))
        {
            if (Crouch == false)
            {
                Crouch = true;
            }
            else if (Crouch == true)
            {
                Crouch = false;
            }
        }
    }

    
   


    // Start is called before the first frame update
    void Start()
    {
        Crouch = false;

        Player = GameObject.Find("Player_TPose");
        pmScript = Player.GetComponent<PlayerMove>();
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
    
}
