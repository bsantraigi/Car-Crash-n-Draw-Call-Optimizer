using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public enum CarViewMode
    {
        Orthographic,
        Classic3D
    }
    delegate void UpdateFunction();
    UpdateFunction updateFunction;

    public CarViewMode carViewMode;
    public GameObject target;
    public GameObject setupTarget;
    public LayerMask TerrainLayer;

    Vector3 offset;
    float offsetMag;
    Camera mainCam;
	// Use this for initialization
    public void Start()
    {
        offset = -(setupTarget.transform.position - gameObject.transform.position);
    }
    public void DelayedStart () {
        mainCam = Camera.main;
        offsetMag = offset.magnitude;
        switch (carViewMode)
        {
            case CarViewMode.Classic3D:
                updateFunction += PerspectiveUpdate;
                break;
            case CarViewMode.Orthographic:
                updateFunction += OrthographicUpdate;
                break;
        }   
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (GamePlayController.Instance.Loaded)
        {
            updateFunction();
        }
	}

    void OrthographicUpdate()
    {
        transform.position = target.transform.position + offset;
    }
    void PerspectiveUpdate()
    {
        Vector3 position = target.transform.position - offsetMag * target.transform.forward + 3*Vector3.up;
        RaycastHit hit;
        {
            Vector3 start = transform.position;
            Debug.DrawLine(start,start + 30*Vector3.down, Color.red);

        }
        if (Physics.Raycast(position, Vector3.up, out hit, 30, TerrainLayer))
        {
            position.y = Mathf.Clamp(position.y, hit.point.y + 1, Mathf.Infinity);
        }
        transform.position = position;
        transform.LookAt(target.transform);
    }
}
