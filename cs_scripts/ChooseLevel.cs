using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChooseLevel : MonoBehaviour
{
    
    public Button planeButton;
    public Button gliderButton;
    
    public Button[] lvlButton;
    public int[] saveState = new int[6];  // stores passed/unpassed missions 1/0
    
    public Sprite unlocked;
    public Sprite locked;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt("Mission_" + 1, 0); // 1 indicates completed
        PlayerPrefs.SetInt("Mission_" + 2, 0); // 1 indicates completed
        PlayerPrefs.SetInt("Mission_" + 3, 0); // 1 indicates completed
        PlayerPrefs.SetInt("Mission_" + 4, 0); // 1 indicates completed
        PlayerPrefs.SetInt("Mission_" + 5, 0); // 1 indicates completed
        PlayerPrefs.SetInt("Mission_" + 6, 0); // 1 indicates completed
        PlayerPrefs.Save(); // Ensure data is saved
        
        saveState = new int[] { 0,0,0,0,0,0};
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SaveMission(int missionId) //values 1 to 6
    {
        Debug.Log("Passed mission "+missionId);
        PlayerPrefs.SetInt("Mission_" + missionId, 1); // 1 indicates completed
        saveState[missionId-1] = 1;
        PlayerPrefs.Save(); // Ensure data is saved
    }
    
    public int IsMissionCompleted(int missionId) // values 1 to 6
    {
        return PlayerPrefs.GetInt("Mission_" + missionId, 0); // returns 0 if value doesnt exist
    }
    
    public void update_panel(){
    
        
        print("savestate is:");
        string arrayString = string.Join(", ", saveState);
        Debug.Log(arrayString);  // Output: "1, 2, 3, 4, 5"
        
        
        
        for (int i = 0; i < 5; i++){
            print(i);
            print(saveState[i]);
            
            if (saveState[i] == 1){
            print("unlock");
                lvlButton[i+1].GetComponent<Image>().sprite = unlocked;
                lvlButton[i+1].interactable = true;
            }else{
                print("lock");
                lvlButton[i+1].GetComponent<Image>().sprite = locked;
                lvlButton[i+1].interactable = false;
        }
        lvlButton[i+1].GetComponent<Image>().sprite = unlocked;
        lvlButton[i+1].interactable = true;
    }
    lvlButton[0].GetComponent<Image>().sprite = unlocked;
    lvlButton[0].interactable = true;
    
    }
}
