using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 initialPos; //開始位置のローカル座標
    public float movableRadiusDis; //動ける範囲（開始位置からの半径距離）
    private float currentDis; //開始位置からポインタ位置までの距離
    private Vector3 pointerLocalPos; //ポインタのローカル座標（開始位置からの相対的なポインタの座標）
    GameObject Joystick; //親オブジェクトのJoystick



    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        gameObject.transform.position = pointerEventData.position;
    }


    public void OnDrag(PointerEventData pointerEventData)
    {
        pointerLocalPos = Joystick.transform.InverseTransformPoint(pointerEventData.position);
        currentDis = (initialPos - pointerLocalPos).magnitude;
        
        if (currentDis < movableRadiusDis)
        {
            gameObject.transform.localPosition = pointerLocalPos;
        }
        else if(currentDis >= movableRadiusDis)
        {
            gameObject.transform.localPosition = pointerLocalPos * (movableRadiusDis / currentDis);
        }
    }


    public void OnEndDrag(PointerEventData pointerEventData)
    {
        gameObject.transform.localPosition = initialPos;
    }



    // Start is called before the first frame update
    void Start()
    {
        Joystick = transform.parent.gameObject;
        initialPos = Joystick.transform.InverseTransformPoint(transform.position);
    }


    
    // Update is called once per frame
    void Update()
    {
        
        
    }
}
