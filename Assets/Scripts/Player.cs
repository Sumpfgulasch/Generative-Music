using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // public variables
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



    // private variables
    private MeshRenderer meshRenderer;
    private Color defaultColor;
    private Color moveColor;

    private enum MouseState { hover, move, none };
    private MouseState mouseState = MouseState.none;
    private bool mouseIsActive;
    private Vector3 mouseStartPos;
    private Vector3 mousePos;
    private Vector3 lastMousePos = Vector3.zero;
    private Vector3 midPoint;
    private float rotationTargetAngle;
    private float scaleTargetValue;

    private PolygonCollider2D collider;


    void Start()
    {
        meshRenderer = this.transform.GetComponentInChildren<MeshRenderer>();
        defaultColor = meshRenderer.material.color;
        moveColor = defaultColor * 1.2f;
        midPoint = this.transform.position;
        collider = this.GetComponent<PolygonCollider2D>();
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

        else if (mouseState == MouseState.move)
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
        Vector2[] points = collider.points;
        for (int i=0; i<points.Length-1; i++)
            points[i] = this.transform.TransformPoint(points[i]);
        for(int i=0; i<points.Length-1; i++)
        {
            Vector2 triangleLineMid = points[i] + ((points[(i + 1)%3] - points[i])/2f);
            Vector2 directionOut = (triangleLineMid + (triangleLineMid - (Vector2)midPoint)).normalized;
            Debug.DrawLine((Vector2)midPoint + 10 * directionOut, midPoint, Color.green);
        }
    }

    // ----------------------- Events ----------------------
    

    void GetInput()
    {
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseState = MouseState.move;
            lastMousePos = mousePos;
        }
        else if (!Input.GetMouseButton(0))
            mouseState = MouseState.hover;
    }

    void OnMouseExit()
    {
        if (mouseState != MouseState.move)
            mouseState = MouseState.none;
    }

    void OnMouseUp()
    {
        mouseState = MouseState.none;
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

    public Vector2 FindNearestPointOnLine(Vector2 start, Vector2 end, Vector2 point)
    {
        //Get heading
        Vector2 heading = (end - start);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector2 lhs = point - start;
        float dotP = Vector2.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return start + heading * dotP;
    }
}
