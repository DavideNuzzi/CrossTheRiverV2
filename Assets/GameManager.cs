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
using UnityEngine.UI;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public List<GameScene> gameScenes;
    private int currentScene = -1;
    private int currentLevelInBlock = 0;
    private List<int> currentBlockIndices;
    private float timer;
    private float timerIncreasing = 0f;

    private MapManager mapManager;
    private ThirdPersonController characterController;
    private PlayerInput playerInput;
    private PostProcessVolume postProcessVolume;
    private GameObject confettiParticles;

    public GameObject coinSound;
    public GameObject penaltySound;

//    private TMP_Text timerUI;
    private TMP_Text startingTextUI;
 //   private TMP_Text scoreUI;
//    private TMP_Text penaltiesUI;
    private TMP_Text winTextUI;
    private TMP_Text levelNameUI;
    private TMP_Text starsCollectedUI;

    private Image backgroundImage;
    private Image starScoreImage;
    private GameObject starsContainer;

    public bool resetFirstStone = false;

    public Animator characterAnimator;
    public bool levelLoading;

    int globalLevelCounter = 0;

    Coroutine timerRoutine = null;

    private Vector3 startingPlayerPosition;
    private bool isTrainingLevel;
    public bool resettingLevel;
    private float score = 0;
    private float levelScore = 0;
    private float levelMaxTime = 0;

    public int starsCollected = 0;

    public bool levelRunning = false;
    string currentLevelName = "";
    int penaltyNum = 0;

    public float totalExperimentTime = 0f;


    public CounterFPS fpsCounter;



    private PlayerInfo playerInfo;

    // Per il salvataggio dei dati
  //  private const string projectId = "cross-the-river-9b5e2";
  //  private static readonly string databaseURL = "https://cross-the-river-9b5e2-default-rtdb.europe-west1.firebasedatabase.app/";
    private static readonly string databaseURL = "https://crosstheriverpilot-default-rtdb.europe-west1.firebasedatabase.app/";

    
    public DataCollector dataCollector;



#if UNITY_WEBGL

    [DllImport("__Internal")]
    private static extern string ReturnUserAgent();

#endif



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
        //Application.targetFrameRate = 40;

        Application.runInBackground = false;

        // Voglio che il game-manager rimanga in ogni livello
        DontDestroyOnLoad(this.gameObject);

     //   Cursor.lockState = CursorLockMode.Confined;

    }

    public void NextScene()
    {
        if (levelLoading) return;

        // Se la scena attuale è il livello di un blocco, devo passare al livello successivo
        if (currentScene >= 0)
        {
            if (gameScenes[currentScene].GetType() == typeof(Block))
            {
           
                Block block = (Block)gameScenes[currentScene];
                if (currentLevelInBlock < block.levels.Count - 1)
                {
                    currentLevelInBlock++;


                    Level level = block.levels[currentBlockIndices[currentLevelInBlock]];

                    StartCoroutine(LoadLevelScene(level));
                    return;
                }
            }
        }
        
        // Altrimenti passo alla scena successiva
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
        else if (scene.GetType() == typeof(Block))
        {
            currentLevelInBlock = 0;

            Block block = (Block)gameScenes[currentScene];

            // Inizializzo la lista degli indici per questo blocco
            currentBlockIndices = new List<int>();
            for (int i = 0; i < block.levels.Count; i++) currentBlockIndices.Add(i);

            System.Random rnd = new System.Random();

            if (block.randomize) currentBlockIndices = currentBlockIndices.OrderBy(x => rnd.Next()).ToList();

            Level level = block.levels[currentBlockIndices[currentLevelInBlock]];


            StartCoroutine(LoadLevelScene(level));
        }


       
    }

    public IEnumerator LoadTutorialScene(TutorialMenu tutorial)
    {
        // QUI DEVO BLOCCARE IL VECCHIO PULSANTE DEL TUTORIAL!!!!!
        levelLoading = true;



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
        levelLoading = false;

    }

    public IEnumerator LoadLevelScene(Level level)
    {
        levelLoading = true;
        levelRunning = false;
        isTrainingLevel = level.isTraining;
        resettingLevel = false;
            
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
  //      timerUI = GameObject.Find("Timer").GetComponent<TMP_Text>();
  //      scoreUI = GameObject.Find("Score").GetComponent<TMP_Text>();
        starsCollectedUI = GameObject.Find("StarsText").GetComponent<TMP_Text>();
        fpsCounter = GameManager.FindObjectOfType<CounterFPS>();

        //  penaltiesUI = GameObject.Find("Penalty").GetComponent<TMP_Text>();
        winTextUI = GameObject.Find("WinText").GetComponent<TMP_Text>();
        levelNameUI = GameObject.Find("LevelName").GetComponent<TMP_Text>();
        starScoreImage = GameObject.Find("StarsFiller").GetComponent<Image>();
        backgroundImage = GameObject.Find("Background").GetComponent<Image>();
        starsContainer = GameObject.Find("StarsContainer");

        characterAnimator = characterController.gameObject.GetComponent<Animator>();
        playerInput = characterController.gameObject.GetComponent<PlayerInput>();

        dataCollector = GameObject.Find("DataCollector").GetComponent<DataCollector>();
        dataCollector.Initialize();
        dataCollector.isCollecting = false;

        winTextUI.gameObject.SetActive(false);

        if (isTrainingLevel) levelNameUI.text = level.sceneName;
        else
        {
            if (gameScenes[currentScene].GetType() == typeof(Block))
            {
                levelNameUI.text = (currentLevelInBlock + 1).ToString() + " / " + ((Block)(gameScenes[currentScene])).levels.Count;
            }
            else levelNameUI.text = "";
        }

        resetFirstStone = level.returnFirstRock;

        timerIncreasing = 0f;
        timer = level.maxTime;
        levelMaxTime = level.maxTime;
        if (level.maxTime == 0 || float.IsNaN(level.maxTime))
        {
            timer = 60;
            levelMaxTime = 60;
        }
        levelScore = 1;

        penaltyNum = 0;

        if (isTrainingLevel)
        {
          //  timerUI.gameObject.SetActive(false);
          //  penaltiesUI.gameObject.SetActive(false);
          //  scoreUI.gameObject.SetActive(false);
            starsCollectedUI.gameObject.SetActive(false);
            starsContainer.SetActive(false);

        }
        UpdateUI();


        // Blocco il personaggio
        playerInput.enabled = false;
        characterController.enabled = false;

        // Creo fisicamente la mappa
        mapManager.platformInfo = level.platformInfo;


        mapManager.ResetMap();
        mapManager.CreateMap();
        mapManager.PlaceStartEndPlatforms();
        mapManager.ShufflePlatforms(2f, 1.2f);


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
        dataCollector.AddSimplifiedPoint(characterController.transform.position, 0);

        startingTextUI.gameObject.SetActive(false);
        dataCollector.isCollecting = true;

        timerRoutine = StartCoroutine(GameTimer());

        levelLoading = false;

        globalLevelCounter++;

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
        dataCollector.isCollecting = false;
        levelRunning = false;
        playerInput.enabled = false;
        characterAnimator.SetBool("Lost", true);

        if (!isTrainingLevel) penaltyNum++;
        UpdateUI();

        yield return new WaitForSeconds(2f);

        characterAnimator.SetBool("Flying", true);


        characterController.enabled = false;

        // Sposto il personaggio
        float t = 0;
        Vector3 p1 = characterController.transform.position;
        Vector3 p2 = startingPlayerPosition + new Vector3(0,0.5f,0);

        // Se esiste una piattaforma precedente vado lì
        if (!resetFirstStone)
        {
            for (int i = dataCollector.simplifiedData.Count - 1; i > 0; i--)
            {
                if (dataCollector.simplifiedData[i].type == 0)
                {
                    p2 = dataCollector.simplifiedData[i].position + new Vector3(0, 0.5f, 0);
                    break;
                }
            }
        }

        float dist = (p2 - p1).magnitude;
        float interpTime = dist/9f;

        //  if (resetFirstStone) interpTime = 2f;
        //  else interpTime = 0.5f;
        interpTime = Mathf.Clamp(interpTime, 0.5f, 2.5f);

        while (t < interpTime)
        {
            float x = p2.x * t / interpTime + p1.x * (1 - t / interpTime);
            float z = p2.z * t / interpTime + p1.z * (1 - t / interpTime);
            float y = p1.y + (p2.y - p1.y) * t / interpTime + 0.5f * 9.81f * t * (interpTime - t);

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
        dataCollector.isCollecting = true;


    }

    public IEnumerator GameTimer()
    {
        while (true)
        {
            timerIncreasing += Time.deltaTime;

            if (!isTrainingLevel)
            {
                //if (levelRunning) timer -= Time.deltaTime;
                timer -= Time.deltaTime;
                levelScore = timer / levelMaxTime;

                UpdateUI();


                if (timer <= 0)
                {
                    timer = 0;
                    // Qui devo chiamare la routine finale
                    if (levelRunning)
                    {
                        StartCoroutine(LosingSequence());
                        break;
                    }

                }
            }
            else
            {
                if (timerIncreasing > 180)
                {
                    dataCollector.isCollecting = false;
                }
            }
            yield return null;

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

    public IEnumerator CalculateScore()
    {
        if (!isTrainingLevel)
        {
            // Aumento il punteggio
            float t = 0f;
            float T = 2f;

            float s1 = score;
            float s2 = score + timer;
            float time1 = timer;

            float coinSoundTimer = 0;

            while (t <= T)
            {
                float fac = t / T;

                float s = (1 - fac) * s1 + fac * s2;
                float time = (1 - fac) * time1;

                score = s;
                timer = time;


                UpdateUI();
                coinSoundTimer += Time.deltaTime;
                if (coinSoundTimer > 0.3f)
                {
                    Instantiate(coinSound);
                    coinSoundTimer = 0f;
                }

                t += Time.deltaTime;
                yield return null;
            }
            timer = 0;
            score = s2;
            UpdateUI();
        }

        yield return new WaitForSeconds(1f);

    }

    public IEnumerator CalculatePenalties()
    {

        // Ora le penalità vengono sottratte 
        int penalties = penaltyNum;
        for (int i = 0; i < penalties; i++)
        {
            penaltyNum--;
            score -= 5;
            Instantiate(penaltySound);
            if (score <= 0) score = 0;
            UpdateUI();

            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator WinningSequence()
    {
        // Tolgo il controllo al giocatore
        playerInput.enabled = false;
        characterAnimator.SetBool("Won", true);
        confettiParticles.SetActive(true);
        dataCollector.isCollecting = false;

        if (timerRoutine != null) StopCoroutine(timerRoutine);
        //      StopCoroutine(GameTimer());

        totalExperimentTime += timerIncreasing;

        // Salvo sul server
        SaveLevelData();

        // Piccola animazione
        yield return new WaitForSeconds(2);

        // Sposto il punteggio al centro
        StartCoroutine(AddBlur());
        winTextUI.rectTransform.anchoredPosition = new Vector2(Screen.width, 0);
        winTextUI.gameObject.SetActive(true);
        yield return StartCoroutine(MoveUI(winTextUI.rectTransform, new Vector2(0, 0)));

        if (!isTrainingLevel)
        {
            yield return StartCoroutine(CalculateStarScore());
        }

      //  yield return StartCoroutine(CalculateScore());
      //  yield return StartCoroutine(CalculatePenalties());

        yield return new WaitForSeconds(2);

        NextScene();

    }


    public IEnumerator CalculateStarScore()
    {
        // Sposto il contenitore delle stelle
        yield return StartCoroutine(MoveUI(starsContainer.GetComponent<RectTransform>(), new Vector2(Screen.width/2f + 50f, -165)));

        // Approssimo al numero di stelle reali
        int starsCollectedLevel = Mathf.CeilToInt(levelScore * 3);

        // Mostro questa cosa
        for (int k = 0; k < starsCollectedLevel; k++)
        {
            if (levelScore < (k+1)/3f) levelScore = (k + 1) / 3f;
            UpdateUI();
            Instantiate(coinSound);
            yield return StartCoroutine(ScaleBounceUI(starsContainer.GetComponent<RectTransform>(), 1.1f,0.5f));
        }

        // Aggiungo al punteggio totale di stelle
        StartCoroutine(ScaleBounceUI(starsContainer.GetComponent<RectTransform>(), 0.3f,1f));
        StartCoroutine(MoveUI(starsContainer.GetComponent<RectTransform>(), new Vector2(Screen.width, -50)));
        yield return new WaitForSeconds(0.4f);
        starsContainer.SetActive(false);

        starsCollected += starsCollectedLevel;
        UpdateUI();
    }

    public IEnumerator ScaleBounceUI (RectTransform ui, float scaleFac, float T)
    {
        float t = 0;
        Vector3 p0 = ui.localScale;
        Vector3 p = ui.localScale * scaleFac;

        while (t <= T)
        {
            float fac = EaseOutElastic(t / T);
            Vector3 posNew = (1 - fac) * p0 + fac * p;
            ui.localScale = posNew;
            t += Time.deltaTime;
            yield return null;
        }
        ui.localScale = p;
    }

    public IEnumerator LosingSequence()
    {
        levelRunning = false;

        // Tolgo il controllo al giocatore
        playerInput.enabled = false;
        characterAnimator.SetBool("Lost", true);
        dataCollector.isCollecting = false;

        // Aggiungo un ultimo punto
        dataCollector.AddSimplifiedPoint(characterController.transform.position, 4);

        totalExperimentTime += timerIncreasing;

        // Salvo sul server
        SaveLevelData();

        // Piccola animazione
        yield return new WaitForSeconds(2);

        // Sposto il punteggio al centro
        StartCoroutine(AddBlur());
        winTextUI.rectTransform.anchoredPosition = new Vector2(Screen.width, 0);
        winTextUI.text = "Time run out! Starting next level";
        winTextUI.gameObject.SetActive(true);

        yield return StartCoroutine(MoveUI(winTextUI.rectTransform, new Vector2(0, 0)));

//        yield return StartCoroutine(CalculatePenalties());
        yield return new WaitForSeconds(2);


        NextScene();

    }


    public void UpdateUI()
    {
    //    timerUI.text = "Time left: " + timer.ToString("F1") + "s";
    //    scoreUI.text = "Score: " + score.ToString("F1");

        starsCollectedUI.text = starsCollected.ToString();
     //   penaltiesUI.text = "Penalties: " + penaltyNum;

        starScoreImage.fillAmount = levelScore;
    }

    /*
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
    */

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
        string userAgent = "unknown";

#if UNITY_WEBGL

        userAgent = ReturnUserAgent();
#endif

        playerInfo = new PlayerInfo()
        {
            username = playerName,
            gender = gender,
            age = age,
            randomSeed = UnityEngine.Random.Range(0, 10000).ToString("D5"),
            userAgent = userAgent
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
            levelOrder = globalLevelCounter
        };
        //string json = JsonUtility.ToJson(runData, true);

        // PER ORA FACCIO UN SALVATAGGIO SEMPLIFICATO PER RISPARMIARE SPAZIO!!!!!!!!!!
        RunDataString dataString = CreateStringData(runData);
        string json = JsonUtility.ToJson(dataString, true);

        //string json = JsonHelper.ArrayToJsonString(dataCollector.data.ToArray(), true);
        RestClient.Put<RunData>(databaseURL + playerInfo.username + "_" + playerInfo.randomSeed + "/Results_"+ globalLevelCounter+"_"+ currentLevelName  + ".json", json)
        .Then(res => { Debug.Log("Successo!"); })
        .Catch(err => Debug.LogError(err.Message));
    }

    public void SaveFinalData(ExperimentResults results)
    {

        string json = JsonUtility.ToJson(results, true);

        RestClient.Put<RunData>(databaseURL + playerInfo.username + "_" + playerInfo.randomSeed + "/FinalResults.json", json)
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

    RunDataString CreateStringData(RunData runData)
    {
        RunDataString runDataString = new RunDataString();

        string[] dataString = new string[runData.data.Length];
        string[] eventsString = new string[runData.simplifiedData.Length];

        for (int i = 0; i < runData.data.Length; i++)
        {
            string posStr = runData.data[i].position.x.ToString("F2", CultureInfo.InvariantCulture) + ";" + runData.data[i].position.y.ToString("F2", CultureInfo.InvariantCulture) + ";" + runData.data[i].position.z.ToString("F2", CultureInfo.InvariantCulture);

            float angleDir = Mathf.Atan2(runData.data[i].direction.z, runData.data[i].direction.x);
            string angleDirStr = angleDir.ToString("F2", CultureInfo.InvariantCulture);
            string timeStr = runData.data[i].time.ToString("F3", CultureInfo.InvariantCulture);

            dataString[i] = timeStr + ";" + posStr + ";" + angleDirStr;
        }

        for (int i = 0; i < runData.simplifiedData.Length; i++)
        {
            string timeStr = runData.simplifiedData[i].time.ToString("F3", CultureInfo.InvariantCulture);
            string posStr = runData.simplifiedData[i].position.x.ToString("F2", CultureInfo.InvariantCulture) + ";" + runData.simplifiedData[i].position.z.ToString("F2", CultureInfo.InvariantCulture);
            string typeStr = runData.simplifiedData[i].type.ToString();

            eventsString[i] = timeStr + ";" + posStr + ";" + typeStr;
        }

        runDataString.levelOrder = runData.levelOrder;
        runDataString.levelName = runData.levelName;
        runDataString.data = dataString;
        runDataString.events = eventsString;
        runDataString.averageFPS = fpsCounter.averageFps.ToString("F2", CultureInfo.InvariantCulture);

        return runDataString;
    }

    [Serializable]
    public struct PlayerInfo
    {
        public string username;
        public string gender;
        public string age;
        public string randomSeed;
        public string userAgent;
    }


    [Serializable]
    public class RunData
    {
        public DataPoint[] data;
        public SimplifiedDataPoint[] simplifiedData;
        public string levelName;
        public int levelOrder;
        public float averageFPS;
    }



    [Serializable]
    public class RunDataString
    {
        public string levelName;
        public int levelOrder;
        public string[] data;
        public string[] events;
        public string averageFPS;
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
            backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, backgroundImage.color.a * 0.9f);
            dof.focusDistance.value *= 1.1f;
            yield return null;
        }
        backgroundImage.color = new Color(0,0,0,0);

    }
    public IEnumerator AddBlur()
    {

        DepthOfField dof = postProcessVolume.profile.GetSetting<DepthOfField>();

        while (dof.focusDistance.value > 0.01f)
        {
         //   backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, backgroundImage.color.a * 1.1f);
            dof.focusDistance.value *= 0.9f;
            yield return null;
        }
       // backgroundImage.color = new Color(0, 0, 0, 0);

    }

    private void Update()
    {
        if (dataCollector) dataCollector.time = timerIncreasing;

      //  if (Input.GetKey(KeyCode.P)) NextScene();


        /*
        if (Input.GetKey(KeyCode.P))
        {
            TutorialMenu level = (TutorialMenu)gameScenes[gameScenes.Count - 1];
            StartCoroutine(LoadTutorialScene(level));
        }
        */
    }

    /*
    private IEnumerator StartGameCoroutine()
    {
        

        LoadScene?.Invoke(newSceneName);
    }
    */


}
