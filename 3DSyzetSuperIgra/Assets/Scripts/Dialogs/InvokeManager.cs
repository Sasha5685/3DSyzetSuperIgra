using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InvokeManager : MonoBehaviour
{
    public static InvokeManager instatiate;
    public GameObject Player;
    public DialogSystem DialogSystem;
    public int IdHistory;
    public FieldLocalize[] SmallTask;
    public TextMeshProUGUI SmallTaskText;
    private GameManager GameManager;
    public GameObject[] ArrowObjects;
    public GameObject Arrow;
    private bool isStartDelayed = false;
    private void Start()
    {
        instatiate = this;
        GameManager = GameManager.instatiate;
        LoadIdHistory();
        StartCoroutine(DelayedStart());

    }
    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1.70f);
        
        DialogSystem.StartLoad();
        Arrow.GetComponent<ArrowLineRenderer>().startObject = Player.transform;
        NextHistoryMoment();
        isStartDelayed = true;
    }
    public void SendMessageEvent(string Event)
    {
        if (!isStartDelayed) return;
        if(Event == "ClickToPersonBaldi")
        {
            if(IdHistory == 0){IdHistory++;NextHistoryMoment();}
        }
        else if(Event == "PlayerInGrannyHouse")
        {
           // if(DialogSystem.IdDialog == 1){IdHistory++;NextHistoryMoment();}
        }
        else if(Event == "ClickToPersonGuguGaga")
        {
            
        }
        else if(Event == "PickCrowBar")
        {
            if(IdHistory == 1){IdHistory++;NextHistoryMoment();}
        }
        else if(Event == "GetInCar")
        {
            if(IdHistory == 2){IdHistory++;NextHistoryMoment();}
        }
    }
    public void NextDialog()
    {
        DialogSystem.DialogComplite();
    }
    public void NextHistoryMoment()
    {
        if(IdHistory == 0){SmallTaskText.gameObject.SetActive(true);SmallTaskText.text = SmallTask[3].GetString(GameManager.Lang); Arrow.SetActive(false);}
        if(IdHistory == 1){NextDialog();SmallTaskText.gameObject.SetActive(true);SmallTaskText.text = SmallTask[0].GetString(GameManager.Lang);Arrow.GetComponent<ArrowLineRenderer>().endObject = ArrowObjects[0].transform; Arrow.SetActive(true);}
        if(IdHistory == 2){SmallTaskText.gameObject.SetActive(true);SmallTaskText.text = SmallTask[1].GetString(GameManager.Lang);Arrow.GetComponent<ArrowLineRenderer>().endObject = ArrowObjects[1].transform; Arrow.SetActive(true);}
        if(IdHistory == 3){SmallTaskText.gameObject.SetActive(true);SmallTaskText.text = SmallTask[2].GetString(GameManager.Lang); Arrow.SetActive(false);}

    }

    private void LoadIdHistory()
    {
        IdHistory = PlayerPrefs.GetInt("IdHistory", 0);
    }
    private void SaveIdHistory()
    {
        PlayerPrefs.SetInt("IdHistory", IdHistory);
    }
    
}
