﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // stuff
    public static Player inst;
    public enum PositionState { Inside, Outside, InnerEdge, OuterEdge, NoTunnel };
    public enum ActionState { None, Play };
    public PositionState positionState = PositionState.NoTunnel;
    public ActionState actionState = ActionState.None;

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
    //[Range(0, 1f)] public float rotationTargetVectorFactor = 0.1f;
    //[Range(0, 1f)] public float scaleTargetVectorFactor = 0.05f;
    public float scaleMax = 2.7f;
    public float scaleMin = 1f;
    public float scaleMaxSpeed = 0.05f;
    public float minMouseToMidDistance = 0.3f;

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
    //[HideInInspector] public float curRotSpeed;
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
    [HideInInspector] public MusicField[] curFields;



    // private variables
    private Vector3 midPoint;
    //private float scaleTargetValue;
    private float curScaleSpeed = 0;
    private float rotTargetValue;
    private float mouseToPlayerDistance;
    private float curPlayerRadius;
    private float tunnelToMidDistance;
    private RaycastHit envPlayerIntersection;
    private float fastWeight = 1f;
    private float mouseX, mouseY, mouseDelta;
    private float mouseToTunnelDistance;
    private float startScale;
    Vector3 targetPos = Vector3.up;
    private InputAction makeMusicAction, selectRightAction, selectLeftAction;
    private IEnumerator triggerRotRoutine, scaleOutEnumerator, scaleRoutine;
    private IEnumerable rotateRoutine;
    private float curRotPressTime, curRotFrequency;
    private List<IEnumerator> curRotateRoutines = new List<IEnumerator>();
    private List<IEnumerator> curTriggerRoutines = new List<IEnumerator>();
    private enum Side { inner, outer};
    private Side curSide = Side.inner;
    private Side curMouseSide, lastMouseSide;



    // get set
    MusicManager MusicManager { get { return MusicManager.inst; } }
    VisualController VisualController { get { return VisualController.inst; } }

    private float DeltaTime { get { return Time.deltaTime * GameManager.inst.FPS; } }

    



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
        triggerRotRoutine = TriggerRoutineFrequently(RotateToNextField(1), curRotPressTime, curRotFrequency, curRotateRoutines);
        scaleOutEnumerator = DampedScale(scaleMax);
        scaleRoutine = DampedScale(scaleMin);

        curRotPressTime = bt_selectionPressTime;
        curRotFrequency = bt_selectionFrequency;
    }





    void Update()
    {
        ManageMovement();
    }




    // ----------------------------- Main method ----------------------------

    void ManageMovement()
    {
        // 1. Handle Data
        GetVertices();
        CalcMovementData();
    }


    // ----------------------------- Private methods ----------------------------


    // MOUSE
    void CalcMovementData()
    {
        // MOUSE
        // Rotation
        //Vector2 mouseToMid = mousePos - midPoint;
        //Vector2 playerAngleVec = outerVertices[0] - midPoint;
        //float curPlayerRot = -Vector2.SignedAngle(mouseToMid, playerAngleVec);
        //curPlayerRot = Mathf.Clamp(curPlayerRot, -rotationMaxSpeed, rotationMaxSpeed); // = max speed
        //rotTargetValue = rotationTargetVectorFactor * curPlayerRot;
        //curRotSpeed = rotTargetValue;

        // Scale
        mouseToPlayerDistance = 0;
        Vector2 intersection = Vector2.zero;
        Vector3 mousePos_extended = midPoint + (mousePos - midPoint).normalized * 10f;
        if (Physics.Raycast(midPoint, outerVertices[0], out envPlayerIntersection))
        {
            curPlayerRadius = ((Vector2)outerVertices[0] - (Vector2)midPoint).magnitude;
            tunnelToMidDistance = ((Vector2)envPlayerIntersection.point - (Vector2)midPoint).magnitude;
            mouseToTunnelDistance = ((Vector2)mousePos - (Vector2)envPlayerIntersection.point).magnitude;
            if (((Vector2)mousePos - (Vector2)midPoint).magnitude < ((Vector2)envPlayerIntersection.point - (Vector2)midPoint).magnitude)
                mouseToTunnelDistance *= -1;
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
        //scaleTargetValue = mouseToPlayerDistance * scaleTargetVectorFactor;
        //scaleTargetValue = Mathf.Clamp(scaleTargetValue, -scaleMaxSpeed, scaleMaxSpeed);
    }

    

    //void MoveTowardsMouse(Side side)
    //{
    //    if (side == Side.inner)
    //        // accalerate towards mouse
    //        curScaleSpeed += scaleTargetValue;
    //    else if (side == Side.outer)
    //    {
    //        curScaleSpeed = scaleTargetValue * outsideSlowFac;
    //        curRotSpeed = rotTargetValue * outsideSlowFac;
    //    }
    //    // scale
    //    curScaleSpeed = Mathf.Clamp(curScaleSpeed, -scaleMaxSpeed, scaleMaxSpeed) * scaleDamp;                  // damp
    //    this.transform.localScale += new Vector3(curScaleSpeed * fastWeight, curScaleSpeed * fastWeight, 0);    // add
    //    this.transform.localScale = this.transform.localScale.ClampVector3_2D(scaleMin, scaleMax);              // clamp

    //    // rotation
    //    this.transform.eulerAngles += new Vector3(0, 0, curRotSpeed * fastWeight);
    //}



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

        //// rotation
        //if (!useKeyboard)
        //    this.transform.eulerAngles += new Vector3(0, 0, curRotSpeed * fastWeight);
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



    /// <summary>
    /// Set data (actionState, curSide, press times), change collider size and stick to edge.
    /// </summary>
    private void PlayMovement(Side side)
    {
        actionState = ActionState.Play;
        curSide = side;
        StopCoroutine(scaleRoutine);
        StickToEdge(curSide);

        // press frequencies
        curRotPressTime = bt_play_selectionPressTime;
        curRotFrequency = bt_play_selectionFrequency;

        // set mouse collider
        //MeshUpdate.SetMouseColliderSize(VisualController.mouseColliderSize_play);

        // Events
        GameEvents.inst.FieldStart();
    }


    /// <summary>
    /// Set data, collider size and scale back to origin.
    /// </summary>
    private void StopPlayMovement(Side side)
    {
        actionState = ActionState.None;

        if (side == Side.inner)
        {
            scaleRoutine = DampedScale(scaleMin);
        }
        else
        {
            scaleRoutine = DampedScale(scaleMax);
        }
        
        StartCoroutine(scaleRoutine);

        // press frequencies
        curRotPressTime = bt_selectionPressTime;
        curRotFrequency = bt_selectionFrequency;

        // set mouse collider
        //MeshUpdate.SetMouseColliderSize(VisualController.mouseColliderSize_move);

        // Events
        GameEvents.inst.FieldLeave();
    }



    // ---------------------------------- Events ----------------------------------

        

    public void OnPlayInside(InputAction.CallbackContext context)
    {
        if (positionState != PositionState.NoTunnel)
        {
            if (context.performed)
            {
                PlayMovement(Side.inner);
            }
            else if (context.canceled)
            {
                StopPlayMovement(Side.inner);
            }
        }
    }
    
    public void OnPlayOutside(InputAction.CallbackContext context)
    {
        if (positionState != PositionState.NoTunnel)
        {
            if (context.performed)
            {
                PlayMovement(Side.outer);
            }
            else if (context.canceled)
            {
                StopPlayMovement(Side.outer);
            }
        }
    }

    public void OnPlay(InputAction.CallbackContext context)
    {
        //if (positionState != PositionState.NoTunnel)
        //{
        //    if (context.performed)
        //    {
        //        PlayMovement(curSide);
        //    }
        //    else if (context.canceled)
        //    {
        //        StopPlayMovement(curSide);
        //    }
        //}
    }
    
    /// <summary>
    /// Keyboard and button selection.
    /// </summary>
    public void OnSelectNext(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 1. Get input
            int direction = (int)context.ReadValue<float>();

            // 2. Rotate to next field
            StopCoroutines(curTriggerRoutines);
            StopCoroutines(curRotateRoutines);

            var rotateRoutine = RotateToNextField(direction);
            triggerRotRoutine = TriggerRoutineFrequently(rotateRoutine, curRotPressTime, curRotFrequency, curRotateRoutines);
            curTriggerRoutines.Add(triggerRotRoutine);
            
            StartCoroutine(triggerRotRoutine);
        }
        else if (context.canceled)
        {
            StopCoroutines(curTriggerRoutines);
        }
    }

    /// <summary>
    /// For mouse movement so far.
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        var pointerPos = context.ReadValue<Vector2>();

        Side mouseSide = CheckForMouseSide(pointerPos);
        bool mouseMiminumDistance = MouseHasMinimumDistance(pointerPos);
        bool allowedToMove = true;
        if (actionState == ActionState.Play && mouseSide == Side.inner)
        {
            allowedToMove = false;
        }
        
        // 1. Allowed to select? 
        if (mouseMiminumDistance && allowedToMove)
        {
            var mouseDirection = ConvertMouseToDirection(pointerPos);

            // 2. Get & set data (ID, positions, ...)
            var ID = PlayerData.GetIDfromRaycast(mouseDirection);
            var data = PlayerData.SetDataByID(ID);
            var fieldChanged = PlayerData.FieldHasChanged();

            if (fieldChanged)
            {
                GameEvents.inst.FieldChange(data);

                StopCoroutines(curRotateRoutines);
                StopCoroutines(curTriggerRoutines);

                var rotateRoutine = RotateToID(ID);
                curRotateRoutines.Add(rotateRoutine);

                // 3. Rotate!
                StartCoroutine(rotateRoutine);
            }
        }
    }




    // ------------------------------ Rotation methods ------------------------------



    /// <summary>
    /// Stop all routines in a list and remove them from the list.
    /// </summary>
    /// <param name="routines"></param>
    private void StopCoroutines(List<IEnumerator> routines)
    {
        foreach (IEnumerator routine in routines)
        {
            StopCoroutine(routine);
        }
        routines = new List<IEnumerator>();
    }


    /// <summary>
    /// Converts the input value by the input system to a direction, starting at midPoint.
    /// </summary>
    /// <param name="input">Mouse screen pos. Value by input system.</param>
    private Vector3 ConvertMouseToDirection(Vector2 input)
    {
        Vector3 pointerPos = Camera.main.ScreenToWorldPoint(new Vector3(input.x, input.y, midPoint.z));
        pointerPos.z = midPoint.z;
        Vector3 pointerDirection = pointerPos - midPoint;

        return pointerDirection;
    }
    
    /// <summary>
    /// Check if the mouse is inside or outside the MusicFields. Fire events if side changed!
    /// </summary>
    /// <param name="input">Position of the mouse from input system.</param>
    private Side CheckForMouseSide(Vector2 input)
    {
        // last
        //lastMouseSide = curMouseSide;

        // calc
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(input.x, input.y, midPoint.z));
        mousePos.z = midPoint.z - 1;
        
        var ray = new Ray(mousePos, Vector3.forward);
        var hit = Physics2D.GetRayIntersection(ray,3);

        // ray
        if (hit && hit.collider.tag.Equals("MouseCollider"))
        {
            GameEvents.inst.MouseInside();
            return Side.inner;
        }
        else
        {
            GameEvents.inst.MouseOutside();
            return Side.outer;
        }
    }

    /// <summary>
    /// Check if the mouse has a miminum distance to the player mid.
    /// </summary>
    private bool MouseHasMinimumDistance(Vector2 input)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(input.x, input.y, midPoint.z));
        float distance = (mousePos - midPoint).magnitude;

        if (distance >= minMouseToMidDistance)
            return true;
        else
            return false;
    }
    

    /// <summary>
    /// Call a coroutine frequently to simulate button behaviour: trigger immediatly, wait for press time, trigger frequently.
    /// </summary>
    /// <param name="routine">The routine. Enumerable als Argument, weil ich jedes mal einen neuen enumerator erzeugen muss, sonst wird die coroutine nicht erneut aufgerufen</param>
    /// <param name="startWaitTime"></param>
    /// <param name="frequency"></param>
    /// <param name="routineRefList">Empty list, to be able to quit all routines later.</param>
    /// <returns></returns>
    private IEnumerator TriggerRoutineFrequently (IEnumerable routine, float startWaitTime, float frequency, List<IEnumerator> routineRefList)
    {
        float timer = 0;

        // 1. Initial
        IEnumerator enumerator = routine.GetEnumerator();
        routineRefList.Add(enumerator);
        StartCoroutine(enumerator);

        // 2. Press time
        while (timer < startWaitTime)
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
                StopCoroutines(routineRefList);

                enumerator = routine.GetEnumerator();
                routineRefList.Add(enumerator);
                StartCoroutine(enumerator);

                timer -= frequency;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        
    }

    
    /// <summary>
    /// Single rotation routine: Rotate in the given direction to the adjacent field, if possible. Get and set data (curID, fieldPos, ...), fire event.
    /// </summary>
    /// <param name="direction">[1 or -1]</param>
    private IEnumerable RotateToNextField(int direction)
    {
        // 1. Get next ID
        int nextID = MusicField.NextFieldID(curField.ID, direction);
        
        // 2. Selectable?
        if (curFields[nextID].selectable)                               // TO DO: sollte hier nicht rein (?)
        {
            // 4. Set data (ID, ...)
            var data = PlayerData.SetDataByID(nextID);

            // FIRE EVENT
            GameEvents.inst.FieldChange(data);                              // TO DO: gehört hier nicht rein!!!

            // 3. Get target rotation
            var targetPos = data.mid;

            

            // 5. ROTATE TO ID
            float maxTime = 1.2f;
            float timer = 0;
            while (timer < maxTime)
            {
                RotateToTarget(targetPos);

                if (actionState == ActionState.Play)
                {
                    StickToEdge(curSide);
                }

                timer += Time.deltaTime;

                yield return null;
            }
        }
        else
        {
            // TO DO: Perform NotDoable-Animation;
        }
    }


    /// <summary>
    /// Rotate over time to a given ID. Calculates the proper field mid position automatically. Used for all fields that are not adjacent.
    /// </summary>
    private IEnumerator RotateToID(int ID)
    {
        // 1. Selectable?
        if (curFields[ID].selectable)
        {

            // 3. Target rotation
            //var targetPos = MusicField.FieldMid(ID);
            var targetPos = TunnelData.fields[ID].mid;

            float maxTime = 1.2f;
            float timer = 0;

            // 4. ROTATE TO ID
            while (timer < maxTime)
            {
                RotateToTarget(targetPos);

                if (actionState == ActionState.Play)
                {
                    StickToEdge(curSide);
                }

                timer += Time.deltaTime;

                yield return null;
            }
        }
        yield return null;
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
            Vector3 scaleSpeed = (maxScale - this.transform.localScale) * bt_scaleDamp * DeltaTime;
            this.transform.localScale += scaleSpeed;

            timer += Time.deltaTime;
            yield return null;
        }
    }


    /// <summary>
    /// Perform Rotation. Damped by bt_rotationDamp.
    /// </summary>
    private void RotateToTarget(Vector3 targetPos)
    {
        Vector3 targetVec = targetPos - this.transform.position;                                                            // TO DO: animation curve, statt damping
        Vector3 curVec = outerVertices[0] - this.transform.position;

        float nextRot = Vector2.SignedAngle(curVec, targetVec) * bt_rotationDamp * DeltaTime;

        this.transform.eulerAngles += new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, nextRot);

        // Hack (gegen 180° & 60° Winkel)
        if ((Mathf.Abs(this.transform.eulerAngles.z) > 180f - 0.1f && Mathf.Abs(this.transform.eulerAngles.z) < 180f + 0.1f) ||
            Mathf.Abs(this.transform.eulerAngles.z) > 60f - 0.1f && Mathf.Abs(this.transform.eulerAngles.z) < 60f + 0.1f)
        {
            this.transform.eulerAngles += new Vector3(0, 0, 0.1f);
        }
    }



}
