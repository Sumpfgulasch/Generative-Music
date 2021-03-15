using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class test : MonoBehaviour, ICancelHandler, ISubmitHandler, IPointerUpHandler, IPointerClickHandler
{
    public void OnCancel(BaseEventData eventData)
    {
        Debug.Log("cancel", gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("click", gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("pointer up", gameObject);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log("submit", gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
