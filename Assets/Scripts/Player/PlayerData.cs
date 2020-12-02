using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    public enum PositionState { inside, outside, innerEdge, outerEdge, noTunnel };
    public enum ActionState { stickToEdge, none };
    public PositionState positionState = PositionState.noTunnel;
    public ActionState actionState = ActionState.none;

    // get set
    PlayerSettings settings { get { return PlayerSettings.instance; } }
    VisualController visualController { get { return VisualController.instance; } }
    EnvironmentData environmentData { get { return EnvironmentData.instance; } }
    

    //[HideInInspector] public PositionState lastPosState;
    [HideInInspector] public float curRotSpeed;
    [HideInInspector] public bool startedBounce = false;
    [HideInInspector] public Vector3[] outerVertices = new Vector3[3];
    [HideInInspector] public Vector3[] innerVertices = new Vector3[3];
    [HideInInspector] public Vector3[] outerVertices_mesh = new Vector3[3];
    [HideInInspector] public Vector3[] innerVertices_mesh = new Vector3[3];
    [HideInInspector] public float curInnerWidth;
    [HideInInspector] public Transform[] outerVertices_obj;
    [HideInInspector] public Transform[] innerVertices_obj;
    [HideInInspector] public bool firstEdgeTouch = false;
    [HideInInspector] public bool edgeChange;
    [HideInInspector] public bool edgePartChange;
    [HideInInspector] public float curEnvEdgePercentage; // cur percentage in 0 to 1; 0 = erster curEnvVertex, 1 = zweiter curEnvVertex (im Uhrzeigersinn)
    [HideInInspector] public int curEnvEdgePart;
    [HideInInspector] public (Vector3, Vector3) curEnvEdge;
    [HideInInspector] public (Vector3, Vector3) curEnvEdge_2nd;
    [HideInInspector] public (Vector3, Vector3) curEnvEdge_3rd;
    [HideInInspector] public (Vector3, Vector3) lastEnvEdge;
    [HideInInspector] public int lastEnvEdgePart;
    [HideInInspector] public float velocity;
    [HideInInspector] public float startScale;
    [HideInInspector] public float curScaleSpeed = 0;


    //// private variables
    //private bool mouseIsActive;
    //private Vector3 mouseStartPos;
    //private Vector3 mousePos;
    //private Vector3 lastMousePos = Vector3.zero;
    //private Vector3 midPoint;
    //private float rotationTargetAngle;
    private Vector3 mousePos;
    private Vector3 midPoint;
    private float scaleTargetValue;
    //private float lastScaleTargetValue;
    //private float scaleDifferenceToLastFrame;
    private float curPlayerRot;

    //private float curMouseRot;
    private float rotTargetValue;
    //private float rotDifferenceToLastFrame;
    float mouseToPlayerDistance;
    float curPlayerRadius;
    float tunnelToMidDistance;
    RaycastHit envPlayerIntersection;
    //private float bounceWeight = 0f;
    //private float bounceRecoverWeight;
    //private float curBounceSpeed;
    private float fastWeight = 1f;
    private float mouseX, mouseY, mouseDelta;
    private float mouseToEnvDistance;
    //private float timer;





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleData()
    {
        GetInput();
        GetVertices();
        SetActionStates();
        CalcMovementData();
    }



    void GetInput()
    {
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.transform.position.z);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        mouseDelta = Mathf.Sqrt(mouseX * mouseX + mouseY * mouseY);

        if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
            fastWeight = settings.fastFactor;
        else if (Input.GetMouseButtonUp(1))
            fastWeight = 1f;
        if (Input.GetMouseButtonDown(0))
        {
            startScale = this.transform.localScale.x;
        }
    }



    void GetVertices()
    {
        // get positions from childed vertex-gameobjects
        for (int i = 0; i < settings.verticesCount; i++)
        {
            outerVertices[i] = outerVertices_obj[i].position;
            innerVertices[i] = innerVertices_obj[i].position;
        }
    }



    void SetActionStates()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            actionState = ActionState.stickToEdge;
        else
            actionState = ActionState.none;
    }




    // MOUSE
    void CalcMovementData()
    {
        // ROTATION
        Vector2 mouseToMid = mousePos - midPoint;
        Vector2 playerAngleVec = outerVertices[0] - midPoint;
        curPlayerRot = -Vector2.SignedAngle(mouseToMid, playerAngleVec);
        curPlayerRot = Mathf.Clamp(curPlayerRot, -settings.rotationMaxSpeed, settings.rotationMaxSpeed); // = max speed
        rotTargetValue = settings.rotationTargetVectorFactor * curPlayerRot;
        curRotSpeed = rotTargetValue;



        // SCALE

        // last variables
        lastEnvEdge = curEnvEdge;


        // Distances
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
            // Current edge
            Vector3 playerMainVertex_extended = midPoint + ((outerVertices[0] - midPoint).normalized * 10f);
            if (ExtensionMethods.LineSegmentsIntersection(out intersection, playerMainVertex_extended, midPoint, environmentData.curVertices[i], environmentData.curVertices[(i + 1) % 3]))
            {
                curEnvEdge.Item1 = environmentData.curVertices[(i + 1) % 3]; // im Uhrzeigersinn (anders als alle anderen Vertex-Arrays)
                curEnvEdge.Item2 = environmentData.curVertices[i];
                curEnvEdge_2nd.Item1 = environmentData.curVertices[(i + 2) % 3];
                curEnvEdge_2nd.Item2 = environmentData.curVertices[(i + 1) % 3];
                curEnvEdge_3rd.Item1 = environmentData.curVertices[(i + 3) % 3];
                curEnvEdge_3rd.Item2 = environmentData.curVertices[(i + 2) % 3];
            }
        }

        // Scale value
        scaleTargetValue = mouseToPlayerDistance * settings.scaleTargetVectorFactor;
        scaleTargetValue = Mathf.Clamp(scaleTargetValue, -settings.scaleMaxSpeed, settings.scaleMaxSpeed);

        // Edge change
        if (curEnvEdge.Item1 == lastEnvEdge.Item1 && curEnvEdge.Item2 == lastEnvEdge.Item2)
            edgeChange = false;
        else
            edgeChange = true;

        // Edge part change
        if (curEnvEdgePart != lastEnvEdgePart)
            edgePartChange = true;
        else
            edgePartChange = false;
        lastEnvEdgePart = curEnvEdgePart;

        // Cur env edge
        curEnvEdgePercentage = (outerVertices[0] - curEnvEdge.Item1).magnitude / (curEnvEdge.Item2 - curEnvEdge.Item1).magnitude;
        curEnvEdgePart = (int)curEnvEdgePercentage.Remap(0, 1f, 0, visualController.envGridLoops);
    }
}
