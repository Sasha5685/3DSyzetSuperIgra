using System;
using UnityEngine;
[Serializable]
public class FieldLocalize 
{
    [SerializeField] private string english;
    [SerializeField] private string russian;
    [SerializeField] private string turkish;
    
    public string GetString(string lang)
    {
        switch (lang)
        {
            case "Russian": return russian;
            case "Turkish": return turkish;
            default: return english;
        }
    }
}
