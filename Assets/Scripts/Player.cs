using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // public variables
    public static Player instance;
    public enum PositionState { inside, outside, edge, noTunnel};
    public enum ActionState { bounceInside, stickToWall, letOutside };
    [Header("General stuff")]
    public int verticesCount = 3;
    [Range(0,1f)]
    public float innerWidth = 0.2f;
    public PositionState positionState = PositionState.noTunnel;
    public ActionState actionState = ActionState.bounceInside;
    [HideInInspector]
    public PositionState lastPosState;
    public float edgeTolerance = 0.01f;

    [Header("Mouse")]
    public float rotationMaxSpeed = 5f;
    [Range(0,1f)]
    public float rotationTargetVectorFactor = 0.1f;
    [Range(0, 1f)]
    public float scaleTargetVectorFactor = 0.05f;
    public float scaleMax = 2.7f;
    public float scaleMin = 1f;
    public float scaleMaxSpeed = 0.05f;
    [Range(0, 1f)]
    public float scaleDamp = 0.2f;
    public float breakoutSpeedMin = 2f;
    [Range(0, 1f)]
    public float breakoutSlowFac = 0.3f;
    [Range(0, 1f)]
    public float scaleEdgeAcc = 0.05f;
    [Range(1, 20f)]
    public float fastFactor = 3f;
    [Header("Bounce")]
    [Range(0.001f, 0.05f)]
    public float bounceEntrySpeedScale = 0.005f;
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

    [Header("Music")]
    public AudioHelm.HelmController helmController;

    [HideInInspector]
    public bool startedBounce = false;
    [HideInInspector]
    public Vector3[] outerVertices = new Vector3[3];
    [HideInInspector]
    public Vector3[] innerVertices = new Vector3[3];
    [HideInInspector]
    public Vector3[] outerMeshVertices = new Vector3[3];
    [HideInInspector]
    public Vector3[] innerMeshVertices = new Vector3[3];
    [HideInInspector]
    public float curInnerWidth;
    [HideInInspector]
    public Transform[] outerVertices_obj;
    [HideInInspector]
    public Transform[] innerVertices_obj;


    // private variables
    private MeshRenderer meshRenderer;
    private Color defaultColor;
    private Color moveColor;


    private bool mouseIsActive;
    private Vector3 mouseStartPos;
    private Vector3 mousePos;
    private Vector3 lastMousePos = Vector3.zero;
    private Vector3 midPoint;
    private float rotationTargetAngle;
    private float scaleTargetValue;
    private float lastScaleTargetValue;
    private float scaleDifferenceToLastFrame;
    private float curScaleSpeed = 0;
    private float curPlayerRot;
    private float curRotSpeed;
    private float curMouseRot;
    private float rotTargetValue;
    private float lastRotTargetValue;
    private float rotDifferenceToLastFrame;
    private float lastRotDifferenceToLastFrame = 0;
    float mouseToPlayerDistance;
    float playerRadius;
    float envDistance;
    RaycastHit envPlayerIntersection;
    private float bounceWeight = 0f;
    private float bounceRecoverWeight;
    private float curBounceSpeed;
    private float fastWeight = 1f;
    private float mouseX, mouseY, mouseDelta;



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
        GetInput();
        GetData();

        SetPlayerActionStates();
        CalcMovementData();

        KeyboardMovement();

        PerformMovement();
    }

    // MOUSE
    void CalcMovementData()
    {
        // ROTATION
        Vector2 mouseToMid = mousePos - midPoint;
        Vector2 playerAngleVec = outerVertices[0] - midPoint;
        curPlayerRot = -Vector2.SignedAngle(mouseToMid, playerAngleVec);
        curPlayerRot = Mathf.Clamp(curPlayerRot, -rotationMaxSpeed, rotationMaxSpeed); // max speed
        rotTargetValue = rotationTargetVectorFactor * curPlayerRot;
        curRotSpeed = rotTargetValue;
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
        //else
        //    print("ERROR: no env hit");
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
                if ((curScaleSpeed > bounceEntrySpeedScale || curRotSpeed > bounceEntrySpeedRot) && lastPosState != PositionState.edge)
                {
                    StartCoroutine(BounceForce());

                    if (!helmController.IsNoteOn(50))
                        helmController.NoteOn(50, 0.5f, 0.2f);
                }
                else if (mouseToPlayerDistance > 0)
                {
                    float borderTargetScaleFactor = envDistance / playerRadius;
                    this.transform.localScale = new Vector3(this.transform.localScale.x * borderTargetScaleFactor, this.transform.localScale.y * borderTargetScaleFactor, this.transform.localScale.z);
                    curScaleSpeed = 0; // unschön

                    helmController.NoteOn(50, 0.5f);
                }
                else
                {
                    curScaleSpeed += scaleTargetValue;
                    helmController.NoteOff(50);
                }
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

                if (!helmController.IsNoteOn(50))
                    helmController.NoteOn(50, 0.5f, 0.2f);
            }
            // MOVE PLAYER TOWARDS EDGE
            else
            {
                float scaleAdd = scaleEdgeAcc;
                if (curScaleSpeed < 0.0001f)
                    curScaleSpeed = 0.0001f;
                if (playerRadius < envDistance)
                    curScaleSpeed = Mathf.Pow(curScaleSpeed, scaleEdgeAcc);
                else
                    curScaleSpeed = Mathf.Pow(curScaleSpeed, 1 + (1 - scaleEdgeAcc));

                helmController.NoteOff(50);
            }
        }

        // LET OUTSIDE
        else if (actionState == ActionState.letOutside)
        {
            if (positionState == PositionState.outside)
            {
                // verlangsamen
                curScaleSpeed = scaleTargetValue * breakoutSlowFac;
                curRotSpeed = rotTargetValue * breakoutSlowFac;
            }
            else
            {
                // normal wie bei bounce
                curScaleSpeed += scaleTargetValue;
            }
        }



        // APPLY & clamp (scale & rot)
        curScaleSpeed = Mathf.Clamp(curScaleSpeed, -scaleMaxSpeed, scaleMaxSpeed) * scaleDamp;
        curScaleSpeed = curScaleSpeed * (1 - bounceRecoverWeight) + curBounceSpeed * bounceWeight; // add bounce force & fast speed
        
        this.transform.localScale += new Vector3(curScaleSpeed * fastWeight, curScaleSpeed * fastWeight, 0);
        this.transform.localScale = ExtensionMethods.ClampVector3_2D(this.transform.localScale, scaleMin, scaleMax);

        this.transform.eulerAngles += new Vector3(0, 0, curRotSpeed * fastWeight);
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

        if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
            fastWeight = fastFactor;
        else if (Input.GetMouseButtonUp(1))
            fastWeight = 1f;
    }

    void GetData()
    {
        // get positions from childed vertex-gameobjects
        for (int i=0; i<verticesCount; i++)
        {
            outerVertices[i] = outerVertices_obj[i].position;
            innerVertices[i] = innerVertices_obj[i].position;
        }
        
    }


    void SetPlayerActionStates()
    {
        // Spieleraktions-States
        if (positionState == PositionState.outside || mouseDelta > breakoutSpeedMin)
            actionState = ActionState.letOutside;
        else
            actionState = ActionState.bounceInside;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            actionState = ActionState.stickToWall;
    }
}
