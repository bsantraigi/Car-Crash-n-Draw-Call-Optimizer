using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UIController : MonoBehaviour {

    public CarAIController_v2 observedVehicle;
    
    public Text speedoMeterText;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        speedoMeterText.text = "SPEED:\n" + observedVehicle.GetSpeed() + " KPH";
	}
}
