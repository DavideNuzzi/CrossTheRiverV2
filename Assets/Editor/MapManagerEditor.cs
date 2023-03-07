using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        MapManager mapManager = (MapManager)target;

        if (GUILayout.Button("Place Trees"))
        {
            mapManager.PlaceTrees();
        }

        // Separo
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 15;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Map loading and saving", titleStyle);
        EditorGUILayout.Space(15f);


        if (GUILayout.Button("Update Map"))
        {
            mapManager.UpdateMapInfoFromEditor();
            mapManager.ResetMap();
            mapManager.CreateMap();
        }

        EditorGUILayout.LabelField("Level to load (ScriptableObject)");
        mapManager.levelToLoad = (Level)EditorGUILayout.ObjectField(mapManager.levelToLoad, typeof(Level), true);

        if (GUILayout.Button("Load Map")) mapManager.LoadMap(mapManager.levelToLoad);

        mapManager.levelSaveFilename = EditorGUILayout.TextField("Map to save", mapManager.levelSaveFilename);

        if (GUILayout.Button("Save Map")) mapManager.SaveMap(mapManager.levelSaveFilename);


    }

    public void OnSceneGUI()
    {
        var t = target as MapManager;

  
        if (t != null && t.playerController != null)
        {
            // Calcolo la distanza massima di salto in base ai parametri del personaggio
            float h = t.playerController.JumpHeight;
            float v = t.playerController.MoveSpeed;
            float g = -t.playerController.Gravity;

            float maxDist = 2f * v * Mathf.Sqrt(2f * h / g);


            if (t.platforms != null)
            {
                if (t.platforms.Count > 0)
                {
                    List<GameObject> platforms = t.platforms;


                    for (int i = 0; i < platforms.Count; i++)
                    {
                        for (int j = i + 1; j < platforms.Count; j++)
                        {
                            // Sparo un raggio verso l'altra piattaforma
                            LayerMask mask = LayerMask.GetMask("Platforms");
                            Ray ray = new Ray(platforms[i].transform.position, platforms[j].transform.position - platforms[i].transform.position);

                            RaycastHit hit;

                            if (Physics.Raycast(ray, out hit, mask))
                            {
                                // Controllo di aver colpito il collider corretto
                                if (hit.collider.gameObject == platforms[j])
                                {
                                    Vector3 p2 = hit.point;

                                    // Mando un raggio nella direzione opposta
                                    Ray ray2 = new Ray(p2, -ray.direction);
                                    RaycastHit hit2;

                                    if (Physics.Raycast(ray2, out hit2, mask))
                                    {
                                        Vector3 p1 = hit2.point;

                                        float d = (p2 - p1).magnitude;
                                        if (d < maxDist * 1.05f)
                                        {
                                            float fac = (d - maxDist * 0.5f) / maxDist * 2f;
                                            Color c = Color.Lerp(Color.green, Color.red, fac);
                                            c += 0.4f * Color.white;
                                            Vector3 meanPoint = (p1 + p2) / 2f;
                                            Handles.color = c;
                                            Handles.DrawDottedLine(p1, p2, 2f);
                                            Handles.Label(meanPoint, d.ToString("n1"));
                                            Handles.color = Color.white;

                                        }
                                    }
                                }
                            }


                        }
                    }
                }

            }
        }

    }
}