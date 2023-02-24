using Models;
using Proyecto26;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using static LevelManager;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public List<GameScene> gameScenes;
    private int currentScene = -1;
    private float timer;

    private MapManager mapManager;
    private ThirdPersonController characterController;
    private PlayerInput playerInput;
    private PostProcessVolume postProcessVolume;
    private GameObject confettiParticles;

    private TMP_Text timerUI;
    private TMP_Text startingTextUI;
    private TMP_Text scoreUI;
    private TMP_Text winTextUI;
    private TMP_Text levelNameUI;

    private Animator characterAnimator;

    private Vector3 startingPlayerPosition;
    private bool isTrainingLevel;
    private bool resettingLevel;
    private float score = 0;

    private bool levelRunning = false;
    string currentLevelName = "";

    private PlayerInfo playerInfo;

    // Per il salvataggio dei dati
    private const string projectId = "cross-the-river-9b5e2";
    private static readonly string databaseURL = "https://cross-the-river-9b5e2-default-rtdb.europe-west1.firebasedatabase.app/";


    public DataCollector dataCollector;


    public static GameManager Instance
    {
        get
        {
            if (_instance == null) _instance = GameObject.FindObjectOfType<GameManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        // Voglio che il game-manager rimanga in ogni livello
        DontDestroyOnLoad(this.gameObject);

     //   Cursor.lockState = CursorLockMode.Confined;

    }

    public void NextScene()
    {
        // Vedo quale sarà la prossima scena
        currentScene++;
        GameScene scene = gameScenes[currentScene];

        // Se è di tipo Tutorial devo istanziare il suo UI prefab e basta
        if (scene.GetType() == typeof(TutorialMenu))
        {
            TutorialMenu tutorial = (TutorialMenu)scene;

            StartCoroutine(LoadTutorialScene(tutorial));
        }
        else if (scene.GetType() == typeof(Level))
        {
            Level level = (Level)scene;

            StartCoroutine(LoadLevelScene(level));

        }
    }

    public IEnumerator LoadTutorialScene(TutorialMenu tutorial)
    {
        AsyncOperation asyncLoading = SceneManager.LoadSceneAsync("TutorialScene", LoadSceneMode.Single);
        Debug.Log("Inizio a caricare la scena del tutorial");

        while (!asyncLoading.isDone)
        {
            Debug.Log("Continuo a caricare");
            yield return null;
        }

        Debug.Log("Caricamento completato");
        yield return null;

        Debug.Log("Creo la UI per il tutorial");
        Instantiate(tutorial.guiPrefab);
        Debug.Log("UI creata");
        yield return null;
    }

    public IEnumerator LoadLevelScene(Level level)
    {
        levelRunning = false;
        isTrainingLevel = level.isTraining;

        AsyncOperation asyncLoading = SceneManager.LoadSceneAsync("LevelScene", LoadSceneMode.Single);
        Debug.Log("Inizio a caricare il livello: "+level.name);
        currentLevelName = level.name;

        while (!asyncLoading.isDone)
        {
            Debug.Log("Continuo a caricare");
            yield return null;
        }

        Debug.Log("Caricamento completato");
        yield return null;

        Cursor.lockState = CursorLockMode.Locked;

        // Trovo i vari controller
        characterController = GameObject.Find("PlayerArmature").GetComponent<ThirdPersonController>();
        mapManager = GameObject.Find("Map").GetComponent<MapManager>();
        postProcessVolume = GameObject.Find("PostPro").GetComponent<PostProcessVolume>();
        startingTextUI = GameObject.Find("ClickAnywhere").GetComponent<TMP_Text>();
        timerUI = GameObject.Find("Timer").GetComponent<TMP_Text>();
        scoreUI = GameObject.Find("Score").GetComponent<TMP_Text>();
        winTextUI = GameObject.Find("WinText").GetComponent<TMP_Text>();
        levelNameUI = GameObject.Find("LevelName").GetComponent<TMP_Text>();

        characterAnimator = characterController.gameObject.GetComponent<Animator>();
        playerInput = characterController.gameObject.GetComponent<PlayerInput>();

        dataCollector = GameObject.Find("DataCollector").GetComponent<DataCollector>();
        dataCollector.Initialize();

        winTextUI.gameObject.SetActive(false);
        levelNameUI.text = level.name;

        timer = 60;
        UpdateUI();
        if (isTrainingLevel) timerUI.gameObject.SetActive(false);

        // Blocco il personaggio
        playerInput.enabled = false;
        characterController.enabled = false;

        // Creo fisicamente la mappa
        mapManager.platformInfo = level.platformInfo;
        mapManager.ResetMap();
        mapManager.CreateMap();
        mapManager.PlaceStartEndPlatforms();

        yield return new WaitForSeconds(0.2f);
        characterController.enabled = true;
        confettiParticles = GameObject.Find("Confetti");
        confettiParticles.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        startingPlayerPosition = characterController.transform.position;

        while (true)
        {
            if (Input.GetMouseButton(0)) break;
            yield return null;
        }

        StartCoroutine(MoveUI(startingTextUI.rectTransform, new Vector2(0, -0.6f*Screen.height)));

        yield return StartCoroutine(RemoveBlur());

        Debug.Log("Livello iniziato");
        levelRunning = true;
        playerInput.enabled = true;

        startingTextUI.gameObject.SetActive(false);

        if (!isTrainingLevel) StartCoroutine(GameTimer());

    }

    public void ResetLevel()
    {
        if (!resettingLevel)
        {
            resettingLevel = true;
            StartCoroutine(ResetLevelRoutine());
        }
    }
    public IEnumerator ResetLevelRoutine()
    {
        levelRunning = false;
        playerInput.enabled = false;
        characterAnimator.SetBool("Lost", true);

        yield return new WaitForSeconds(2f);

        characterAnimator.SetBool("Flying", true);


        characterController.enabled = false;

        // Sposto il personaggio
        float t = 0;
        Vector3 p1 = characterController.transform.position;
        Vector3 p2 = startingPlayerPosition;
        float dist = (p2 - p1).magnitude;
        float interpTime = dist/15f;

        while (t < interpTime)
        {
            float x = p2.x * t / interpTime + p1.x * (1 - t / interpTime);
            float z = p2.z * t / interpTime + p1.z * (1 - t / interpTime);
            float y = p1.y + (p2.y - p1.y) * t / interpTime + 0.5f * 9.81f * t * (interpTime - t);
        //    if (t < interpTime/2f)  y = 5f * t / interpTime * 2f + p1.y * (1f - t / interpTime * 2f);
        //    else                    y = p2.y * (2f * t / interpTime - 1f) + 2f * 5f * (1 - t/interpTime);

            characterController.transform.position = new Vector3(x, y, z);
            t += Time.deltaTime;
            yield return null;
        }

        resettingLevel = false;
        levelRunning = true;
        playerInput.enabled = true;
        characterController.enabled = true;
        characterAnimator.SetBool("Lost", false);
        characterAnimator.SetBool("Flying", false);

    }

    public IEnumerator GameTimer()
    {
        while (true)
        {
            if (levelRunning) timer -= Time.deltaTime;
            UpdateUI();
            yield return null;

            if (timer <= 0)
            {
                timer = 0;
                // Qui devo chiamare la routine finale
                break;
            }
        }
    }

    public void GoalReached()
    {
        if (levelRunning)
        {
            levelRunning = false;
            StartCoroutine(WinningSequence());
        }
    }

    public IEnumerator WinningSequence()
    {
        // Tolgo il controllo al giocatore
        playerInput.enabled = false;
        characterAnimator.SetBool("Won", true);
        confettiParticles.SetActive(true);

        // Salvo sul server
        SaveLevelData();

        // Piccola animazione
        yield return new WaitForSeconds(3);

        // Sposto il punteggio al centro
        StartCoroutine(AddBlur());
        //  yield return StartCoroutine(MoveScoreUI(new Vector2(-Screen.width/2f + 100f, -Screen.height/2f)));
        winTextUI.rectTransform.anchoredPosition = new Vector2(Screen.width,0);
        winTextUI.gameObject.SetActive(true);
        yield return StartCoroutine(MoveUI(winTextUI.rectTransform, new Vector2(0, 0)));
        // yield return StartCoroutine(MoveUI(scoreUI.rectTransform, new Vector2(-Screen.width / 2f + 100f, -Screen.height / 2f)));

        if (!isTrainingLevel)
        {
            // Aumento il punteggio
            float t = 0f;
            float T = 2f;

            float s1 = score;
            float s2 = score + timer;
            float time1 = timer;

            while (t <= T)
            {
                float fac = t / T;

                float s = (1 - fac) * s1 + fac * s2;
                float time = (1 - fac) * time1;

                score = s;
                timer = time;

                UpdateUI();
                t += Time.deltaTime;
                yield return null;
            }
            timer = 0;
            score = s2;
            UpdateUI();
        }

        yield return new WaitForSeconds(2);


        NextScene();

    }

    public void UpdateUI()
    {
        timerUI.text = "Time left: " + timer.ToString("F1") + "s";
        scoreUI.text = "Score: " + score.ToString("F1");
    }

    public IEnumerator MoveScoreUI(Vector2 pos)
    {
        float t = 0;
        float T = 1f;
        Vector2 p0 = scoreUI.rectTransform.anchoredPosition;

        while(t <= T)
        {

            float fac = t / T;
            Vector2 posNew = (1 - fac) * p0 + fac * pos;
            scoreUI.rectTransform.anchoredPosition = posNew;
            t += Time.deltaTime;
            yield return null;
        }
        scoreUI.rectTransform.anchoredPosition = pos;

    }

    public IEnumerator MoveUI(RectTransform ui, Vector2 pos)
    {
        float t = 0;
        float T = 1f;
        Vector2 p0 = ui.anchoredPosition;

        while (t <= T)
        {

            float fac = EaseOutElastic(t / T);
            Vector2 posNew = (1 - fac) * p0 + fac * pos;
            ui.anchoredPosition = posNew;
            t += Time.deltaTime;
            yield return null;
        }
        ui.anchoredPosition = pos;
    }

    public float EaseOutElastic(float t)
    {
        float c4 = 2 * Mathf.PI / 3f;
        if (t == 0) return 0;
        else if (t == 1) return 1;
        else return Mathf.Pow(2, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    public void StartGame(string playerName, string age, string gender)
    {
        // Creo una cartella sul server per questo utente
        //     string json = JsonHelper.ArrayToJsonString(dataCollector.data.ToArray(), true);

        playerInfo = new PlayerInfo()
        {
            username = playerName,
            gender = gender,
            age = age,
            randomSeed = UnityEngine.Random.Range(0, 10000).ToString("D5")
        };

        string infoString = JsonUtility.ToJson(playerInfo, true);

        RestClient.Patch<PlayerInfo>(databaseURL + playerInfo.username + "_" + playerInfo.randomSeed + "/Info.json",infoString)
        .Then(res => { Debug.Log("Successo!"); })
        .Catch(err => Debug.LogError(err.Message));



        // Inizio il gioco
        NextScene();
    }
    
    
    void SaveLevelData()
    {
        RunData runData = new RunData()
        {
            data = dataCollector.data.ToArray(),
            simplifiedData = dataCollector.simplifiedData.ToArray(),
            levelName = currentLevelName,
            levelOrder = currentScene
        };
        string json = JsonUtility.ToJson(runData, true);

        //string json = JsonHelper.ArrayToJsonString(dataCollector.data.ToArray(), true);
        RestClient.Put<RunData>(databaseURL + playerInfo.username + "_" + playerInfo.randomSeed + "/Results_"+ currentLevelName  + ".json", json)
        .Then(res => { Debug.Log("Successo!"); })
        .Catch(err => Debug.LogError(err.Message));
    }

    void PostRun(string userID)
    {

        /*
   //     string json = JsonHelper.ArrayToJsonString(dataCollector.data.ToArray(), true);
        RestClient.Put<RunData>("https://cross-the-river-9b5e2-default-rtdb.europe-west1.firebasedatabase.app/" + userID + ".json", json)
        .Then(res => { Debug.Log("Successo!"); })
        .Catch(err => Debug.LogError(err.Message));
        */
    }

    [Serializable]
    public struct PlayerInfo
    {
        public string username;
        public string gender;
        public string age;
        public string randomSeed;
    }


    [Serializable]
    public class RunData
    {
        public DataPoint[] data;
        public SimplifiedDataPoint[] simplifiedData;
        public string levelName;
        public int levelOrder;

    }


    /*
    public IEnumerator StartGame(string playerName, string playerID)
    {
        AsyncOperation asyncLoading = SceneManager.LoadSceneAsync("LevelScene", LoadSceneMode.Single);
        Debug.Log("Inizio a caricare");

        while (!asyncLoading.isDone)
        {
            Debug.Log("Continuo a caricare");
            yield return null;
        }

        Debug.Log("Scena caricata");
        yield return new WaitForSeconds(2.5f);
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(RemoveBlur());

        yield return null;

    }
    */

    public IEnumerator RemoveBlur()
    {

        DepthOfField dof = postProcessVolume.profile.GetSetting<DepthOfField>();
        while (dof.focusDistance.value < 100f)
        {
            dof.focusDistance.value *= 1.1f;
            yield return null;
        }
    }
    public IEnumerator AddBlur()
    {

        DepthOfField dof = postProcessVolume.profile.GetSetting<DepthOfField>();
        while (dof.focusDistance.value > 0.1f)
        {
            dof.focusDistance.value *= 0.9f;
            yield return null;
        }
    }

    private void Update()
    {
        if (dataCollector) dataCollector.time = timer;
        

        if (Input.GetKeyDown(KeyCode.N) && Input.GetKeyDown(KeyCode.Space))
        {
            StopAllCoroutines();
            NextScene();
        }
    }
    /*
    private IEnumerator StartGameCoroutine()
    {
        

        LoadScene?.Invoke(newSceneName);
    }
    */


}
