using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // Singleton instance
    public GameObject aircraft;

    public TMP_InputField nameInputField;
    
    public GameObject chooselevelPanel;
    public GameObject registerPanel;
    public GameObject menuPanel;
    public GameObject guidePanel;
    public GameObject lbPanel;
    public GameObject gamePanel; // Reference to the main game panel
    public GameObject instrumentPanel; // Reference to the Instruments panel
    public GameObject lvlInfoPanel;
    public GameObject successPanel;
    public GameObject failPanel;
    
    public Button playButton;
    public Button flyButton;
    public Button guideButton;
    public Button lbButton;
    public Button menuButton;
    public Button menuButton_guide;
    public Button menuButton_success;
    public Button menuButton_fail;
    public Button saveUsernameButton;
    public Button planeButton;
    public Button gliderButton;
    
    public Button lvl1;
    public Button lvl2;
    public Button lvl3;
    public Button lvl4;
    public Button lvl5;
    public Button lvl6;
    
    public GameObject[] aircraftPrefabs;
    public int aircraftIndex;
    public int chosenLevel;
    public string sceneName;

    private GameObject plane;
    private StaticCamControl camcontrol;
    private Instr_update InstrumentUpdate;

    private string playerName;
    
    public GameObject goalobj;
    private GoalChecker goalchecker;
    public Text lvlInfo;
    
    private IVehicleControl controller;
    
    public ChooseLevel chooseLvlScript;
    public bool[] saveState = new bool[12];
    private string loadedSceneName = ""; // Track the last loaded scene name
    
    
    

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        print("Init UI Manager");
        
        chooseLvlScript = chooselevelPanel.GetComponent<ChooseLevel>();
        goalchecker = goalobj.GetComponent<GoalChecker>();
        
        
        //OnPlayButtonClicked();

        // Add listeners to buttons
        if (saveUsernameButton != null)
            saveUsernameButton.onClick.AddListener(OnSaveButtonClicked);

        if (flyButton != null)
            flyButton.onClick.AddListener(OnFlyButtonClicked);
            
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (guideButton != null)
            guideButton.onClick.AddListener(OnGuideButtonClicked);

        if (lbButton != null)
            lbButton.onClick.AddListener(OnLBButtonClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuButtonClicked);
            
        if (menuButton_guide != null)
            print("Menu button (guide) listener added");
            menuButton_guide.onClick.AddListener(OnMenuButtonClicked);
            
        if (menuButton_success != null)
            menuButton_success.onClick.AddListener(OnMenuButtonClicked);
            
        if (menuButton_fail != null)
            menuButton_fail.onClick.AddListener(OnMenuButtonClicked);
            
        if (planeButton != null)
            planeButton.onClick.AddListener(OnPlaneButtonClicked);
            
        if (gliderButton != null)
            gliderButton.onClick.AddListener(OnGliderButtonClicked);
            
        lvl1.onClick.AddListener(lvl1Clicked);
        lvl2.onClick.AddListener(lvl2Clicked);
        lvl3.onClick.AddListener(lvl3Clicked);
        lvl4.onClick.AddListener(lvl4Clicked);
        lvl5.onClick.AddListener(lvl5Clicked);
        lvl6.onClick.AddListener(lvl6Clicked);

        // Set initial UI states
        
        menuPanel.SetActive(true);
        chooselevelPanel.SetActive(false);
        guidePanel.SetActive(false);
        failPanel.SetActive(false);
        successPanel.SetActive(false);
        gamePanel.SetActive(false);
        instrumentPanel.SetActive(false);

        // Pause the simulation
        Time.timeScale = 0f;
        
        //OnPlayButtonClicked();
        //aircraftIndex = 0;
        //chosenLevel = 5;
        //OnFlyButtonClicked();
    }
    
    void Update(){
        if (aircraft != null){
        
            if(goalchecker.goal_reached(aircraft.transform.position)){
                CleanupLevelAndAircraft();
                chooseLvlScript.SaveMission(chosenLevel); // sets mission as passed
                lbPanel.SetActive(false);
                menuPanel.SetActive(false);
                gamePanel.SetActive(false);
                instrumentPanel.SetActive(false);
                successPanel.SetActive(true);
                // Handle leaderboard button click

                Cursor.lockState = CursorLockMode.None; // or CursorLockMode.Confined
                Cursor.visible = true;
                
                return;
                }
                
            if(aircraft.GetComponent<IVehicleControl>().has_crashed() == true){
                CleanupLevelAndAircraft();
                
                lbPanel.SetActive(false);
                menuPanel.SetActive(false);
                gamePanel.SetActive(false);
                instrumentPanel.SetActive(false);
                failPanel.SetActive(true);
                // Handle leaderboard button click

                Cursor.lockState = CursorLockMode.None; // or CursorLockMode.Confined
                Cursor.visible = true;
            }
            
                }
        
    }
    
    public void lvl1Clicked(){
        chosenLevel = 1;
    }
    public void lvl2Clicked(){
        chosenLevel = 2;
    }
    public void lvl3Clicked(){
        chosenLevel = 3;
    }
    public void lvl4Clicked(){
        chosenLevel = 4;
    }
    public void lvl5Clicked(){
        chosenLevel = 5;
    }
    public void lvl6Clicked(){
        chosenLevel = 6;
    }
    
    public void OnPlaneButtonClicked(){
        aircraftIndex = 0;
    }
    
    public void OnGliderButtonClicked(){
        aircraftIndex = 1;
    }

    public void OnSaveButtonClicked()
    {
        playerName = nameInputField.text;
        registerPanel.SetActive(false);
        menuPanel.SetActive(true);
        lbPanel.SetActive(false);
        guidePanel.SetActive(false);
    }
    
    public void OnPlayButtonClicked(){
        
        chooseLvlScript.update_panel();
        
        menuPanel.SetActive(false);
        chooselevelPanel.SetActive(true);
    }
    
    public void OnFlyButtonClicked()
    {
        chooselevelPanel.SetActive(false);
        gamePanel.SetActive(true);
        instrumentPanel.SetActive(true);
        registerPanel.SetActive(false);
        failPanel.SetActive(false);
        successPanel.SetActive(false);
        // chosenScene
        
        
        StartCoroutine(LoadAndSetActiveScene(chosenLevel));
        
    }

    public void OnGuideButtonClicked()
    {
        guidePanel.SetActive(true);
        menuPanel.SetActive(false);
        // Handle guide button click
    }

    public void OnLBButtonClicked()
    {
        lbPanel.SetActive(false);
        menuPanel.SetActive(true);
        // Handle leaderboard button click
    }

    public void OnMenuButtonClicked()
    {
        
        lbPanel.SetActive(false);
        guidePanel.SetActive(false);
        menuPanel.SetActive(true);
        failPanel.SetActive(false);
        successPanel.SetActive(false);
        // Handle menu button click
    }

    private IEnumerator LoadAndSetActiveScene(int chosenLevel)
    {
        print($"Load level {chosenLevel}");
        // Set goals
        goalchecker.set_level(chosenLevel);
        
        if (chosenLevel == 1 || chosenLevel == 2){sceneName = "Mountain Race";}
        else if(chosenLevel == 3 || chosenLevel == 4){sceneName = "Dunes";}
        else if(chosenLevel == 5 || chosenLevel == 6){sceneName = "Wake Island";}
        else{};
        
        loadedSceneName = sceneName;
        // Load the scene additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
            {yield return null;}
            
         // Set the newly loaded scene as the active scene
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(loadedScene);

            
        if (FindObjectOfType<DuneGenerator>() != null){
            FindObjectOfType<DuneGenerator>().GenerateDunes();
        }
        CloudWake[] cloudWakeInstances = FindObjectsOfType<CloudWake>();

        if (cloudWakeInstances.Length > 0)
        {
            foreach (CloudWake cloudWake in cloudWakeInstances)
            {
                cloudWake.SpawnClouds();}}
        
        
        
        LoadAircraft(chosenLevel);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void LoadAircraft(int chosenLevel)
    {
        // Instantiate the selected aircraft prefab
        aircraft = Instantiate(aircraftPrefabs[aircraftIndex]);
        //instrumentPanel.Instr_update.set_player(aircraft);
        InstrumentUpdate = instrumentPanel.GetComponent<Instr_update>();
        InstrumentUpdate.set_player(aircraft);
        
        // Find the existing cloud finder and pass them the plane control script
        GameObject cloudFinder = GameObject.Find("CloudFinder");
        GameObject cloudParentLow = GameObject.Find("CloudParent Low");
        GameObject cloudParentHigh = GameObject.Find("CloudParent High");
        
        if (cloudFinder != null){
            cloudFinder.GetComponent<CloudFinder>().find_planecontrolscript();
        }
        if (cloudParentLow != null){
            cloudParentLow.GetComponent<CloudWake>().find_planecontrolscript();
        }
        if (cloudParentHigh != null){
            cloudParentHigh.GetComponent<CloudWake>().find_planecontrolscript();
        }
        
        
        
        // Find the Instruments panel in the instantiated aircraft
        Canvas aircr_instruments = GameObject.Find("Instruments").GetComponent<Canvas>();
        controller = aircraft.GetComponent<IVehicleControl>();
        
        
        // Find the camera in scene and set to follow player 
        //Camera aircraftCamera = aircraft.GetComponentInChildren<Camera>();
        GameObject cameraObject = GameObject.Find("MainCamera");
        Camera aircraftCamera = cameraObject.GetComponent<Camera>();
        cameraObject.GetComponent<CamFollower>().Follow_player(aircraft);
        
        if (chosenLevel == 1){  // Mountain Race 1
            aircraft.transform.position = new Vector3(32f, 32f, 992f);
            aircraft.transform.rotation = Quaternion.Euler(0f, 167f, 0f);
            aircraft.GetComponent<PlaneControl>().set_parameters(0.005f, 0.03f);
            cloudFinder.GetComponent<CloudFinder>().set_all_atm_winds(new Vector3(1f, 0f, -1.5f));
            cloudFinder.GetComponent<CloudFinder>().set_tornado_present(false);
            
            lvlInfo.text = "Follow the river up the mountains until you find its source. Then climb to 300 meters of altitude.";
            
        } else if (chosenLevel == 2){  // Mountain Race 2
            aircraft.transform.position = new Vector3(483f, 122f, 801f);
            aircraft.transform.rotation = Quaternion.Euler(0f, 111f, 0f);
            aircraft.GetComponent<PlaneControl>().set_parameters(0.005f, 0.03f);
            cameraObject.GetComponent<CamFollower>().set_camera(new Vector3(-10f, 0f, 10f), new Vector3(0f,-225f,0f));
            
            cloudFinder.GetComponent<CloudFinder>().set_all_atm_winds(new Vector3(4.5f, 0f, -1.5f));
            cloudFinder.GetComponent<CloudFinder>().set_tornado_present(false);
            
            lvlInfo.text = "Learn how to use slope wind. Find the airport and land.";

            
            
        } else if (chosenLevel == 3){  // Dunes 1
            aircraft.GetComponent<PlaneControl>().slope_const = 8;
            aircraft.transform.position = new Vector3(50f, 32f, 970f);
            aircraft.transform.rotation = Quaternion.Euler(0f, 132f, 0f);
            aircraft.GetComponent<PlaneControl>().set_parameters(0.01f, 0.02f);
            cameraObject.GetComponent<CamFollower>().set_camera(new Vector3(-10f, 0f, 10f), new Vector3(0f,-225f,0f));
            
            cloudFinder.GetComponent<CloudFinder>().set_all_atm_winds(new Vector3(1f, 0f, -4f));
            cloudFinder.GetComponent<CloudFinder>().set_tornado_present(true);
            cloudFinder.GetComponent<CloudFinder>().set_cloudbase(400);
            
            
            
            lvlInfo.text = "Fight for lift! Find the airport and land.";

            
        } else if (chosenLevel == 4){  // Dunes 2
            aircraft.transform.position = new Vector3(50f, 32f, 970f);
            aircraft.transform.rotation = Quaternion.Euler(0f, 132f, 0f);
            aircraft.GetComponent<PlaneControl>().set_parameters(0.01f, 0.02f);
            cameraObject.GetComponent<CamFollower>().set_camera(new Vector3(-10f, 0f, 10f), new Vector3(0f,-225f,0f));
            
            cloudFinder.GetComponent<CloudFinder>().set_all_atm_winds(new Vector3(1f, 0f, -4f));
            cloudFinder.GetComponent<CloudFinder>().set_tornado_present(true);
            cloudFinder.GetComponent<CloudFinder>().set_cloudbase(400);
            
            lvlInfo.text = "Use the environment to climb to 300m";
            
            

            
        } else if (chosenLevel == 5){  // Wake Island 1
            aircraft.transform.position = new Vector3(1900f, 100f, 350f);
            //aircraft.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            aircraft.GetComponent<PlaneControl>().set_parameters(0.007f, 0.03f);
            aircraft.GetComponent<PlaneControl>().set_init_vel(new Vector3(-4f, 0f, 0f));
            cameraObject.GetComponent<CamFollower>().set_camera(new Vector3(+10f, 3f, 0f), new Vector3(23f,275f,0f));
            
            cloudParentLow.GetComponent<CloudWake>().set_all_atm_winds(new Vector3(7f, 0f, 0f));
            cloudParentHigh.GetComponent<CloudWake>().set_all_atm_winds(new Vector3(7f, 0f, 0f));
            
            lvlInfo.text = "What goes up, goes down. Find the airport and land.";
            
            
        } else if (chosenLevel == 6){  // Wake Island 2
            aircraft.transform.position = new Vector3(1900f, 100f, 350f);
            aircraft.GetComponent<PlaneControl>().set_parameters(0.007f, 0.03f);
            //aircraft.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            aircraft.GetComponent<PlaneControl>().set_init_vel(new Vector3(-4f, 0f, 0f));
            cameraObject.GetComponent<CamFollower>().set_camera(new Vector3(+10f, 3f, 0f), new Vector3(23f,275f,0f));
            
            cloudParentLow.GetComponent<CloudWake>().set_all_atm_winds(new Vector3(4f, 0f, 0f));
            cloudParentHigh.GetComponent<CloudWake>().set_all_atm_winds(new Vector3(4f, 0f, 0f));
            
            lvlInfo.text = "Master your skills. Climb to 400m";

        }
    }
    
    private void CleanupLevelAndAircraft()
    {
        // Destroy the aircraft if it exists
        if (aircraft != null)
        {
            Destroy(aircraft);
            aircraft = null; // Clear the reference
        }

        // Unload the loaded level scene if it exists
        if (!string.IsNullOrEmpty(loadedSceneName))
        {
            SceneManager.UnloadSceneAsync(loadedSceneName);
            loadedSceneName = ""; // Clear the scene name reference
        }

        // Pause the game (optional, if you want to pause when returning to the menu)
        Time.timeScale = 0f;
    }
    
}
