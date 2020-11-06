using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // public variables
    public static Player instance;
    public enum State { inside, outside, perfect, noTunnel};
    [Header("General stuff")]
    public Transform[] outerVertices_hack;
    public State state;

    [Header("Mouse")]
    public float rotationMaxSpeed = 5f;
    public float rotationTargetVectorFactor = 0.1f;
    public float scaleTargetVectorFactor = 0.05f;
    public float scaleAcc = 0.005f;
    public float scaleMaxSpeed = 0.05f;
    public float scaleMax = 2.7f;
    public float scaleMin = 1f;
    public float scaleSensitivity = 0.3f;

    [Header("Keyboard")]
    public float kb_scaleSpeed = 1f;
    [Range(0.1f, 1f)]
    public float kb_slowScale = 0.9f;
    public float kb_rotationSpeed = 1f;
    [Range(0.1f, 1f)]
    public float kb_slowRotation = 0.9f;
    
    // private variables
    private MeshRenderer meshRenderer;
    private Color defaultColor;
    private Color moveColor;

    public enum MouseState { hover, scale, rotate, StickToWall, none };
    public MouseState mouseState = MouseState.none;
    private bool mouseIsActive;
    private Vector3 mouseStartPos;
    private Vector3 mousePos;
    private Vector3 lastMousePos = Vector3.zero;
    private Vector3 midPoint;
    private float rotationTargetAngle;
    private float scaleTargetValue;
    private float lastScaleTargetValue;
    private float scaleDifferenceToLastFrame;
    private float curPlayerAngle;
    private float curMouseAngle;
    private float angleTargetValue;
    private float lastAngleTargetValue;

    private Vector3[] outerVertices = new Vector3[3];


    void Start()
    {
        instance = this;
        meshRenderer = this.transform.GetComponentInChildren<MeshRenderer>();
        defaultColor = meshRenderer.material.color;
        moveColor = defaultColor * 1.2f;
        midPoint = this.transform.position;
    }


    void Update()
    {
        GetInput();
        GetData();
        ManageMovement();
    }

    // -------------------- MOVEMENT ---------------------

    void ManageMovement()
    {
        
        MouseMovement();
        KeyboardMovement();
    }

    // MOUSE
    void MouseMovement()
    {
        if (mouseState == MouseState.hover)
        {
            meshRenderer.material.color = Color.white;
        }

        else if (mouseState == MouseState.StickToWall)
        {
            
        }

        else if (mouseState == MouseState.scale)
        {
            
        }

        else
        {
            meshRenderer.material.color = defaultColor;
        }



        // SCALE
        // 1) Get mouse to player-distance
        float mouseToPlayerDistance = 0;
        Vector2 intersection;
        Vector3 mousePos_extended = midPoint + (mousePos - midPoint).normalized * 10f;
        for (int i = 0; i < outerVertices.Length; i++)
        {
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, mousePos_extended, midPoint, outerVertices[i], outerVertices[(i + 1) % 3]))
            {
                mouseToPlayerDistance = ((Vector2)mousePos - intersection).magnitude;
                if ((mousePos - midPoint).magnitude < (intersection - (Vector2)midPoint).magnitude)
                    // Maus ist innerhalb Dreieck
                    mouseToPlayerDistance *= -1;

                Debug.DrawLine(new Vector3(mousePos.x, mousePos.y, this.transform.position.z), new Vector3(intersection.x, intersection.y, this.transform.position.z), Color.green);
            }
        }

        // 2) Add scale
        scaleTargetValue = scaleTargetVectorFactor * mouseToPlayerDistance;
        // max speed
        scaleTargetValue = Mathf.Clamp(scaleTargetValue, -scaleMaxSpeed, scaleMaxSpeed);
        // max acceleration
        scaleDifferenceToLastFrame = scaleTargetValue - lastScaleTargetValue;
        if (scaleDifferenceToLastFrame >= scaleAcc)
            scaleTargetValue = lastScaleTargetValue + Mathf.Sign(scaleDifferenceToLastFrame) * scaleAcc;
        // last scale
        lastScaleTargetValue = scaleTargetValue;
        // apply & clamp
        this.transform.localScale += new Vector3(scaleTargetValue, scaleTargetValue, 0);
        this.transform.localScale = ExtensionMethods.ClampVector3_2D(this.transform.localScale, scaleMin, scaleMax);


        // ROTATION
        Vector2 mouseToMid = mousePos - midPoint;
        Vector2 playerAngleVec = outerVertices[0] - midPoint;
        curPlayerAngle = Vector2.SignedAngle(mouseToMid, playerAngleVec);
        // max speed
        curPlayerAngle = Mathf.Clamp(curPlayerAngle, -rotationMaxSpeed, rotationMaxSpeed);

        angleTargetValue = rotationTargetVectorFactor * curPlayerAngle;
        print("targetAngle: " + angleTargetValue);
        // last rot
        lastScaleTargetValue = scaleTargetValue;
        this.transform.eulerAngles += new Vector3(0, 0, angleTargetValue);

        Debug.DrawLine(outerVertices[0], midPoint, Color.green);
        Debug.DrawLine(mousePos, midPoint, Color.blue); // HIER WEITERMACHEN

    }

    // KEYBOARD
    void KeyboardMovement()
    {
        // ROTATION
        float horizontalAxis = Input.GetAxis("Horizontal");
        float slowAxis = Input.GetAxis("Slow");
        float slowRotValue = slowAxis + 1 - (2 * slowAxis * kb_slowRotation);
        if (horizontalAxis != 0)
        {
            this.transform.eulerAngles -= new Vector3(0, 0, horizontalAxis * kb_rotationSpeed * slowRotValue);
        }

        // SCALE
        float verticalAxis = Input.GetAxis("Vertical");
        float slowScaleFactor = slowAxis + 1 - (2 * slowAxis * kb_slowScale);
        float scaleValue = -verticalAxis * kb_scaleSpeed * slowScaleFactor;
        if (verticalAxis != 0)
        {
            this.transform.localScale += new Vector3(scaleValue, scaleValue, 0);
        }

        this.transform.localScale = ExtensionMethods.ClampVector3_2D(this.transform.localScale, scaleMin, scaleMax);

    }

    

    // ----------------------- Events ----------------------


    

    void OnMouseDown()
    {
        
    }

    void OnMouseOver()
    {
        //print("mousehover");
        //if (Input.GetMouseButtonDown(1))
        //{
        //    mouseState = MouseState.StickToWall;
        //    lastMousePos = mousePos;
        //}
        //else if (Input.GetMouseButton(0))
        //{
        //    mouseState = MouseState.scale;
        //    lastMousePos = mousePos;
        //}
        //else if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        //    mouseState = MouseState.hover;
    }

    void OnMouseExit()
    {
        //if (mouseState != MouseState.StickToWall && mouseState != MouseState.scale)
        //    mouseState = MouseState.none;
    }

    void OnMouseUp()
    {
        //mouseState = MouseState.none;
    }

    void OnMouseUp_Right()
    {
        if (Input.GetMouseButtonUp(1))
        {
            mouseState = MouseState.none;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        print("Collisionenter");
    }

    void OnTriggerEnter(Collider other)
    {
        print("triggerEnter");
    }

    void GetData()
    {
        for (int i = 0; i < 3; i++)
            outerVertices[i] = outerVertices_hack[i].position;
    }

    void GetInput()
    {
        OnMouseUp_Right();
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.transform.position.z);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
    }
}
