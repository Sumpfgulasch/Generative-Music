using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePart
{
    public enum Type { MainKey, OutsideMainKey, NewMainKey };

    // Properties
    public int ID;
    public Type type;
    public Vector3 start;
    public Vector3 end;
    public bool isCorner;
    public Vector3 cornerMid;

    // Private variables
    private LineRenderer lineRend;
    private Color color;


    // Contructor
    public EdgePart(int _ID, Type _type, Vector3 _start, Vector3 _end, bool _isCorner)
    {
        ID = _ID;

        // set color, linerend
    }
    
    public EdgePart(int _ID, Type _type, Vector3 _start, Vector3 _end, bool _isCorner, Vector3 _cornerMid)
    {
        ID = _ID;
    }


    void Start()
    {
        
    }


    void Update()
    {
        // LOGIK
        // if (typeChanged || isPlayed)
        //      ChangeColor()
    }
}
