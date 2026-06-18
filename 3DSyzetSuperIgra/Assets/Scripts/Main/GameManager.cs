using UnityEngine;
using YG;

public class GameManager : MonoBehaviour
{
    public static GameManager instatiate;
    public PlayerController PlayerController;
    public bool RunningGame = true;
    public string DeviceType = "PC";
    public string Lang;
    public GameObject SettingsPanel;
    public bool IsActiveSettings;
    public bool deviceIsDesktop;
    
    // Событие для остановки всех звуков при паузе
    public static event System.Action<bool> OnPauseStateChanged;
    
    private void Awake() 
    {
        instatiate = this;
        
        Lang = YG2.envir.language;
        deviceIsDesktop = YG2.envir.isDesktop;
        
        DeviceType = deviceIsDesktop ? "PC" : "Mobile";
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
            SetSettingsPanel(!IsActiveSettings);
        }
    }
    
    // В GameManager в методе SetSettingsPanel:
    public void SetSettingsPanel(bool active)
    {
        IsActiveSettings = active;
        SettingsPanel.SetActive(active);
        Cursor.visible = active;
        
        DialogSystem dialogSystem = FindObjectOfType<DialogSystem>();
        
        if (active)
        {
            RunningGame = false;
            PlayerController.StopGame();
            PlayerController.SetAdButtonsEnabled(false);
            
            if (dialogSystem != null)
            {
                dialogSystem.PauseDialogSound();
            }
        }
        else
        {
            RunningGame = true;
            PlayerController.ResumeGame();
            PlayerController.SetAdButtonsEnabled(true);
            
            if (dialogSystem != null)
            {
                dialogSystem.ResumeDialogSound();
            }
        }
    }
}