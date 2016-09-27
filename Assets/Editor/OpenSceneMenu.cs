using UnityEngine;
using UnityEditor;
using System.Collections;

public class OpenSceneMenu : Editor {

	[MenuItem("OpenScene/MainGame")]
    public static void OpenScene()
    {
        Debug.Log("Hello");
    }
}
