using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CarUserController : MonoBehaviour
{
    public Transform CenterOfMass;
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteerAngle;
    public float topSpeed;
    public float topRevSpeed;
    public float brakingTorque;
    public float motorTorque;
    public Text SpeedOMeter;
    public ParticleSystem Smoke;

    // Damage Control Variables
    public float maxFuel;
    public float maxHealth;
    public float FuelPerSec;
    public float UltimateDamageForce;
    Vector3 _position;
    float health;
    float PseudoHealth;

    float fuel;
    float PseudoFuel;

    Index _index;
    public Index index
    {
        get
        {
            return _index;
        }
    }
    public Vector3 position
    {
        get
        {
            return _position;
        }
    }

    public bool isDead;
    float currentSpeed;
    Rigidbody carRigidBody;
    bool isBraking;
    MapGenerator mapGenerator;

    public static CarUserController instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        carRigidBody = GetComponent<Rigidbody>();
        SpeedOMeter = GameObject.FindGameObjectWithTag("SpeedOMeter").GetComponent<Text>();
        carRigidBody.centerOfMass = CenterOfMass.localPosition;
        mapGenerator = MapGenerator.Instance;
        health = maxHealth;
        fuel = maxFuel;
        PseudoHealth = health;
        PseudoFuel = fuel;
    }
    public void FixedUpdate()
    {
        _position = transform.position;
        if (!isDead)
        {
            float motor = maxMotorTorque * MultiPlatformInput.GetAxis("Vertical");
            float steering = maxSteerAngle * MultiPlatformInput.GetAxis("Horizontal");
            if (Input.GetButton("Jump"))
            {
                isBraking = true;
            }
            else
            {
                isBraking = false;
            }


            currentSpeed = carRigidBody.velocity.magnitude * 3.6f;

            foreach (AxleInfo axleInfo in axleInfos)
            {
                if (axleInfo.steering)
                {
                    axleInfo.SteerAngle = steering;
                }
                if (axleInfo.motor)
                {
                    if (!isBraking)
                    {
                        axleInfo.MotorTorque = motor;
                        axleInfo.BrakeTorque = 0;

                    }
                    else
                    {
                        axleInfo.MotorTorque = 0;
                        axleInfo.BrakeTorque = brakingTorque;
                    }
                }
                axleInfo.ApplyLocalPositionToVisuals();
                //axleInfo.UpdateFriction();
            }
            mapGenerator.UpdateMinimap(position, transform.eulerAngles);


            // Update the current index
            _index = mapGenerator.WorldPointToMapIndex(position);

            fuel -= FuelPerSec * Time.fixedDeltaTime;
        }
    }
    void Update()
    {
        PseudoHealth = Mathf.Lerp(PseudoHealth, health, 0.4f);
        PseudoFuel = Mathf.Lerp(PseudoFuel, fuel, 0.4f);
        UpdateUI();
    }
    void UpdateUI()
    {
        SpeedOMeter.text = string.Format("{0} KMPH", (int)currentSpeed);
    }
    public void Kill()
    {
        isDead = true;
        foreach(AxleInfo axle in axleInfos)
        {
            if (axle.motor)
            {
                axle.MotorTorque = 0;
                axle.BrakeTorque = brakingTorque*10;
            }
        }
        Smoke.Play();
    }

    void OnCollisionEnter(Collision collision)
    {
        float damageForce = collision.impulse.magnitude / UltimateDamageForce;
        health -= damageForce * 100;
        if (health <= 0)
        {
            GamePlayController.Instance.GameOver();
        }
    }
    public float GetRemainingHealth()
    {
        return PseudoHealth / maxHealth;
    }
    public float GetRemainingFuel()
    {
        return PseudoFuel / maxFuel;
    }

    public void AddFuel(float val)
    {
        fuel += val;
        if(fuel >= maxFuel)
        {
            fuel = maxFuel;
        }
    }

    public void AddHealth(float val)
    {
        health += val;
        if (health >= maxHealth)
        {
            health = maxHealth;
        }
    }

}

