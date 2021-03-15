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
                Debug.Log("pointer hits UI", gameObject);
                return true;
            }
        }
        Debug.Log("pointer does NOT hit UI", gameObject);
        return false;
    }
}
