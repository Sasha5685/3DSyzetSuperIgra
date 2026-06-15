using System;
using UnityEngine;

[Serializable]
public class Dialog
{
    public int IDDialog;
    [TextArea(2, 5)]public string Message;
    public AudioClip AudioClip;
    public Animation SetAnimation;
    public GameObject PrefabPerson;

}