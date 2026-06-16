using UnityEngine.Events;
using TMPro;
using UnityEngine;

public class DialogSystem : MonoBehaviour
{
    [SerializeField] private DialogsList DialogsList;
    [SerializeField] private GameObject PrefabTextMeshPro;
    public int IdDialog;
    private Dialog ThisDialog;
    
    [Header("Точки спавна на сцене")]
    [SerializeField] private Transform[] PersonSpawnPoints;
    [SerializeField] private Transform[] TextSpawnPoints;    
    //Объекты на сцене
    private GameObject PersonDialog;
    private GameObject TextMeshProDialog;
    private GameManager GameManager;
    [SerializeField] private AudioSource globalAudioSource;
    public void StartLoad()
    {
        GameManager = GameManager.instatiate;
        LoadIdDialog();
        DemonstrationDialogInScene();
    }
    private void LoadIdDialog()
    {
        IdDialog = PlayerPrefs.GetInt("DialogId",0);
        ThisDialog = DialogsList.FullDialogsList[IdDialog];
    }
    private void SaveIdDialog(int DialogId)
    {
        PlayerPrefs.SetInt("DialogId", DialogId);
    }

    private void DemonstrationDialogInScene()
    {
        PersonDialog = Instantiate(ThisDialog.PrefabPerson, PersonSpawnPoints[IdDialog].position, PersonSpawnPoints[IdDialog].rotation);
        TextMeshProDialog = Instantiate(PrefabTextMeshPro, TextSpawnPoints[IdDialog].position, TextSpawnPoints[IdDialog].rotation);
        TextMeshProDialog.GetComponent<TextMeshPro>().text = ThisDialog.Message.GetString(GameManager.Lang);
        globalAudioSource.PlayOneShot(ThisDialog.AudioClip.GetAudio(GameManager.Lang));
        PersonDialog.GetComponentInChildren<Animator>().Play(ThisDialog.SetAnimation.name);

    }
    private void ClearDialogInScene()
    {
        Destroy(PersonDialog);
        Destroy(TextMeshProDialog);
    }
    public void DialogComplite()
    {
        if (globalAudioSource.isPlaying)
            globalAudioSource.Stop();
        
        ClearDialogInScene();
        IdDialog++;
        ThisDialog = DialogsList.FullDialogsList[IdDialog];
        DemonstrationDialogInScene();
    }
}



