using UnityEngine;
using System.Collections;

public class MultiPlatformInput : MonoBehaviour {
    static Vector2 origin;
    static Vector2 current;
    static MultiPlatformInput()
    {
        origin = -Vector2.one;
        current = -Vector2.one;
    }
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
            {
                origin = touch.position;
                Debug.Log("Origin");
            }
            else
            {
                current = touch.position;
            }
        }
        else
        {
            origin = current;
        }
    }
    public static float GetAxis(string axis)
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        return Input.GetAxis(axis);
#else
       Vector2 offset = Vector2.zero;
        offset = current - origin;
        switch (axis)        {
            case "Horizontal":
                return Mathf.Clamp(offset.x / 10, -1, 1);
                break;
            case "Vertical":
                return Mathf.Clamp(offset.y / 10, -1, 1);
                break;
        }
        return 0;
#endif

    }

    public static float GetAxisRaw(string axis)
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        return Input.GetAxisRaw(axis);
#else
       Vector2 offset = Vector2.zero;
        offset = current - origin;
        switch (axis)        {
            case "Horizontal":
                return Mathf.Clamp(offset.x / 10, -1, 1);
                break;
            case "Vertical":
                return Mathf.Clamp(offset.y / 10, -1, 1);
                break;
        }
        return 0;

#endif
    }
}
