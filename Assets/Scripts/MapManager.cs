using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Proyecto26;

public class MapManager : MonoBehaviour
{
    public Transform platformsContainer;
    public Transform treesContainer;
    public Transform platformGoal;
    public Transform platformStart;

    public Transform terrainLeft;
    public Transform terrainRight;

    public GameObject obstaclePrefab;

    public float treeDistanceStep = 5.0f;
    public float treeJitter = 0.5f;
    public float treeDensity = 0.8f;
    public Vector2 treeScaleLimits = new Vector2(1.5f, 3f);
    public Vector2 treeLimitsLOD = new Vector2(50f, 150f);

    public GameObject[] platformPrefabs;
    public GameObject[] treePrefabs;

    public ThirdPersonController playerController;

    [HideInInspector]
    public List<GameObject> platforms = new List<GameObject>();
    [HideInInspector]
    public List<PlatformInfo> platformInfo = new List<PlatformInfo>();

    public List<PlatformInfo> shortestPath;
    public List<PlatformInfo> shortestPathShortcut;
    public List<PlatformInfo> shortestPathNoShortcut;
    public bool sameQuadrantShortcut;
    public bool sameQuadrantNoShortcut;
    public Vector2 pathVecShortcut;
    public Vector2 pathVecNoShortcut;
    public Vector2 goalVec;
    int targetPlatform;
    int decisionPoint;
    int firstLarge;
    int firstSmall;
    public float angleShortcut;
    public float angleNoShortcut;

    [HideInInspector]
    public Level levelToLoad;
    [HideInInspector]
    public string levelSaveFilename;


#if UNITY_EDITOR
    public void SaveMap(string filename)
    {
        UpdateMapInfoFromEditor();
        string path = "Assets/ScriptableObjects/Levels/New/" + filename + ".asset";
        Level level = ScriptableObject.CreateInstance<Level>();
        level.sceneName = filename;

        List<PlatformInfo> newPlatforms = new List<PlatformInfo>();
        for (int i = 0; i < platformInfo.Count; i++)
        {
            newPlatforms.Add(new PlatformInfo()
            {
                position = new Vector2(platformInfo[i].position.x, platformInfo[i].position.y),
                isSlippery = false,
                scale = platformInfo[i].scale
            });
        }


        level.platformInfo = newPlatforms;

        // Calcolo il tempo
        float timeEstimate = (shortestPath.Count + 2) * 1.4f * 3f;
        float totalTimeRounded = Mathf.Ceil(timeEstimate / 5f) * 5f;
        level.maxTime = totalTimeRounded;

        AssetDatabase.CreateAsset(level, path);

    }

    public void SaveMapFlipped(string filename)
    {
        UpdateMapInfoFromEditor();
        string path = "Assets/ScriptableObjects/Levels/New/" + filename + "_f.asset";
        Level level = ScriptableObject.CreateInstance<Level>();
        level.sceneName = filename;

        List<PlatformInfo> flippedPlatforms = new List<PlatformInfo>();
        for (int i = 0; i < platformInfo.Count; i++)
        {
            flippedPlatforms.Add(new PlatformInfo()
            {
                position = new Vector2(-platformInfo[i].position.x, platformInfo[i].position.y),
                isSlippery = false,
                scale = platformInfo[i].scale
            });        }

        level.platformInfo = flippedPlatforms;

        // Calcolo il tempo
        float timeEstimate = (shortestPath.Count + 2) * 1.4f * 3f;
        float totalTimeRounded = Mathf.Ceil(timeEstimate / 5f) * 5f;
        level.maxTime = totalTimeRounded;

        AssetDatabase.CreateAsset(level, path);

    }
#endif

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (shortestPath != null)
        {
            if (shortestPath.Count > 0)
            {
                for (int i = 0; i < shortestPath.Count-1; i++)
                {
                    Gizmos.DrawLine(new Vector3(shortestPath[i].position.x,0, shortestPath[i].position.y), new Vector3(shortestPath[i + 1].position.x,0, shortestPath[i + 1].position.y));
                }
            }
        }

        Gizmos.color = Color.yellow;
        if (shortestPathShortcut != null)
        {
            if (shortestPathShortcut.Count > 0)
            {
                for (int i = 0; i < shortestPathShortcut.Count - 1; i++)
                {
                    Gizmos.DrawLine(Vector3.left * 0.1f + new Vector3(shortestPathShortcut[i].position.x, 0, shortestPathShortcut[i].position.y), Vector3.left * 0.1f + new Vector3(shortestPathShortcut[i + 1].position.x, 0, shortestPathShortcut[i + 1].position.y));
                }
            }
        }

        Gizmos.color = Color.blue;
        if (shortestPathNoShortcut != null)
        {
            if (shortestPathNoShortcut.Count > 0)
            {
                for (int i = 0; i < shortestPathNoShortcut.Count - 1; i++)
                {
                    Gizmos.DrawLine(- Vector3.left * 0.1f + new Vector3(shortestPathNoShortcut[i].position.x, 0, shortestPathNoShortcut[i].position.y),- Vector3.left * 0.1f + new Vector3(shortestPathNoShortcut[i + 1].position.x, 0, shortestPathNoShortcut[i + 1].position.y));
                }
            }
        }


        if (platformInfo.Count > 0)
        {
            //pathVecNoShortcut = new Vector2(1,0);
            //pathVecShortcut = new Vector2(0, 1);

            Gizmos.color = Color.yellow * 0.5f;
            Vector2 decisionPoint = platformInfo[firstLarge - 1].position;
            Gizmos.DrawLine(new Vector3(decisionPoint.x, 1, decisionPoint.y), new Vector3(decisionPoint.x + pathVecShortcut.x, 1, decisionPoint.y + pathVecShortcut.y));

            Gizmos.color = Color.blue * 0.5f;
            Gizmos.DrawLine(new Vector3(decisionPoint.x, 1, decisionPoint.y), new Vector3(decisionPoint.x + pathVecNoShortcut.x, 1, decisionPoint.y + pathVecNoShortcut.y));
            Gizmos.color = Color.black;
            Gizmos.DrawLine(new Vector3(decisionPoint.x, 1, decisionPoint.y), new Vector3(decisionPoint.x + goalVec.x, 1, decisionPoint.y + goalVec.y));

        }


    }
    class PlatformInfoDijkstra
    {
        public PlatformInfo platform;
        public List<PlatformInfoDijkstra> neighbours;
        public int distanceFromOrigin;
        public bool visited;
        public PlatformInfoDijkstra previous;
    }

    public void CalculatePaths()
    {
        FindDecisionPoint();
        FindLastPlatform();

        shortestPath = ShortestPath(-1);
        shortestPathShortcut = ShortestPath(firstLarge);
        shortestPathNoShortcut = ShortestPath(firstSmall);
    }

    public void GetPathVectors(float discount)
    {
        // Trovo il vettore che descriva la direzione di quel path
        pathVecShortcut = GetPathVector(firstLarge - 1, shortestPathShortcut, discount);
        pathVecNoShortcut = GetPathVector(firstLarge -1, shortestPathNoShortcut, discount);

        sameQuadrantNoShortcut = false;
        sameQuadrantShortcut = false;

        if (pathVecNoShortcut.x * goalVec.x > 0) sameQuadrantNoShortcut = true;
        if (pathVecShortcut.x * goalVec.x > 0) sameQuadrantShortcut = true;

        //  angleNoShortcut = Vector2.Angle(goalVec, pathVecNoShortcut);
        //  angleShortcut = Vector2.Angle(goalVec, pathVecShortcut);

        angleNoShortcut = CalculateAnglesCumulative(shortestPathNoShortcut);
        angleShortcut = CalculateAnglesCumulative(shortestPathShortcut);

    }

    public float CalculateAnglesCumulative(List<PlatformInfo> path)
    {
        // Mi muovo lungo il path
        float gamma = 1f;

        float cumulativeAngle = 0;
        for (int i = path.Count-1; i > 0; i--)
        {

            if (i > 0)
            {
                Vector2 localGoalVec = (platformInfo[targetPlatform].position - path[i].position + new Vector2(0, 4f)).normalized;
                Vector2 localDir = (path[i - 1].position - path[i].position).normalized;
                float angle = Vector2.Angle(localDir, localGoalVec) ;
                cumulativeAngle += angle * gamma;
                gamma = gamma * 0.9f;
            }
        }
        return cumulativeAngle;
    }

    public Vector2 GetPathVector(int decisionPlatform, List<PlatformInfo> shortestPath, float discount)
    {
        // Ciclo su tutte le pietre dopo la decision platform e vedo il loro angolo, pesandolo con un discount factor
        bool afterDecision = false;
        Vector2 pathVec = Vector2.zero;

        float gamma = 1f;
        for (int i = shortestPath.Count - 1; i  > 0; i--)
        {
            if (!afterDecision)
            {
                if ((shortestPath[i].position - platformInfo[decisionPlatform].position).magnitude < 0.1f) afterDecision = true;
            }
            else
            {
                pathVec += (shortestPath[i].position - platformInfo[decisionPlatform].position).normalized * gamma;
                gamma *= discount;
            }
        }
        pathVec.Normalize();

        return pathVec;

    }

    void FindLastPlatform()
    {
        int platformNum = platformInfo.Count;

        targetPlatform = -1;
        float maxHeight = float.MinValue;

        for (int i = 0; i < platformNum; i++)
        {
            if (platformInfo[i].position.y > maxHeight)
            {
                maxHeight = platformInfo[i].position.y;
                targetPlatform = i;
            }
        }
        goalVec = (platformInfo[targetPlatform].position - platformInfo[decisionPoint].position + new Vector2(0, 4f)).normalized;
    }

    void FindDecisionPoint()
    {
        int platformNum = platformInfo.Count;
        float hexagonSize = 1.2f * 3.7f;
        List<int> neighs = null;
        int i = 0;

        for (i = 0; i < platformNum; i++)
        {
            int neighCount = 0;
            bool oneSmall = false;
            neighs = new List<int>();

            for (int j = i+1; j < platformNum; j++)
            {
                if ((platformInfo[i].position - platformInfo[j].position).magnitude < hexagonSize)
                {
                    if (platformInfo[j].scale < 0.9f) oneSmall = true;
 
                    neighs.Add(j);
                    neighCount++;
                }
            }

            if (neighCount >= 2 && oneSmall)
            {
                break;
            }
        }

        decisionPoint = i;
        firstLarge = neighs[1];
        firstSmall = neighs[0];

        if (platformInfo[neighs[0]].scale > platformInfo[neighs[1]].scale)
        {
            firstLarge = neighs[0];
            firstSmall = neighs[1];
        }
    }

    public List<PlatformInfo> ShortestPath(int omitPlatform)
    {
        int platformNum = platformInfo.Count;
        float hexagonSize = 1.2f * 3.7f;

        // Cerco lo shortest path con Dijkstra
        List<PlatformInfo> shortestPath = new List<PlatformInfo>();
        List<PlatformInfoDijkstra> platformsDijkstra = new List<PlatformInfoDijkstra>();

        for (int i = 0; i < platformNum; i++)
        {
            platformsDijkstra.Add(new PlatformInfoDijkstra()
            {
                platform = platformInfo[i],
                neighbours = new List<PlatformInfoDijkstra>(),
                distanceFromOrigin = int.MaxValue,
                visited = false,
                previous = null
            });
        }

        // Creo una mappa dei vicini per ogni cella
        for (int i = 0; i < platformNum; i++)
        {
            for (int j = (i + 1); j < platformNum; j++)
            {
                if (i != omitPlatform && j != omitPlatform)
                {
                    if ((platformsDijkstra[i].platform.position - platformsDijkstra[j].platform.position).magnitude < hexagonSize)
                    {
                        platformsDijkstra[i].neighbours.Add(platformsDijkstra[j]);
                        platformsDijkstra[j].neighbours.Add(platformsDijkstra[i]);
                    }
                }
            }
        }

        // Seleziono la cella target
        PlatformInfoDijkstra targetCell = null;
        float maxHeight = float.MinValue;

        for (int i = 0; i < platformNum; i++)
        {
            if (platformsDijkstra[i].platform.position.y > maxHeight)
            {
                maxHeight = platformsDijkstra[i].platform.position.y;
                targetCell = platformsDijkstra[i];
            }
        }

        // Setto la distanza del primo a zero
        platformsDijkstra[0].distanceFromOrigin = 0;

        bool finished = false;


        while (!finished)
        {
            // Ordino la lista in base al valore della distanza
            platformsDijkstra.Sort((x, y) => x.distanceFromOrigin.CompareTo(y.distanceFromOrigin));

            // Prendo la più vicina
            PlatformInfoDijkstra currentCell = platformsDijkstra[0];

            // Se la minima è infinito, non posso raggiungere l'obiettivo
            if (currentCell.distanceFromOrigin == int.MaxValue)
            {
                finished = true;
                break;
            }

            // Se ho raggiunto l'obiettivo mi fermo
            if (currentCell == targetCell)
            {
                finished = true;
                break;
            }

            // Altrimenti prendo le sue vicine e aggiorno la loro distanza
            for (int i = 0; i < currentCell.neighbours.Count; i++)
            {
                if (currentCell.distanceFromOrigin + 1 < currentCell.neighbours[i].distanceFromOrigin)
                {
                    currentCell.neighbours[i].distanceFromOrigin = currentCell.distanceFromOrigin + 1;
                    currentCell.neighbours[i].previous = currentCell;
                }
            }

            // Elimino questa cella dalla lista
            platformsDijkstra.Remove(currentCell);
        }

        // Creo il path
        PlatformInfoDijkstra cell = targetCell;
        shortestPath.Add(cell.platform);

        while (cell.previous != null)
        {
            shortestPath.Add(cell.previous.platform);
            cell = cell.previous;
        }

        return shortestPath;
    }



    public void ShortestPath()
    {
        int platformNum = platformInfo.Count;
        float hexagonSize = 1.2f * 3.7f;

        // Cerco lo shortest path con Dijkstra
        shortestPath = new List<PlatformInfo>();
        List<PlatformInfoDijkstra> platformsDijkstra = new List<PlatformInfoDijkstra>();

        for (int i = 0; i < platformNum; i++)
        {
            platformsDijkstra.Add(new PlatformInfoDijkstra()
            {
                platform = platformInfo[i],
                neighbours = new List<PlatformInfoDijkstra>(),
                distanceFromOrigin = int.MaxValue,
                visited = false,
                previous = null
            }) ;
        }

        // Creo una mappa dei vicini per ogni cella
        for (int i = 0; i < platformNum; i++)
        {
            for (int j = (i+1); j < platformNum; j++)
            {
                if ((platformsDijkstra[i].platform.position - platformsDijkstra[j].platform.position).magnitude < hexagonSize)
                {
                    platformsDijkstra[i].neighbours.Add(platformsDijkstra[j]);
                    platformsDijkstra[j].neighbours.Add(platformsDijkstra[i]);
                }
            }
        }

        // Seleziono la cella target
        PlatformInfoDijkstra targetCell = null;
        float maxHeight = float.MinValue;

        for (int i = 0; i < platformNum; i++)
        {
            if (platformsDijkstra[i].platform.position.y > maxHeight)
            {
                maxHeight = platformsDijkstra[i].platform.position.y;
                targetCell = platformsDijkstra[i];
            }
        }

        // Setto la distanza del primo a zero
        platformsDijkstra[0].distanceFromOrigin = 0;

        bool finished = false;


        while (!finished)
        {
            // Ordino la lista in base al valore della distanza
            platformsDijkstra.Sort((x,y) => x.distanceFromOrigin.CompareTo(y.distanceFromOrigin));

            // Prendo la più vicina
            PlatformInfoDijkstra currentCell = platformsDijkstra[0];

            // Se la minima è infinito, non posso raggiungere l'obiettivo
            if (currentCell.distanceFromOrigin == int.MaxValue)
            {
                finished = true;
                break;
            }

            // Se ho raggiunto l'obiettivo mi fermo
            if (currentCell == targetCell)
            {
                finished = true;
                break;
            }

            // Altrimenti prendo le sue vicine e aggiorno la loro distanza
            for (int i = 0; i < currentCell.neighbours.Count; i++)
            {
                if (currentCell.distanceFromOrigin + 1 < currentCell.neighbours[i].distanceFromOrigin)
                {
                    currentCell.neighbours[i].distanceFromOrigin = currentCell.distanceFromOrigin + 1;
                    currentCell.neighbours[i].previous = currentCell;
                }
            }

            // Elimino questa cella dalla lista
            platformsDijkstra.Remove(currentCell);
        }
        
        // Creo il path
        PlatformInfoDijkstra cell = targetCell;
        shortestPath.Add(cell.platform);

        while (cell.previous != null)
        {
            shortestPath.Add(cell.previous.platform);
            cell = cell.previous;
        }

        float timeEstimate = (shortestPath.Count + 2) * 1.4f * 3f;
        float totalTimeRounded = Mathf.Ceil(timeEstimate / 5f) * 5f;
        Debug.Log("Shortest path length = " + shortestPath.Count + "\t time estimate 3 stars = " + timeEstimate / 3f + "\t time total = " + timeEstimate + "\t rounded = "+ totalTimeRounded);
    }

    public void LoadMap(Level level)
    {
        List<PlatformInfo> newPlatforms = new List<PlatformInfo>();
        for (int i = 0; i < level.platformInfo.Count; i++)
        {
            newPlatforms.Add(new PlatformInfo()
            {
                position = new Vector2(level.platformInfo[i].position.x, level.platformInfo[i].position.y),
                isSlippery = false,
                scale = level.platformInfo[i].scale
            });
        }

        platformInfo = newPlatforms;
        ResetMap();
        CreateMap();
        //ShortestPath();
        CalculatePaths();

    }

    public void UpdateMapInfoFromEditor()
    {
        platformInfo = new List<PlatformInfo>();

        for (int i = 0; i < platforms.Count; i++)
        {
            if (platforms[i] != null)
            {
                GameObject platform = platforms[i];
                PlatformInfo info = new PlatformInfo()
                {
                    scale = platform.transform.localScale.x,
                    position = new Vector2(platform.transform.position.x, platform.transform.position.z),
                    isSlippery = platform.GetComponent<Platform>().info.isSlippery
                };

                platformInfo.Add(info);
            }
        }


        CalculatePaths();
    }

    public void LoadMapJSON(string filename)
    {

        StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/Levels/" + filename + ".json", false);
        string json = reader.ReadToEnd();
        reader.Close();
        var a = JsonHelper.ArrayFromJson<PlatformInfo>(json);
        Debug.Log(a);

    }


    public void CreateMap()
    {
        platforms = new List<GameObject>();

        for (int i = 0; i < platformInfo.Count; i++)
        {
            PlatformInfo info = platformInfo[i];

            GameObject platformObj = GameObject.Instantiate(platformPrefabs[Random.Range(0,platformPrefabs.Length)]);
            platformObj.name = "Platform_" + i.ToString(); 
            Platform platform = platformObj.GetComponent<Platform>();

            platform.info = info;
            platform.ApplyPositionAndScale();
            platform.transform.parent = platformsContainer.transform;
            platform.mapManager = this;

            if (info.isSlippery == true)
            {
                GameObject obstacle = GameObject.Instantiate(obstaclePrefab, platform.info.position, Quaternion.identity);
                obstacle.transform.parent = platform.transform;
                obstacle.transform.localPosition = new Vector3(0, 0.15f, 0);
            }

            platforms.Add(platformObj);
        }
    }

    public void ResetMap()
    {
        // Cancello tutti i figli del container per le piattaforme
        for (int i = 0; i < platformsContainer.childCount; i++)
        {
            Transform child = platformsContainer.GetChild(i);
            GameObject.DestroyImmediate(child.gameObject);
            i--;
        }
    }

    public void ShufflePlatforms(float shuffleFac, float hexGridSize)
    {
        Random.InitState(0);

        Vector2[] displacementVectors = new Vector2[platforms.Count];

        for (int i = 0; i < platforms.Count; i++)
        {
            Vector2 pathDir = Vector2.zero;
            int count = 0;
            for (int j = 0; j < platforms.Count; j++)
            {
                if (i != j)
                {
                    if ((platforms[j].transform.position - platforms[i].transform.position).magnitude < 3.5f * hexGridSize)
                    {
                        Vector2 d = new Vector2(platforms[j].transform.position.x - platforms[i].transform.position.x, platforms[j].transform.position.z - platforms[i].transform.position.z);
                        Vector2 dir = d.normalized;

                        if (j > i) dir *= -1;

                        pathDir += dir;
                        count++;
                    }
                }
            }
            if (pathDir.magnitude > 0)
            {
                pathDir /= (float)count;

                // PER CORREGGERE L'ERRORE DELLE MAPPE FLIPPATE DEVO OTTENERE ANCHE L'ASSE VERTICALE CON UN PRODOTTO VETTORIALE
                Vector3 up = Vector3.Cross(new Vector3(pathDir.x, 0, pathDir.y), new Vector3(0, 0, 1));
                Vector3 orthoVector = Vector3.Cross(new Vector3(pathDir.x, 0, pathDir.y), up);
                displacementVectors[i] = new Vector2(orthoVector.x, orthoVector.z);
            }
        }

        for (int i = 0; i < platforms.Count; i++)
        {
            GameObject platform = platforms[i];
            platform.transform.position += (Random.value - 0.5f) * shuffleFac * new Vector3( displacementVectors[i].x,0, displacementVectors[i].y);
        }
    }

    public void PlaceStartEndPlatforms()
    {
        // Trovo le posizioni ideali per le piattaforme
        // Posizione media su X
        float meanX = 0;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        for (int i = 0; i < platforms.Count; i++)
        {
            Vector3 p = platforms[i].transform.position;

            meanX += p.x;
            if (p.z < minZ)
            {
                minZ = p.z;
                minX = p.x;
            }

            if (p.z > maxZ)
            {
                maxZ = p.z;
                maxX = p.x;
            }
        }
        meanX = meanX / platforms.Count;

        Instantiate(platformGoal, new Vector3(maxX, 0, maxZ + 4f), platformGoal.rotation);
        Instantiate(platformStart, new Vector3(minX, 0.835f, minZ - 4f), platformStart.rotation);
        playerController.transform.parent.position = new Vector3(minX, 1f, minZ - 4f);

        terrainLeft.position = new Vector3(0, 0, maxZ - 20);
        terrainLeft.position = new Vector3(0, 0, minZ + 20);


    }

    public void PlaceTrees()
    {
        for (int i = 0; i < treesContainer.childCount; i++)
        {
            Collider treeZone = treesContainer.GetChild(i).GetComponent<BoxCollider>();
            Vector3 max = treeZone.bounds.max;
            Vector3 min = treeZone.bounds.min;

            float xMin = min.x;
            float yMin = min.z;
            float xMax = max.x;
            float yMax = max.z;

            // Cancello tutti gli alberi già presenti
            for (int j = 0; j < treeZone.transform.childCount; j++)
            {
                Transform child = treeZone.transform.GetChild(j);
                GameObject.DestroyImmediate(child.gameObject);
                j--;
            }

            for (float x = xMin; x < xMax; x += treeDistanceStep)
            {
                for (float y = yMin; y < yMax; y += treeDistanceStep)
                {
                    if (Random.value < treeDensity)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(new Vector3(x, min.y, y), new Vector3(0, -1, 0), out hit))
                        {
                            Vector3 hitPoint = hit.point;
                            Vector3 jitter = new Vector3(treeJitter, 0, treeJitter) * (Random.value * 2f - 1f);
                            Quaternion rotation = Quaternion.Euler(0, Random.value * 360f, 0);
                            float r = Random.value;
                            float treeScale = treeScaleLimits.x * r + treeScaleLimits.y * (1 - r);

                            // Il modello scelto dipende dalla distanza
                            float distance = (new Vector3(hitPoint.x, 0, hitPoint.z)).magnitude;
                            GameObject treeModel = treePrefabs[0];
                            if (distance > treeLimitsLOD.x)
                            {
                                if (distance < treeLimitsLOD.y) treeModel = treePrefabs[1];
                                else
                                {
                                    treeModel = treePrefabs[2];
                                    rotation = Quaternion.Euler(0, 0, 0);
                                }
                            }


                            GameObject tree = Instantiate(treeModel, hitPoint + jitter, rotation);


                            tree.transform.localScale = treeScale * Vector3.one;
                            tree.transform.parent = treeZone.transform;
                        }
                    }
                 
                }
            }
        }
    }
}
