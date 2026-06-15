using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogSystem : MonoBehaviour
{
    [SerializeField] private DialogsList DialogsList;
    [SerializeField] private GameObject PrefabTextMeshPro;
    private int IdDialog;
    private Dialog ThisDialog;
    
    [Header("Точки спавна на сцене (перетащите объекты)")]
    [SerializeField] private Transform[] PersonSpawnPoints;  // Массив точек для персонажей
    [SerializeField] private Transform[] TextSpawnPoints;    // Массив точек для текста
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
[Serializable]
public class Dialog
{
    public int IDDialog;
    [TextArea(2, 5)]public string Message;
    public AudioClip AudioClip;
    public Animation SetAnimation;
    public GameObject PrefabPerson;

    public Action NextDialogAction;

}
[CreateAssetMenu(fileName = "DialogList", menuName = "Super3DGame/Dialog/NewDialogList")]
public class DialogsList: ScriptableObject
{
    public List<Dialog> FullDialogsList = new List<Dialog>();
}

