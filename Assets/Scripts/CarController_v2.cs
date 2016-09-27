using UnityEngine;
using System.Collections;

public class CarController_v2 : MonoBehaviour {

    public GameObject VehicleObject; // The parent object or the car
    public Rigidbody frontWheelBody;
    public Rigidbody rearWheelBody;
    public float acceleration;
    public float rotationSpeed;

    Rigidbody carRigidBody;

    // Use this for initialization
    void Start () {
        carRigidBody = VehicleObject.GetComponent<Rigidbody>();
	}

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        rearWheelBody.AddForce(acceleration * VehicleObject.transform.forward * vertical, ForceMode.Force);
        Debug.DrawLine(VehicleObject.transform.position, VehicleObject.transform.position + 20 * VehicleObject.transform.forward, Color.red);
        if(horizontal != 0)
        {
            float yAngle = horizontal * 90 * Time.fixedDeltaTime;
            // VehicleObject.transform.Rotate(0, 90*Time.fixedDeltaTime, 0, Space.World);
            frontWheelBody.gameObject.transform.Rotate(0, yAngle, 0, Space.World);
            rearWheelBody.gameObject.transform.Rotate(0, yAngle, 0, Space.World);
        }
        // carRigidBody.AddForceAtPosition(VehicleObject.transform.right * horizontal * rotationSpeed, VehicleObject.transform.position + VehicleObject.transform.forward);
        
        // carRigidBody.AddTorque(0, rotationSpeed * Time.fixedDeltaTime * horizontal, 0);

    }
}
