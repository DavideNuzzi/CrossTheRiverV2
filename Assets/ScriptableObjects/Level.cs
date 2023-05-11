using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Scene Data/Level")]
public class Level : GameScene
{
    [Header("Level specific")]
    public bool isTraining;
    public bool isSlippery;
    public bool isTall;
    public float maxTime;
    public List<PlatformInfo> platformInfo;

}