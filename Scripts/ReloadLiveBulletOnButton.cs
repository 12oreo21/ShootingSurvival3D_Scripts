using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ReloadLiveBulletOnButton : MonoBehaviour, IPointerDownHandler
{
    GameObject MainCamera;
    LiveBullet lbScript;

    GameObject Reload;
    Image reloadImage;

    GameObject PistolReadySlider;
    Slider prsSlider;


    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (prsSlider.value == 1 && lbScript.bulletNum < lbScript.bulletNumMax)
        {
            StartCoroutine("Reloading");
        }

    }


    public bool finishReload;
    IEnumerator Reloading()
    {
        finishReload = false;

        yield return new WaitForSeconds(2);

        lbScript.bulletNum = lbScript.bulletNumMax;
        finishReload = true;
    }


    //UIのReloadをリロード中に可視化して回転させる
    public void ReloadUI()
    {
        if (finishReload == false)
        {
            reloadImage.color = new Color(reloadImage.color.r, reloadImage.color.g, reloadImage.color.b, 100f / 255f);
            Reload.transform.localRotation *= Quaternion.AngleAxis(-700f * Time.deltaTime, new Vector3(0, 0, 1));
        }
        else if (finishReload)
        {
            reloadImage.color = new Color(reloadImage.color.r, reloadImage.color.g, reloadImage.color.b, 0f / 255f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MainCamera = GameObject.Find("MainCamera");
        lbScript = MainCamera.GetComponent<LiveBullet>();

        Reload = GameObject.Find("Reload");
        reloadImage = Reload.GetComponent<Image>();

        finishReload = true;

        PistolReadySlider = GameObject.Find("PistolReadySlider");
        prsSlider = PistolReadySlider.GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        ReloadUI();
    }
}