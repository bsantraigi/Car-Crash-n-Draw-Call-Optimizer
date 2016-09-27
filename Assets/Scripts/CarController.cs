using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CarController : MonoBehaviour {

	public float acceleration;
    public float rotationSpeed;
    public GameObject FrontWheelCenter;
    public GameObject RearWheelCenter;
    public Text SpeedText;
    float speed;

	Rigidbody carRigidBody;

	// Use this for initialization
	void Start () {
		carRigidBody = GetComponent<Rigidbody> ();
	}
	
	void Update()
    {
        SpeedText.text = (int) speed + " MPH";
    }

	void FixedUpdate () {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        speed = carRigidBody.velocity.magnitude;
        if(vertical != 0)
        {
            carRigidBody.AddForce(transform.forward * acceleration * vertical);
        }
        if (horizontal != 0)
        {
            transform.RotateAround(RearWheelCenter.transform.position, transform.up, horizontal*vertical*rotationSpeed*Time.fixedDeltaTime);
            // transform.Rotate(new Vector3(0, rotationSpeed * Time.fixedDeltaTime * horizontal, 0));
        }
	}
}
