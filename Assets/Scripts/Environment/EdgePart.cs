using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePart
{
    public enum Type { InsideMainKey, OutsideMainKey, NewMainKey};

    // Properties
    public int ID;
    public Type type;
    public Vector3 start;
    public Vector3 end;
    public bool isCorner;
    public Vector3 cornerMid;

    // Private variables
    protected LineRenderer lineRend;
    private Color color;

    
    // Contructors
    public EdgePart()
    {
        
    }

    public EdgePart(Type _type)
    {
        type = _type;
    }

    public EdgePart(int _ID, LineRenderer _lineRend, bool _isCorner)
    {
        ID = _ID;
        lineRend = _lineRend;
        isCorner = _isCorner;
    }
    

    



    void Update()
    {
        // LOGIK
        // if (typeChanged || isPlayed)
        //      ChangeColor()
    }

    public void Set(Vector3 _start, Vector3 _end)
    {
        //ID = _ID;
        start = _start;
        end = _end;
        //isCorner = _isCorner;

        start.z = Player.inst.transform.position.z - 0.001f;
        end.z = Player.inst.transform.position.z - 0.001f;

        lineRend.SetPosition(0, start);
        lineRend.SetPosition(1, end);
    }

    public void Set(int _ID, Vector3 _start, Vector3 _end, bool _isCorner)
    {
        ID = _ID;
        start = _start;
        end = _end;
        isCorner = _isCorner;

        start.z = Player.inst.transform.position.z - 0.001f;
        end.z = Player.inst.transform.position.z - 0.001f;

        lineRend.SetPosition(0, start);
        lineRend.SetPosition(1, end);
    }
}



// ---------------------------------------------------------------------------------------

    

public class PlayerEdgePart : EdgePart
{
    // Contructor
    public PlayerEdgePart(Type _type, LineRenderer _lineRend)
    {
        type = _type;
        lineRend = _lineRend;
    }

    public new enum Type {Main, Second};
    public new Type type;
    public bool changed;


    public new void Set(Vector3 _start, Vector3 _end)
    {
        //ID = _ID;
        start = _start;
        end = _end;
        //isCorner = _isCorner;

        start.z = Player.inst.transform.position.z - 0.002f;
        end.z = Player.inst.transform.position.z - 0.002f;

        lineRend.SetPosition(0, start);
        lineRend.SetPosition(1, end);
    }

}


// ---------------------------------------------------------------------------------------


public class Edge
{
    public bool changed;
    public float percentage;
    public Vector3 start, end;
    public bool firstTouch;

    public void Set (Vector3 _start, Vector3 _end, bool _changed, float _percentage)
    {
        start = _start;
        end = _end;
        changed = _changed;
        percentage = _percentage;
    }
}
