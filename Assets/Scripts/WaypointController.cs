using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointController : MonoBehaviour {

    Transform[] children;

    public Transform[] waypoints
    {
        get { return children; }
    }

    public Transform this[int i]
    {
        get { return children[i]; }
    }

    void Start()
    {
        children = transform.GetComponentsInChildren<Transform>();
    }

    void OnDrawGizmos()
    {
        Transform[] waypoints = transform.GetComponentsInChildren<Transform>();
        foreach(Transform t in waypoints)
        {
            Gizmos.DrawWireCube(t.position, Vector3.one*0.5f);
        }
        for(int i=0; i<waypoints.Length - 1; i++)
        {
            Debug.DrawLine(waypoints[i].position, waypoints[i + 1].position, Color.red);
        }
    }    
}
