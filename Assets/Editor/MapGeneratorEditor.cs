using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;


        // Prima mostro le variabili pubbliche della classe base
        DrawDefaultInspector();

        /*
        if (GUILayout.Button("Save current map"))
        {
            mapGenerator.SaveMapJSON(mapGenerator.mapSaveName);
            //  mapManager.SaveMapJSON("MappaProva.json");
        }
        */

        // Poi creo un pannello per il poisson sampling

        // Titolo del pannello 
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 15;
        //   titleStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Poisson Sampling Settings", titleStyle);
        EditorGUILayout.Space(15f);

        // Variabili per il Poisson sampling
        mapGenerator.mapSize = EditorGUILayout.Vector2Field("Map Size", mapGenerator.mapSize);
        EditorGUILayout.Space(5f);

        // Raggio dei dischi
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Disk Radius Mode", GUILayout.Width(Screen.width * 0.4f));
        mapGenerator.poissonRadiusMode = EditorGUILayout.Popup(mapGenerator.poissonRadiusMode, mapGenerator.poissonRadiusOptions);
        EditorGUILayout.EndHorizontal();

        if (mapGenerator.poissonRadiusMode == 0)
        {
            mapGenerator.poissonRadius = EditorGUILayout.FloatField("Disk Radius", mapGenerator.poissonRadius);
        }
        if (mapGenerator.poissonRadiusMode == 1 || mapGenerator.poissonRadiusMode == 2)
        {
            mapGenerator.poissonRadiusRandomMin = EditorGUILayout.FloatField("Minimum Disk Radius", mapGenerator.poissonRadiusRandomMin);
            mapGenerator.poissonRadiusRandomMax = EditorGUILayout.FloatField("Maximum Disk Radius", mapGenerator.poissonRadiusRandomMax);

            if (mapGenerator.poissonRadiusRandomMin < 0.25f)
            {
                EditorGUILayout.HelpBox("Minimum radius is too low, generation may be slow!", MessageType.Warning);
            }
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Disk Radius Texture Map");
        mapGenerator.poissonRadiusTexture = (Texture2D)EditorGUILayout.ObjectField(mapGenerator.poissonRadiusTexture, typeof(Texture2D), false);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(5f);

        // Dimensione delle piattaforme
        mapGenerator.platformScale = EditorGUILayout.FloatField("Platform Size", mapGenerator.platformScale);

        if (GUILayout.Button("Genera mappa (Poisson disk sampling)"))
        {
            // Creo la struttura con i parametri per la generazione
            PoissonSamplingParams poissonParams = new PoissonSamplingParams()
            {
                mapSize = mapGenerator.mapSize,
                mode = mapGenerator.poissonRadiusMode,
                radius = mapGenerator.poissonRadius,
                radiusMax = mapGenerator.poissonRadiusRandomMax,
                radiusMin = mapGenerator.poissonRadiusRandomMin,
                radiusTexture = mapGenerator.poissonRadiusTexture,
                platformScale = mapGenerator.platformScale
            };

            List<PlatformInfo> info = mapGenerator.GenerateMapPoissonSampling(poissonParams);
            mapGenerator.mapManager.platformInfo = info;
            mapGenerator.mapManager.ResetMap();
            mapGenerator.mapManager.CreateMap();
            mapGenerator.GenerateNetwork();

        }

        // Titolo del pannello 
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Relax Algorithm Settings", titleStyle);
        EditorGUILayout.Space(15f);

        mapGenerator.neighbourhoodRadius = EditorGUILayout.FloatField("Neighbourhood Radius", mapGenerator.neighbourhoodRadius);
        mapGenerator.platformTargetDistances[0] = EditorGUILayout.FloatField("Target distance 1", mapGenerator.platformTargetDistances[0]);
        mapGenerator.platformTargetDistances[1] = EditorGUILayout.FloatField("Target distance 2", mapGenerator.platformTargetDistances[1]);


        if (GUILayout.Button("Relax Distances"))
        {
            mapGenerator.ForceDistances();
        }


        // 
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Hexagonal Grid Generation", titleStyle);
        EditorGUILayout.Space(15f);

        mapGenerator.mapSizeHex = EditorGUILayout.Vector2Field("Map Size", mapGenerator.mapSizeHex);
        mapGenerator.platformScaleHex = EditorGUILayout.FloatField("Platform Scale", mapGenerator.platformScaleHex);
        mapGenerator.hexSize = EditorGUILayout.FloatField("Hexagon size", mapGenerator.hexSize);
        mapGenerator.shuffleFactor = EditorGUILayout.FloatField("Shuffle factor", mapGenerator.shuffleFactor);
        mapGenerator.minPathLength = EditorGUILayout.IntField("Min path length", mapGenerator.minPathLength);
        mapGenerator.maxPathLength = EditorGUILayout.IntField("Max path length", mapGenerator.maxPathLength);
        mapGenerator.smallPathNumber = EditorGUILayout.IntField("Number of small paths", mapGenerator.smallPathNumber);
        mapGenerator.smallPathStraightProbability = EditorGUILayout.FloatField("Small Path Straightness", mapGenerator.smallPathStraightProbability);
        mapGenerator.smallPathMinimumLength = EditorGUILayout.IntField("Small path minimum length", mapGenerator.smallPathMinimumLength);


        HexGridParams hexParams = new HexGridParams()
        {
            mapSize = mapGenerator.mapSizeHex,
            platformScale = mapGenerator.platformScaleHex,
            size = mapGenerator.hexSize,
            minPathLength = mapGenerator.minPathLength,
            maxPathLength = mapGenerator.maxPathLength,
            smallPathNumber = mapGenerator.smallPathNumber,
            smallPathStraightProbability = mapGenerator.smallPathStraightProbability,
            smallPathMinimumLength = mapGenerator.smallPathMinimumLength
        };

        if (GUILayout.Button("Generate simple path"))
        {
            List<PlatformInfo> info = mapGenerator.HexGridGenerationSimple(hexParams);
            mapGenerator.mapManager.platformInfo = info;
            mapGenerator.mapManager.ResetMap();
            mapGenerator.mapManager.CreateMap();
            mapGenerator.mapManager.ShortestPath();

            //   mapGenerator.mapManager.PlaceStartEndPlatforms()
        }

        if (GUILayout.Button("Generate complex path"))
        {
          
            List<PlatformInfo> info = mapGenerator.HexGridGenerationComplex(mapGenerator.mapManager.platformInfo,hexParams);
            mapGenerator.mapManager.platformInfo = info;
            mapGenerator.mapManager.ResetMap();
            mapGenerator.mapManager.CreateMap();
            mapGenerator.mapManager.ShortestPath();

            //   mapGenerator.mapManager.PlaceStartEndPlatforms()
        }

        if (GUILayout.Button("Shuffle Platforms"))
        {
            List<PlatformInfo> info = mapGenerator.ShufflePlatformsOrthogonal(mapGenerator.mapManager.platformInfo, mapGenerator.shuffleFactor, hexParams);
            mapGenerator.mapManager.platformInfo = info;
            mapGenerator.mapManager.ResetMap();
            mapGenerator.mapManager.CreateMap();
            mapGenerator.mapManager.ShortestPath();

        }

        /*
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Hexagonal Grid Random Size", titleStyle);
        EditorGUILayout.Space(15f);

        mapGenerator.platformScaleHexSmall = EditorGUILayout.FloatField("Platform Scale Small", mapGenerator.platformScaleHexSmall);
        mapGenerator.platformScaleHexLarge = EditorGUILayout.FloatField("Platform Scale Small", mapGenerator.platformScaleHexLarge);


        if (GUILayout.Button("Generate"))
        {
            HexGridParamsRegular hexParams = new HexGridParamsRegular()
            {
                mapSize = mapGenerator.mapSizeHex,
                size = mapGenerator.hexSize,
                platformScaleSmall = mapGenerator.platformScaleHexSmall,
                platformScaleLarge = mapGenerator.platformScaleHexLarge,
            };

            List<PlatformInfo> info = mapGenerator.RegularHexRandomSize(hexParams);
            mapGenerator.mapManager.platformInfo = info;
            mapGenerator.mapManager.ResetMap();
            mapGenerator.mapManager.CreateMap();
        }

        */



    }


    public void OnSceneGUI()
    {
        var t = target as MapGenerator;

        if (t != null)
        {
            if (t.edges != null)
            {
                List<NetworkEdge> edges = t.edges;

                for (int i = 0; i < edges.Count; i++)
                {
                    PlatformInfo p1 = t.mapManager.platformInfo[edges[i].nodeID1];
                    PlatformInfo p2 = t.mapManager.platformInfo[edges[i].nodeID2];
                    Vector3 pos1 = new Vector3(p1.position.x, 0.5f, p1.position.y);
                    Vector3 pos2 = new Vector3(p2.position.x, 0.5f, p2.position.y);
                    float d = Vector2.Distance(p1.position, p2.position);

                    Vector3 meanPoint = (pos1 + pos2) / 2f;
                    Handles.DrawDottedLine(pos1, pos2, 2f);
                    Handles.Label(meanPoint, d.ToString("n1") + "/" + edges[i].weight.ToString("n1"));
                }

            }
        }

    }

}