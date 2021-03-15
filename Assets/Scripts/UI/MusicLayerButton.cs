using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MusicLayerButton : Button, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Added")]
    public int layer;
    //public Image targetImage;
    public Image unfilled;
    public Image filled;
    public Image glow;


    



    // ------------------------------ Public functions ------------------------------



    




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

        // 1. Glow-image
        EnableGlow();

        // 2. MusicManager
        MusicManager.inst.ChangeLayer(layer);
    }


    //public override void OnPointerUp(PointerEventData eventData)
    //{
    //    base.OnPointerUp(eventData);
    //    //Debug.Log("on pointer up; obj: " + eventData.selectedObject, gameObject);
    //    emptyImage.color = normalColor;
    //    recordImage.color = normalColor;
    //}

    //public override void OnPointerEnter(PointerEventData eventData)
    //{
    //    //Debug.Log("pointer enter", gameObject);
    //    emptyImage.color = highlightedColor;
    //    recordImage.color = highlightedColor;
    //}

    //public override void OnPointerExit(PointerEventData eventData)
    //{
    //    //Debug.Log("pointer exit", gameObject);

    //    emptyImage.color = normalColor;
    //    recordImage.color = normalColor;
    //}

    

    /// <summary>
    /// Disable old and enable new glow-image.
    /// </summary>
    private void EnableGlow()
    {
        // 1. Disable old & enable new graphics
        UIManager.inst.activeLayerButton.glow.enabled = false;
        glow.enabled = true;

        // 2. Set active-variable
        UIManager.inst.activeLayerButton = this;
    }


    private void EnableImage(Image image, bool value)
    {
        image.enabled = value;
    }
}
