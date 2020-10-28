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
    public float kb_rotationSpeed = 1f;

    // private variables
    private MeshRenderer meshRenderer;
    private Color defaultColor;
    private Color moveColor;

    private enum MouseState {hover, move, none};
    MouseState mouseState = MouseState.none;
    private bool mouseIsActive;
    private Vector3 mouseStartPos;
    private Vector3 mousePos;
    private Vector3 lastMousePos = Vector3.zero;
    private Vector3 midPoint;
    private float rotationTargetAngle;
    private float scaleTargetValue;


    void Start()
    {
        meshRenderer = this.transform.GetComponentInChildren<MeshRenderer>();
        defaultColor = meshRenderer.material.color;
        moveColor = defaultColor * 1.2f;
        midPoint = this.transform.position;
    }


    void Update()
    {
        GetInput();
        ManageMovement();
    }


    void ManageMovement()
    {
        MouseMovement();
        KeyboardMovement();
    }


    void KeyboardMovement ()
    {
        // rotation
        float horizontalAxis = Input.GetAxis("Horizontal");
        if (horizontalAxis != 0)
        {
            this.transform.eulerAngles -= new Vector3(0, 0, horizontalAxis * kb_rotationSpeed);
        }


        // scale
        float verticalAxis = Input.GetAxis("Vertical");
        if (verticalAxis != 0)
        {
            this.transform.localScale += new Vector3(verticalAxis * kb_scaleSpeed, verticalAxis * kb_scaleSpeed, 0);
        }
        this.transform.localScale = ClampVector3(this.transform.localScale, scaleMin, scaleMax);
    }
    

    void MouseMovement()
    {
        if (mouseState == MouseState.hover)
        {
            meshRenderer.material.color = Color.white;
        }

        else if (mouseState == MouseState.move)
        {
            meshRenderer.material.color = moveColor;

            // rotation
            Vector2 mouseToMid = mousePos - midPoint;
            Vector2 lastMouseToMid = lastMousePos - midPoint;
            rotationTargetAngle = Vector2.SignedAngle(lastMouseToMid, mouseToMid);
            this.transform.eulerAngles += new Vector3(0, 0, rotationTargetAngle);

            // scale
            scaleTargetValue = (mouseToMid.magnitude - lastMouseToMid.magnitude) * scaleSensitivity;
            this.transform.localScale += new Vector3(scaleTargetValue, scaleTargetValue, 0);

            lastMousePos = mousePos;

        }

        else
        {
            meshRenderer.material.color = defaultColor;
        }
    }

    //public Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 end, Vector2 point)
    //{
    //    //Get heading
    //    Vector2 heading = (end - origin);
    //    float magnitudeMax = heading.magnitude;
    //    heading.Normalize();

    //    //Do projection from the point but clamp it
    //    Vector2 lhs = point - origin;
    //    float dotP = Vector2.Dot(lhs, heading);
    //    dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
    //    return origin + heading * dotP;
    //}

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
            //mouseStartPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            //mouseStartPos = Camera.main.ScreenToWorldPoint(mouseStartPos);
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



    // Extension Methods

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
