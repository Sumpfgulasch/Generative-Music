using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;
    [HideInInspector] public InputActionAssetClass inputActionAssetClass;

    [HideInInspector] public int FPS;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        inputActionAssetClass = new InputActionAssetClass();
        inputActionAssetClass.Enable();
    }

    void Start()
    {
        inst = this;

        FPS = Screen.currentResolution.refreshRate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
