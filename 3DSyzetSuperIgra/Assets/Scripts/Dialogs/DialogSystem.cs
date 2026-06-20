using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class DialogSystem : MusicSystem
{
    [SerializeField] private DialogsList DialogsList;
    [SerializeField] private GameObject PrefabTextMeshPro;
    public int IdDialog;
    private Dialog ThisDialog;
    
    [Header("Точки спавна на сцене")]
    [SerializeField] private Transform[] PersonSpawnPoints;
    [SerializeField] private Transform[] TextSpawnPoints;    
    
    private GameObject PersonDialog;
    private GameObject TextMeshProDialog;
    private GameManager GameManager;
    
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    
    private bool isDialogActive = false; // Флаг активного диалога

    private void Awake()
    {
        InitSystem(sfxMixerGroup);
    }

    public void StartLoad()
    {
        GameManager = GameManager.instatiate;
        LoadIdDialog();
        DemonstrationDialogInScene();
    }
    
    private void LoadIdDialog()
    {
        IdDialog = PlayerPrefs.GetInt("DialogId", 0);
        ThisDialog = DialogsList.FullDialogsList[IdDialog];
    }
    
    private void DemonstrationDialogInScene()
    {
        isDialogActive = true; // Диалог активен
        
        PersonDialog = Instantiate(ThisDialog.PrefabPerson, 
            PersonSpawnPoints[IdDialog].position, 
            PersonSpawnPoints[IdDialog].rotation);
        
        TextMeshProDialog = Instantiate(PrefabTextMeshPro, 
            TextSpawnPoints[IdDialog].position, 
            TextSpawnPoints[IdDialog].rotation);
        TextMeshProDialog.GetComponent<TextMeshPro>().text = ThisDialog.Message.GetString(GameManager.Lang);
        
        PlayDialogSound();
        
        if (PersonDialog != null)
        {
            Animator animator = PersonDialog.GetComponentInChildren<Animator>();
            if (animator != null && ThisDialog.SetAnimation != null)
            {
                animator.Play(ThisDialog.SetAnimation.name);
            }
        }
    }
    
    private void PlayDialogSound()
    {
        AudioClip clip = ThisDialog.AudioClip.GetAudio(GameManager.Lang);
        ShotSound(clip);
    }
    
    private void ClearDialogInScene()
    {
        isDialogActive = false; // Диалог не активен
        
        if (PersonDialog != null) Destroy(PersonDialog);
        if (TextMeshProDialog != null) Destroy(TextMeshProDialog);
    }
    
    public void DialogComplite()
    {
        StopSound();
        
        ClearDialogInScene();
        IdDialog++;
        
        if (IdDialog >= DialogsList.FullDialogsList.Count)
        {
            Debug.Log("Все диалоги завершены");
            return;
        }
        
        ThisDialog = DialogsList.FullDialogsList[IdDialog];
        DemonstrationDialogInScene();
    }

    public void PauseDialogSound()
    {
        // Пауза только если диалог активен
        // if (!isDialogActive) return;
        
        // if (globalAudioSource != null && globalAudioSource.isPlaying)
        // {
        //     globalAudioSource.Pause();
        // }
    }

    public void ResumeDialogSound()
    {
        // Возобновляем только если диалог активен
        // if (!isDialogActive) return;
        
        // if (globalAudioSource != null && globalAudioSource.clip != null)
        // {
        //     if (!globalAudioSource.isPlaying && globalAudioSource.time > 0)
        //     {
        //         globalAudioSource.UnPause();
        //     }
        //     else if (!globalAudioSource.isPlaying && globalAudioSource.time == 0)
        //     {
        //         globalAudioSource.Play();
        //     }
        // }
    }
}