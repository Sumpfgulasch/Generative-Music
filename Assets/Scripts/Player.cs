using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    // public variables
    [Header("CONTROLS")]
    [Header("Mouse")]
    public float rotationSpeed = 5f;
    public float scaleSpeed = 5f;
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

    [Header("Environment distance visualisation")]
    public float offset = 1f;

    // private variables
    private MeshRenderer meshRenderer;
    private Color defaultColor;
    private Color moveColor;

    private enum MouseState { hover, scale, scaleRot, none };
    private MouseState mouseState = MouseState.none;
    private bool mouseIsActive;
    private Vector3 mouseStartPos;
    private Vector3 mousePos;
    private Vector3 lastMousePos = Vector3.zero;
    private Vector3 midPoint;
    private float rotationTargetAngle;
    private float scaleTargetValue;

    private PolygonCollider2D collider;

    private LineRenderer lineRenderer;


    void Start()
    {
        meshRenderer = this.transform.GetComponentInChildren<MeshRenderer>();
        defaultColor = meshRenderer.material.color;
        moveColor = defaultColor * 1.2f;
        midPoint = this.transform.position;
        collider = this.GetComponent<PolygonCollider2D>();
        lineRenderer = this.GetComponent<LineRenderer>();
    }


    void Update()
    {
        ManageMovement();
        VisualizeCurrentPlane();
    }

    // -------------------- MOVEMENT ---------------------

    void ManageMovement()
    {
        GetInput();
        MouseMovement();
        KeyboardMovement();

        this.transform.localScale = ClampVector3(this.transform.localScale, scaleMin, scaleMax);
    }

    // MOUSE
    void MouseMovement()
    {
        if (mouseState == MouseState.hover)
        {
            meshRenderer.material.color = Color.white;
        }

        else if (mouseState == MouseState.scaleRot)
        {
            meshRenderer.material.color = moveColor;
            
            Vector2 mouseToMid = mousePos - midPoint;
            Vector2 lastMouseToMid = lastMousePos - midPoint;
            lastMousePos = mousePos;

            // ROTATION
            rotationTargetAngle = Vector2.SignedAngle(lastMouseToMid, mouseToMid);
            this.transform.eulerAngles += new Vector3(0, 0, rotationTargetAngle);

            // SCALE
            scaleTargetValue = (mouseToMid.magnitude - lastMouseToMid.magnitude) * scaleSensitivity;
            if (mouseToMid.magnitude < scaleMax)
                this.transform.localScale += new Vector3(scaleTargetValue, scaleTargetValue, 0);
        }

        else if (mouseState == MouseState.scale)
        {
            meshRenderer.material.color = moveColor;

            Vector2 mouseToMid = mousePos - midPoint;
            Vector2 lastMouseToMid = lastMousePos - midPoint;
            lastMousePos = mousePos;

            // SCALE
            scaleTargetValue = (mouseToMid.magnitude - lastMouseToMid.magnitude) * scaleSensitivity;
            if (mouseToMid.magnitude < scaleMax)
                this.transform.localScale += new Vector3(scaleTargetValue, scaleTargetValue, 0);
        }

        else
        {
            meshRenderer.material.color = defaultColor;
        }
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
        float scaleValue = verticalAxis * kb_scaleSpeed * slowScaleFactor;
        if (verticalAxis != 0)
        {
            this.transform.localScale += new Vector3(scaleValue, scaleValue, 0);
        }

    }

    // ---------------------- VISUALISATION ---------------------

    void VisualizeCurrentPlane()
    {
        // 1) init
        RaycastHit[] edgeHits = new RaycastHit[3];
        Vector3[] environmentVertices = new Vector3[3];
        Vector3 intersection;

        // 2) Prepare raycast
        Vector2[] points = collider.points;
        for (int i=0; i<points.Length; i++)
            points[i] = this.transform.TransformPoint(points[i]);
        for(int i=0; i<points.Length; i++)
        {
            Vector3 triangleEdgeMid = points[i] + ((points[(i + 1)%3] - points[i])/2f);
            triangleEdgeMid.z = this.transform.position.z;
            Vector3 directionOut = (triangleEdgeMid - midPoint).normalized;
            RaycastHit hit;

            // 3) Raycasts (from player to environment)
            if (Physics.Raycast(midPoint, directionOut, out hit))
            {
                edgeHits[i] = hit;
                Debug.DrawLine(triangleEdgeMid, hit.point, Color.red);
            }
        }
        // 4) Construct environment triangle by line intersections
        for (int i = 0; i < edgeHits.Length; i++)
        {
            Vector3 point1, point2;
            Vector3 direction1, direction2;
            point1 = edgeHits[i].point;
            point1.z = edgeHits[0].point.z;
            point2 = edgeHits[(i+1)%3].point;
            point2.z = edgeHits[0].point.z;
            direction1 = Vector3.Cross(Vector3.forward, edgeHits[i].normal);
            direction1.z = 0;
            direction2 = Vector3.Cross(Vector3.forward, edgeHits[(i + 1) % 3].normal);
            direction2.z = 0;

            if (ExtensionMethods.LineLineIntersection(out intersection, point1, direction1, point2, direction2))
            {
                // 5) Offset
                intersection = intersection + (this.transform.position - intersection).normalized * offset;
                environmentVertices[i] = intersection;
            }
        }

        for (int i = 0; i<environmentVertices.Length; i++)
            Debug.DrawLine(environmentVertices[i], environmentVertices[(i + 1) % 3], Color.blue);
        

        // 6) Add extra points for LineRenderer
        List<Vector3> newPositions = environmentVertices.ToList();
        int insertCounter = 0;
        for (int i = 1; i < environmentVertices.Length; i++)
        {
            // insert before
            newPositions.Insert(i + insertCounter, environmentVertices[i]);
            insertCounter++;
            // insert after
            newPositions.Insert(i + 1 + insertCounter, environmentVertices[i]);
            insertCounter++;
        }
        newPositions.Add(environmentVertices[0]);

        

        // 7) Add to LineRenderer
        lineRenderer.positionCount = newPositions.Count;
        lineRenderer.SetPositions(newPositions.ToArray());

    }

    // ----------------------- Events ----------------------


    void GetInput()
    {
        OnMouseUp_Right();
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            mouseState = MouseState.scaleRot;
            lastMousePos = mousePos;
        }
        else if (Input.GetMouseButton(0))
        {
            mouseState = MouseState.scale;
            lastMousePos = mousePos;
        }
        else if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            mouseState = MouseState.hover;
    }

    void OnMouseExit()
    {
        if (mouseState != MouseState.scaleRot && mouseState != MouseState.scale)
            mouseState = MouseState.none;
    }

    void OnMouseUp()
    {
        mouseState = MouseState.none;
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



    // ------------------- Extension Methods ---------------------

    Vector3 ClampVector3(Vector3 value, float min, float max)
    {
        Vector3 clampedVector3;
        clampedVector3 = new Vector3(
            Mathf.Clamp(value.x, min, max),
            Mathf.Clamp(value.y, min, max),
            value.z);
        return clampedVector3;
    }
}
