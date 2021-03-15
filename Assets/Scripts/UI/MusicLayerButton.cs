using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MusicLayerButton : Button, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int layer;
    //public Image targetImage;
    public Image emptyImage;
    public Image recordImage;
    public Image glow;

    public Color normalColor;
    public Color highlightedColor;
    public Color pressedColor;


    



    // ------------------------------ Public functions ------------------------------



    public void EnableImage(Image image, bool value)
    {
        image.enabled = value;
    }




    // ------------------------------ Private functions ------------------------------




    private void ScaleImage(GameObject obj, float wait, float time, float targetValue)
    {

    }


    private IEnumerator Scale(GameObject obj, float wait, float time, float targetValue)
    {
        // 1. Wait
        yield return new WaitForSeconds(wait);

        // 2. 
        var objTransform = obj.GetComponent<RectTransform>();
        float t = 0;
        while (t < time)
        {
            float scale = Mathf.Lerp(1, targetValue, t / time);
            objTransform.localScale = Vector3.one * scale;

            t += Time.deltaTime;
            yield return null;
        }

        objTransform.localScale = Vector3.one * targetValue;

    }






    // ---------------------------------- Events ----------------------------------



    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        //emptyImage.color = pressedColor;
        //recordImage.color = pressedColor;

        UIManager.inst.activeTrack.enabled = false;
        EnableImage(glow, true);
        UIManager.inst.activeTrack = glow;
    }


    //public override void OnPointerUp(PointerEventData eventData)
    //{
    //    base.OnPointerUp(eventData);
    //    //Debug.Log("on pointer up; obj: " + eventData.selectedObject, gameObject);
    //    emptyImage.color = normalColor;
    //    recordImage.color = normalColor;
    //}

    public override void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("pointer enter", gameObject);
        emptyImage.color = highlightedColor;
        recordImage.color = highlightedColor;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("pointer exit", gameObject);

        emptyImage.color = normalColor;
        recordImage.color = normalColor;
    }
}
