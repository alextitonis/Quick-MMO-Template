using UnityEngine;

using UnityEditor;

public class MapColliderBuilderEditor : EditorWindow
{

    [MenuItem("Window/Colliders Exporter")]
    public static void ShowWindow()
    {
        GetWindow<MapColliderBuilderEditor>("Colliders Exporter");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Export Colliders"))
            MapColliderBuilder.ExportData();

        if (GUILayout.Button("Destroy All Colliders"))
            MapColliderBuilder.DestroyColliders();
    }
}