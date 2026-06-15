using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogList", menuName = "Super3DGame/Dialog/NewDialogList")]
public class DialogsList: ScriptableObject
{
    public List<Dialog> FullDialogsList = new List<Dialog>();
}
