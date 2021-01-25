using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // stuff
    public static Player inst;
    public enum PositionState { inside, outside, innerEdge, outerEdge, noTunnel };
    public enum ActionState { none, stickToEdge };
    public PositionState positionState = PositionState.noTunnel;
    public ActionState actionState = ActionState.none;

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
    [Range(0.1f, 1f)]
    public float kb_slowScale = 0.9f;
    public float kb_rotationSpeed = 1f;
    [Range(0.1f, 1f)]
    public float kb_slowRotation = 0.9f;

    
    [Header("All button devices")]
    [Range(0.01f, 1f)]
    public float bt_rotationDamp = 0.4f;
    public float bt_scaleDamp = 1f;
    public float bt_selectionPressTime = 0.3f;
    public float bt_selectionFrequency = 0.13f;
    public float bt_play_selectionPressTime = 0.7f;
    public float bt_play_selectionFrequency = 0.4f;


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
    [HideInInspector] public PlayerField curField;
    [HideInInspector] public PlayerField[] curSecondaryFields;
    [HideInInspector] public Edge curEdge;
    [HideInInspector] public Edge[] curSecEdges;
    [HideInInspector] public bool tunnelEnter;
    [HideInInspector] public MusicField[] curFieldSet;



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
    private IEnumerator rotateEnumerator, triggerRotateEnumerator, scaleOutEnumerator, scaleEnumerator;
    private IEnumerable rotateEnumerable;
    private float selectionPressTime, selectionFrequency;
    private enum Side { inner, outer};
    private Side curSide = Side.inner;



    // get set
    MusicManager musicManager { get { return MusicManager.inst; } }
    VisualController visualController { get { return VisualController.inst; } }

    private float deltaTime { get { return Time.deltaTime * GameManager.inst.FPS; } }

    



    private void Awake()
    {

    }

    private void OnEnable()
    {

    }

    void Start()
    {
        inst = this;
        midPoint = this.transform.position;
        rotateEnumerator = RotateToNextField(1).GetEnumerator(); // hack inits
        scaleOutEnumerator = DampedScale(scaleMax);
        scaleEnumerator = DampedScale(scaleMin);

        selectionPressTime = bt_selectionPressTime;
        selectionFrequency = bt_selectionFrequency;
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
        
        if (!useKeyboard) // to do: an maus und input system anpassen
        {
            // regular move
            if (actionState == ActionState.none)
            {
                MoveTowardsMouse(curSide);
            }
            // action: stick to edge
            else if (actionState == ActionState.stickToEdge)
            {
                StickToEdge(curSide);
            }
        }

    }


    public Vector3 GetNextTargetRotation(int direction)
    {
        int curID = curField.ID;
        Vector3 targetPos = MusicField.NextEdgePartMid(curID, direction);

        return targetPos;
    }

    private void RotateToTarget(Vector3 targetPos)
    {
        Vector3 targetVec = targetPos - this.transform.position;
        Vector3 curVec = outerVertices[0] - this.transform.position;

        float nextRot = Vector2.SignedAngle(curVec, targetVec) * bt_rotationDamp * deltaTime;

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



    void MoveTowardsMouse(Side side)
    {
        if (side == Side.inner)
            // accalerate towards mouse
            curScaleSpeed += scaleTargetValue;
        else if (side == Side.outer)
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



    void StickToEdge(Side side)
    {

        // SCALE only
        if (side == Side.inner)
        {
            float borderTargetScaleFactor = tunnelToMidDistance / curPlayerRadius;
            this.transform.localScale = new Vector3(this.transform.localScale.x * borderTargetScaleFactor, this.transform.localScale.y * borderTargetScaleFactor, this.transform.localScale.z);
            curScaleSpeed = 0; // unschön

            // TO DO: bounce?
        }

        else if (side == Side.outer)
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

        

    public void OnPlayInside(InputAction.CallbackContext context)
    {
        // = SET ACTION STATES

        if (positionState != PositionState.noTunnel)
        {
            lastActionState = actionState;

            if (context.performed)
            {
                // state & movement
                actionState = Player.ActionState.stickToEdge;
                curSide = Side.inner;
                StopCoroutine(scaleEnumerator);
                StickToEdge(curSide);

                // start variables; todo: entfernen weil nicht mehr gebraucht (? was ist mit maus)
                startScale = this.transform.localScale.x; // für velocity
                //startPosState = positionState;

                // press frequencies
                selectionPressTime = bt_play_selectionPressTime;
                selectionFrequency = bt_play_selectionFrequency;

                // Events
                GameEvents.inst.FirstTouch();

            }
            else if (context.canceled)
            {
                // state
                actionState = Player.ActionState.none;

                scaleEnumerator = DampedScale(scaleMin);
                StartCoroutine(scaleEnumerator);

                // press frequencies
                selectionPressTime = bt_selectionPressTime;
                selectionFrequency = bt_selectionFrequency;

                // Events
                GameEvents.inst.Leave();
            }
        }
    }

    // to do: kürzen
    public void OnPlayOutside(InputAction.CallbackContext context)
    {
        // = SET ACTION STATES

        if (positionState != PositionState.noTunnel)
        {
            lastActionState = actionState;

            if (context.performed)
            {
                // state & movement
                actionState = Player.ActionState.stickToEdge;
                curSide = Side.outer;
                StopCoroutine(scaleEnumerator);
                StickToEdge(curSide);

                // press frequencies
                selectionPressTime = bt_play_selectionPressTime;
                selectionFrequency = bt_play_selectionFrequency;

                // Events
                GameEvents.inst.FirstTouch();

            }
            else if (context.canceled)
            {
                // state
                actionState = Player.ActionState.none;
                // to do: coroutine um in ursprungs-pos zu gehen -> max scale
                scaleEnumerator = DampedScale(scaleMax);
                StartCoroutine(scaleEnumerator);

                // press frequencies
                selectionPressTime = bt_selectionPressTime;
                selectionFrequency = bt_selectionFrequency;

                // Events
                GameEvents.inst.Leave();
            }
        }
    }

    public void OnPlay(InputAction.CallbackContext context)
    {
        // = SET ACTION STATES

        if (positionState != PositionState.noTunnel)
        {
            lastActionState = actionState;

            if (context.performed)
            {
                // state & movement
                actionState = Player.ActionState.stickToEdge;
                StopCoroutine(scaleEnumerator);
                StickToEdge(curSide);

                // press frequencies
                selectionPressTime = bt_play_selectionPressTime;
                selectionFrequency = bt_play_selectionFrequency;

                // Events
                GameEvents.inst.FirstTouch();

            }
            else if (context.canceled)
            {
                // state
                actionState = Player.ActionState.none;
                // to do: coroutine um in ursprungs-pos zu gehen -> max scale
                if (curSide == Side.outer)
                {
                    scaleEnumerator = DampedScale(scaleMax);
                    StartCoroutine(scaleEnumerator);
                }
                else
                {
                    scaleEnumerator = DampedScale(scaleMin);
                    StartCoroutine(scaleEnumerator);
                }
                    

                // press frequencies
                selectionPressTime = bt_selectionPressTime;
                selectionFrequency = bt_selectionFrequency;

                // Events
                GameEvents.inst.Leave();
            }
        }
    }





    public void OnSelectNext(InputAction.CallbackContext context)
    {
        

        if (context.performed)
        {
            int direction = (int)context.ReadValue<float>();
            rotateEnumerable = RotateToNextField(direction);
            triggerRotateEnumerator = ButtonPressBehaviour1(rotateEnumerable, selectionPressTime, selectionFrequency);

            StopCoroutine(rotateEnumerator);
            StartCoroutine(triggerRotateEnumerator);

            if (actionState == ActionState.none)
            {
                // hier rhythmisch
            }
            else
            {
                // hier tempo variierend
            }
        }
        else if (context.canceled)
        {
            StopCoroutine(triggerRotateEnumerator);
        }

        
    }

    private IEnumerator ButtonPressBehaviour1 (IEnumerable enumerable, float startPressTime, float frequency)
    {
        // = Call a coroutine frequently to simulate button behaviour: trigger immediatly, wait for press time, trigger frequently
        // Info: enumerable als Argument, weil ich jedes mal einen neuen enumerator erzeugen muss, sonst wird die coroutine nicht erneut aufgerufen

        float timer = 0;

        // 1. Initial
        rotateEnumerator = enumerable.GetEnumerator();
        StartCoroutine(rotateEnumerator);

        // 2. Press time
        while (timer < startPressTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        timer = frequency;

        // 3. Repetitions
        while (true)
        {
            if (timer >= frequency)
            {
                StopCoroutine(rotateEnumerator);

                rotateEnumerator = enumerable.GetEnumerator();
                StartCoroutine(rotateEnumerator);

                timer -= frequency;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        
    }
    

    private IEnumerable RotateToNextField(int direction)
    {
        // = rotate to next field, with break; if player is making music, do stickToEdge(), too

        int nextID = MusicField.NextFieldID(curField.ID, direction);

        if (curFieldSet[nextID].selectable)
        {
            var targetPos = GetNextTargetRotation(direction);

            float maxTime = 1.2f;
            float timer = 0;

            while (timer < maxTime)
            {
                RotateToTarget(targetPos);

                if (actionState == ActionState.stickToEdge)
                {
                    StickToEdge(curSide);
                }

                timer += Time.deltaTime;

                yield return null;
            }
        }
        else
        {
            // TO DO: PerformNotDoable();
        }

        
    }

    /// <summary>
    /// Scales the player to a target scale over time.
    /// </summary>
    public IEnumerator DampedScale(float targetScale, float timeToStart = 0)
    {
        yield return new WaitForSeconds(timeToStart);

        float maxTime = 2.2f;
        float timer = 0;
        Vector3 maxScale = new Vector3(targetScale, targetScale, 1);

        while (timer < maxTime)
        {
            //print("damped scale: " + timer);
            Vector3 scaleSpeed = (maxScale - this.transform.localScale) * bt_scaleDamp * deltaTime;
            this.transform.localScale += scaleSpeed;

            timer += Time.deltaTime;
            yield return null;
        }
    }



}
