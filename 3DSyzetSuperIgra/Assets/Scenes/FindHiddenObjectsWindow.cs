#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindHiddenObjectsWindow : EditorWindow
{
    [MenuItem("Tools/Найти и удалить призраков")]
    public static void ShowWindow()
    {
        GetWindow<FindHiddenObjectsWindow>("Ловец Призраков");
    }

    private void OnGUI()
    {
        GUILayout.Label("Поиск объектов, скрытых из иерархии", EditorStyles.boldLabel);
        if (GUILayout.Button("Найти всех 'призраков'"))
        {
            FindAndListHiddenObjects();
        }
    }

    private void FindAndListHiddenObjects()
    {
        // Resources.FindObjectsOfTypeAll находит ВСЕ объекты, даже скрытые
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        List<GameObject> hiddenObjects = new List<GameObject>();

        foreach (var go in allObjects)
        {
            // Проверяем, скрыт ли объект из иерархии и не является ли он префабом или чем-то системным
            if (go != null && (go.hideFlags == HideFlags.HideInHierarchy || go.hideFlags == HideFlags.HideAndDontSave))
            {
                // Доп. проверка: есть ли у него трансформ и находится ли он в текущей сцене
                if (go.transform != null && go.scene.name != null)
                {
                    hiddenObjects.Add(go);
                }
            }
        }

        if (hiddenObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("Результат", "Призраков не найдено.", "OK");
            return;
        }

        // Выводим список найденных объектов
        foreach (var obj in hiddenObjects)
        {
            Debug.Log($"Найден скрытый объект: '{obj.name}', Тип: {obj.GetType()}, Flags: {obj.hideFlags}", obj);
            // Чтобы было совсем наглядно, можно выделить его на сцене
            Selection.activeObject = obj;
        }

        EditorUtility.DisplayDialog("Результат", $"Найдено скрытых объектов: {hiddenObjects.Count}\nИмена выведены в Console.", "OK");
    }
}
#endif