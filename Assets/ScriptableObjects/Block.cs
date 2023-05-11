using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Scene Data/Block")]
public class Block : GameScene
{
    [Header("Block Specific")]
    public bool randomize;
    public List<Level> levels;
}