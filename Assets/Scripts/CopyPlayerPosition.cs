using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPlayerPosition : MonoBehaviour
{
    public enum Side { Inner, Outer};
    public Side copySide = Side.Inner;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var outerVertices = Player.inst.OuterVertices;
        var innerVertices = Player.inst.InnerVertices;

        if (outerVertices != null && innerVertices != null)
        {
            if (copySide == Side.Inner)
            {
                if (innerVertices != null)
                {
                    transform.position = innerVertices[0];
                }
            }
            else
            {
                if (outerVertices != null)
                {
                    transform.position = outerVertices[0];
                }

            }
        }
    }
}
