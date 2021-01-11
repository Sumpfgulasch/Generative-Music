using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // stuff
    public static Player inst;
    public enum PositionState { inside, outside, innerEdge, outerEdge, noTunnel };
    public enum ActionState { stickToEdge, none };
    [HideInInspector] public PositionState positionState = PositionState.noTunnel;
    [HideInInspector] public ActionState actionState = ActionState.none;

    // Public variables
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
    [Range(0.1f, 1f)]
    public float kb_rotationDamp = 0.8f;

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
    protected private Vector3 midPoint;
    private float scaleTargetValue;
    private float curScaleSpeed = 0;
    private float rotTargetValue;
    private float mouseToPlayerDistance;
    private float curPlayerRadius;
    private float tunnelToMidDistance;
    private RaycastHit envPlayerIntersection;
    private float fastWeight = 1f;
    private float mouseX, mouseY, mouseDelta;
    private float mouseToEnvDistance;
    private float startScale;
    Vector3 targetPos = Vector3.up;
    private InputAction makeMusicAction, selectRightAction, selectLeftAction;



    // get set
    MusicManager musicManager { get { return MusicManager.inst; } }
    VisualController visualController { get { return VisualController.inst; } }

    private float deltaTime { get { return Time.deltaTime * GameManager.inst.FPS; } }

    



    private void Awake()
    {
        var inputActionAssetClass = GameManager.inst.inputActionAssetClass;
        makeMusicAction = inputActionAssetClass.Gameplay.MakeMusic;
        // todo: other actions

        // Add listeners
        makeMusicAction.started += OnMakeMusicStarted;

    }

    private void OnEnable()
    {
        makeMusicAction.Enable();
        selectRightAction.Enable();
        selectLeftAction.Enable();
    }

    void Start()
    {
        inst = this;
        midPoint = this.transform.position;
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
        //PlayerData.SetActionStates();
        CalcMovementData();
        PlayerData.CalcEdgeData();

        // 2. Perform movement
        PerformMovement();
    }


    // ----------------------------- Private methods ----------------------------


    // MOUSE
    void CalcMovementData()
    {
        // MOUSE
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



    

    void PerformMovement()
    {
        // = manage states

        // Keyboard: select next
        if (useKeyboard)
            RotateToTarget(targetPos);



        // regular move
        if (actionState == ActionState.none)
        {
            // Mouse
            if (!useKeyboard)
            {
                if (positionState == PositionState.inside || positionState == PositionState.innerEdge)
                    MoveTowardsMouse("inner");
                else
                    MoveTowardsMouse("outer");
            }

            // HACK
            if (useKeyboard)
                ButtonScale(-1);
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

    }


    private void ButtonScale(int direction)
    {
        this.transform.localScale += new Vector3(kb_scaleSpeed * 2, kb_scaleSpeed * 2, 0) * direction;
        this.transform.localScale = this.transform.localScale.ClampVector3_2D(scaleMin, scaleMax);              // clamp
    }


    public Vector3 GetNextTargetRotation(int direction)
    {
        int curID = curEdgePart.ID;
        Vector3 targetPos = EdgePart.NextEdgePartMid(curID, direction);

        return targetPos;
    }

    private void RotateToTarget(Vector3 targetPos)
    {
        Vector3 targetVec = targetPos - this.transform.position;
        Vector3 curVec = outerVertices[0] - this.transform.position;

        float nextRot = Vector2.SignedAngle(curVec, targetVec) * kb_rotationDamp * deltaTime;

        this.transform.eulerAngles += new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, nextRot);

        // Hack1 (gegen 180° & 60° Winkel)
        if ((Mathf.Abs(this.transform.eulerAngles.z) > 180f - 0.1f && Mathf.Abs(this.transform.eulerAngles.z) < 180f + 0.1f) ||
            Mathf.Abs(this.transform.eulerAngles.z) > 60f - 0.1f && Mathf.Abs(this.transform.eulerAngles.z) < 60f + 0.1f)
        {
            this.transform.eulerAngles += new Vector3(0, 0, 0.1f);
        }

        // Hack2 (bringt nix)
        CalcMovementData();
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
        // scale
        curScaleSpeed = Mathf.Clamp(curScaleSpeed, -scaleMaxSpeed, scaleMaxSpeed) * scaleDamp;                  // damp
        this.transform.localScale += new Vector3(curScaleSpeed * fastWeight, curScaleSpeed * fastWeight, 0);    // add
        this.transform.localScale = this.transform.localScale.ClampVector3_2D(scaleMin, scaleMax);              // clamp

        // rotation
        this.transform.eulerAngles += new Vector3(0, 0, curRotSpeed * fastWeight);
    }



    void StickToEdge(string side)
    {
        // SCALE only
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
                this.transform.localScale = new Vector3(this.transform.localScale.x * borderTargetScaleFactor, this.transform.localScale.y * borderTargetScaleFactor, this.transform.localScale.z);

            }
            else
            {
                float innerVertexDistance = (innerVertices[0] - midPoint).magnitude;
                float borderTargetScaleFactor = (tunnelToMidDistance + stickToOuterEdge_holeSize) / innerVertexDistance;
                this.transform.localScale = new Vector3(this.transform.localScale.x * borderTargetScaleFactor, this.transform.localScale.y * borderTargetScaleFactor, this.transform.localScale.z);
            }
            curScaleSpeed = 0; // unschön
        }

        // rotation
        if (!useKeyboard)
            this.transform.eulerAngles += new Vector3(0, 0, curRotSpeed * fastWeight);
    }


    void GetInput()
    {
        mousePos = Mouse.current.position.ReadValue();
        mousePos.z = this.transform.position.z;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
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



    // ------------------------------ Input Actions ------------------------------

    public void OnMakeMusic(InputAction.CallbackContext context)
    {
        print("make music start");
    }

    public void OnMakeMusicStarted(InputAction.CallbackContext context)
    {
        // If action.started
        startScale = this.transform.localScale.x;
        startPosState = positionState;
    }

    public void OnSelectRight(InputAction.CallbackContext context)
    {
        targetPos = GetNextTargetRotation(1);
    }

    public void OnSelectLeft(InputAction.CallbackContext context)
    {
        targetPos = GetNextTargetRotation(-1);
    }
}
