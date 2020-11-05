using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // public variables
    public static Player instance;
    public enum State { inside, outside, perfect, noTunnel};
    public State state;

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

    private enum MouseState { hover, scale, scaleRot, none };
    private MouseState mouseState = MouseState.none;
    private bool mouseIsActive;
    private Vector3 mouseStartPos;
    private Vector3 mousePos;
    private Vector3 lastMousePos = Vector3.zero;
    private Vector3 midPoint;
    private float rotationTargetAngle;
    private float scaleTargetValue;


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
        MouseMovement();
        KeyboardMovement();

        //this.transform.localScale = ClampVector3(this.transform.localScale, scaleMin, scaleMax);
        this.transform.localScale.ClampVector3_2D(scaleMin, scaleMax);
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
        float scaleValue = -verticalAxis * kb_scaleSpeed * slowScaleFactor;
        if (verticalAxis != 0)
        {
            this.transform.localScale += new Vector3(scaleValue, scaleValue, 0);
        }

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
}
