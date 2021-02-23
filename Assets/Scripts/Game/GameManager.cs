using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    // Public
    public static GameManager inst;
    [HideInInspector] public int FPS;

    // Private
    private Vector2 screenSize, lastScreenSize;


    private void Awake()
    {
        inst = this;

        FPS = Screen.currentResolution.refreshRate;
    }

    private void OnEnable()
    {
        
    }

    void Start()
    {
        
    }

    


    void Update()
    {
        CheckScreenResize();
    }



    // ----------------------------- private functions ----------------------------


    /// <summary>
    /// If screen size changed, fire event.
    /// </summary>
    private void CheckScreenResize()
    {
        lastScreenSize = screenSize;
        screenSize = new Vector2(Screen.width, Screen.height);

        if (screenSize != lastScreenSize)
        {
            GameEvents.inst.onScreenResize?.Invoke();
        }
    }

    
}
