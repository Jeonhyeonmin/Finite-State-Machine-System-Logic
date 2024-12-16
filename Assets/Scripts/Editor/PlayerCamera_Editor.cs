using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCamera))]
public class PlayerCamera_Editor : Editor
{
    public PlayerCamera cam;

    public override void OnInspectorGUI()
    {
        cam = (PlayerCamera)target;

        base.OnInspectorGUI();

        EditorGUILayout.Space(30);
        EditorGUILayout.LabelField("Player Camera Settings", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = Color.yellow } });
        if (GUILayout.Button("Reset Camera Position"))
        {
            cam.ResetCameraPosition();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cam);
        }
    }

    public void OnSceneGUI()
    {
        if (!cam || !cam.Target)
        {
            return;
        }

        Transform camTarget = cam.Target;
        Vector3 targetPosition = camTarget.position;
        targetPosition.y += cam.LookAtHeight;

        Handles.color = new Color(1, 0, 0, 0.15f);
        Handles.DrawSolidDisc(targetPosition, Vector3.up, cam.Distance);
        Handles.color = new Color(0, 1, 0, 0.75f);
        Handles.DrawWireDisc(targetPosition, Vector3.up, cam.Distance);

        Handles.color = new Color(1, 0, 0, 0.5f);
        cam.Distance = Handles.ScaleSlider(cam.Distance, targetPosition, -camTarget.forward, Quaternion.identity, 2f, 0.1f);
        cam.Distance = Mathf.Clamp(cam.Distance, 1f, float.MaxValue);

        Handles.color = new Color(0, 0, 1, 0.5f);
        cam.Height = Handles.ScaleSlider(cam.Height, targetPosition, Vector3.up, Quaternion.identity, 2f, 0.1f);
        cam.Height = Mathf.Clamp(cam.Height, 0f, float.MaxValue);

        GUIStyle uIStyle = new GUIStyle();
        uIStyle.fontSize = 15;
        uIStyle.normal.textColor = Color.black;

        Texture2D backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, Color.white);
        backgroundTexture.Apply();
        uIStyle.normal.background = backgroundTexture;

        uIStyle.alignment = TextAnchor.MiddleCenter;
        Handles.Label(targetPosition + (-camTarget.forward * 2f), "Distance", uIStyle);

        uIStyle.alignment = TextAnchor.MiddleCenter;
        Handles.Label(targetPosition + (Vector3.up * 2f), "Height", uIStyle);

        cam.HandleCamera();
    }
}
