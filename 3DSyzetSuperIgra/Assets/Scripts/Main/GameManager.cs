using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class GameManager : MonoBehaviour
{
    public static GameManager instatiate;
    public PlayerController PlayerController;
    public bool RunningGame;
    public string DeviceType = "PC";
    public string Lang;
    public GameObject SettingsPanel;
    public bool IsActiveSettings;
    private void Awake() 
    {
        instatiate = this;
        Lang = YG2.lang;
        StartGame();
    }
    
    private void StartGame()
    {
        PlayerController.Initialized(DeviceType);
    }

    public void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //IsActiveSettings = !IsActiveSettings;
            SetSettingsPanel(true);
        }
    }
    public void SetSettingsPanel(bool active)
    {
        SettingsPanel.SetActive(active);
        Cursor.visible = active;
        if(active == true)
        {
            PlayerController.StopGame();
        }
        else
        {
            PlayerController.ResumeGame();
        }
        
    }
}

