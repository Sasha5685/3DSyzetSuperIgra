using UnityEngine.Events;
using TMPro;
using UnityEngine;

public class DialogSystem : MonoBehaviour
{
    [SerializeField] private DialogsList DialogsList;
    [SerializeField] private GameObject PrefabTextMeshPro;
    private int IdDialog;
    private Dialog ThisDialog;
    
    [Header("Точки спавна на сцене")]
    [SerializeField] private Transform[] PersonSpawnPoints;
    [SerializeField] private Transform[] TextSpawnPoints;    
    [SerializeField] private UnityEvent NextDialogAction;
    //Объекты на сцене
    private GameObject PersonDialog;
    private GameObject TextMeshProDialog;

    private void Awake()
    {
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
        TextMeshProDialog.GetComponent<TextMeshPro>().text = ThisDialog.Message;

    }
    private void ClearDialogInScene()
    {
        Destroy(PersonDialog);
        Destroy(TextMeshProDialog);
    }
}



