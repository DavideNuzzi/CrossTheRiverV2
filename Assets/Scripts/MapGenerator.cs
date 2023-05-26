using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using Proyecto26;

public class MapGenerator : MonoBehaviour
{
    public MapManager mapManager;

    // Parametri per Poisson
    [HideInInspector]
    public Vector2 mapSize = new Vector2(20f, 20f);
    [HideInInspector]
    public string[] poissonRadiusOptions = new string[] { "Value", "Random", "Texture" };
    [HideInInspector]
    public float poissonRadius = 1.5f;
    [HideInInspector]
    public int poissonRadiusMode = 0;
    [HideInInspector]
    public float poissonRadiusRandomMin = 1f;
    [HideInInspector]
    public float poissonRadiusRandomMax = 5f;
    [HideInInspector]
    public Texture2D poissonRadiusTexture = null;
    [HideInInspector]
    public float platformScale = 0.5f;
    [HideInInspector]
    public bool areSlippery = false;

    public string mapSaveName = "Prova";

    // Parametri per il rilassamento
    [HideInInspector]
    public float neighbourhoodRadius;
    [HideInInspector]
    public float[] platformTargetDistances = new float[2];

    [HideInInspector]
    public List<NetworkEdge> edges;

    // Parametri per la griglia esagonale
    [HideInInspector]
    public Vector2 mapSizeHex = new Vector2(20f, 20f);
    [HideInInspector]
    public float platformScaleHex = 0.5f;
    [HideInInspector]
    public float hexSize = 1f;
    [HideInInspector]
    public float shuffleFactor = 0.2f;
    [HideInInspector]
    public int minPathLength = 3;
    [HideInInspector]
    public int maxPathLength = 7;
    [HideInInspector]
    public float smallPathStraightProbability = 1f;
    [HideInInspector]
    public int smallPathMinimumLength = 3;
    [HideInInspector]
    public int smallPathNumber = 3;




    [HideInInspector]
    public float platformScaleHexSmall = 0.7f;
    [HideInInspector]
    public float platformScaleHexLarge = 1.1f;

    

    /*
    public void SaveMapAsset(string filename)
    {
        string path = "Assets/ScriptableObjects/Levels/" + filename + ".asset";
     //   Level level = new Level();
        Level level = ScriptableObject.CreateInstance<Level>();
        level.sceneName = filename;
        level.platformInfo = mapManager.platformInfo;
        AssetDatabase.CreateAsset(level, path);

    }

    public void SaveMapJSON(string filename)
    {
        SaveMapAsset(filename);

        string mapToJson = JsonHelper.ArrayToJsonString(mapManager.platformInfo.ToArray(), true);
        StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + "/Levels/" + filename, false);
        writer.Write(mapToJson);
        writer.Close();

    }
    */

    public List<PlatformInfo> RegularHexRandomSize(HexGridParamsRegular hexParams)
    {
        List<PlatformInfo> platformInfo = new List<PlatformInfo>();

        mapSize = hexParams.mapSize;

        float scaleBig = 1.1f;
        float scaleSmall = 0.7f;
        float s = 1.3f;

        // Genero un percorso con piattaforme "grandi" ma tortuoso
        // L'idea è che se sono andato verso l'alto posso andare solo a sinistra e destra
        // Se sono andato a sinistra posso andare solo verso sinistra o verso l'alto
        // Lo stesso per la destra


        PlatformInfo platform = new PlatformInfo() { position = new Vector2(0, -mapSize.y / 2f), isSlippery = false, scale = scaleBig };
        platformInfo.Add(platform);

        int lastDir = -1;


        // Ripeto fino a che non sono alla fine
        bool finished = false;
        int n = 0;

        while (!finished)
        {
            // Genero la lista delle possibili nuove piattaforme
            PlatformInfo lastPlatform = platformInfo[platformInfo.Count - 1];

            Vector2 nextPosition = new Vector2();
            int nextDirection = 0;

            if (GetPossibileNextPlatform(lastDir, lastPlatform, s, mapSize, out nextPosition, out nextDirection))
            {

                lastDir = nextDirection;

                PlatformInfo newPlatform = new PlatformInfo()
                {
                    position = nextPosition,
                    isSlippery = false,
                    scale = scaleBig
                };
                platformInfo.Add(newPlatform);


            }
            else
            {
                finished = true;
            }

            n++;
            if (n > 50) break;

        }

        List<PlatformInfo> largePlatforms = new List<PlatformInfo>(platformInfo);


        // Genero ora le piattaforme piccole attorno
        for (int i = 0; i < 100; i++)
        {
            int r = Random.Range(0, platformInfo.Count);
            PlatformInfo randPlatform = platformInfo[r];

            /*
            Vector2[] deltas = new Vector2[6]
            {       
                new Vector2(-1.5f * Mathf.Sqrt(3f) * s, 1.5f * s),
                new Vector2(0, 3f * s),
                new Vector2(1.5f * Mathf.Sqrt(3f) * s, 1.5f * s),
                new Vector2(-1.5f * Mathf.Sqrt(3f) * s, -1.5f * s),
                new Vector2(0, -3f * s),
                new Vector2(1.5f * Mathf.Sqrt(3f) * s, -1.5f * s)
            };
            */


            Vector2[] deltas = new Vector2[6]
            {
                new Vector2(0, 3f * s),
                new Vector2(0, 3f * s),
                new Vector2(0, 3f * s),
                new Vector2(-1.5f * Mathf.Sqrt(3f) * s, 1.5f * s),
                new Vector2(0, -3f * s),
                new Vector2(1.5f * Mathf.Sqrt(3f) * s, 1.5f * s)
            };

            Vector2 newPos = randPlatform.position + deltas[Random.Range(0, 6)];

            if (newPos.x >= -mapSize.x / 2f && newPos.x <= mapSize.x / 2f && newPos.y >= -mapSize.y / 2f && newPos.y <= mapSize.y / 2f)
            {
                bool goodPos = true;

                for (int j = 0; j < platformInfo.Count; j++)
                {
                    if ((platformInfo[j].position - newPos).magnitude < 0.7f * s)
                    {
                        goodPos = false;
                        break;
                    }
                }

                if (goodPos)
                {
                    PlatformInfo newPlatform = new PlatformInfo()
                    {
                        position = newPos,
                        isSlippery = false,
                        scale = scaleSmall
                    };
                    platformInfo.Add(newPlatform);
                }
            }
        }
        return platformInfo;
    }

    bool GetPossibileNextPlatform(int lastDir, PlatformInfo lastPlatform, float s, Vector2 mapSize, out Vector2 nextPosition, out int nextDirection)
    {
        List<Vector2> possiblePositions = new List<Vector2>();
        List<int> possibleDirections = new List<int>();

        if (lastDir == -1) // Prima piattaforma
        {
            possiblePositions = new List<Vector2>()
            {
                lastPlatform.position +new Vector2(-1.5f * Mathf.Sqrt(3f) * s, 1.5f * s),
                lastPlatform.position +new Vector2(0, 3f * s),
                lastPlatform.position +new Vector2(1.5f * Mathf.Sqrt(3f) * s, 1.5f * s)
            };

            possibleDirections = new List<int>() { 0, 1, 2 };
        }
        else if (lastDir == 0) // Sinistra
        {
            possiblePositions = new List<Vector2>()
            {
              //  lastPlatform.position +new Vector2(-1.5f * Mathf.Sqrt(3f) * s, 1.5f * s),
                lastPlatform.position +new Vector2(-1.5f * Mathf.Sqrt(3f) * s, 1.5f * s),
                lastPlatform.position +new Vector2(0, 3f * s)
            };

          //  possibleDirections = new List<int>() { 0, 0, 1 };
            possibleDirections = new List<int>() { 0, 1 };

        }
        else if (lastDir == 1) // Dritto
        {
            possiblePositions = new List<Vector2>()
            {
                lastPlatform.position +new Vector2(-1.5f * Mathf.Sqrt(3f) * s, 1.5f * s),
                lastPlatform.position +new Vector2(1.5f * Mathf.Sqrt(3f) * s, 1.5f * s)
            };

            possibleDirections = new List<int>() { 0, 2 };

        }
        else if (lastDir == 2) // Destra
        {
            possiblePositions = new List<Vector2>()
            {
                lastPlatform.position +new Vector2(0, 3f * s),
                lastPlatform.position +new Vector2(1.5f * Mathf.Sqrt(3f) * s, 1.5f * s)
            };

            possibleDirections = new List<int>() { 1, 2 };

        }

        for (int i = 0; i < possiblePositions.Count; i++)
        {
            Vector2 newPos = possiblePositions[i];
            if (!(newPos.x >= -mapSize.x / 2f && newPos.x <= mapSize.x / 2f && newPos.y >= -mapSize.y / 2f && newPos.y <= mapSize.y / 2f))
            {
                possiblePositions.RemoveAt(i);
                possibleDirections.RemoveAt(i);
                i--;
            }
        }

        if (possiblePositions.Count > 0)
        {
            int r = Random.Range(0, possiblePositions.Count);
            nextPosition = possiblePositions[r];
            nextDirection = possibleDirections[r];
            return true;
        }
        else
        {
            nextPosition = Vector2.zero;
            nextDirection = -1;
            return false;
        }
    }

    /*
    public List<PlatformInfo> HexGridGenerationSimple(HexGridParams hexParams)
    {
        // Costruisco il path più breve
        List<PlatformInfo> platformInfo = new List<PlatformInfo>();
        Vector2 mapSize = hexParams.mapSize;
        float s = hexParams.size;

        PlatformInfo platform = new PlatformInfo() { position = new Vector2(0, -mapSize.y / 2f), isSlippery = false, scale = hexParams.platformScale };
        platformInfo.Add(platform);

        // Ripeto fino a che non sono alla fine
        bool finished = false;
        while (!finished)
        {
            // Genero la lista delle possibili nuove piattaforme
            PlatformInfo lastPlatform = platformInfo[platformInfo.Count - 1];

            List <PlatformInfo> possiblePlatforms = new List<PlatformInfo>();
            Vector2[] deltas = new Vector2[3]
            {
                new Vector2(-1.5f * Mathf.Sqrt(3f) * s, 1.5f * s),
                new Vector2(0, 3f * s),
                new Vector2(1.5f * Mathf.Sqrt(3f) * s, 1.5f * s)
            };


            for (int i = 0; i < 3; i++)
            {
                Vector2 newPos = lastPlatform.position + deltas[i];
                if (newPos.x >= - mapSize.x/2f && newPos.x <= mapSize.x/2f && newPos.y >= -mapSize.y/2f && newPos.y <= mapSize.y/2f)
                {
                    PlatformInfo newPlatform = new PlatformInfo()
                    {
                        position = newPos,
                        isSlippery = false,
                        scale = hexParams.platformScale
                    };

                    possiblePlatforms.Add(newPlatform);
                }
            }

            if (possiblePlatforms.Count > 0)
            {
                int r = Random.Range(0, possiblePlatforms.Count);
                platformInfo.Add(possiblePlatforms[r]);
            }
            else 
            {
                finished = true;
            }
        }
        return platformInfo;
    }
     
    public List<PlatformInfo> HexGridGenerationComplex(List<PlatformInfo> platformInfo, HexGridParams hexParams)
    {
        Vector2 mapSize = hexParams.mapSize;
        float s = hexParams.size;

        // Lista delle distanze per i terzi vicini
        Vector2[] deltas3 = new Vector2[6]
        {
            new Vector2(-Mathf.Sqrt(3)*s, 3 * s),
            new Vector2(Mathf.Sqrt(3)*s, 3 * s),
            new Vector2(-Mathf.Sqrt(3)*s, -3 * s),
            new Vector2(Mathf.Sqrt(3)*s, -3 * s),
            new Vector2(-2 * Mathf.Sqrt(3)*s, 0),
            new Vector2(2 * Mathf.Sqrt(3)*s, 0)
        };

        Vector2[] deltas4 = new Vector2[12]
        {
            new Vector2(-Mathf.Sqrt(3)/2f * s, 5 * s),
            new Vector2(Mathf.Sqrt(3)/2f * s, 5 * s ),
            new Vector2(-Mathf.Sqrt(3)/2f * s, -5 * s),
            new Vector2(Mathf.Sqrt(3)/2f * s, -5 * s),
            new Vector2(-Mathf.Sqrt(3)/2f*5f*s, 1.5f * s),
            new Vector2(-Mathf.Sqrt(3)*4f*s, 3f * s),            
            new Vector2(Mathf.Sqrt(3)/2f*5f*s, 1.5f * s),
            new Vector2(Mathf.Sqrt(3)*4f*s, 3f * s),
            new Vector2(-Mathf.Sqrt(3)/2f*5f*s, -1.5f * s),
            new Vector2(-Mathf.Sqrt(3)*4f*s, -3f * s),
            new Vector2(Mathf.Sqrt(3)/2f*5f*s, -1.5f * s),
            new Vector2(Mathf.Sqrt(3)*4f*s, -3f * s),
        };

        Vector2[] deltas = deltas4;
        // Vector2[] deltas = deltas3;

        List<PlatformInfo> difficultPlatforms = new List<PlatformInfo>();

        for (int n = 0; n < 1000; n++)
        {
            int r = Random.Range(0, platformInfo.Count);
            PlatformInfo platform = platformInfo[r];
            // Scelgo una piattaforma a caso
            if (difficultPlatforms.Count > 0)
            {
                r = Random.Range(0, difficultPlatforms.Count);
                platform = difficultPlatforms[r];
            }

            // Provo a generare una piattaforma in una nuova posizione da secondo vicino
            Vector2 newPos = platform.position + deltas[Random.Range(0, deltas.Length)];

            if (newPos.x >= -mapSize.x / 2f && newPos.x <= mapSize.x / 2f && newPos.y >= -mapSize.y / 2f && newPos.y <= mapSize.y / 2f)
            {
                // Controllo se la posizione sia buona
                float dist = 4.3f * s;
            //    float dist = 3f * s;

                bool goodPos = GoodNewPositionHex(platformInfo, newPos, dist);

                if (goodPos)
                {
                    PlatformInfo newPlatform = new PlatformInfo() { position = newPos, isSlippery = false, scale = hexParams.platformScale * 0.8f };
                    platformInfo.Add(newPlatform);
                    difficultPlatforms.Add(newPlatform);
                }
            }
        }
        return platformInfo;
    }
   

    */

    public List<PlatformInfo> HexGridGenerationComplex(List<PlatformInfo> platformInfo, HexGridParams hexParams)
    {
        // Rimuovo tutte le pietre piccole che già ci sono
        for (int i = 0; i < platformInfo.Count; i++)
        {
            if (platformInfo[i].scale < hexParams.platformScale * 0.9f)
            {
                platformInfo.RemoveAt(i);
                i--;
            }
        }

        Vector2 mapSize = hexParams.mapSize;
        float s = hexParams.size;


        int pathsToGenerate = hexParams.smallPathNumber;
        float straightProbability = hexParams.smallPathStraightProbability;
        int pathMinimumLength = hexParams.smallPathMinimumLength;


        // Scelgo una pietra a caso e provo a generare un path da essa
        // Il path continua in verticale finché non trova una nuova roccia a "distanza"
        int pathsAdded = 0;

        for (int n = 0; n < 500; n++)
        {
            PlatformInfo randomPlatformStart = platformInfo[Random.Range(0, platformInfo.Count)];
            List<PlatformInfo> possiblePath = new List<PlatformInfo>();
            bool pathValid = true;

            if (randomPlatformStart.scale < hexParams.platformScale) continue;

            while (pathValid)
            {
                Vector2 lastPlatformPos = randomPlatformStart.position;
                if (possiblePath.Count > 0) lastPlatformPos = possiblePath[possiblePath.Count - 1].position;

                Vector2 delta = Vector2.zero;
                float r = Random.value;

                if (r < straightProbability) delta = new Vector2(0, 3f * s);
                else if (r < straightProbability + (1 - straightProbability) / 2f + 1e-5) delta = new Vector2(1.5f * s  *Mathf.Sqrt(3), 1.5f * s);
                else delta = new Vector2(-1.5f * s * Mathf.Sqrt(3), 1.5f * s);


                Vector2 newPos = lastPlatformPos + delta;

                // Controllo se la nuova posizione coincide con una pietra già presente, in tal caso il path è finito
                bool goodPos = GoodNewPositionHex(platformInfo, newPos, 2.5f * s);

                if (goodPos)
                {
                    PlatformInfo newPlatform = new PlatformInfo()
                    {
                        position = newPos,
                        isSlippery = false,
                        scale = hexParams.platformScale * 0.7f
                    };

                    possiblePath.Add(newPlatform);
                }
                else
                {
                    break;
                }

                // Se ho superato il bordo smetto
                if (newPos.y > mapSize.y / 2f || newPos.x < -mapSize.x / 2f || newPos.x > mapSize.x / 2f) pathValid = false;
            }


            // Non creo davvero il path se è troppo corto
            if (possiblePath.Count >= pathMinimumLength && pathValid)
            {
                platformInfo.AddRange(possiblePath);
                pathsAdded++;
            }

            if (pathsAdded >= pathsToGenerate) break;
        }

        return platformInfo;

    }

    public List<PlatformInfo> HexGridGenerationSimple(HexGridParams hexParams)
    {
        List<PlatformInfo> platformInfo = new List<PlatformInfo>();

        Vector2 mapSize = hexParams.mapSize;
        float s = hexParams.size;
        int minPathLength = hexParams.minPathLength;
        int maxPathLength = hexParams.maxPathLength + 1;

        bool lastPath = false;

        // Metto la prima piattaforma e imposto casualmente la prima direzione e lunghezza
        PlatformInfo platform = new PlatformInfo() { position = new Vector2(0, -mapSize.y / 2f), isSlippery = false, scale = hexParams.platformScale };
        platformInfo.Add(platform);

        int dir = -1; // Sinistra
        if (Random.value < 0.5f) dir = 1; // Destra

        for (int i = 0; i < 100; i++)
        {
            int length = Random.Range(minPathLength, maxPathLength);
            dir *= -1;

            // Controllo se sono all'ultimo, in tal caso lo forzo
            Vector2 oldPos = platformInfo[platformInfo.Count - 1].position;
            float goalDeltaY = Mathf.Abs(oldPos.y - mapSize.y / 2f);
            float goalDeltaX = oldPos.x;
            float upShift = 1.5f * s;

            if (goalDeltaY < maxPathLength * upShift)
            {
                length = Mathf.RoundToInt(goalDeltaY / (upShift));
                if (goalDeltaX > 0) dir = -1;
                else dir = 1;
                lastPath = true;
            }

            for (int n = 0; n < length; n++)
            {
                Vector2 newPos = platformInfo[platformInfo.Count - 1].position + new Vector2(dir * 1.5f * Mathf.Sqrt(3) * s, 1.5f * s);
                PlatformInfo newPlatform = new PlatformInfo()
                {
                    position = newPos,
                    isSlippery = false,
                    scale = hexParams.platformScale
                };

                platformInfo.Add(newPlatform);
            }

            if (lastPath) break;

        }

        return platformInfo;
    }


    bool GoodNewPositionHex(List<PlatformInfo> platforms, Vector2 newPos, float minDist)
    {
        bool good = true;

        for (int i = 0; i < platforms.Count; i++)
        {
            if ((platforms[i].position - newPos).magnitude < minDist)
            {
                good = false;
                break;
            }
        }

        return good;
    }

    public List<PlatformInfo> ShufflePlatforms(List<PlatformInfo> platforms, float shuffleFactor)
    {
        for (int i = 0; i < platforms.Count; i++)
        {
            Vector2 newPos = platforms[i].position + (Vector2.one * (Random.value * 2f - 1f)) * shuffleFactor;
            platforms[i] = new PlatformInfo() { position = newPos, isSlippery = platforms[i].isSlippery, scale = platforms[i].scale };
        }

        return platforms;
    }

    public List<PlatformInfo> ShufflePlatformsOrthogonal(List<PlatformInfo> platforms, float shuffleFactor, HexGridParams hexParams)
    {
        // Shuffling delle piattaforme fatto in maniera intelligente, cioè ortogonalmente alla direzione che le separa dalle altre piattaforme vicine
        // Se è più di una prendo la media pesata
        Vector2[] displacementVectors = new Vector2[platforms.Count];

        for (int i = 0; i < platforms.Count; i++)
        {
            Vector2 pathDir = Vector2.zero;
            float weight = 0;

            for (int j = 0; j < platforms.Count; j++)
            {
                if (i != j)
                {
                    if ((platforms[j].position - platforms[i].position).magnitude < 3.5f * hexParams.size)
                    {
                        Vector2 dir = (platforms[j].position - platforms[i].position).normalized;

                        if (j > i) dir *= -1;

                        weight += platforms[j].scale;
                        pathDir += dir * platforms[j].scale;
                    }
                }
            }
            if (weight > 0)
            {
                pathDir /= weight;
                Vector3 orthoVector = Vector3.Cross(new Vector3(pathDir.x, 0, pathDir.y), new Vector3(0, 1, 0));
                displacementVectors[i] = new Vector2(orthoVector.x, orthoVector.z);
            }
        }

        for (int i = 0; i < platforms.Count; i++)
        {
            PlatformInfo platform = platforms[i];
            platform.position += (Random.value - 0.5f) * shuffleFactor * displacementVectors[i];
          //  platform.position += displacementVectors[i];

            platforms[i] = platform;

        }

        return platforms;
    }


    public List<PlatformInfo> GenerateMapPoissonSampling(PoissonSamplingParams poissonParams)
    {
        List<PlatformInfo> platformInfo = new List<PlatformInfo>();
        List<float> diskRadiusList = new List<float>();

        Vector2 mapSize = poissonParams.mapSize;

        // Per ora lo approccio così: non fisso il numero di dischi, ma continuo
        // a generarli indefinitamente. Se per almeno n = 100 iterazioni non ho
        // messo nessun disco, suppongo di aver finito
        bool completed = false;
        int iterSinceLastPlacement = 0;

        while(!completed)
        {
            // Genero le coordinate della nuova piattaforma
            // Tengo conto della modalità di generazione per la scelta del raggio di esclusione
            float radius = 0;

            if (poissonParams.mode == 0) radius = poissonParams.radius;
            else if (poissonParams.mode == 1) radius = Random.Range(poissonParams.radiusMin, poissonParams.radiusMax);

            float x = Random.value * (mapSize.x - 2f * radius) + radius - mapSize.x / 2f;
            float y = Random.value * (mapSize.y - 2f * radius) + radius - mapSize.y / 2f;
            Vector2 pos = new Vector2(x, y);

            if (poissonParams.radiusTexture != null)
            {
                Texture2D tex = poissonParams.radiusTexture;
                int x1 = (int)((x * 2f + mapSize.x) / (2f * mapSize.x) * tex.width);
                int y1 = (int)((y * 2f + mapSize.y) / (2f * mapSize.y) * tex.height);

                float intensity = tex.GetPixel(x1,y1).r;
                if (intensity < 0.5f)
                {
                 //   iterSinceLastPlacement++;
                    continue;
                }
            }


            // Controllo se è troppo vicina a una già inserita
            bool goodPosition = true;
            for (int i = 0; i < platformInfo.Count; i++)
            {
                Vector2 p = platformInfo[i].position;
                if ((p - pos).magnitude < diskRadiusList[i] + radius)
                {
                    goodPosition = false;
                    break;
                }
            }

            if (goodPosition)
            {
                float platformScale = poissonParams.platformScale;
                PlatformInfo newPlatform = new PlatformInfo() { position = pos, isSlippery = false, scale = platformScale };
                platformInfo.Add(newPlatform);
                diskRadiusList.Add(radius);
                iterSinceLastPlacement = 0;
            }
            else
            {
                iterSinceLastPlacement++;
            }

            if (iterSinceLastPlacement > 2000) completed = true;
        }

        return platformInfo;

    }

    bool CCW(Vector2 A,Vector2 B, Vector2 C)
    {
        return (C.y - A.y) * (B.x - A.x) > (B.y - A.y) * (C.x - A.x);
    }

    bool EdgesIntersect(NetworkEdge e1, NetworkEdge e2)
    {
        Vector2 A = mapManager.platformInfo[e1.nodeID1].position;
        Vector2 B = mapManager.platformInfo[e1.nodeID2].position;
        Vector2 C = mapManager.platformInfo[e2.nodeID1].position;
        Vector2 D = mapManager.platformInfo[e2.nodeID2].position;
        return CCW(A, C, D) != CCW(B, C, D) && CCW(A, B, C) != CCW(A, B, D);
    }

    public void GenerateNetwork()
    {
        // Genero tutti gli edges tra piattaforme
        edges = new List<NetworkEdge>();

        /*
        for (int i = 0; i < mapManager.platformInfo.Count; i++)
        {
            for (int j = i + 1; j < mapManager.platformInfo.Count; j++)
            {
                PlatformInfo p1 = mapManager.platformInfo[i];
                PlatformInfo p2 = mapManager.platformInfo[j];
                float d = Vector2.Distance(p1.position, p2.position);

                if (d < neighbourhoodRadius)
                {
                    float weight = platformTargetDistances[Random.Range(0, platformTargetDistances.Length)];
                    NetworkEdge edge = new NetworkEdge()
                    {
                        nodeID1 = i,
                        nodeID2 = j,
                        weight = weight
                    };

                    edges.Add(edge);
                }
            }
        }
        */

        // Genero tutti quanti gli edges
        float maxDist = Mathf.Max(platformTargetDistances);

        for (int i = 0; i < mapManager.platformInfo.Count; i++)
        {
            for (int j = i + 1; j < mapManager.platformInfo.Count; j++)
            {
                PlatformInfo p1 = mapManager.platformInfo[i];
                PlatformInfo p2 = mapManager.platformInfo[j];
                float d = Vector2.Distance(p1.position, p2.position);

                if (d <= maxDist * 2f)
                {
                    // Il peso è casuale, ma sempre lo stesso per una data coppia di piattaforme
                    /*
                    Random.InitState(i * mapManager.platformInfo.Count + j);
                    float weight = platformTargetDistances[Random.Range(0, platformTargetDistances.Length)];
                    */

                    
                    // Il peso è dato dalla distanza, nella lista, più vicina alla reale
                    float minDist = float.MaxValue;
                    float weight = -1;

                    for (int k = 0; k < platformTargetDistances.Length; k++)
                    {
                        if (Mathf.Abs(d - platformTargetDistances[k]) < minDist)
                        {
                            minDist = Mathf.Abs(d - platformTargetDistances[k]);
                            weight = platformTargetDistances[k];
                        }
                    }
                    

                    NetworkEdge edge = new NetworkEdge()
                    {
                        nodeID1 = i,
                        nodeID2 = j,
                        weight = weight
                    };

                    edges.Add(edge);
                }
            }
        }

        // Elimino gli edges che si sovrappongono, scegliendo sempre di tenere quello più corto
        for (int n = 0; n < 10; n++)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                NetworkEdge e1 = edges[i];

                for (int j = i + 1; j < edges.Count; j++)
                {
                    NetworkEdge e2 = edges[j];

                    // Salto se hanno una stessa piattaforma d'origine
                    if (e1.nodeID1 == e2.nodeID1 || e1.nodeID1 == e2.nodeID2 || e1.nodeID2 == e2.nodeID1 || e1.nodeID2 == e2.nodeID2) continue;

                    if (EdgesIntersect(e1, e2))
                    {
                        float l1 = (mapManager.platformInfo[e1.nodeID1].position - mapManager.platformInfo[e1.nodeID2].position).magnitude;
                        float l2 = (mapManager.platformInfo[e2.nodeID1].position - mapManager.platformInfo[e2.nodeID2].position).magnitude;

                        if (l1 < l2)
                        {
                            edges.RemoveAt(j);
                            j--;
                        }
                        else
                        {
                            edges.RemoveAt(i);
                            e1 = edges[i];
                        }

                    }


                }
            }
        }
    }

    public void ForceDistances()
    {
        GenerateNetwork();

        // Ogni edge applicherà una forza a entrambi i nodi proporzionale alla differenza tra la loro distanza
        // e la distanza desiderata
        for (int n = 0; n < 1000; n++)
        {
            if (n % 50 == 0) GenerateNetwork();


            for (int i = 0; i < edges.Count; i++)
            {
                NetworkEdge edge = edges[i];
                PlatformInfo p1 = mapManager.platformInfo[edge.nodeID1];
                PlatformInfo p2 = mapManager.platformInfo[edge.nodeID2];
                Vector2 r = p1.position - p2.position;
                float d = r.magnitude;

                Vector2 force = 1f * (d - edge.weight) * r.normalized;
                p2.position += force * 0.01f;
                p1.position -= force * 0.01f;

                // Ricordo che le strutture vengono copiate per valore
                mapManager.platformInfo[edge.nodeID1] = p1;
                mapManager.platformInfo[edge.nodeID2] = p2;
            }
        }
        GenerateNetwork();

        mapManager.ResetMap();
        mapManager.CreateMap();
    }

}


public struct HexGridParams
{
    public Vector2 mapSize;
    public float platformScale;
    public float size;
    public int minPathLength;
    public int maxPathLength;
    public float smallPathStraightProbability;
    public int smallPathMinimumLength;
    public int smallPathNumber;

}

public struct HexGridParamsRegular
{
    public Vector2 mapSize;
    public float size;
    public float platformScaleSmall;
    public float platformScaleLarge;

}

public struct PoissonSamplingParams
{
    public Vector2 mapSize;
    public int mode;
    public float radius;
    public float radiusMin;
    public float radiusMax;
    public Texture2D radiusTexture;
    public float platformScale;
}

public struct NetworkEdge
{
    public int nodeID1;
    public int nodeID2;
    public float weight;
}
