using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MapControlHub : EditorWindow{
    string AssetName = "";
    string message = "";
    string[] files_arr;
    int selectIndex = 0;
    [MenuItem("Extensions/MapControl")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MapControlHub));
    }
    public static string[] RefreshFileList()
    {
        DirectoryInfo dirInfo = new DirectoryInfo("Assets/Saved Maps");
        List<string> files = new List<string>();
        foreach (FileInfo fi in dirInfo.GetFiles())
        {
            if (fi.Name.EndsWith(".map"))
            {
                files.Add(fi.Name);
            }
        }
        return files.ToArray();
    }
    void OnGUI()
    {
        if(files_arr == null)
        {
            files_arr = RefreshFileList();
        }
        GUILayout.Label("Export Map to Assets:", EditorStyles.boldLabel);
        GUILayout.Space(5);
        AssetName = EditorGUILayout.TextField("Save Map As: ", AssetName);
        
        if (GUILayout.Button("Save Map"))
        {
            // Ask for name here
            MapGenerator mapGenerator = MapGenerator.Instance;
            if (mapGenerator != null)
            {
                string path = "Assets/Saved Maps/" + AssetName + ".map";
                FileInfo fi = new FileInfo(path);
                bool saveFile = false;
                if (fi.Exists)
                {
                    if(EditorUtility.DisplayDialog("Confirm Overwrite", "File with name " + AssetName + ".map already exists. Overwrite?", "Overwrite", "No"))
                    {
                        saveFile = true;
                    }
                }
                else
                {
                    saveFile = true;
                }
                if(saveFile)
                {
                    //AssetDatabase.CreateAsset(mapGenerator.tileMap, "Assets/Saved Maps/" + AssetName + ".asset");
                    File.WriteAllBytes(path, MyFileIO.ObjectToBytesArray(mapGenerator.tileMap));
                    message = "Map successfully saved as " + path + " at " + System.DateTime.Now;
                    files_arr = RefreshFileList();
                    AssetDatabase.Refresh();
                    // Serialize and save the tilemap
                }

            }
            else
            {
                message = "Create a map first. It is null right now.";
            }
        }
        

        GUILayout.Label("Import Map from Assets:", EditorStyles.boldLabel);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Choose map: ");

        selectIndex = EditorGUILayout.Popup(selectIndex, files_arr);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Load Map"))
        {
            string path = "Assets/Saved Maps/" + files_arr[selectIndex];
            // TileMap tileMap = AssetDatabase.LoadAssetAtPath<TileMap>("Assets/Saved Maps/" + AssetName + ".asset");
            TileMap tileMap = MyFileIO.ByteArrayToObject(File.ReadAllBytes(path)) as TileMap;
            if(tileMap != null)
            {
                message = "Tilemap loaded " + tileMap.tiles;
                MapGenerator mapGenerator = MapGenerator.Instance;
                mapGenerator.CreateMapFrom(tileMap);
            }
        }
        if(GUILayout.Button("Refresh List"))
        {
            files_arr = RefreshFileList();
            message = files_arr.Length + "";
        }

        GUILayout.Label("Message:", EditorStyles.boldLabel);
        GUILayout.Label(message, EditorStyles.wordWrappedLabel);
    }

    
}