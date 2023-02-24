using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTutorialMenu", menuName = "Scene Data/Tutorial Menu")]

public class TutorialMenu : GameScene
{
    [Header("Menu specific")]
    public GameObject guiPrefab;
}
