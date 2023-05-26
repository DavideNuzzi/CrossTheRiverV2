using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public PlatformInfo info;
    public MapManager mapManager;

    private void Start()
    {
    }

    public void ApplyPositionAndScale()
    {
        transform.position = new Vector3(info.position.x, 0, info.position.y);
        transform.localScale = new Vector3(info.scale, 1 * info.scale, info.scale);
        transform.eulerAngles = new Vector3(0, Random.value * 360f, 0);
    }

    private void OnValidate()
    {
     
        /*
        mapManager.UpdateMapInfoFromEditor();
        mapManager.ResetMap();
        mapManager.CreateMap();
        */
    }


}

[System.Serializable]
public class PlatformInfo
{
    public float scale;
    public Vector2 position;
    public bool isSlippery;
}
