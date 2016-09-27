using UnityEngine;
using System.Collections;

public class SunController : MonoBehaviour {

    public float Speed;
    Light sun;
    bool sign = true; //Positive
    void Start()
    {
        sun = GetComponent<Light>();
    }
	// Update is called once per frame
	void FixedUpdate () {
        transform.RotateAround(Vector3.zero, Vector3.right, Speed * Time.deltaTime);
        if(transform.position.y < 0 && sign)
        {
            sun.intensity = 0;
            sign = false;
        }
        else if(transform.position.y > 0 && !sign)
        {
            sun.intensity = 0.5f;
            sign = true;
        }
    }
}
