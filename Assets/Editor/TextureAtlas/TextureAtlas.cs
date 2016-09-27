using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TextureAtlas : EditorWindow{
    [MenuItem("Extensions/AtlasGenerator")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureAtlas));
    }

    List<Texture2D> textures;
    void OnGUI()
    {
        if(textures == null)
        {
            textures = new List<Texture2D>();
        }
        for(int i = 0; i< textures.Count; i++)
        {
            textures[i] = EditorGUILayout.ObjectField(textures[i], typeof(Texture2D), false) as Texture2D;
        }
        Object obj = null;
        obj = EditorGUILayout.ObjectField(obj, typeof(Texture2D), false);
        if(obj != null)
        {
            textures.Add(obj as Texture2D);
        }
        if (GUILayout.Button("Add Texture"))
        {
            Debug.Log("Open Popup menu with file list");
            
        }
    }
}

