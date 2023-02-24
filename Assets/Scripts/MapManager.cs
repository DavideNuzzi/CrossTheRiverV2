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


    [HideInInspector]
    public Level levelToLoad;
    [HideInInspector]
    public string levelSaveFilename;


#if UNITY_EDITOR
    public void SaveMap(string filename)
    {
        UpdateMapInfoFromEditor();

        string path = "Assets/ScriptableObjects/Levels/" + filename + ".asset";
        Level level = ScriptableObject.CreateInstance<Level>();
        level.sceneName = filename;
        level.platformInfo = platformInfo;
        AssetDatabase.CreateAsset(level, path);

    }
#endif


    public void LoadMap(Level level)
    {
        platformInfo = level.platformInfo;
        ResetMap();
        CreateMap();
    }

    void UpdateMapInfoFromEditor()
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
                    isSlippery = false
                };

                platformInfo.Add(info);
            }
        }
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
