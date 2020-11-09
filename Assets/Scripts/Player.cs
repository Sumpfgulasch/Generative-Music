﻿using System.Collections;
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
    [HideInInspector]
    public float curScaleSpeed = 0;
    private float curPlayerRot;
    private float curMouseRot;
    private float rotTargetValue;
    private float lastRotTargetValue;
    private float rotDifferenceToLastFrame;
    private float lastRotDifferenceToLastFrame = 0;

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
        // ROTATION
        Vector2 mouseToMid = mousePos - midPoint;
        Vector2 playerAngleVec = outerVertices[0] - midPoint;
        curPlayerRot = -Vector2.SignedAngle(mouseToMid, playerAngleVec);
        // max speed
        curPlayerRot = Mathf.Clamp(curPlayerRot, -rotationMaxSpeed, rotationMaxSpeed);
        rotTargetValue = rotationTargetVectorFactor * curPlayerRot;
        // acc
        rotDifferenceToLastFrame = rotTargetValue - lastRotTargetValue;
        if (Mathf.Abs(rotDifferenceToLastFrame) >= rotationAccLimit && rotDifferenceToLastFrame > 0)
        {
            if (Mathf.Abs(rotTargetValue) <= 0.01)
            {
                rotTargetValue = 0.01f * Mathf.Sign(rotTargetValue);
                //lastRotTargetValue = rotTargetValue;
                //lastRotDifferenceToLastFrame = 0.01f  * rotationAcc;
            }
            //else
            //    rotTargetValue = lastRotDifferenceToLastFrame * rotationAcc;

            //print("ACC! lastDiff:: " + lastRotDifferenceToLastFrame + ", curDiff: " + rotDifferenceToLastFrame);
        }
        // last rot
        lastRotDifferenceToLastFrame = rotTargetValue - lastRotTargetValue;
        lastRotTargetValue = rotTargetValue;
        // apply
        this.transform.eulerAngles += new Vector3(0, 0, rotTargetValue);




        // SCALE
        // V1
        // 1) Get mouse to player-distance
        float mouseToPlayerDistance = 0;
        Vector2 intersection = Vector2.zero;
        Vector3 mousePos_extended = midPoint + (mousePos - midPoint).normalized * 10f;
        for (int i = 0; i < outerVertices.Length; i++)
        {
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, mousePos_extended, midPoint, outerVertices[i], outerVertices[(i + 1) % 3]))
            {
                mouseToPlayerDistance = ((Vector2)mousePos - intersection).magnitude;
                if ((mousePos - midPoint).magnitude < (intersection - (Vector2)midPoint).magnitude)
                    // Maus ist innerhalb Dreieck
                    mouseToPlayerDistance *= -1;
            }
        }

        #region old scale
        //// 2) Add scale
        //scaleTargetValue = scaleTargetVectorFactor * mouseToPlayerDistance;
        //// max speed
        //scaleTargetValue = Mathf.Clamp(scaleTargetValue, -scaleMaxSpeed, scaleMaxSpeed);
        //// max acceleration
        ////scaleDifferenceToLastFrame = scaleTargetValue - lastScaleTargetValue;
        ////if (scaleDifferenceToLastFrame >= scaleAcc)
        ////    scaleTargetValue = lastScaleTargetValue + Mathf.Sign(scaleDifferenceToLastFrame) * scaleAcc;
        //// last scale
        //lastScaleTargetValue = scaleTargetValue;
        //// apply & clamp
        //this.transform.localScale += new Vector3(scaleTargetValue, scaleTargetValue, 0);
        //this.transform.localScale = ExtensionMethods.ClampVector3_2D(this.transform.localScale, scaleMin, scaleMax);
        #endregion

        if (state == State.outside)
        {
            //curScaleSpeed = -Mathf.Abs(Player.instance.curScaleSpeed);
            //curScaleSpeed = 0;
            Vector3 envPlayerIntersection;
            
            RaycastHit hit;
            if (Physics.Raycast(midPoint, outerVertices[0], out hit))
            {
                Debug.DrawLine(hit.point, outerVertices[0], Color.yellow);
                float playerRadius = ((Vector2)outerVertices[0] - (Vector2)midPoint).magnitude;
                float envDistance = ((Vector2)hit.point - (Vector2)midPoint).magnitude;
                float borderTargetScaleFactor = playerRadius / envDistance;
                print("borderFac: " + borderTargetScaleFactor);
                this.transform.localScale = new Vector3(this.transform.localScale.x * borderTargetScaleFactor, this.transform.localScale.y * borderTargetScaleFactor, this.transform.localScale.z);
            }
            else
                print("ERROR: no env hit");

        }
        else
        {
            // INSIDE

            scaleTargetValue = scaleTargetVectorFactor * mouseToPlayerDistance;
            // max speed
            scaleTargetValue = Mathf.Clamp(scaleTargetValue, -scaleMaxSpeed, scaleMaxSpeed);
            curScaleSpeed += scaleTargetValue;
            curScaleSpeed = Mathf.Clamp(curScaleSpeed, -scaleMaxSpeed, scaleMaxSpeed) * scaleDamp;
        }

        // apply & clamp
        this.transform.localScale += new Vector3(curScaleSpeed, curScaleSpeed, 0);
        this.transform.localScale = ExtensionMethods.ClampVector3_2D(this.transform.localScale, scaleMin, scaleMax);
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
