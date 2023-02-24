using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using Proyecto26;
using System;

public class LevelManager : MonoBehaviour
{
    public Animator animator;
    public bool playerReachedGoal = false;
    public bool playerFellWater = false;

    private bool timerRunning = true;
    private float timer = 0f;
    private float resetTimer = 0f;

    public DataCollector dataCollector;

    private static LevelManager _instance;

    private const string projectId = "cross-the-river-9b5e2"; // You can find this in your Firebase project settings
    private static readonly string databaseURL = $"https://{projectId}.firebaseio.com/";

    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<LevelManager>();
            }

            return _instance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        dataCollector.time = timer;
        if (timerRunning) timer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.R) )
        {
            SceneManager.LoadScene(0);
        }

        if (playerFellWater)
        {
            animator.SetBool("Lost", true);
            resetTimer += Time.deltaTime;
        }

        if (resetTimer > 3f)
        {
            SceneManager.LoadScene(0);
        }
        // Controllo se il player ha vinto
        if (playerReachedGoal)
        {
            if (timerRunning)
            {
                animator.SetBool("Won", true);

                dataCollector.SaveData("Prova.json");
                dataCollector.SaveSimplifiedData("ProvaSimple.json");

               // PostRun("0");

            }
            timerRunning = false;

        }
    }

    void PostRun(string userID)
    {
        // RunData runData = new RunData() { data = dataCollector.data.ToArray(), simplifiedData = dataCollector.simplifiedData.ToArray() };
        string json = JsonHelper.ArrayToJsonString(dataCollector.data.ToArray(), true);
        RestClient.Put<RunData>("https://cross-the-river-9b5e2-default-rtdb.europe-west1.firebasedatabase.app/"+userID+".json", json)
        .Then(res => {Debug.Log("Successo!");})
        .Catch(err => Debug.LogError(err.Message));
    }


    [Serializable]
    public class RunData
    {
        public DataPoint[] data;
        public SimplifiedDataPoint[] simplifiedData;
        public string levelName;
    }





    void PostTest()
    {
        DummyRecord dummyRecord = new DummyRecord("paperino");
        RestClient.Post<DummyRecord>("https://cross-the-river-9b5e2-default-rtdb.europe-west1.firebasedatabase.app/prova.json", dummyRecord).
            Then(res => {
                Debug.Log("Successo!");
        })
        .Catch(err => Debug.LogError(err.Message));

    }

    [Serializable]
    public class DummyRecord
    {
        public string pippo;

        public DummyRecord(string pippo)
        {
            this.pippo = pippo;
        }
    }


    GUIStyle textStyle = new GUIStyle();

    // Use this for initialization
    void Start()
    {
        //textStyle.fontSize = 30;
        //textStyle.alignment = TextAnchor.MiddleCenter;
        //textStyle.fontStyle = FontStyle.Bold;
        //    textStyle.normal.textColor = Color.white;
        //PostTest();
    }

    void OnGUI()
    {
        //Display the fps and round to 2 decimals
        //GUI.Label(new Rect(0, 0, Screen.width, 40f), timer.ToString("F2") + "s", textStyle);
    }
}
