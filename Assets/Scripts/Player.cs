using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // public variables
    public static Player instance;
    public enum PositionState { inside, outside, edge, noTunnel};
    [Header("General stuff")]
    public Transform[] outerVertices_hack;
    public PositionState positionState;

    [Header("Mouse")]
    public float rotationMaxSpeed = 5f;
    public float rotationAccLimit = 5f;
    public float rotationAcc = 1f;
    public float rotationTargetVectorFactor = 0.1f;
    public float scaleTargetVectorFactor = 0.05f;
    public float scaleAcc = 0.005f;
    public float scaleMax = 2.7f;
    public float scaleMin = 1f;
    public float scaleSensitivity = 0.3f;
    [Header("Mouse V2")]
    public float scaleAcc2 = 0.05f;
    public float scaleBreak = 0.1f;
    public float scaleMaxSpeed = 0.05f;
    public float scaleDamp = 0.2f;
    public float breakoutSpeedMin = 2f;
    public float breakoutSlowFac = 0.3f;
    public float scaleEdgeAcc = 0.05f;

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

    public enum ActionState { bounceInside, stickToWall, letOutside };
    public ActionState actionState = ActionState.bounceInside;
    private bool mouseIsActive;
    private Vector3 mouseStartPos;
    private Vector3 mousePos;
    private Vector3 lastMousePos = Vector3.zero;
    private Vector3 midPoint;
    private float rotationTargetAngle;
    private float scaleTargetValue;
    private float lastScaleTargetValue;
    private float scaleDifferenceToLastFrame;
    [HideInInspector]
    public float curScaleSpeed = 0;
    private float curPlayerRot;
    private float curMouseRot;
    private float rotTargetValue;
    private float lastRotTargetValue;
    private float rotDifferenceToLastFrame;
    private float lastRotDifferenceToLastFrame = 0;

    private Vector3[] outerVertices = new Vector3[3];
    float mouseToPlayerDistance;
    float playerRadius;
    float envDistance;
    RaycastHit envPlayerIntersection;


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
        ManageMovement();
    }

    // -------------------- MOVEMENT ---------------------

    void ManageMovement()
    {
        SetPlayerActionStates();
        CalcMovementData();
        KeyboardMovement();

        PerformMovement();
    }

    // MOUSE
    void CalcMovementData()
    {
        // Input & data
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.transform.position.z);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        for (int i = 0; i < 3; i++)
            outerVertices[i] = outerVertices_hack[i].position;


        // ROTATION
        Vector2 mouseToMid = mousePos - midPoint;
        Vector2 playerAngleVec = outerVertices[0] - midPoint;
        curPlayerRot = -Vector2.SignedAngle(mouseToMid, playerAngleVec);
        curPlayerRot = Mathf.Clamp(curPlayerRot, -rotationMaxSpeed, rotationMaxSpeed); // max speed
        rotTargetValue = rotationTargetVectorFactor * curPlayerRot;
        // acc
        //rotDifferenceToLastFrame = rotTargetValue - lastRotTargetValue;
        //if (Mathf.Abs(rotDifferenceToLastFrame) >= rotationAccLimit && rotDifferenceToLastFrame > 0)
        //{
        //    if (Mathf.Abs(rotTargetValue) <= 0.01)
        //        rotTargetValue = 0.01f * Mathf.Sign(rotTargetValue);
        //}
        lastRotDifferenceToLastFrame = rotTargetValue - lastRotTargetValue; // last rot
        lastRotTargetValue = rotTargetValue;

        

        // SCALE
        mouseToPlayerDistance = 0;
        Vector2 intersection = Vector2.zero;
        Vector3 mousePos_extended = midPoint + (mousePos - midPoint).normalized * 10f;
        for (int i = 0; i < outerVertices.Length; i++)
        {
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, mousePos_extended, midPoint, outerVertices[i], outerVertices[(i + 1) % 3]))
            {
                mouseToPlayerDistance = ((Vector2)mousePos - intersection).magnitude;
                if ((mousePos - midPoint).magnitude < (intersection - (Vector2)midPoint).magnitude)
                    mouseToPlayerDistance *= -1; // Maus ist innerhalb Dreieck
            }
        }

        scaleTargetValue = mouseToPlayerDistance * scaleTargetVectorFactor;
        scaleTargetValue = Mathf.Clamp(scaleTargetValue, -scaleMaxSpeed, scaleMaxSpeed);

        if (Physics.Raycast(midPoint, outerVertices[0], out envPlayerIntersection))
        {
            playerRadius = ((Vector2)outerVertices[0] - (Vector2)midPoint).magnitude;
            envDistance = ((Vector2)envPlayerIntersection.point - (Vector2)midPoint).magnitude;
        }
        else
            print("ERROR: no env hit");
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
    }



    void PerformMovement()
    {
        // = manage states


        // BOUNCE INSIDE
        if (actionState == ActionState.bounceInside)
        {
            if (positionState == PositionState.edge)
            {
                // perform bounce
            }
            else
            {
                curScaleSpeed += scaleTargetValue;
            }
        }

        // STICK TO WALL
        else if (actionState == ActionState.stickToWall)
        {
            // STICK TO EDGE
            if (positionState == PositionState.edge)
            {
                float borderTargetScaleFactor = envDistance / playerRadius;
                this.transform.localScale = new Vector3(this.transform.localScale.x * borderTargetScaleFactor, this.transform.localScale.y * borderTargetScaleFactor, this.transform.localScale.z);
                curScaleSpeed = 0; // unschön
            }
            // MOVE PLAYER TOWARDS EDGE
            else
            {
                float scaleAdd = scaleEdgeAcc;
                if (playerRadius > envDistance)
                    scaleAdd *= -1; // (outside)
                curScaleSpeed += scaleAdd;
            }
        }

        // LET OUTSIDE
        else if (actionState == ActionState.letOutside)
        {
            if (positionState == PositionState.outside)
            {
                // verlangsamen
                curScaleSpeed = scaleTargetValue * breakoutSlowFac;
            }
            else
            {
                // normal wie bei bounce
                curScaleSpeed += scaleTargetValue;
            }
        }

        // APPLY & clamp (scale & rot)
        curScaleSpeed = Mathf.Clamp(curScaleSpeed, -scaleMaxSpeed, scaleMaxSpeed) * scaleDamp;
        this.transform.localScale += new Vector3(curScaleSpeed, curScaleSpeed, 0);
        this.transform.localScale = ExtensionMethods.ClampVector3_2D(this.transform.localScale, scaleMin, scaleMax);
        this.transform.eulerAngles += new Vector3(0, 0, rotTargetValue);
    }


    void SetPlayerActionStates()
    {
        // Spieleraktions-States
        if (Input.GetMouseButtonDown(0))
            actionState = ActionState.stickToWall;

        if (Input.GetMouseButtonUp(0))
            actionState = ActionState.bounceInside;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float mouseDelta = Mathf.Sqrt(mouseX * mouseX + mouseY * mouseY);
        if (mouseDelta > breakoutSpeedMin)
            actionState = ActionState.letOutside;
    }
    

    // ----------------------- Events ----------------------


    

    void OnMouseDown()
    {
        
    }

    void OnMouseOver()
    {

    }

    void OnMouseExit()
    {

    }

    void OnMouseUp()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        print("Collisionenter");
    }

    void OnTriggerEnter(Collider other)
    {
        print("triggerEnter");
    }
}
