﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    // Public variables
    public static Player inst;
    public enum PositionState { inside, outside, innerEdge, outerEdge, noTunnel };
    public enum ActionState { stickToEdge, move };
    [HideInInspector] public PositionState positionState = PositionState.noTunnel;
    [HideInInspector] public ActionState actionState = ActionState.move;
    
    [Header("General stuff")]
    public int verticesCount = 3;
    [Range(0, 1f)] public float innerWidth = 0.2f;
    public bool constantInnerWidth = true;
    public float stickToEdgeTolerance = 0.01f;
    public float stickToOuterEdge_holeSize = 0.05f;
    public bool useKeyboard;

    [Header("Mouse")]
    public float rotationMaxSpeed = 5f;
    [Range(0, 1f)] public float rotationTargetVectorFactor = 0.1f;
    [Range(0, 1f)] public float scaleTargetVectorFactor = 0.05f;
    public float scaleMax = 2.7f;
    public float scaleMin = 1f;
    public float scaleMaxSpeed = 0.05f;
    [Range(0, 1f)] public float scaleDamp = 0.2f;
    public float outsideSpeedMin = 2f;
    [Range(0, 1f)] public float outsideSlowFac = 0.3f;
    [Range(0, 1f)] public float scaleEdgeAcc = 0.05f;
    [Range(1, 20f)] public float fastFactor = 3f;
    [Header("Bounce")]
    [Range(0.001f, 0.05f)] public float bounceEntrySpeedScale = 0.005f;
    public float bounceEntrySpeedRot = 0.1f;
    public float maxBounceSpeed = 0.01f;
    public float bounceTime = 2f;
    [Range(1, 20f)]
    public float bounceFactor = 10f;
    public float bounceRecoverTime = 0.5f;
    [Header("Keyboard")]
    public float kb_scaleSpeed = 1f;
    [Range(0.1f, 1f)]
    public float kb_slowScale = 0.9f;
    public float kb_rotationSpeed = 1f;
    [Range(0.1f, 1f)]
    public float kb_slowRotation = 0.9f;

    // Public attributes
    [HideInInspector] public PositionState lastPosState;
    [HideInInspector] public ActionState lastActionState;
    [HideInInspector] public PositionState startPosState;
    [HideInInspector] public float curRotSpeed;
    [HideInInspector] public bool startedBounce = false;
    [HideInInspector] public Vector3[] outerVertices = new Vector3[3];
    [HideInInspector] public Vector3[] innerVertices = new Vector3[3];
    [HideInInspector] public Vector3[] outerVertices_mesh = new Vector3[3];
    [HideInInspector] public Vector3[] innerVertices_mesh = new Vector3[3];
    [HideInInspector] public float curInnerWidth;
    [HideInInspector] public Transform[] outerVertices_obj;
    [HideInInspector] public Transform[] innerVertices_obj;
    [HideInInspector] public float velocity;
    [HideInInspector] public Vector3 mousePos;
    [HideInInspector] public PlayerEdgePart curEdgePart;
    [HideInInspector] public PlayerEdgePart[] curSecEdgeParts;
    [HideInInspector] public Edge curEdge;
    [HideInInspector] public Edge[] curSecEdges;
    [HideInInspector] public bool tunnelEnter;



    // private variables
    private float curPlayerRot;
    private bool mouseIsActive;
    private Vector3 mouseStartPos;
    private Vector3 lastMousePos = Vector3.zero;
    protected private Vector3 midPoint;
    private float rotationTargetAngle;
    private float scaleTargetValue;
    private float lastScaleTargetValue;
    private float scaleDifferenceToLastFrame;
    private float curScaleSpeed = 0;
    private float curMouseRot;
    private float rotTargetValue;
    private float rotDifferenceToLastFrame;
    private float mouseToPlayerDistance;
    private float curPlayerRadius;
    private float tunnelToMidDistance;
    private RaycastHit envPlayerIntersection;
    private float bounceWeight = 0f;
    private float bounceRecoverWeight;
    private float curBounceSpeed;
    private float fastWeight = 1f;
    private float mouseX, mouseY, mouseDelta;
    private float mouseToEnvDistance;
    private float timer;
    private float startScale;



    // get set
    MusicManager musicManager { get { return MusicManager.inst; } }
    VisualController visualController { get { return VisualController.inst; } }

    

    void Start()
    {
        inst = this;
        midPoint = this.transform.position;
        //this.transform.localScale = new Vector3(scaleMin, scaleMin, this.transform.localScale.z);
    }


    void Update()
    {
        ManageMovement();
    }


    // ----------------------------- Main method ----------------------------

    void ManageMovement()
    {
        // 1. Handle Data
        GetInput();
        GetVertices();
        PlayerData.SetPositionStates();
        PlayerData.SetActionStates();
        CalcMovementData();
        PlayerData.CalcEdgeData();

        // 2. Perform movement
        PerformMouseMovement();
    }


    // ----------------------------- Private methods ----------------------------


    // MOUSE
    void CalcMovementData()
    {
        // MOUSE

        if (!useKeyboard)
        {
            // Rotation
            Vector2 mouseToMid = mousePos - midPoint;
            Vector2 playerAngleVec = outerVertices[0] - midPoint;
            float curPlayerRot = -Vector2.SignedAngle(mouseToMid, playerAngleVec);
            curPlayerRot = Mathf.Clamp(curPlayerRot, -rotationMaxSpeed, rotationMaxSpeed); // = max speed
            rotTargetValue = rotationTargetVectorFactor * curPlayerRot;
            curRotSpeed = rotTargetValue;

            // Scale
            mouseToPlayerDistance = 0;
            Vector2 intersection = Vector2.zero;
            Vector3 mousePos_extended = midPoint + (mousePos - midPoint).normalized * 10f;
            if (Physics.Raycast(midPoint, outerVertices[0], out envPlayerIntersection))
            {
                curPlayerRadius = ((Vector2)outerVertices[0] - (Vector2)midPoint).magnitude;
                tunnelToMidDistance = ((Vector2)envPlayerIntersection.point - (Vector2)midPoint).magnitude;
                mouseToEnvDistance = ((Vector2)mousePos - (Vector2)envPlayerIntersection.point).magnitude;
                if (((Vector2)mousePos - (Vector2)midPoint).magnitude < ((Vector2)envPlayerIntersection.point - (Vector2)midPoint).magnitude)
                    mouseToEnvDistance *= -1;
            }
            for (int i = 0; i < outerVertices.Length; i++)
            {
                if (ExtensionMethods.LineSegmentsIntersection(out intersection, mousePos_extended, midPoint, outerVertices[i], outerVertices[(i + 1) % 3]))
                {
                    mouseToPlayerDistance = ((Vector2)mousePos - intersection).magnitude;
                    if ((mousePos - midPoint).magnitude < (intersection - (Vector2)midPoint).magnitude)
                    {
                        mouseToPlayerDistance *= -1; // Maus ist innerhalb Dreieck
                    }
                }
            }

            // Scale value
            scaleTargetValue = mouseToPlayerDistance * scaleTargetVectorFactor;
            scaleTargetValue = Mathf.Clamp(scaleTargetValue, -scaleMaxSpeed, scaleMaxSpeed);
        }

        // KEYBOARD
        else
        {
            int curID = curEdgePart.ID;
            float curRot = this.transform.eulerAngles.z;
            if (InputManager.SelectClockWise())
            {
                int nextID = (curEdgePart.ID + 1).Modulo(VisualController.inst.EdgePartCount);
                Vector3 target = (EnvironmentData.edgeParts[nextID].start + EnvironmentData.edgeParts[nextID].end) / 2f;
                Vector3 targetVec = target - this.transform.position;
                float nextRot = Vector2.Angle(Vector2.up, targetVec);
            }
        }

        

    }



    

    void PerformMouseMovement()
    {
        // = manage states

        // regular move
        if (actionState == ActionState.move)
        {
            if (!useKeyboard)
            {
                if (positionState == PositionState.inside || positionState == PositionState.innerEdge)
                    MoveTowardsMouse("inner");
                else
                    MoveTowardsMouse("outer");
            }
            else
            {

            }
        }
        // action: stick to edge
        else if (actionState == ActionState.stickToEdge)
        {
            if (startPosState == PositionState.inside || startPosState == PositionState.innerEdge)
            {
                StickToEdge("inner");
            }
            else
            {
                StickToEdge("outer");
            }
        }

        // APPLY & clamp (scale & rot)
        curScaleSpeed = Mathf.Clamp(curScaleSpeed, -scaleMaxSpeed, scaleMaxSpeed) * scaleDamp;
        curScaleSpeed = curScaleSpeed * (1 - bounceRecoverWeight) + curBounceSpeed * bounceWeight; // add bounce force & fast speed

        this.transform.localScale += new Vector3(curScaleSpeed * fastWeight, curScaleSpeed * fastWeight, 0);
        this.transform.localScale = ExtensionMethods.ClampVector3_2D(this.transform.localScale, scaleMin, scaleMax);

        this.transform.eulerAngles += new Vector3(0, 0, curRotSpeed * fastWeight);
    }



    void MoveTowardsEdge(string side)
    {
        if (side == "inner")
        {
            float scaleAdd = scaleEdgeAcc;
            if (curScaleSpeed < 0.0001f)
                curScaleSpeed = 0.0001f;

        }
        else if (side == "outer")
        {
            if (curScaleSpeed > -0.0001f)
                curScaleSpeed = -0.0001f;
        }

        if (curPlayerRadius < tunnelToMidDistance)
        {
            curScaleSpeed = Mathf.Pow(Mathf.Abs(curScaleSpeed), scaleEdgeAcc);
        }
        else
        {
            curScaleSpeed = -Mathf.Pow(Mathf.Abs(curScaleSpeed), scaleEdgeAcc);
        }
    }



    void MoveTowardsMouse(string side)
    {
        if (side == "inner")
            // accalerate towards mouse
            curScaleSpeed += scaleTargetValue;
        else if (side == "outer")
        {
            curScaleSpeed = scaleTargetValue * outsideSlowFac;
            curRotSpeed = rotTargetValue * outsideSlowFac;
        }
    }



    void StickToEdge(string side)
    {
        if (side == "inner")
        {
            float borderTargetScaleFactor = tunnelToMidDistance / curPlayerRadius;
            this.transform.localScale = new Vector3(this.transform.localScale.x * borderTargetScaleFactor, this.transform.localScale.y * borderTargetScaleFactor, this.transform.localScale.z);
            curScaleSpeed = 0; // unschön

            // TO DO: bounce?
        }

        else if (side == "outer")
        {
            if (constantInnerWidth)
            {
                float borderTargetScaleFactor = (tunnelToMidDistance + innerWidth + stickToOuterEdge_holeSize) / curPlayerRadius;
                this.transform.localScale *= borderTargetScaleFactor;
                
            }
            else
            {
                float innerVertexDistance = (innerVertices[0] - midPoint).magnitude;
                float borderTargetScaleFactor = (tunnelToMidDistance + stickToOuterEdge_holeSize) / innerVertexDistance;
                this.transform.localScale *= borderTargetScaleFactor;
            }
            curScaleSpeed = 0; // unschön
        }
    }



    IEnumerator BounceForce()
    {
        startedBounce = true;
        float time = 0;
        bounceRecoverWeight = 1;
        while (time < bounceTime)
        {
            time += Time.deltaTime;
            bounceWeight = 1 - time / bounceTime;
            curBounceSpeed = -maxBounceSpeed;
            yield return null;
        }
        time = 0;
        while (time < bounceRecoverTime)
        {
            time += Time.deltaTime;
            bounceRecoverWeight = 1 - time / bounceRecoverTime;
            yield return null;
        }
        startedBounce = false;
    }


    void GetInput()
    {
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.transform.position.z);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        mouseDelta = Mathf.Sqrt(mouseX * mouseX + mouseY * mouseY);

        if (InputManager.FastMoveStart())
            fastWeight = fastFactor;
        else if (InputManager.FastMoveEnd())
            fastWeight = 1f;
        if (InputManager.ActionStart())
        {
            startScale = this.transform.localScale.x;
            startPosState = positionState;
        }
    }

    void GetVertices()
    {
        // get positions from childed vertex-gameobjects
        for (int i = 0; i < verticesCount; i++)
        {
            outerVertices[i] = outerVertices_obj[i].position;
            innerVertices[i] = innerVertices_obj[i].position;
        }
    }


    public float GetVelocityFromDistance()
    {
        float scaleSize = this.transform.localScale.x - startScale;
        velocity = scaleSize.Remap(scaleMin, 0.4f, LoopData.minVelocity, LoopData.maxVelocity);
        velocity = Mathf.Clamp(velocity, LoopData.minVelocity, LoopData.maxVelocity);
        return velocity;
    }
}
