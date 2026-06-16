using System;
using UnityEngine;

[Serializable]
public class Dialog
{
    public int IDDialog;
    public FieldLocalize Message;
    public SoundLocalize AudioClip;
    public AnimationClip  SetAnimation;
    public GameObject PrefabPerson;

}