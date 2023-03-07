using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Proyecto26;

public class DataCollector : MonoBehaviour
{
    public Transform playerTransform;
    public int skipFrames = 0;

    public List<DataPoint> data;
    public List<SimplifiedDataPoint> simplifiedData;

    public bool isCollecting = true;
    public float time;
    int frame = 0;

    public void Initialize()
    {
        isCollecting = true;
        data = new List<DataPoint>();
        simplifiedData = new List<SimplifiedDataPoint>();
    }

    // Start is called before the first frame update
    public void AddSimplifiedPoint (Vector3 position, int type)
    {
        SimplifiedDataPoint point = new SimplifiedDataPoint() { position = position, time = time, type = type};
        simplifiedData.Add(point);
    }

    public bool CheckSamePoint (Vector3 position)
    {
        if (simplifiedData.Count > 0)
        {
            SimplifiedDataPoint lastPoint = simplifiedData[simplifiedData.Count - 1];
            float distance = (position - lastPoint.position).magnitude;

            if (distance < 0.1f) return true;
            else return false;
        }
        else return false;

    }
    // Update is called once per frame
    void Update()
    {
        if(isCollecting)
        {   
            if (frame >= skipFrames)
            {
                DataPoint point = new DataPoint() {position = playerTransform.position, 
                    direction = playerTransform.forward,
                    time = time };
                data.Add(point);
                frame = 0;
            }
            frame++;
        }
    }

    public void SaveData(string filename)
    {  
        string dataToJson = JsonHelper.ArrayToJsonString(data.ToArray(), true);
        StreamWriter writer = new StreamWriter(filename, false);
        writer.Write(dataToJson);
        writer.Close();
    }

    public void SaveSimplifiedData(string filename)
    {
        string dataToJson = JsonHelper.ArrayToJsonString(simplifiedData.ToArray(), true);
        StreamWriter writer = new StreamWriter(filename, false);
        writer.Write(dataToJson);
        writer.Close();

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < data.Count-4; i+= 3)
        {
            Gizmos.DrawLine(data[i].position, data[i + 3].position);
        }
        
        Gizmos.color = Color.blue;

        for (int i = 0; i < data.Count - 1; i++)
        {
            if ((data[i].direction - data[i+1].direction).magnitude > 0.01) Gizmos.DrawLine(data[i].position, data[i].position + data[i].direction);
        }



        for (int i = 0; i < simplifiedData.Count; i++)
        {
            if (simplifiedData[i].type == 0) Gizmos.color = Color.yellow;
            if (simplifiedData[i].type == 1) Gizmos.color = Color.blue;
            if (simplifiedData[i].type == 2) Gizmos.color = Color.red;
            if (simplifiedData[i].type == 3) Gizmos.color = Color.magenta;

            Gizmos.DrawSphere(simplifiedData[i].position, 0.2f);
        }
    }
}

[System.Serializable]
public struct DataPoint
{
    public Vector3 position;
    public Vector3 direction;
    public float time;
}

[System.Serializable]
public struct SimplifiedDataPoint
{
    public Vector3 position;
    public int type;
    public float time;
}