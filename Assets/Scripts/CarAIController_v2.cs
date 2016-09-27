using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// This is a static waypoints based physics AI for controlling a car
/// </summary>
public class CarAIController_v2 : MonoBehaviour
{
    public enum DriveMode
    {
        RearWheelDrive,
        FrontWheelDrive,
        FourWheelDrive
    }

    /// If the target variable is set it will derive a path using
    /// the A* implementation using PathFinding class
    public Transform Target;
    public float ViewDistance;
    public DriveMode driveMode;
    public AxleInfo FrontAxle;
    public AxleInfo RearAxle;
    public WaypointController waypointController;
    public float maxMotorTorque;
    public float maxSteerAngle;
    public float topSpeed;
    public float maxBrakeTorque;
    public float minWaypointDistance;
    public float safeTurnSpeed;
    public int AStarPerNFrames;
    public bool isExternalWaypoint;
    public Transform WaypointsParent;
    [Range(1,2)]public float CompulsoryTurningFraction = 1;
    public Slider SpeedSlider;
    public Slider BrakeSlider;


    Vector3[] waypoints;
    int currentWaypointIndex;
    float currentSpeed;
    float speedCap;
    float brakeTorque;
    float motorTorque;
    float steerAngle;
    float workingTopSpeed;
    Vector3 _dir;
    Vector3 dir
    {
        get
        {
            return _dir;
        }
        set
        {
            _dir = value;
            dirMag = _dir.magnitude;
        }
    }
    float dirMag;
    Vector3 _idealPath;
    float idealMag;
    Vector3 idealPath
    {
        get
        {
            return _idealPath;
        }
        set
        {
            _idealPath = value;
            idealMag = _idealPath.magnitude;
            minWaypointDistance = Mathf.Clamp(idealMag * 0.1f, 1, 3);
        }
    }
    Vector3 lastPos;
    bool isAllResourceLoaded;
    Rigidbody carRigidBody;
    MapGenerator mapGenerator;
    bool endOfPath = false;

    void OnDrawGizmos()
    {
        if (waypoints != null)
        {
            if (waypoints.Length > currentWaypointIndex)
            {
                Debug.DrawLine(transform.position, waypoints[currentWaypointIndex], Color.cyan);
                Debug.DrawRay(transform.position, transform.forward, Color.cyan);
            }

            foreach (Vector3 t in waypoints)
            {
                Gizmos.DrawWireCube(t, Vector3.one * 1f);
            }
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                Debug.DrawLine(waypoints[i], waypoints[i + 1], Color.red);
            }
        }
    }

    void Start()
    {
        lastPos = transform.position;
        carRigidBody = GetComponent<Rigidbody>();
        mapGenerator = MapGenerator.Instance;
        
        if (isExternalWaypoint)
        {
            Transform[] points = WaypointsParent.GetComponentsInChildren<Transform>();
            waypoints = new Vector3[points.Length];
            for(int i = 0; i< points.Length; i++)
            {
                waypoints[i] = points[i].position;
            }
            idealPath = waypoints[0] - transform.position;
        }        
        StartCoroutine("FSM");
    }
    void Update()
    {
        if (isAllResourceLoaded)
        {
            FrontAxle.ApplyLocalPositionToVisuals();
            RearAxle.ApplyLocalPositionToVisuals();
        }
        UpdateSliders();
    }

    void Init()
    {
        currentWaypointIndex = 0;
    }

    Index LastTargetTile = new Index(0, 0);
    IEnumerator FSM()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        CarUserController carUserController = null;
        if (go != null)
        {
            Target = go.transform;
            carUserController = CarUserController.instance;
        }
        while ((Target != null && !carUserController.isDead) || isExternalWaypoint)
        {
            if (!isExternalWaypoint)
            {
                // IF TARGET/PLAYER IN RANGE FIND PATH
                Vector3 tPos = Target.position;
                Vector3 ownPos = transform.position;
                float dist = Vector3.Distance(ownPos, tPos);
                if (dist < ViewDistance)
                {
                    // SET THE WAYPOINTS IN THE WAYPOINTS ARRAY
                    Index start = mapGenerator.WorldPointToMapIndex(ownPos);
                    Index end = mapGenerator.WorldPointToMapIndex(tPos);
                    if (!end.Equals(LastTargetTile))
                    {
                        List<Index> path;
                        List<Vector3> pathV = new List<Vector3>();
                        ///TIMER
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        path = PathFinding.FindPath_AStar(start, end, MapGenerator.Instance);

                        if (path != null)
                        {
                            foreach (Index i in path)
                            {
                                Vector3 v = mapGenerator.IndexToWorldLocation(i);
                                v.x += mapGenerator.tileWidth / 2;
                                v.z += mapGenerator.tileHeight / 2;
                                pathV.Add(v);
                            }

                            SimplifyPath_UpdateWaypoints(pathV);
                            //isAllResourceLoaded = true;
                            idealPath = waypoints[0] - transform.position;
                            endOfPath = false;
                        }
                        LastTargetTile = end;

                        ///TIMER
                        sw.Stop();
                        Debug.Log("Time Elapsed: " + sw.ElapsedMilliseconds + " ms ");
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }

            for (int i = 0; i < AStarPerNFrames; i++)
            {
                FrontAxle.UpdateFriction();
                RearAxle.UpdateFriction();
                if (waypoints != null)
                {
                    ComputeSpeedAndSteer();
                    LookAhead();
                    Steer();
                    Move();
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
    List<Vector3> SimplifiedPath = new List<Vector3>();
    void SimplifyPath_UpdateWaypoints(List<Vector3> path)
    {
        if(path.Count < 2)
        {
            // No need to simplify
            currentWaypointIndex = 0;
            waypoints = path.ToArray();
            return;
        }
        // Debug.Log("Path Length: " + path.Count);
        SimplifiedPath.Clear();
        SimplifiedPath.Add(path[0]);
        Vector3 lastDir = path[1] - path[0];
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 next = path[i + 1];
            Vector3 current = path[i];
            Vector3 currentDir = next - current;
            if (currentDir != lastDir)
            {
                SimplifiedPath.Add(current);
            }
            lastDir = currentDir;
        }
        SimplifiedPath.Add(path[path.Count - 1]);

        waypoints = SimplifiedPath.ToArray();
        currentWaypointIndex = 0;
    }

    public int GetSpeed()
    {        
        return (int)(currentSpeed);
    }

    
    void ComputeSpeedAndSteer()
    {
        Vector3 currentPos = transform.position;
        currentSpeed = carRigidBody.velocity.magnitude * 3.6f;
        dir = (waypoints[currentWaypointIndex] - currentPos);
        lastPos = currentPos;
    }

    void Move()
    {        
        switch (driveMode)
        {
            case DriveMode.FourWheelDrive:
                FrontAxle.MotorTorque = motorTorque;
                RearAxle.MotorTorque = motorTorque;

                FrontAxle.BrakeTorque = brakeTorque;
                break;
            case DriveMode.RearWheelDrive:
                RearAxle.MotorTorque = motorTorque;

                RearAxle.BrakeTorque = brakeTorque;
                break;
            case DriveMode.FrontWheelDrive:
                FrontAxle.MotorTorque = motorTorque;

                FrontAxle.BrakeTorque = brakeTorque;
                break;
        }
        
    }

    void Steer()
    {
        // Caclulate Steer angle
        Vector3 forward = transform.forward;
        float angle = Vector3.Angle(dir, forward); // IN DEGREES

        if (dirMag >= CompulsoryTurningFraction*minWaypointDistance)
        {
            steerAngle = Mathf.Lerp(0, maxSteerAngle, angle / 45);
            Vector3 c = Vector3.Cross(dir, forward);
            steerAngle = (c.y < 0) ? steerAngle : -steerAngle;
        }
        else if(currentWaypointIndex < waypoints.Length - 1)
        {
            steerAngle = maxSteerAngle;
            Vector3 futureDir = waypoints[currentWaypointIndex + 1] - transform.position;
            Vector3 c = Vector3.Cross(futureDir, forward);
            steerAngle = (c.y < 0) ? steerAngle : -steerAngle;
        }
        
        // steerAngle = Mathf.Clamp((c.y < 0) ? angle : -angle, -maxSteerAngle, maxSteerAngle);
        
        FrontAxle.SteerAngle = steerAngle;
    }

    void LookAhead()
    {
        if (dirMag <= minWaypointDistance)
        {
            idealPath = waypoints[currentWaypointIndex]; // OLD
            Debug.Log("was " + idealPath);
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                // No more waypoints available
                endOfPath = true;
            }
            idealPath = waypoints[currentWaypointIndex] - idealPath; // NEW - OLD
            Debug.Log("is " + idealPath);

            /// SET WORKING TOPSPEED
            workingTopSpeed = Mathf.Lerp(0, topSpeed, Mathf.Clamp01(idealMag / 12));
        }

        float angle = Vector3.Angle(dir, idealPath);
        speedCap = workingTopSpeed * Mathf.Pow(dirMag * Mathf.Cos(angle) / idealMag, 1);
        if (dirMag <= CompulsoryTurningFraction * minWaypointDistance)
        {
            // speedCap = Mathf.Clamp(speedCap, -safeTurnSpeed, safeTurnSpeed);
            speedCap = safeTurnSpeed;
        }else if(speedCap < safeTurnSpeed)
        {
            speedCap = safeTurnSpeed*dirMag/minWaypointDistance;
        }
        if (currentSpeed <= speedCap)
        {
            motorTorque = Mathf.Lerp(maxMotorTorque, 0, currentSpeed / speedCap);
            brakeTorque = 0;
        }
        else
        {
            motorTorque = 0;
            brakeTorque = Mathf.Lerp(0, maxBrakeTorque, Mathf.Clamp01(currentSpeed / speedCap));
        }
    }

    void UpdateSliders()
    {
        if(SpeedSlider != null)
        {
            SpeedSlider.value = currentSpeed / safeTurnSpeed;
        }
        if(BrakeSlider != null)
        {
            BrakeSlider.value = brakeTorque / maxBrakeTorque;
        }
    }
}