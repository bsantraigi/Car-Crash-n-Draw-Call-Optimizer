using UnityEngine;
using System.Collections;

public class CarController_v3 : MonoBehaviour {

    // Variables for Forward Wheel Rotation Hinge Motor
    public HingeJoint frontWheelHinge;
    public float TurnSpeed; // Positive
    public float TurnForce; // Positive
    JointMotor frontTurningMotor; // Helps the car to turn

    // Variables for Rear Wheel Motor
    public HingeJoint rearWheelHinge;
    public float DriveMaxSpeed;
    public float DriveAcceleration;
    JointMotor rearDriveMotor; // Helps the car go forward

    // Variable for Direction Input
    float horizontalCache = 0;
    float verticalCache = 0;

    void Start()
    {
        CheckPositive(TurnSpeed, "Turn Speed");
        CheckPositive(TurnForce, "Turn Force");

        CheckPositive(DriveMaxSpeed, "Car Speed");
        CheckPositive(DriveAcceleration, "Acceleration");

        // Turning Motors
        frontTurningMotor = frontWheelHinge.motor;
        frontTurningMotor.force = TurnForce;
        frontWheelHinge.motor = frontTurningMotor;

        // Dirve Motors
        rearDriveMotor = rearWheelHinge.motor;
        rearDriveMotor.force = DriveAcceleration;
        rearWheelHinge.motor = rearDriveMotor;
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        if (horizontalCache != horizontal)
        {
            if (horizontal != 0)
            {
                frontTurningMotor = frontWheelHinge.motor;
                frontWheelHinge.useMotor = true;
                if (horizontal == 1)
                {
                    frontTurningMotor.targetVelocity = -TurnSpeed;
                }
                else
                {
                    frontTurningMotor.targetVelocity = TurnSpeed;
                }
                frontWheelHinge.motor = frontTurningMotor;
            }
            else {
                frontWheelHinge.useMotor = false;                
            }
            horizontalCache = horizontal;
        }

        if (verticalCache != vertical)
        {
            if(vertical == 0)
            {
                rearWheelHinge.useMotor = false;
            }
            else
            {
                rearWheelHinge.useMotor = true;
                rearDriveMotor = rearWheelHinge.motor;
                if(vertical == 1)
                {
                    rearDriveMotor.targetVelocity = -DriveMaxSpeed;
                }
                else
                {
                    rearDriveMotor.targetVelocity = DriveMaxSpeed;
                }
                rearWheelHinge.motor = rearDriveMotor;
            }
            verticalCache = vertical;
        }
    }

    void CheckPositive(float var, string name)
    {
        if(var <= 0)
        {
            Debug.LogError(name + " must be positive.");
        }
    }
}
