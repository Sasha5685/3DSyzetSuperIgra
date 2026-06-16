using System;
using UnityEngine;
[Serializable]
public class SoundLocalize 
{
    [SerializeField] private AudioClip english;
    [SerializeField] private AudioClip russian;
    
    public AudioClip GetAudio(string lang)
    {
        switch (lang)
        {
            case "ru": return russian;
            default: return english;
        }
    }
}
