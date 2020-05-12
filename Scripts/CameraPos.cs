using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CameraPos : MonoBehaviour
{
    GameObject Canvas;

    Transform PistolShootButton;
    Transform PistolReadySlider;
    Slider prsSlider;
    Transform SliderFill;
    Image sliderFillImage;
    Transform PSButton;
    Image psbImage;

    Vector3 initialLocalCameraPos;

    GameObject Player;

    Transform CrouchButton;
    Transform CButton; //uGUI>CrouchButton>CButton
    CrouchButtonPress cbpScript; //"CrouchButtonPress.cs"
    Image cbImage;

    Transform Panel_PlayerRotation; //uGUI>Panel_PlayerRotation
    PanelDrag pdScript; //"PanelDrag.cs"
    Image pdImage;

    GameObject PlayerAxis;

    GameObject CardinalPointOfCamera;


    // Start is called before the first frame update
    void Start()
    {
        Canvas = GameObject.FindGameObjectWithTag("Canvas");

        PistolShootButton = Canvas.transform.Find("PistolShootButton");
        PistolReadySlider = PistolShootButton.Find("PistolReadySlider");
        prsSlider = PistolReadySlider.GetComponent<Slider>();
        SliderFill = PistolReadySlider.Find("Fill Area").Find("Fill");
        sliderFillImage = SliderFill.GetComponent<Image>();
        PSButton = PistolShootButton.Find("PSButton");
        psbImage = PSButton.GetComponent<Image>();

        initialLocalCameraPos = transform.localPosition;

        cameraDoubleTapReset = true;
        cameraZoomin = true;
        cameraZoomout = true;
        cameraCrouchZoomin = true;
        cameraCrouchZoomout = true;

        CrouchButton = Canvas.transform.Find("CrouchButton");
        CButton = CrouchButton.Find("CButton");
        cbpScript = CButton.GetComponent<CrouchButtonPress>();
        cbImage = CButton.GetComponent<Image>();


        Panel_PlayerRotation = Canvas.transform.Find("Panel_PlayerRotation");
        pdImage = Panel_PlayerRotation.GetComponent<Image>();
        pdScript = Panel_PlayerRotation.GetComponent<PanelDrag>();
        

        CurLocalPosWhenDragFinished = transform.localPosition;

        PlayerAxis = transform.parent.gameObject;
        Player = PlayerAxis.transform.GetChild(0).gameObject;
        CardinalPointOfCamera = PlayerAxis.transform.GetChild(2).gameObject;


        intialDisFromCardinalToCamera = (CardinalPosOriginal - originalLocalCameraPos).magnitude;

        initialDisFromCardinalToCameraWhenPistol = (CardinalPosPistol - originalLocalCameraPosWhenPistol).magnitude;

        initialDisFromCardinalToCameraWhenPistolCrouch = (CardinalPosPistolCrouch - originalLocalCameraPosWhenPistolCrouch).magnitude;

        independentCameraTrans = false;
    }


    bool cameraDoubleTapReset;
    bool cameraZoomin;
    bool cameraZoomout;
    bool cameraCrouchZoomin;
    bool cameraCrouchZoomout;
    float cameraMoveSpeed = 10.0f;
    private Vector3 curLocalPosWhenDragFinished;
    private Vector3 CurLocalPosWhenDragFinished
    {
        get { return curLocalPosWhenDragFinished; }
        set { curLocalPosWhenDragFinished = value; }
    }
    bool doOnceCurLocalPosWhenDragFinished = false;
    private Vector3 curPlayerRotation;
    private Vector3 CurPlayerRotation
    {
        get { return curPlayerRotation; }
        set { curPlayerRotation = value; }
    }
    bool doOnceCurPlayerRotation = true;

    Vector3 CardinalPosOriginal = new Vector3(0f, 1.97f, 0f);
    Vector3 CardinalPosWall = new Vector3(0f, 1.5f, 0f);
    Vector3 CardinalPosPistol = new Vector3(0.35f, 1.65f, 0);
    Vector3 CardinalPosPistolCrouch = new Vector3(0.35f, 1.45f, 0);

    bool viewobstacleRaycast;
    RaycastHit viewobstacleHitInfo;
    Vector3 directionFromCardinalToCamera;
    float intialDisFromCardinalToCamera;


    bool doOnceReturnToOriginalPos;

    Vector3 originalLocalCameraPos = new Vector3(0, 2.5f, -3.0f);

    bool viewobstacleRaycastWhenPistol;
    RaycastHit viewobstacleHitInfoWhenPistol;
    Vector3 originalLocalCameraPosWhenPistol = new Vector3(0.5f, 1.7f, -1.2f);
    float initialDisFromCardinalToCameraWhenPistol;

    bool viewobstacleRaycastWhenPistolCrouch;
    RaycastHit viewobstacleHitInfoWhenPistolCrouch;
    Vector3 originalLocalCameraPosWhenPistolCrouch = new Vector3(0.5f, 1.5f, -1.2f);
    float initialDisFromCardinalToCameraWhenPistolCrouch;
    
    int layerMask = ~(1 << 9) & ~(1 << 2); //layer"Player"と"IgnoreRaycast"以外に当たる（"Playerには当たらない"）

    bool raycastWhenTrans;
    Vector3 dirWhenTrans;
    RaycastHit hitInfoWhenTrans;
    float disWhenTrans = 30.0f;

    Vector3 destination;
    public Quaternion cameraCurLocalRotation;

    float cameralocalEulerAnglesX_DownMax = 300f;
    float cameralocalEulerAnglesX_UpMax = 60f;

    bool independentCameraTrans;

    // Update is called once per frame
    void Update()
    {
        if (transform.localEulerAngles.x <= 300f && transform.localEulerAngles.x > 180f)
        {
            transform.localEulerAngles = new Vector3(cameralocalEulerAnglesX_DownMax, transform.localEulerAngles.y, transform.localEulerAngles.z);
            if (pdScript.panelDragVector.y > 0)
            {
                pdScript.panelDragVector.y = 0f;
            }
        }
        else if (transform.localEulerAngles.x >= 60f && transform.localEulerAngles.x < 180f)
        {
            transform.localEulerAngles = new Vector3(cameralocalEulerAnglesX_UpMax, transform.localEulerAngles.y, transform.localEulerAngles.z);
            if (pdScript.panelDragVector.y < 0)
            {
                pdScript.panelDragVector.y = 0f;
            }
        }

        //Pistolを構えていない時
        if (prsSlider.value == 0 && !independentCameraTrans)
        {   
            directionFromCardinalToCamera = (transform.position - CardinalPointOfCamera.transform.position).normalized;
            viewobstacleRaycast = Physics.Raycast(CardinalPointOfCamera.transform.position, directionFromCardinalToCamera, out viewobstacleHitInfo, intialDisFromCardinalToCamera, layerMask);
            //Pistolを構えておらずカメラが壁際にある時に、壁の位置に移動（カメラが壁で視界を遮られない様にするため）
            if (viewobstacleRaycast)
            {   
                if (!viewobstacleHitInfo.transform.CompareTag("Player"))
                {
                    doOnceReturnToOriginalPos = true;
                    Vector3 curLocalPos = transform.localPosition;
                    Vector3 destination = PlayerAxis.transform.InverseTransformPoint(viewobstacleHitInfo.point);
                    curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                    transform.localPosition = curLocalPos;

                    Vector3 curLocalPosCardinal = CardinalPointOfCamera.transform.localPosition;
                    Vector3 destinationCardianl = CardinalPosWall;
                    curLocalPosCardinal = Vector3.Lerp(curLocalPosCardinal, destinationCardianl, Time.deltaTime * cameraMoveSpeed);
                    CardinalPointOfCamera.transform.localPosition = curLocalPosCardinal;
                    transform.LookAt(CardinalPointOfCamera.transform.position);

                    //Pistolを構えておらず、PanelをDragしている時
                    if (pdScript.cameraDrag)
                    {
                        if (transform.localPosition.z < 0f && transform.localPosition.x < 0f)
                        {  
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * -0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのYZ平面の移動
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.forward, pdScript.panelDragVector.y * 0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                        }
                        else if (transform.localPosition.z >= 0f && transform.localPosition.x < 0f)
                        {   
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * 0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのYZ平面の移動
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.forward, pdScript.panelDragVector.y * 0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                        }
                        else if (transform.localPosition.z < 0f && transform.localPosition.x >= 0f)
                        {   
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * -0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのYZ平面の移動
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.forward, pdScript.panelDragVector.y * -0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                        }
                        else if (transform.localPosition.z >= 0f && transform.localPosition.x >= 0f)
                        {   
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * 0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのYZ平面の移動
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.forward, pdScript.panelDragVector.y * -0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                        }
                        //CameraのRotationは常にPlayer方向を向く
                        transform.LookAt(CardinalPointOfCamera.transform.position);
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.up, pdScript.panelDragVector.x * 0.4f * Time.deltaTime); //左右のPanelドラッグによる、カメラのZX平面の移動
                        CurLocalPosWhenDragFinished = transform.localPosition;
                        doOnceCurLocalPosWhenDragFinished = true;
                    }
                    
                    //Dragが終了して、Zoomoutもしていない時
                    else if (!pdScript.cameraDrag)
                    {
                        if (doOnceCurLocalPosWhenDragFinished)
                        {   
                            transform.localPosition = CurLocalPosWhenDragFinished;
                            doOnceCurLocalPosWhenDragFinished = false;
                        }
                    }
                }
            }
            //Pistolを構えておらずカメラが壁際にない時
            else if (!viewobstacleRaycast || (viewobstacleRaycast && viewobstacleHitInfo.transform.CompareTag("Player")))
            {
               
                //「壁際にある」から「壁際にない」に条件変化した際に一度だけ「元の位置にCardinalを戻す」&&「Cameraの位置を元の軌道上に戻す」
                if (doOnceReturnToOriginalPos)
                {   
                    Vector3 curLocalPosCardinal = CardinalPointOfCamera.transform.localPosition;
                    Vector3 destinationCardinal = CardinalPosOriginal;
                    curLocalPosCardinal = Vector3.Lerp(curLocalPosCardinal, destinationCardinal, Time.deltaTime * cameraMoveSpeed);
                    CardinalPointOfCamera.transform.localPosition = curLocalPosCardinal;

                    Vector3 curLocalPosCamera = transform.localPosition;
                    Vector3 destinationCamera = destinationCardinal + ((transform.localPosition - CardinalPointOfCamera.transform.localPosition).normalized * intialDisFromCardinalToCamera);
                    curLocalPosCamera = Vector3.Lerp(curLocalPosCamera, destinationCamera, Time.deltaTime * cameraMoveSpeed);
                    transform.localPosition = curLocalPosCamera;

                    transform.LookAt(CardinalPointOfCamera.transform.position);

                    if ((curLocalPosCardinal.y - destinationCardinal.y) <= 0.001f && (curLocalPosCardinal.y - destinationCardinal.y) >= -0.001f)
                    {   
                        CardinalPointOfCamera.transform.localPosition = destinationCardinal;
                        transform.localPosition = destinationCamera;
                        transform.LookAt(CardinalPointOfCamera.transform.position);

                        doOnceReturnToOriginalPos = false;
                    }
                }

                //Pistolを構えておらず、PanelをDragしている時
                if (pdScript.cameraDrag)
                {
                    if (transform.localPosition.z < 0f && transform.localPosition.x < 0f)
                    {   
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * -0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのYZ平面の移動
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.forward, pdScript.panelDragVector.y * 0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                    }
                    else if (transform.localPosition.z >= 0f && transform.localPosition.x < 0f)
                    {
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * 0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのYZ平面の移動
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.forward, pdScript.panelDragVector.y * 0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                    }
                    else if (transform.localPosition.z < 0f && transform.localPosition.x >= 0f)
                    {   
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * -0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのYZ平面の移動
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.forward, pdScript.panelDragVector.y * -0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                    }
                    else if (transform.localPosition.z >= 0f && transform.localPosition.x >= 0f)
                    {   
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * 0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのYZ平面の移動
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.forward, pdScript.panelDragVector.y * -0.2f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                    }
                    //CameraのRotationは常にPlayer方向を向く
                    transform.LookAt(CardinalPointOfCamera.transform.position);
                    transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.up, pdScript.panelDragVector.x * 0.4f * Time.deltaTime); //左右のPanelドラッグによる、カメラのZX平面の移動
                    CurLocalPosWhenDragFinished = transform.localPosition;
                    doOnceCurLocalPosWhenDragFinished = true;
                }
                //Dragが終了して、Zoomoutもしていない時
                else if (!pdScript.cameraDrag)
                {
                    
                    if (doOnceCurLocalPosWhenDragFinished)
                    {   
                        transform.localPosition = CurLocalPosWhenDragFinished;
                        doOnceCurLocalPosWhenDragFinished = false;
                    }
                }
            }
        }
        
        

        //Pistolを構えている時
        if (prsSlider.value == 1 && !independentCameraTrans)
        {
            //Pistolを構えていて、立っている時
            if (cbpScript.Crouch == false)
            {
                Vector3 directionWhenPistol = (transform.position - CardinalPointOfCamera.transform.position).normalized;
                float disWhenPistol = (originalLocalCameraPosWhenPistol - CardinalPointOfCamera.transform.localPosition).magnitude;
                viewobstacleRaycastWhenPistol = Physics.Raycast(CardinalPointOfCamera.transform.position, directionWhenPistol, out viewobstacleHitInfoWhenPistol, disWhenPistol, layerMask);

                //Pistolを構えて立っていて、カメラが壁際にある時
                if (viewobstacleRaycastWhenPistol)
                {
                    if (!viewobstacleHitInfoWhenPistol.transform.CompareTag("Player"))
                    {
                        doOnceReturnToOriginalPos = true;
                        Vector3 curLocalPos = transform.localPosition;
                        Vector3 destination = PlayerAxis.transform.InverseTransformPoint(viewobstacleHitInfoWhenPistol.point);
                        curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                        transform.localPosition = curLocalPos;
                        transform.LookAt(CardinalPointOfCamera.transform.position);

                        //Pistolを構えて立ち、カメラが壁際にあり、PanelをDragしている時
                        if (pdScript.cameraDrag)
                        {
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * -0.3f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動                                                                                                                                   
                            transform.LookAt(CardinalPointOfCamera.transform.position); //CameraのRotationは常にPlayer方向を向く
                            CurLocalPosWhenDragFinished = transform.localPosition;
                            doOnceCurLocalPosWhenDragFinished = true;
                        }
                        //Dragが終了して、Zoomoutもしていない時
                        else if (!pdScript.cameraDrag)
                        {
                            if (doOnceCurLocalPosWhenDragFinished)
                            {
                                transform.localPosition = CurLocalPosWhenDragFinished;
                                //CameraのRotationは常にPlayer方向を向く
                                transform.LookAt(CardinalPointOfCamera.transform.position);
                                doOnceCurLocalPosWhenDragFinished = false;
                            }
                        }
                    }
                }
                //Pistolを構えて立っていて、カメラが壁際にない時
                else if (!viewobstacleRaycastWhenPistol || (viewobstacleRaycastWhenPistol && viewobstacleHitInfoWhenPistol.transform.CompareTag("Player")))
                {
                    //「壁際にある」から「壁際にない」に条件変化した際に一度だけ「元の位置にCardinalを戻す」&&「Cameraの位置を元の軌道上に戻す」
                    if (doOnceReturnToOriginalPos)
                    {
                        Vector3 curLocalPosCamera = transform.localPosition;
                        Vector3 destinationCamera = CardinalPointOfCamera.transform.localPosition + ((transform.localPosition - CardinalPointOfCamera.transform.localPosition).normalized * initialDisFromCardinalToCameraWhenPistol);
                        curLocalPosCamera = Vector3.Lerp(curLocalPosCamera, destinationCamera, Time.deltaTime * cameraMoveSpeed);
                        transform.localPosition = curLocalPosCamera;

                        transform.LookAt(CardinalPointOfCamera.transform.position);

                        if ((curLocalPosCamera.z - destinationCamera.z) <= 0.001f && (curLocalPosCamera.z - destinationCamera.z) >= -0.001f)
                        {
                            transform.localPosition = destinationCamera;
                            transform.LookAt(CardinalPointOfCamera.transform.position);

                            doOnceReturnToOriginalPos = false;
                        }
                    }


                    //Pistolを構えていて、PanelをDragしている時
                    if (pdScript.cameraDrag)
                    {
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * -0.3f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                        //CameraのRotationは常にPlayer方向を向く
                        transform.LookAt(CardinalPointOfCamera.transform.position);
                        CurLocalPosWhenDragFinished = transform.localPosition;
                        doOnceCurLocalPosWhenDragFinished = true;
                    }
                    //Dragが終了して、Zoomoutもしていない時
                    else if (!pdScript.cameraDrag)
                    {
                        if (doOnceCurLocalPosWhenDragFinished)
                        {   
                            transform.localPosition = CurLocalPosWhenDragFinished;
                            //CameraのRotationは常にPlayer方向を向く
                            transform.LookAt(CardinalPointOfCamera.transform.position);
                            doOnceCurLocalPosWhenDragFinished = false;
                        }
                    }
                }
            }
            //Pistolを構えていて、しゃがんでいる時
            else if (cbpScript.Crouch == true)
            {
                Vector3 directionWhenPistolCrouch = (transform.position - CardinalPointOfCamera.transform.position).normalized;
                float disWhenPistolCrouch = (originalLocalCameraPosWhenPistolCrouch - CardinalPointOfCamera.transform.localPosition).magnitude;
                viewobstacleRaycastWhenPistolCrouch = Physics.Raycast(CardinalPointOfCamera.transform.position, directionWhenPistolCrouch, out viewobstacleHitInfoWhenPistolCrouch, disWhenPistolCrouch, layerMask);
                //Pistolを構えてしゃがんでいて、カメラが壁際にある時
                if (viewobstacleRaycastWhenPistolCrouch)
                {
                    if (!viewobstacleHitInfoWhenPistolCrouch.transform.CompareTag("Player"))
                    {
                        doOnceReturnToOriginalPos = true;
                        Vector3 curLocalPos = transform.localPosition;
                        Vector3 destination = PlayerAxis.transform.InverseTransformPoint(viewobstacleHitInfoWhenPistolCrouch.point);
                        curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                        transform.localPosition = curLocalPos;
                        transform.LookAt(CardinalPointOfCamera.transform.position);

                        //Pistolを構えて立ち、カメラが壁際にあり、PanelをDragしている時
                        if (pdScript.cameraDrag)
                        {
                            transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * -0.3f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動                                                                                                                                   
                            transform.LookAt(CardinalPointOfCamera.transform.position); //CameraのRotationは常にPlayer方向を向く
                            CurLocalPosWhenDragFinished = transform.localPosition;
                            doOnceCurLocalPosWhenDragFinished = true;
                        }
                        //Dragが終了して、Zoomoutもしていない時
                        else if (!pdScript.cameraDrag)
                        {
                            if (doOnceCurLocalPosWhenDragFinished)
                            {
                                transform.localPosition = CurLocalPosWhenDragFinished;
                                //CameraのRotationは常にPlayer方向を向く
                                transform.LookAt(CardinalPointOfCamera.transform.position);
                                doOnceCurLocalPosWhenDragFinished = false;
                            }
                        }
                    }
                }
                //Pistolを構えてしゃがんでいて、カメラが壁際にない時
                else if (!viewobstacleRaycastWhenPistolCrouch || (viewobstacleRaycastWhenPistolCrouch && viewobstacleHitInfoWhenPistolCrouch.transform.CompareTag("Player")))
                {
                    //「壁際にある」から「壁際にない」に条件変化した際に一度だけ「元の位置にCardinalを戻す」&&「Cameraの位置を元の軌道上に戻す」
                    if (doOnceReturnToOriginalPos)
                    {
                        Vector3 curLocalPosCamera = transform.localPosition;
                        Vector3 destinationCamera = CardinalPointOfCamera.transform.localPosition + ((transform.localPosition - CardinalPointOfCamera.transform.localPosition).normalized * initialDisFromCardinalToCameraWhenPistolCrouch);
                        curLocalPosCamera = Vector3.Lerp(curLocalPosCamera, destinationCamera, Time.deltaTime * cameraMoveSpeed);
                        transform.localPosition = curLocalPosCamera;

                        transform.LookAt(CardinalPointOfCamera.transform.position);

                        if ((curLocalPosCamera.z - destinationCamera.z) <= 0.001f && (curLocalPosCamera.z - destinationCamera.z) >= -0.001f)
                        {
                            transform.localPosition = destinationCamera;
                            transform.LookAt(CardinalPointOfCamera.transform.position);

                            doOnceReturnToOriginalPos = false;
                        }
                    }


                    //Pistolを構えていて、PanelをDragしている時
                    if (pdScript.cameraDrag)
                    {
                        transform.RotateAround(CardinalPointOfCamera.transform.position, CardinalPointOfCamera.transform.right, pdScript.panelDragVector.y * -0.3f * Time.deltaTime); //上下のPanelドラッグによる、カメラのXY平面の移動
                        //CameraのRotationは常にPlayer方向を向く
                        transform.LookAt(CardinalPointOfCamera.transform.position);
                        CurLocalPosWhenDragFinished = transform.localPosition;
                        doOnceCurLocalPosWhenDragFinished = true;
                    }
                    //Dragが終了して、Zoomoutもしていない時
                    else if (!pdScript.cameraDrag)
                    {
                        if (doOnceCurLocalPosWhenDragFinished)
                        {
                            transform.localPosition = CurLocalPosWhenDragFinished;
                            //CameraのRotationは常にPlayer方向を向く
                            transform.LookAt(CardinalPointOfCamera.transform.position);
                            doOnceCurLocalPosWhenDragFinished = false;
                        }
                    }
                }
            }
        }



        
        //立っている時にCameraのズームイン(Pistol構えていない時 && !Crouch → 構えている時 && !Crouch)
        if (prsSlider.value == 1 && cbpScript.Crouch == false && cameraZoomin == true)
        {
            if (doOnceCurPlayerRotation)
            {
                cameraDoubleTapReset = false;
                sliderFillImage.raycastTarget = false;
                psbImage.raycastTarget = false;
                pdImage.raycastTarget = false;
                cbImage.raycastTarget = false;
                independentCameraTrans = true;
                dirWhenTrans = (CardinalPointOfCamera.transform.position - transform.position).normalized;
                raycastWhenTrans = Physics.Raycast(transform.position, dirWhenTrans, out hitInfoWhenTrans, disWhenTrans, layerMask);
                if (raycastWhenTrans)
                {
                    transform.parent = null;
                    PlayerAxis.transform.LookAt(new Vector3(hitInfoWhenTrans.point.x, PlayerAxis.transform.position.y, hitInfoWhenTrans.point.z), Vector3.up);
                    transform.parent = PlayerAxis.transform;
                    destination = CardinalPosPistol + ((CardinalPosPistol - PlayerAxis.transform.InverseTransformPoint(hitInfoWhenTrans.point)).normalized * initialDisFromCardinalToCameraWhenPistol);
                }
                else if (!raycastWhenTrans)
                {
                    transform.parent = null;
                    Vector3 endPoint = transform.position + (dirWhenTrans * disWhenTrans);
                    PlayerAxis.transform.LookAt(new Vector3(endPoint.x, PlayerAxis.transform.position.y, endPoint.z), Vector3.up);
                    transform.parent = PlayerAxis.transform;
                    destination = CardinalPosPistol + ((CardinalPosPistol - PlayerAxis.transform.InverseTransformPoint(endPoint)).normalized * initialDisFromCardinalToCameraWhenPistol);
                }
                Player.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                doOnceCurPlayerRotation = false;
            }
            else if (!doOnceCurPlayerRotation)
            {
                Vector3 curLocalPos = transform.localPosition;
                curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                transform.localPosition = curLocalPos;

                Vector3 curLocalPosCardinal = CardinalPointOfCamera.transform.localPosition;
                Vector3 destinationCardinal = CardinalPosPistol;
                curLocalPosCardinal = Vector3.Lerp(curLocalPosCardinal, destinationCardinal, Time.deltaTime * cameraMoveSpeed);
                CardinalPointOfCamera.transform.localPosition = curLocalPosCardinal;

                transform.LookAt(CardinalPointOfCamera.transform.position);

                //Camera位置を目標位置まできちんと移動させる
                if ((transform.localPosition.y - destination.y) <= 0.001f && (transform.localPosition.y - destination.y) >= -0.001f)
                {
                    transform.localPosition = destination;
                    CardinalPointOfCamera.transform.localPosition = destinationCardinal;
                    transform.LookAt(CardinalPointOfCamera.transform.position);
                    cameraDoubleTapReset = true;
                    cameraZoomin = false;
                    cameraZoomout = true;
                    cameraCrouchZoomin = true;
                    cameraCrouchZoomout = true;
                    doOnceCurPlayerRotation = true;
                    sliderFillImage.raycastTarget = true;
                    psbImage.raycastTarget = true;
                    pdImage.raycastTarget = true;
                    cbImage.raycastTarget = true;
                    independentCameraTrans = false;
                }
            }
        }
        //立っている時にCameraのズームアウト(Pistol構えている時 && !Crouch → 構えていない時 && !Crouch)
        else if (prsSlider.value == 0 && cbpScript.Crouch == false && cameraZoomout == true) 
        {
            if (doOnceCurPlayerRotation)
            {
                cameraDoubleTapReset = false;
                sliderFillImage.raycastTarget = false;
                psbImage.raycastTarget = false;
                pdImage.raycastTarget = false;
                cbImage.raycastTarget = false;
                pdScript.cameraDrag = false;
                independentCameraTrans = true;
                dirWhenTrans = (CardinalPointOfCamera.transform.position - transform.position).normalized;
                raycastWhenTrans = Physics.Raycast(transform.position, dirWhenTrans, out hitInfoWhenTrans, disWhenTrans, layerMask);
                if (raycastWhenTrans)
                {
                    PlayerAxis.transform.LookAt(new Vector3(hitInfoWhenTrans.point.x, PlayerAxis.transform.position.y, hitInfoWhenTrans.point.z), Vector3.up);
                    destination = CardinalPosOriginal + ((CardinalPosOriginal - PlayerAxis.transform.InverseTransformPoint(hitInfoWhenTrans.point)).normalized * intialDisFromCardinalToCamera);
                }
                else if (!raycastWhenTrans)
                {
                    Vector3 endPoint = transform.position + (dirWhenTrans * disWhenTrans);
                    PlayerAxis.transform.LookAt(new Vector3(endPoint.x, PlayerAxis.transform.position.y, endPoint.z), Vector3.up);
                    destination = CardinalPosOriginal + ((CardinalPosOriginal - PlayerAxis.transform.InverseTransformPoint(endPoint)).normalized * intialDisFromCardinalToCamera);
                }
                doOnceCurPlayerRotation = false;
            }
            else if (!doOnceCurPlayerRotation)
            {
                Vector3 curLocalPos = transform.localPosition;
                curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                transform.localPosition = curLocalPos;

                Vector3 curLocalPosCardinal = CardinalPointOfCamera.transform.localPosition;
                Vector3 destinationCardinal = CardinalPosOriginal;
                curLocalPosCardinal = Vector3.Lerp(curLocalPosCardinal, destinationCardinal, Time.deltaTime * cameraMoveSpeed);
                CardinalPointOfCamera.transform.localPosition = curLocalPosCardinal;

                transform.LookAt(CardinalPointOfCamera.transform.position);

                //Camera位置を目標位置まできちんと移動させる
                if ((transform.localPosition.x - destination.x) <= 0.001f && (transform.localPosition.x - destination.x) >= -0.001f)
                {
                    transform.localPosition = destination;
                    CardinalPointOfCamera.transform.localPosition = destinationCardinal;
                    transform.LookAt(CardinalPointOfCamera.transform.position);
                    cameraDoubleTapReset = true;
                    cameraZoomin = true;
                    cameraZoomout = false;
                    cameraCrouchZoomin = true;
                    cameraCrouchZoomout = true;
                    doOnceCurPlayerRotation = true;
                    sliderFillImage.raycastTarget = true;
                    pdImage.raycastTarget = true;
                    cbImage.raycastTarget = true;
                    independentCameraTrans = false;
                }
            }
            

        }


        //Crouch時にCameraのズームイン(Pistol構えていない時 && Crouch → 構えている時 && Crouch)
        if (prsSlider.value == 1 && cbpScript.Crouch && cameraCrouchZoomin == true)
        {
            if (doOnceCurPlayerRotation)
            {
                cameraDoubleTapReset = false;
                sliderFillImage.raycastTarget = false;
                psbImage.raycastTarget = false;
                pdImage.raycastTarget = false;
                cbImage.raycastTarget = false;
                independentCameraTrans = true;
                dirWhenTrans = (CardinalPointOfCamera.transform.position - transform.position).normalized;
                raycastWhenTrans = Physics.Raycast(transform.position, dirWhenTrans, out hitInfoWhenTrans, disWhenTrans, layerMask);
                if (raycastWhenTrans)
                {
                    transform.parent = null;
                    PlayerAxis.transform.LookAt(new Vector3(hitInfoWhenTrans.point.x, PlayerAxis.transform.position.y, hitInfoWhenTrans.point.z), Vector3.up);
                    transform.parent = PlayerAxis.transform;
                    destination = CardinalPosPistolCrouch + ((CardinalPosPistolCrouch - PlayerAxis.transform.InverseTransformPoint(hitInfoWhenTrans.point)).normalized * initialDisFromCardinalToCameraWhenPistol);
                }
                else if (!raycastWhenTrans)
                {
                    transform.parent = null;
                    Vector3 endPoint = transform.position + (dirWhenTrans * disWhenTrans);
                    PlayerAxis.transform.LookAt(new Vector3(endPoint.x, PlayerAxis.transform.position.y, endPoint.z), Vector3.up);
                    transform.parent = PlayerAxis.transform;
                    destination = CardinalPosPistolCrouch + ((CardinalPosPistolCrouch - PlayerAxis.transform.InverseTransformPoint(endPoint)).normalized * initialDisFromCardinalToCameraWhenPistol);
                }
                Player.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                doOnceCurPlayerRotation = false;
            }
            else if (!doOnceCurPlayerRotation)
            {
                Vector3 curLocalPos = transform.localPosition;
                curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                transform.localPosition = curLocalPos;

                Vector3 curLocalPosCardinal = CardinalPointOfCamera.transform.localPosition;
                Vector3 destinationCardinal = CardinalPosPistolCrouch;
                curLocalPosCardinal = Vector3.Lerp(curLocalPosCardinal, destinationCardinal, Time.deltaTime * cameraMoveSpeed);
                CardinalPointOfCamera.transform.localPosition = curLocalPosCardinal;

                transform.LookAt(CardinalPointOfCamera.transform.position);

                //Camera位置を目標位置まできちんと移動させる
                if ((transform.localPosition.y - destination.y) <= 0.001f && (transform.localPosition.y - destination.y) >= -0.001f)
                {
                    transform.localPosition = destination;
                    CardinalPointOfCamera.transform.localPosition = destinationCardinal;
                    transform.LookAt(CardinalPointOfCamera.transform.position);
                    cameraDoubleTapReset = true;
                    cameraZoomin = true;
                    cameraZoomout = true;
                    cameraCrouchZoomin = false;
                    cameraCrouchZoomout = true;
                    doOnceCurPlayerRotation = true;
                    sliderFillImage.raycastTarget = true;
                    psbImage.raycastTarget = true;
                    pdImage.raycastTarget = true;
                    cbImage.raycastTarget = true;
                    independentCameraTrans = false;
                }
            }
        }
        //Crouch時にCameraのズームアウト(Pistol構えている時 && Crouch → 構えていない時 && Crouch)
        else if (prsSlider.value == 0 && cbpScript.Crouch && cameraCrouchZoomout == true)
        {
            if (doOnceCurPlayerRotation)
            {
                cameraDoubleTapReset = false;
                sliderFillImage.raycastTarget = false;
                psbImage.raycastTarget = false;
                pdImage.raycastTarget = false;
                cbImage.raycastTarget = false;
                independentCameraTrans = true;
                dirWhenTrans = (CardinalPointOfCamera.transform.position - transform.position).normalized;
                raycastWhenTrans = Physics.Raycast(transform.position, dirWhenTrans, out hitInfoWhenTrans, disWhenTrans, layerMask);
                if (raycastWhenTrans)
                {
                    PlayerAxis.transform.LookAt(new Vector3(hitInfoWhenTrans.point.x, PlayerAxis.transform.position.y, hitInfoWhenTrans.point.z), Vector3.up);
                    destination = CardinalPosOriginal + ((CardinalPosOriginal - PlayerAxis.transform.InverseTransformPoint(hitInfoWhenTrans.point)).normalized * intialDisFromCardinalToCamera);
                }
                else if (!raycastWhenTrans)
                {
                    Vector3 endPoint = transform.position + (dirWhenTrans * disWhenTrans);
                    PlayerAxis.transform.LookAt(new Vector3(endPoint.x, PlayerAxis.transform.position.y, endPoint.z), Vector3.up);
                    destination = CardinalPosOriginal + ((CardinalPosOriginal - PlayerAxis.transform.InverseTransformPoint(endPoint)).normalized * intialDisFromCardinalToCamera);
                }
                doOnceCurPlayerRotation = false;
            }
            else if (!doOnceCurPlayerRotation)
            {
                Vector3 curLocalPos = transform.localPosition;
                curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                transform.localPosition = curLocalPos;

                Vector3 curLocalPosCardinal = CardinalPointOfCamera.transform.localPosition;
                Vector3 destinationCardinal = CardinalPosOriginal;
                curLocalPosCardinal = Vector3.Lerp(curLocalPosCardinal, destinationCardinal, Time.deltaTime * cameraMoveSpeed);
                CardinalPointOfCamera.transform.localPosition = curLocalPosCardinal;

                transform.LookAt(CardinalPointOfCamera.transform.position);

                //Camera位置を目標位置まできちんと移動させる
                if ((transform.localPosition.x - destination.x) <= 0.001f && (transform.localPosition.x - destination.x) >= -0.001f)
                {
                    transform.localPosition = destination;
                    CardinalPointOfCamera.transform.localPosition = destinationCardinal;
                    transform.LookAt(CardinalPointOfCamera.transform.position);
                    cameraDoubleTapReset = true;
                    cameraZoomin = true;
                    cameraZoomout = true;
                    cameraCrouchZoomin = true;
                    cameraCrouchZoomout = false;
                    doOnceCurPlayerRotation = true;
                    sliderFillImage.raycastTarget = true;
                    pdImage.raycastTarget = true;
                    cbImage.raycastTarget = true;
                    independentCameraTrans = false;
                }
            }
            
        }




        //UIのPanelがダブルタップされた時 && Pitolを構えていない時（Playerの真後ろ定位置を追従）
        if (pdScript.cameraToFixedPositionWhenDoubleTap && prsSlider.value == 0 && cameraDoubleTapReset)
        {
            if (!cbpScript.Crouch)
            {
                if (doOnceCurPlayerRotation)
                {
                    CurPlayerRotation = Player.transform.eulerAngles;
                    Player.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    transform.parent = null;
                    PlayerAxis.transform.eulerAngles = CurPlayerRotation;
                    transform.parent = PlayerAxis.transform;
                    cameraZoomin = false;
                    cameraZoomout = false;
                    cameraCrouchZoomin = false;
                    cameraCrouchZoomout = false;
                    sliderFillImage.raycastTarget = false;
                    independentCameraTrans = true;
                    doOnceCurPlayerRotation = false;
                }


                Vector3 curLocalPos = transform.localPosition;
                Vector3 destination = initialLocalCameraPos;
                curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                transform.localPosition = curLocalPos;

                Vector3 curLocalPosCardinal = CardinalPointOfCamera.transform.localPosition;
                Vector3 destinationCardinal = CardinalPosOriginal;
                curLocalPosCardinal = Vector3.Lerp(curLocalPosCardinal, destinationCardinal, Time.deltaTime * cameraMoveSpeed);
                CardinalPointOfCamera.transform.localPosition = curLocalPosCardinal;
                
                transform.LookAt(CardinalPointOfCamera.transform.position);

                if ((transform.localPosition.x - destination.x) <= 0.001f && (transform.localPosition.x - destination.x) >= -0.001f)
                {
                    pdScript.cameraToFixedPositionWhenDoubleTap = false;
                    doOnceCurPlayerRotation = true;
                    cameraZoomin = true;
                    cameraZoomout = true;
                    cameraCrouchZoomin = true;
                    cameraCrouchZoomout = true;
                    sliderFillImage.raycastTarget = true;
                    transform.localPosition = destination;
                    CardinalPointOfCamera.transform.localPosition = destinationCardinal;
                    transform.LookAt(CardinalPointOfCamera.transform.position);
                    independentCameraTrans = false;
                }
            }

            else if (cbpScript.Crouch)
            {
                if (doOnceCurPlayerRotation)
                {
                    CurPlayerRotation = Player.transform.eulerAngles;
                    Player.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    transform.parent = null;
                    PlayerAxis.transform.eulerAngles = CurPlayerRotation;
                    transform.parent = PlayerAxis.transform;
                    cameraZoomin = false;
                    cameraZoomout = false;
                    cameraCrouchZoomin = false;
                    cameraCrouchZoomout = false;
                    sliderFillImage.raycastTarget = false;
                    independentCameraTrans = true;
                    doOnceCurPlayerRotation = false;
                }


                Vector3 curLocalPos = transform.localPosition;
                Vector3 destination = initialLocalCameraPos;
                curLocalPos = Vector3.Lerp(curLocalPos, destination, Time.deltaTime * cameraMoveSpeed);
                transform.localPosition = curLocalPos;

                Vector3 curLocalPosCardinal = CardinalPointOfCamera.transform.localPosition;
                Vector3 destinationCardinal = CardinalPosOriginal;
                curLocalPosCardinal = Vector3.Lerp(curLocalPosCardinal, destinationCardinal, Time.deltaTime * cameraMoveSpeed);
                CardinalPointOfCamera.transform.localPosition = curLocalPosCardinal;

                transform.LookAt(CardinalPointOfCamera.transform.position);

                if ((transform.localPosition.x - destination.x) <= 0.001f && (transform.localPosition.x - destination.x) >= -0.001f)
                {
                    pdScript.cameraToFixedPositionWhenDoubleTap = false;
                    doOnceCurPlayerRotation = true;
                    cameraZoomin = true;
                    cameraZoomout = true;
                    cameraCrouchZoomin = true;
                    cameraCrouchZoomout = true;
                    sliderFillImage.raycastTarget = true;
                    transform.localPosition = destination;
                    CardinalPointOfCamera.transform.localPosition = destinationCardinal;
                    transform.LookAt(CardinalPointOfCamera.transform.position);
                    independentCameraTrans = false;
                }
            }
        }
    }
}
