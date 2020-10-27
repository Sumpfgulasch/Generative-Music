using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float scaleSpeed = 5f;
    public float maxScale = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Scale();
        Rotate();
    }

    void Scale()
    {
        // Mouse
        float mouseX = Input.GetAxis("Mouse X");
        this.transform.localScale += new Vector3(mouseX, mouseX, 0) * scaleSpeed;
        this.transform.localScale = new Vector3(Mathf.Clamp(this.transform.localScale.x, 0, maxScale), Mathf.Clamp(this.transform.localScale.y, 0, maxScale), this.transform.localScale.z);
    }

    void Rotate()
    {
        // Mouse
        this.transform.eulerAngles += new Vector3(0, 0, Input.GetAxis("Mouse Y") * rotationSpeed);
    }

    public Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 end, Vector2 point)
    {
        //Get heading
        Vector2 heading = (end - origin);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector2 lhs = point - origin;
        float dotP = Vector2.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return origin + heading * dotP;
    }
}
