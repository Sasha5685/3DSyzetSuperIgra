using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class InvokeManager : MusicSystem
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
    private const float HighlightCheckInterval = 1f;
    private float lastCheckTime;

    private Inventory Inventory;

    [SerializeField] private GameObject[] SpawnObjects;
    [SerializeField] private PlayableDirector PlayableDirector;
    [SerializeField] private TimelineAsset[] Playables;
    [SerializeField] private GameObject[] TimeLineObjects;
    [SerializeField] private AudioMixerGroup AudioMixerGroup;
    [SerializeField] private AudioClip[] AudioClips;
    [SerializeField] private GameObject Canvas;
    public GameObject PlayerObjects;
    private void Start()
    {
        InitSystem(AudioMixerGroup, false);
        instatiate = this;
        GameManager = GameManager.instatiate;
        LoadIdHistory();
        StartCoroutine(DelayedStart());
        Inventory = Inventory.instatiate;
        PlayableDirector.stopped += OnCutsceneEnd;


    }
    private void OnDestroy()
    {
        PlayableDirector.stopped -= OnCutsceneEnd;
    }
    // Вызывается при окончании кат-сцены
    private void OnCutsceneEnd(PlayableDirector director)
    {
        if(IdHistory == 4){IdHistory++;NextHistoryMoment();}
    }
    private void Update()
    {
        if (Time.time - lastCheckTime > HighlightCheckInterval)
        {
            lastCheckTime = Time.time;
            CheckMes();
        }
    }
    private void CheckMes()
    {
        if(IdHistory == 1)
        {
            if (Inventory.instatiate.HandItem("Car key") && Inventory.instatiate.HandItem("Tire iron"))
            {
                IdHistory++;NextHistoryMoment();
            }
        }
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
           
        }
        else if(Event == "ClickToPersonGuguGaga")
        {
            
        }
        else if(Event == "PickCrowBar")
        {
            if(IdHistory == 1){ Arrow.SetActive(false);}
        }
        else if(Event == "PickCarKey")
        {
            if(IdHistory == 2){IdHistory++;NextHistoryMoment();}
        }
        else if(Event == "GetInCar")
        {
            if(IdHistory == 2){IdHistory++;NextHistoryMoment();}
        }
        else if(Event == "TriggerIgnoreRayCast")
        {
            if(IdHistory == 3){IdHistory++;NextHistoryMoment();}
            if(IdHistory == 5){IdHistory++;NextHistoryMoment();}
        }
    }
    public void NextDialog()
    {
        DialogSystem.DialogComplite();
    }
    public void NextHistoryMoment()
    {
        if(IdHistory == 0){SmallTaskText.gameObject.SetActive(true);SmallTaskText.text = SmallTask[3].GetString(GameManager.Lang); Arrow.SetActive(false);}
        if(IdHistory == 1){NextDialog();SmallTaskText.gameObject.SetActive(true);SmallTaskText.text = SmallTask[0].GetString(GameManager.Lang);if(ArrowObjects[0]!= null){Arrow.GetComponent<ArrowLineRenderer>().endObject = ArrowObjects[0].transform; Arrow.SetActive(true);}}
        if(IdHistory == 2){SmallTaskText.gameObject.SetActive(true);SmallTaskText.text = SmallTask[1].GetString(GameManager.Lang);Arrow.GetComponent<ArrowLineRenderer>().endObject = ArrowObjects[1].transform; Arrow.SetActive(true);}
        if(IdHistory == 3){SmallTaskText.gameObject.SetActive(true);SmallTaskText.text = SmallTask[2].GetString(GameManager.Lang); Arrow.SetActive(false);SpawnObjects[0].SetActive(true);}
        if(IdHistory == 4){PlayableDirector.playableAsset = Playables[0];Canvas.SetActive(false);PlayableDirector.Play();PlayerController.instatiate.RunningGame = false;Inventory.instatiate.BlockInventory = true;TimeLineObjects[0].SetActive(true);TimeLineObjects[1].SetActive(true);PlayerObjects.SetActive(false);}
        if(IdHistory == 5){PlayerController.instatiate.RunningGame = true;Canvas.SetActive(true);Inventory.instatiate.BlockInventory = false;SpawnObjects[0].SetActive(false);SpawnObjects[1].SetActive(true);TimeLineObjects[0].SetActive(false);TimeLineObjects[1].SetActive(false);PlayerObjects.SetActive(true);}
        if(IdHistory == 6){SpawnObjects[1].SetActive(false);PlaySound(AudioClips[0]);SpawnObjects[0].SetActive(false);}
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
