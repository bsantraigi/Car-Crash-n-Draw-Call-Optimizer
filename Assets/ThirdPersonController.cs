using UnityEngine;
using System.Collections;

public class ThirdPersonController : MonoBehaviour {

    public float speed;
    public Animator BotAnimator;

    bool isWalking = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float v = MultiPlatformInput.GetAxis("Vertical");
        float h = MultiPlatformInput.GetAxis("Horizontal");
        if (!isWalking)
        {
            if (v != 0 || h != 0)
            {
                Debug.Log("True");
                BotAnimator.SetBool("Walking", true);
                isWalking = true;
            }
        }
        else
        {
            if (v == 0 && h == 0)
            {
                Debug.Log("false");
                BotAnimator.SetBool("Walking", false);
                isWalking = false;
            }
        }
        transform.Translate(new Vector3(h, 0, v) * speed);
    }
}
