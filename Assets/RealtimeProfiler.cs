using System.Collections.Generic;
using System.Data;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class RealtimeProfiler : MonoBehaviour
{
    string statsText;
    ProfilerRecorder setPassCallsRecorder;
    ProfilerRecorder drawCallsRecorder;
    ProfilerRecorder verticesRecorder;
    ProfilerRecorder trianglesRecorder;
    ProfilerRecorder totalBatchesRecorder;

    public CounterFPS fpsCounter;
    public DataCollector dataCollector;

    public GUISkin guiSkin;
    public Material waterCool;
    public Material waterSimple;

    public bool resetMouse;

    Light sunLight;
    GameObject water;
    GameObject[] decorations;

    bool profilerOn = false;
    bool decorationsActive = true;
    bool coolWaterActive = true;
    bool shadowsActive = true;


    void OnEnable()
    {
        setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        totalBatchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");

        fpsCounter = FindObjectOfType<CounterFPS>();
        dataCollector = FindObjectOfType<DataCollector>();

        decorations = GameObject.FindGameObjectsWithTag("Decorations");
        water = GameObject.Find("RiverWater");
        sunLight = GameObject.Find("Directional Light").GetComponent<Light>();


    }

    void OnDisable()
    {
        setPassCallsRecorder.Dispose();
        drawCallsRecorder.Dispose();
        verticesRecorder.Dispose();
        totalBatchesRecorder.Dispose();
        trianglesRecorder.Dispose();

    }

    void Update()
    {
        var sb = new StringBuilder(1500);
        if (setPassCallsRecorder.Valid)
            sb.AppendLine($"SetPass Calls: {setPassCallsRecorder.LastValue}");
        if (drawCallsRecorder.Valid)
            sb.AppendLine($"Draw Calls: {drawCallsRecorder.LastValue}");
        if (verticesRecorder.Valid)
            sb.AppendLine($"Vertices: {verticesRecorder.LastValue}");
        if (trianglesRecorder.Valid)
            sb.AppendLine($"Triangles: {trianglesRecorder.LastValue}");
        if (totalBatchesRecorder.Valid)
            sb.AppendLine($"Total Batches: {totalBatchesRecorder.LastValue}");


        if (fpsCounter != null) sb.AppendLine($"FPS: {fpsCounter.fps:F2}");

        if (dataCollector != null)
        {
            if (dataCollector.data != null)
            {
                sb.AppendLine($"Data points collected: {dataCollector.data.Count}");
                sb.AppendLine($"Data size estimate: {(dataCollector.data.Count * 4f * 7f/1024f):F2}kb");
            }
        }

        statsText = sb.ToString();


        if (Input.GetKeyDown(KeyCode.P)) 
        {
            if (profilerOn)
            {
                profilerOn = false;
                if (resetMouse) Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                profilerOn = true;
                if (resetMouse) Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    void OnGUI()
    {
        if (profilerOn)
        {
            GUI.skin = guiSkin;

            GUILayout.Box(statsText);

            if (GUILayout.Button("Toggle decorations"))
            {
                if (decorationsActive)
                {
                    decorationsActive = false;
                    foreach (GameObject g in decorations) g.SetActive(false);
                }
                else
                {
                    decorationsActive = true;
                    foreach (GameObject g in decorations) g.SetActive(true);
                }
            }
            if (GUILayout.Button("Change water shader"))
            {
                if (coolWaterActive)
                {
                    coolWaterActive = false;
                    water.GetComponent<MeshRenderer>().material = waterSimple;
                }
                else
                {
                    coolWaterActive = true;
                    water.GetComponent<MeshRenderer>().material = waterCool;

                }
            }

            if (GUILayout.Button("Shadows"))
            {
                if (shadowsActive)
                {
                    shadowsActive = false;
                    sunLight.shadows = LightShadows.None;
                }
                else
                {
                    shadowsActive = true;
                    sunLight.shadows = LightShadows.Soft;

                }
            }
            if (GUILayout.Button("Next level"))
            {
                GameManager.Instance.StopAllCoroutines();
                GameManager.Instance.NextScene();
            }
        }
    }
}