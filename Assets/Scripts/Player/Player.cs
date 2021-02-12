using System.Collections;
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
    public float stickToOuterEdge_holeSize = 0.05f;

    [Header("Mouse")]
    public float rotationMaxSpeed = 5f;
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
    private Vector3 midPoint;
    //private float curScaleSpeed = 0;
    //private float rotTargetValue;
    //private float fastWeight = 1f;
    //private float mouseX, mouseY, mouseDelta;
    //private float startScale;
    //Vector3 targetPos = Vector3.up;
    private InputAction makeMusicAction, selectRightAction, selectLeftAction;
    private IEnumerator triggerRotRoutine, scaleOutEnumerator, scaleRoutine;
    private IEnumerable rotateRoutine;
    private float curRotPressTime, curRotFrequency;
    private List<IEnumerator> curRotateRoutines = new List<IEnumerator>();
    private List<IEnumerator> curTriggerRoutines = new List<IEnumerator>();
    private enum Side { inner, outer};
    private Side curPlaySide = Side.inner;
    private Side curMouseSide, lastMouseSide;



    // Properties
    MusicManager MusicManager { get { return MusicManager.inst; } }
    VisualController VisualController { get { return VisualController.inst; } }
    private float DeltaTime { get { return Time.deltaTime * GameManager.inst.FPS; } }
    private Vector3[] outerVertices = new Vector3[3];
    [HideInInspector] public Vector3[] OuterVertices
    {
        get
        {
            if (outerVertices_obj[0] != null)
            {
                for (int i = 0; i < verticesCount; i++)
                    outerVertices[i] = outerVertices_obj[i].position;
                return outerVertices;
            }
            else
                return null;
        }
    }
    private Vector3[] innerVertices = new Vector3[3];
    [HideInInspector] public Vector3[] InnerVertices
    {
        get
        {
            if (innerVertices_obj[0] != null)
            {
                for (int i = 0; i < verticesCount; i++)
                    innerVertices[i] = innerVertices_obj[i].position;
                return innerVertices;
            }
            else
                return null;
        }
    }
    




    private void Awake()
    {

    }

    private void OnEnable()
    {

    }

    void Start()
    {
        inst = this;
        midPoint = transform.position;
        triggerRotRoutine = TriggerRoutineFrequently(RotateToNextField(1), curRotPressTime, curRotFrequency, curRotateRoutines);
        scaleOutEnumerator = DampedScale(scaleMax);
        scaleRoutine = DampedScale(scaleMin);

        curRotPressTime = bt_selectionPressTime;
        curRotFrequency = bt_selectionFrequency;
    }

    

    void Update()
    {
        
    }





    // ----------------------------- Private methods ----------------------------

        
    

    void StickToEdge(Side side)
    {
        // calc
        float curPlayerRadius = ((Vector2)OuterVertices[0] - (Vector2)midPoint).magnitude;
        float tunnelToMidDistance = 0;
        RaycastHit hit;
        if (Physics.Raycast(midPoint, OuterVertices[0], out hit))
            tunnelToMidDistance = ((Vector2)hit.point - (Vector2)midPoint).magnitude;

        // SCALE only
        if (side == Side.inner)
        {
            float targetScaleFactor = tunnelToMidDistance / curPlayerRadius;
            transform.localScale = new Vector3(transform.localScale.x * targetScaleFactor, transform.localScale.y * targetScaleFactor, transform.localScale.z);

            // TO DO: bounce?
        }

        else if (side == Side.outer)
        {
            if (constantInnerWidth)
            {
                float targetScaleFactor = (tunnelToMidDistance + innerWidth + stickToOuterEdge_holeSize) / curPlayerRadius;
                transform.localScale = new Vector3(transform.localScale.x * targetScaleFactor, transform.localScale.y * targetScaleFactor, transform.localScale.z);

            }
            else
            {
                float innerVertexDistance = (InnerVertices[0] - midPoint).magnitude;
                float targetScaleFactor = (tunnelToMidDistance + stickToOuterEdge_holeSize) / innerVertexDistance;
                transform.localScale = new Vector3(transform.localScale.x * targetScaleFactor, transform.localScale.y * targetScaleFactor, transform.localScale.z);
            }
        }
    }


    /// <summary>
    /// Set data (actionState, curSide, press times), change collider size and stick to edge. Update player width(!).
    /// </summary>
    private void PlayMovement(Side side)
    {
        actionState = ActionState.Play;
        curPlaySide = side;
        StopCoroutine(scaleRoutine);
        StickToEdge(curPlaySide);

        // press frequencies
        curRotPressTime = bt_play_selectionPressTime;
        curRotFrequency = bt_play_selectionFrequency;

        // set mouse collider
        //MeshUpdate.SetMouseColliderSize(VisualController.mouseColliderSize_play);

        MeshUpdate.UpdatePlayer();

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
            if (curField.IsNotSpawning)                           // TO DO: genauer überlegen
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
        if (context.performed)
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
        lastMouseSide = curMouseSide;

        // calc
        var mousePos = Camera.main.ScreenToWorldPoint(new Vector3(input.x, input.y, midPoint.z));
        mousePos.z = midPoint.z - 1;
        var ray = new Ray(mousePos, Vector3.forward);
        var hit = Physics2D.GetRayIntersection(ray,3);

        // ray
        if (hit && hit.collider.tag.Equals("MouseCollider"))
        {
            curMouseSide = Side.inner;
        }
        else
        {
            curMouseSide = Side.outer;
        }

        // FIRE EVENT
        if (curMouseSide != lastMouseSide)
        {
            if (curMouseSide == Side.inner)
                GameEvents.inst.MouseInside();
            else
                GameEvents.inst.MouseOutside();
        }

        return curMouseSide;
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
        if (curFieldSet[nextID].isSelectable)                               // TO DO: sollte hier nicht rein (?)
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
                    StickToEdge(curPlaySide);
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
        if (curFieldSet[ID].isSelectable)
        {

            // 3. Target rotation
            var targetPos = TunnelData.fields[ID].mid;

            float maxTime = 1.2f;
            float timer = 0;

            // 4. ROTATE TO ID
            while (timer < maxTime)
            {
                RotateToTarget(targetPos);

                if (actionState == ActionState.Play)
                {
                    StickToEdge(curPlaySide);
                }

                timer += Time.deltaTime;

                yield return null;
            }
        }
        yield return null;
    }


    /// <summary>
    /// Scale the player over time, starting quick and becoming slow. Also update the player width(!). Used for when the player releases the play-button.
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

            MeshUpdate.UpdatePlayer();

            timer += Time.deltaTime;
            yield return null;
        }
    }


    /// <summary>
    /// Perform rotation once. Damped by bt_rotationDamp. Update player mesh (!!).
    /// </summary>
    private void RotateToTarget(Vector3 targetPos)
    {
        // 1. Rotate
        Vector3 targetVec = targetPos - this.transform.position;                                                            // TO DO: animation curve, statt damping
        Vector3 curVec = OuterVertices[0] - this.transform.position;

        float nextRot = Vector2.SignedAngle(curVec, targetVec) * bt_rotationDamp * DeltaTime;

        this.transform.eulerAngles += new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, nextRot);

        // Hack (gegen 180° & 60° Winkel)
        if ((Mathf.Abs(this.transform.eulerAngles.z) > 180f - 0.1f && Mathf.Abs(this.transform.eulerAngles.z) < 180f + 0.1f) ||
            Mathf.Abs(this.transform.eulerAngles.z) > 60f - 0.1f && Mathf.Abs(this.transform.eulerAngles.z) < 60f + 0.1f)
        {
            this.transform.eulerAngles += new Vector3(0, 0, 0.1f);
        }

        // 2. Update player meshes
        MeshUpdate.UpdatePlayer();
    }



}
