using UnityEngine;
using System.Collections;

public class CarAIController_v1 : MonoBehaviour
{
    public WheelCollider FrontLeftWheel;
    public WheelCollider FrontRightWheel;
    public WheelCollider RearLeftWheel;
    public WheelCollider RearRightWheel;

    public float maxMotorTorque;
    public float maxSteerAngle;
    public float topSpeed;
    public float maxBrakeTorque;
    public float minWaypointDistance;
    public float safeTurnAngleMax; // The steering angle at which the car must start to slow down
    public float safeTurnSpeed;

    public WaypointController waypointController;

    Transform[] waypoints;
    int currentWaypointIndex;
    float currentSpeed;
    bool isSteering = false;
    float workingTopSpeed;
    float angleToTarget;
    float brakeTorque;
    float motorTorque;
    float steerAngle;
    Vector3 dir;
    bool isAllResourceLoaded = false;

    void OnDrawGizmos()
    {
        if (waypoints != null)
        {
            Debug.DrawLine(transform.position, waypoints[currentWaypointIndex].position, Color.cyan);
            Debug.DrawRay(transform.position, transform.forward, Color.cyan);
        }
    }

    void Start()
    {
        StartCoroutine("Loader");
    }

    void Init()
    {
        currentWaypointIndex = 0;
    }

    IEnumerator Loader()
    {
        while (waypoints == null)
        {
            waypoints = waypointController.waypoints;
            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log("Waypoints loaded");
        isAllResourceLoaded = true;
        Init();
    }

    void Update()
    {
        if (isAllResourceLoaded)
        {
            ComputeSpeedAndSteer();
            Steer();
            LookAhead();
            Move();
        }
    }
    void ComputeSpeedAndSteer()
    {
        currentSpeed = Mathf.Abs(FrontLeftWheel.rpm * 2 * Mathf.PI * FrontLeftWheel.radius * 60 / 1000f);
        dir = (waypoints[currentWaypointIndex].position - transform.position);
        angleToTarget = Vector3.Angle(transform.forward, dir) * Mathf.Deg2Rad;
    }

    void Move()
    {
        FrontLeftWheel.motorTorque = motorTorque;
        FrontRightWheel.motorTorque = motorTorque;
        FrontLeftWheel.brakeTorque = brakeTorque;
        FrontRightWheel.brakeTorque = brakeTorque;
    }

    void Steer()
    {
        if (dir.magnitude <= minWaypointDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }

        // Caclulate Steer angle
        Vector3 transformedWaypt = transform.InverseTransformPoint(waypoints[currentWaypointIndex].position);
        float dirFactor = transformedWaypt.x / transformedWaypt.magnitude;
        steerAngle = maxSteerAngle * dirFactor;
        FrontLeftWheel.steerAngle = steerAngle;
        FrontRightWheel.steerAngle = steerAngle;

        // Set working topspeed
        workingTopSpeed = Mathf.Abs(topSpeed * (dirFactor));
    }

    void LookAhead()
    {
        
        int next = (currentWaypointIndex + 1) % waypoints.Length;
        int superNext = (currentWaypointIndex + 2) % waypoints.Length;


        if (currentSpeed <= workingTopSpeed)
        {
            motorTorque = maxMotorTorque;
            if (currentSpeed <= safeTurnSpeed)
            {
                brakeTorque = 0;
            }
            else
            {
                float angleToOneAfter = Vector3.Angle(transform.forward, waypoints[next].position - waypoints[currentWaypointIndex].position) * Mathf.Deg2Rad;
                if (angleToOneAfter > safeTurnAngleMax)
                {
                    // Need to slow down immediately
                    brakeTorque = (1 - Mathf.Cos(angleToOneAfter)) * maxBrakeTorque * (1 - Mathf.Exp(1 / dir.magnitude));
                }
            }
        }
        else
        {
            brakeTorque = maxBrakeTorque;
            motorTorque = 0;
        }

        if (steerAngle >= maxSteerAngle * 0.5 && currentSpeed >= 0.4 * topSpeed)
        {
            if (brakeTorque <= 0.2 * maxBrakeTorque)
            {

            }
        }
    }
}