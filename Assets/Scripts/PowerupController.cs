using UnityEngine;
using System.Collections;

public enum PowerUpType
{
    Health,
    Fuel,
    Objective
}

public class PowerupController : MonoBehaviour {

    public PowerUpType type;
    public float powerValue;
    CarUserController carUser;
    bool isActive = false;

    void Start()
    {
        StartCoroutine(WaitForLoading());
    }

    IEnumerator WaitForLoading()
    {
        while (CarUserController.instance == null)
        {
            yield return new WaitForSeconds(0.2f);
        }
        carUser = CarUserController.instance;
        isActive = true;
    }
    void OnTriggerEnter(Collider collider)
    {
        if (isActive)
        {
            GameObject go = collider.gameObject;

            switch (type)
            {
                case PowerUpType.Fuel:
                    carUser.AddFuel(powerValue);
                    HUDControl_MapScene.Instance.Greet("+" + powerValue + " Fuel");
                    break;
                case PowerUpType.Health:
                    carUser.AddHealth(powerValue);
                    HUDControl_MapScene.Instance.Greet("+" + powerValue + " Health");
                    break;
                case PowerUpType.Objective:

                    break;
            }
            this.gameObject.SetActive(false);
        }
    }
}


