using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class CorrectMapErrors : MonoBehaviour
{
    public List<GameScene> gameScenes;
    public float smallPlatformScale = 0.8f;
    public float bigPlatformScale = 1.2f;
    public float scaleTreshold = 0.9f;
    public float levelTime = 80f;

#if UNITY_EDITOR


    public void ChangePlatformSizes()
    {
        foreach (GameScene scene in gameScenes)
        {
            if (scene.GetType() == typeof(Block))
            {
                Block block = (Block) scene;
                foreach (Level level in block.levels)
                {
                    EditorUtility.SetDirty(level);
                    ChangePlatformSizesLevel(level);
                }
            }
            if (scene.GetType() == typeof(Level))
            {
                Level level = (Level) scene;
                EditorUtility.SetDirty(level);
                ChangePlatformSizesLevel(level);
            }

        }

    }


    public void ChangeLevelTime()
    {
        foreach (GameScene scene in gameScenes)
        {
            if (scene.GetType() == typeof(Block))
            {
                Block block = (Block)scene;
                foreach (Level level in block.levels)
                {
                    EditorUtility.SetDirty(level);
                    level.maxTime = levelTime;
                }
            }
            if (scene.GetType() == typeof(Level))
            {
                Level level = (Level)scene;
                EditorUtility.SetDirty(level);
                level.maxTime = levelTime;
            }

        }
    }

    public void ChangePlatformSizesLevel(Level level)
    {
        List<PlatformInfo> platformInfo = level.platformInfo;

        foreach (PlatformInfo info in platformInfo)
        {
            if (info.scale < scaleTreshold)
            {
                info.scale = smallPlatformScale;
            }
            else
            {
                info.scale = bigPlatformScale;
            }
        }
    }

    public void SaveFlippedMaps()
    {
        foreach (GameScene scene in gameScenes)
        {
            if (scene.GetType() == typeof(Block))
            {
                Block block = (Block)scene;
                foreach (Level level in block.levels)
                {
                    // Prendo le informazioni del livello
                    List<PlatformInfo> platformInfo = level.platformInfo;
                    string filename = level.sceneName;

                    // Inizio a creare lo scriptable object flippato
                    string path = "Assets/ScriptableObjects/Levels/New/" + filename + "_f.asset";

                    List<PlatformInfo> flippedPlatforms = new List<PlatformInfo>();
                    for (int i = 0; i < platformInfo.Count; i++)
                    {
                        flippedPlatforms.Add(new PlatformInfo()
                        {
                            position = new Vector2(-platformInfo[i].position.x, platformInfo[i].position.y),
                            isSlippery = false,
                            scale = platformInfo[i].scale
                        });
                    }

                    Level levelNew = ScriptableObject.CreateInstance<Level>();
                    levelNew.sceneName = filename;
                    levelNew.platformInfo = flippedPlatforms;
                    levelNew.maxTime = level.maxTime;

                    // Salvo
                    AssetDatabase.CreateAsset(levelNew, path);
                }
            }
        }
    }

#endif


}
