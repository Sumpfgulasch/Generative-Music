using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpdate : MonoBehaviour
{
    // = perform movement according to states


    public static PlayerUpdate instance;

    // get set
    PlayerSettings settings { get { return PlayerSettings.instance; } }
    //VisualController visualController { get { return VisualController.instance; } }
    //EnvironmentData environmentData { get { return EnvironmentData.instance; } }
    PlayerData data { get { return PlayerData.instance; } }
    MusicManager musicManager { get { return MusicManager.instance; } }


    void Start()
    {
        instance = this;
    }

    

    public void PerformMovement()
    {
        KeyboardMovement();
        MouseMovement();
    }


    // -------------------------------------- private methods ------------------------------------------


    // KEYBOARD
    void KeyboardMovement()
    {
        // ROTATION
        float horizontalAxis = Input.GetAxis("Horizontal");
        float slowAxis = Input.GetAxis("Slow");
        float slowRotValue = slowAxis + 1 - (2 * slowAxis * settings.kb_slowRotation);
        if (horizontalAxis != 0)
        {
            this.transform.eulerAngles -= new Vector3(0, 0, horizontalAxis * settings.kb_rotationSpeed * slowRotValue);
        }

        // SCALE
        float verticalAxis = Input.GetAxis("Vertical");
        float slowScaleFactor = slowAxis + 1 - (2 * slowAxis * settings.kb_slowScale);
        float scaleValue = -verticalAxis * settings.kb_scaleSpeed * slowScaleFactor;
        if (verticalAxis != 0)
        {
            this.transform.localScale += new Vector3(scaleValue, scaleValue, 0);
        }
    }



    void MouseMovement()
    {
        // = manage states

        // regular move
        if (data.actionState == PlayerData.ActionState.none)
        {

            if (data.positionState == PlayerData.PositionState.inside)
            {
                MoveTowardsMouse("inner");
                musicManager.StopChord(musicManager.controllers[0]);
                musicManager.StopChord(musicManager.controllers[1]);
            }

            else if (data.positionState == PlayerData.PositionState.innerEdge)
            {
                MoveTowardsMouse("inner");
            }

            else if (data.positionState == PlayerData.PositionState.outside)
            {
                MoveTowardsMouse("outer");
                musicManager.StopChord(musicManager.controllers[1]);
                musicManager.StopChord(musicManager.controllers[0]);
            }

            else if (data.positionState == PlayerData.PositionState.outerEdge)
            {
                MoveTowardsMouse("outer");
            }

        }
        // action: stick to edge
        else if (data.actionState == PlayerData.ActionState.stickToEdge)
        {
            if (data.positionState == PlayerData.PositionState.inside)
            {
                MoveTowardsEdge("inner");
            }

            else if (data.positionState == PlayerData.PositionState.innerEdge)
            {
                StickToEdge("inner");

                musicManager.SetPitchOnEdge(60, musicManager.controllers[0]);
                data.velocity = GetVelocityFromDistance();

                musicManager.StopChord(musicManager.controllers[1]);
                musicManager.PlayChord(musicManager.controllers[0], data.velocity);
            }

            else if (data.positionState == PlayerData.PositionState.outside)
            {
                MoveTowardsEdge("outer");
            }
            else if (data.positionState == PlayerData.PositionState.outerEdge)
            {
                StickToEdge("outer");

                data.velocity = GetVelocityFromDistance();

                musicManager.StopChord(musicManager.controllers[0]);
                musicManager.PlayChord(musicManager.controllers[1], data.velocity);
            }
        }

        // APPLY & clamp (scale & rot)
        data.curScaleSpeed = Mathf.Clamp(data.curScaleSpeed, -settings.scaleMaxSpeed, settings.scaleMaxSpeed) * settings.scaleDamp;
        data.curScaleSpeed = data.curScaleSpeed * (1 - data.bounceRecoverWeight) + curBounceSpeed * bounceWeight; // add bounce force & fast speed

        this.transform.localScale += new Vector3(curScaleSpeed * fastWeight, curScaleSpeed * fastWeight, 0);
        this.transform.localScale = ExtensionMethods.ClampVector3_2D(this.transform.localScale, scaleMin, scaleMax);

        this.transform.eulerAngles += new Vector3(0, 0, curRotSpeed * fastWeight);
    }



    void MoveTowardsEdge(string side)
    {
        if (side == "inner")
        {
            float scaleAdd = scaleEdgeAcc;
            if (curScaleSpeed < 0.0001f)
                curScaleSpeed = 0.0001f;

        }
        else if (side == "outer")
        {
            if (curScaleSpeed > -0.0001f)
                curScaleSpeed = -0.0001f;
        }

        if (curPlayerRadius < tunnelToMidDistance)
        {
            curScaleSpeed = Mathf.Pow(Mathf.Abs(curScaleSpeed), scaleEdgeAcc);
        }
        else
        {
            curScaleSpeed = -Mathf.Pow(Mathf.Abs(curScaleSpeed), scaleEdgeAcc);
        }
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
    }



    void StickToEdge(string side)
    {
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
                this.transform.localScale *= borderTargetScaleFactor;

            }
            else
            {
                float innerVertexDistance = (innerVertices[0] - midPoint).magnitude;
                float borderTargetScaleFactor = (tunnelToMidDistance + stickToOuterEdge_holeSize) / innerVertexDistance;
                this.transform.localScale *= borderTargetScaleFactor;
            }
            curScaleSpeed = 0; // unschön
        }
    }



    IEnumerator BounceForce()
    {
        startedBounce = true;
        float time = 0;
        bounceRecoverWeight = 1;
        while (time < bounceTime)
        {
            time += Time.deltaTime;
            bounceWeight = 1 - time / bounceTime;
            curBounceSpeed = -maxBounceSpeed;
            yield return null;
        }
        time = 0;
        while (time < bounceRecoverTime)
        {
            time += Time.deltaTime;
            bounceRecoverWeight = 1 - time / bounceRecoverTime;
            yield return null;
        }
        startedBounce = false;
    }



    float GetVelocityFromDistance()
    {
        float scaleSize = this.transform.localScale.x - startScale;
        velocity = scaleSize.Remap(settings.scaleMin, 0.4f, 0.3f, 0.7f);
        return velocity;
    }
}
