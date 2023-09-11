using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static GameManager;
using Random = UnityEngine.Random;


public class ExportMaps : MonoBehaviour
{
    public List<GameScene> gameScenes;

    public void ExportAllMaps()
    {
        foreach (GameScene scene in gameScenes)
        {
            if (scene.GetType() == typeof(Block))
            {
                Block block = (Block)scene;
                foreach (Level level in block.levels)
                {
                    Export(level);
                }
            }
            if (scene.GetType() == typeof(Level))
            {
                Level level = (Level)scene;
                Export(level);
            }

        }
    }

    public List<PlatformInfo> CloneAndShufflePlatforms(List<PlatformInfo> platformInfo)
    {
        List<PlatformInfo> platformsNew = new List<PlatformInfo>();
        Vector2[] displacements = GetDisplacementVectors(1.2f, platformInfo);

        Random.InitState(0);
        float shuffleFac = 2f;

        for (int i = 0; i < platformInfo.Count; i++)
        {
            PlatformInfo platform = platformInfo[i];
            Vector2 disp = (Random.value - 0.5f) * shuffleFac * displacements[i];

            PlatformInfo platformNew = new PlatformInfo()
            {
                position = new Vector2(platform.position.x + disp.x, platform.position.y + disp.y),
                scale = platform.scale,
                isSlippery = platform.isSlippery
            };

            platformsNew.Add(platformNew);
        }

        return platformsNew;
    }

    [Serializable]
    struct LevelInfo
    {
        public List<PlatformInfo> platformInfo;
        public bool isTraining;
    }

    public void Export(GameScene level)
    {
        LevelInfo levelCopy = new LevelInfo();
        levelCopy.platformInfo = CloneAndShufflePlatforms(((Level)level).platformInfo);
        levelCopy.isTraining = ((Level)level).isTraining;

        string jsonString = JsonUtility.ToJson(levelCopy, true);
        StreamWriter writer = new StreamWriter("MapsJSON_New/" + level.name + ".json");
        writer.Write(jsonString);
        writer.Close();
    }

    public Vector2[] GetDisplacementVectors(float hexGridSize, List<PlatformInfo> platforms)
    {

        Vector2[] displacementVectors = new Vector2[platforms.Count];

        for (int i = 0; i < platforms.Count; i++)
        {
            Vector2 pathDir = Vector2.zero;
            int count = 0;
            for (int j = 0; j < platforms.Count; j++)
            {
                if (i != j)
                {
                    if ((platforms[j].position - platforms[i].position).magnitude < 3.5f * hexGridSize)
                    {
                        Vector2 d = new Vector2(platforms[j].position.x - platforms[i].position.x, platforms[j].position.y - platforms[i].position.y);
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
                Vector3 up = Vector3.Cross(new Vector3(pathDir.x, 0, pathDir.y), new Vector3(0, 0, 1));
                Vector3 orthoVector = Vector3.Cross(new Vector3(pathDir.x, 0, pathDir.y), up);
                displacementVectors[i] = new Vector2(orthoVector.x, orthoVector.z);
            }
        }

        return displacementVectors;

    }
}
