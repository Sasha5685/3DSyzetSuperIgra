using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerController PlayerController;
    public bool RunningGame;
    private void Awake() 
    {
        StartGame();
    }
    
    private void StartGame()
    {
        PlayerController.Initialized("PC");
    }
}

