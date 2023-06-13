using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class CorrectMapErrors : MonoBehaviour
{
    public List<GameScene> gameScenes;
    public float smallPlatformScale = 0.8f;
    public float bigPlatformScale = 1.2f;
    public float scaleTreshold = 0.9f;
    public float levelTime = 80f;

    public void ChangePlatformSizes()
    {
        foreach (GameScene scene in gameScenes)
        {
            if (scene.GetType() == typeof(Block))
            {
                Block block = (Block) scene;
                foreach (Level level in block.levels)
                {
                    ChangePlatformSizesLevel(level);
                }
            }
            if (scene.GetType() == typeof(Level))
            {
                Level level = (Level) scene;
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
                    level.maxTime = levelTime;
                }
            }
            if (scene.GetType() == typeof(Level))
            {
                Level level = (Level)scene;
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



}
