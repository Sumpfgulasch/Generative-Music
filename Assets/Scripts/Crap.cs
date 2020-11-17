using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crap : MonoBehaviour
{
    private FMOD.Studio.EventInstance fmodInstance;

    [FMODUnity.EventRef]
    public string fmodEvent;
    [SerializeField] [Range(0, 1f)]
    private float paramValue = 1f;
    private float testValue;


    bool change = true;
    // Start is called before the first frame update
    void Start()
    {
        fmodInstance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (change)
            {
                fmodInstance.setParameterByName("Parameter 1", paramValue);
                change = false;
            }
            else
            {
                fmodInstance.setParameterByName("Parameter 1", 0);
                change = true;
            }
            fmodInstance.getParameterByName("Parameter 1", out testValue);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        fmodInstance.start();
    }
}
