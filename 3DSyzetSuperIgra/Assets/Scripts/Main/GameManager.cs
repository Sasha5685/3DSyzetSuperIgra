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
}

