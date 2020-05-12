using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PanelDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{

    private Vector3 initialPos;
    private Vector3 currentPos;
    public Vector3 panelDragVector;

    public bool cameraDrag = false;
    bool isDoubleTapStart1;
    bool isDoubleTapStart2;
    public bool cameraToFixedPositionWhenDoubleTap = false;

    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        initialPos = pointerEventData.position;
        cameraDrag = true;
    }


    public void OnDrag(PointerEventData pointerEventData)
    {
        currentPos = pointerEventData.position;
        panelDragVector = currentPos - initialPos;
    }


    public void OnEndDrag(PointerEventData pointerEventData)
    {
        panelDragVector = new Vector3(0, 0, 0);
        cameraDrag = false;
    }


    public void OnPointerDown(PointerEventData pointerEventData)
    {
        isDoubleTapStart1 = true;
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        //OnPointerDownを実装するために空でも必要。
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }


    float doubleTapTime1;
    float doubleTapTime2;
    // Update is called once per frame
    void Update()
    {
        if (isDoubleTapStart1)
        {
            doubleTapTime1 += Time.deltaTime;

            if (doubleTapTime1 < 0.5f)
            {
                if (isDoubleTapStart2)
                {
                    doubleTapTime2 += Time.deltaTime;
                    if (doubleTapTime2 < 0.5f)
                    {
                        doubleTapTime2 = 0.0f;
                        if (Input.GetMouseButtonDown(0))
                        {
                            cameraToFixedPositionWhenDoubleTap = true;
                            isDoubleTapStart1 = false;
                            isDoubleTapStart2 = false;
                        }
                    }
                    else if (doubleTapTime2 >= 0.5f)
                    {
                        // reset
                        isDoubleTapStart1 = false;
                        isDoubleTapStart2 = false;
                        doubleTapTime2 = 0.0f;
                    }
                }
                else if (!isDoubleTapStart2)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        isDoubleTapStart2 = true;
                    }
                }
            }
            else if (doubleTapTime1 >= 0.5f)
            {
                isDoubleTapStart1 = false;
                isDoubleTapStart2 = false;
                doubleTapTime1 = 0f;
                doubleTapTime2 = 0f;
            }
        }
    }
}
