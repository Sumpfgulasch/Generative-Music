using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIOps : MonoBehaviour
{
    public static UIOps inst;

    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    

    void Start()
    {
        inst = this;

        graphicRaycaster = GetComponent<GraphicRaycaster>();
        eventSystem = GetComponent<EventSystem>();

        HighlightLayerButton(0);
        
    }

    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointerPos">In pixels.</param>
    /// <returns></returns>
    public List<RaycastResult> RaycastGraphics(Vector2 pointerPos)
    {
        pointerEventData = new PointerEventData(eventSystem)
        {
            position = pointerPos
        };

        var raycastResults = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, raycastResults);

        return raycastResults;
    }

    public bool PointerHitsUI(Vector2 pointerPos)
    {
        var hits = RaycastGraphics(pointerPos);
        foreach(RaycastResult hit in hits)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Change the image of the active track: enable the filled or unfilled image.
    /// </summary>
    /// <param name="layer">[0-4].</param>
    /// <param name="value">true == filled (contains recording), false == unfilled (no recording).</param>
    public void EnableRecordedTrackImage(int layer, bool value)
    {
        var activeButton = UIManager.inst.layerButtons[layer];
        activeButton.filled.enabled = value;
        if (value == true)
            activeButton.targetGraphic = UIManager.inst.activeLayerButton.filled;
        else
            activeButton.targetGraphic = UIManager.inst.activeLayerButton.unfilled;
    }

    public void HighlightLayerButton(int layer)
    {
        // 1. Disable old & enable new graphics
        UIManager.inst.activeLayerButton.glow.enabled = false;
        UIManager.inst.layerButtons[layer].glow.enabled = true;

        // 2. Set active-variable
        UIManager.inst.activeLayerButton = UIManager.inst.layerButtons[layer];
    }


    public void SetPrecisionText(MusicManager.Precision value)
    {
        MeshRef.inst.quantizePrecision.text = value.ToString();

        //if (value == MusicManager.Precision.fine)
        //{
        //    text.text = value.ToString();
        //}
        //else if (value == MusicManager.Precision.middle)
        //{

        //}
        //else if (value == MusicManager.Precision.rough)
        //{

        //}
    }
}
