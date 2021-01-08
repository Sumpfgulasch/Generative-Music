using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject start;
    public GameObject end;
    float FPS;

    private float deltaTime
    {
        get { return Time.deltaTime * FPS; }
    }

    // Start is called before the first frame update
    void Start()
    {
        FPS = Screen.currentResolution.refreshRate;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position -= new Vector3(0, 0, ObjectSpawner.instance.moveSpeed * deltaTime);
    }
}
