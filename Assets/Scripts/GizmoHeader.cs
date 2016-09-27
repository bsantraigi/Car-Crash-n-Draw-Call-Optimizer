using UnityEngine;
using System.Collections;

public class GizmoHeader : MonoBehaviour {
    public Color color;
    [Range(0.1f, 3f)]
    public float size;
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, size);
    }
}
