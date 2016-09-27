using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapEditorScript : Editor
{
    bool PropertiesLoaded = false;
    PathFinding pathfinder;
    int FrameCounter = 0;
    string[] files_arr;

    int selectIndex = 0;

    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = target as MapGenerator;
        if (files_arr == null)
        {
            files_arr = MapControlHub.RefreshFileList();
        }

        DrawDefaultInspector();

        EditorGUI.BeginDisabledGroup(mapGenerator.GenerateNewMap == true);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Play Map");

        int newIndex = EditorGUILayout.Popup(selectIndex, files_arr);
        if(newIndex != selectIndex)
        {
            selectIndex = newIndex;
            mapGenerator.selectedFile = files_arr[selectIndex];
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Refresh Texture"))
        {
            mapGenerator.ReCalculateRoadTypes();
        }

        if (GUILayout.Button("Regenerate"))
        {
            
            mapGenerator.SetInstance();
            mapGenerator.Init();
            mapGenerator.CreateMap();
        }

        
    }
    public void OnSceneGUI()
    {
        Event e = Event.current;
        FrameCounter++;
        Vector2 lastMousePosition = e.mousePosition;
        RaycastHit hit;
        if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(lastMousePosition), out hit))
        {
            MapGenerator mapGenerator = target as MapGenerator;

            Vector3 point = hit.point;
            point.x = (int)(point.x / mapGenerator.tileWidth);
            point.x *= mapGenerator.tileWidth;
            point.x += mapGenerator.tileWidth / 2;

            point.z = (int)(point.z / mapGenerator.tileHeight);
            point.z *= mapGenerator.tileHeight;
            point.z += mapGenerator.tileHeight / 2;
            DrawCube(point);
            if (e.type == EventType.keyDown)
            {
                Index i = mapGenerator.WorldPointToMapIndex(point);
                switch (e.keyCode)
                {
                    case KeyCode.K:
                        mapGenerator.ChangeTile(i, TileType.ROAD);
                        break;
                    case KeyCode.L:
                        mapGenerator.ChangeTile(i, TileType.GROUND);
                        break;
                }

            }
        }
        if (FrameCounter == 8)
        {
            FrameCounter = 0;
            SceneView.RepaintAll();
        }
    }

    void DrawCube(Vector3 point)
    {
        Handles.color = new Color(1, 1, 0, 0.5f);
        Handles.CubeCap(1, point, Quaternion.identity, 7);
    }

    
}
