using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    public Wheel leftWheel;
    public Wheel rightWheel;
    
    public bool motor;
    public bool steering;
    public float SteerAngle
    {
        set
        {
            leftWheel.collider.steerAngle = value;
            rightWheel.collider.steerAngle = value;
        }
    }
    public float MotorTorque
    {
        set
        {
            leftWheel.collider.motorTorque = value;
            rightWheel.collider.motorTorque = value;
        }
    }
    public float BrakeTorque
    {
        set
        {
            leftWheel.collider.brakeTorque = value;
            rightWheel.collider.brakeTorque = value;
        }
    }
    public void ApplyLocalPositionToVisuals()
    {
        Vector3 pos;
        Quaternion quat;
        // Left wheel
        leftWheel.collider.GetWorldPose(out pos, out quat);
        leftWheel.SetPose(pos, quat);

        // Right wheel
        rightWheel.collider.GetWorldPose(out pos, out quat);
        rightWheel.SetPose(pos, quat);
    }
    public void UpdateFriction()
    {
        WheelHit hit;
        WheelCollider wheel = leftWheel.collider;
        if (wheel.GetGroundHit(out hit))
        {
            // Update the forward friction to the friction of the current ground material
            WheelFrictionCurve fFriction = wheel.forwardFriction;
            fFriction.stiffness = hit.collider.material.staticFriction;
            wheel.forwardFriction = fFriction;
            // Update the sideways friction to the friction of the current ground material
            WheelFrictionCurve sFriction = wheel.sidewaysFriction;
            sFriction.stiffness = hit.collider.material.staticFriction;
            wheel.sidewaysFriction = sFriction;
        }
        wheel = rightWheel.collider;
        if (wheel.GetGroundHit(out hit))
        {
            // Update the forward friction to the friction of the current ground material
            WheelFrictionCurve fFriction = wheel.forwardFriction;
            fFriction.stiffness = hit.collider.material.staticFriction;
            wheel.forwardFriction = fFriction;
            // Update the sideways friction to the friction of the current ground material
            WheelFrictionCurve sFriction = wheel.sidewaysFriction;
            sFriction.stiffness = hit.collider.material.staticFriction;
            wheel.sidewaysFriction = sFriction;
        }
    }
}

[System.Serializable]
public struct Wheel
{
    public WheelCollider collider;
    public Transform visual;
    public void SetPose(Vector3 pos, Quaternion rotation)
    {
        visual.position = pos;
        visual.rotation = rotation;
    }
}
