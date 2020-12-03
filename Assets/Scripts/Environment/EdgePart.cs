using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePart
{
    public enum Type { InsideMainKey, OutsideMainKey, NewMainKey, PlayerMain, PlayerSec };

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
    public EdgePart(Type _type)
    {
        type = _type;
    }

    public EdgePart(int _ID, Vector3 _start, Vector3 _end, bool _isCorner)
    {
        ID = _ID;

        // set color, linerend
    }
    

    



    void Update()
    {
        // LOGIK
        // if (typeChanged || isPlayed)
        //      ChangeColor()
    }

    public void Set(int _ID, Vector3 _start, Vector3 _end, bool _isCorner)
    {
        ID = _ID;

    }
}
