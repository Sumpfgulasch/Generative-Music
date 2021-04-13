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

        UpdateLayerButton(0);

        SetPrecisionText(MusicManager.inst.curPrecision);
        
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

   


    /// <summary>
    /// Change layer in MusicManager (ggf. stop playing notes) and update layer button image and variable.
    /// </summary>
    /// <param name="layer"></param>
    public void ChangeLayer(int layer)
    {
        MusicManager.inst.ChangeLayer(layer);
        UpdateLayerButton(layer);
    }




    public void IncreasePrecision()
    {
        var precision = MusicManager.inst.IncreasePrecision();

        SetPrecisionText(precision);
    }





    /// <summary>
    /// Disable old glow-image, enable new. Set activeLayerButton in UI-manager.
    /// </summary>
    /// <param name="layer"></param>
    private void UpdateLayerButton(int layer)
    {
        // 1. Disable old & enable new graphics
        UIManager.inst.activeLayerButton.glow.enabled = false;
        UIManager.inst.layerButtons[layer].glow.enabled = true;

        // 2. Set active-variable
        UIManager.inst.activeLayerButton = UIManager.inst.layerButtons[layer];
    }


    private void SetPrecisionText(MusicManager.Precision value)
    {
        MeshRef.inst.quantizePrecision.text = value.ToString();
    }
}
