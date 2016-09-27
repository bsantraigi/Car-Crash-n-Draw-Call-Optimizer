using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HUDControl_MapScene : MonoBehaviour {
    public Slider PlayerHealthSlider;
    public Slider PlayerFuelSlider;
    public Text GreetText;
    public Animator GreetAnimator;

    public static HUDControl_MapScene Instance;

    bool isActive = false;
    void Start()
    {
        if(SceneManager.GetActiveScene().name != "mapScene")
        {
            Debug.Log("Disabling Mapscene HUDControl");
        }
        else
        {
            Instance = this;
            StartCoroutine(WaitForLoading());
        }
    }
    IEnumerator WaitForLoading()
    {
        while(CarUserController.instance == null)
        {
            yield return new WaitForSeconds(0.2f);
        }
        isActive = true;
    }

    void Update()
    {
        if (isActive)
        {
            UpdateHealth(CarUserController.instance.GetRemainingHealth());
            UpdateFuel(CarUserController.instance.GetRemainingFuel());
        }
    }

    public void UpdateHealth(float value)
    {
        PlayerHealthSlider.value = value;
    }

    public void UpdateFuel(float value)
    {
        PlayerFuelSlider.value = value;
    }

    public void Greet(string message)
    {
        GreetText.text = message;
        GreetAnimator.SetTrigger("Powerup");
    }
}
