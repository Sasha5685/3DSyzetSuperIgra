using System.Collections.Generic;
using UnityEngine;

public class VisibleManager : MonoBehaviour
{
    [SerializeField] private float checkInterval = 0.25f;

    [SerializeField] private float outlineDistance = 15f;
    [SerializeField] private float hideDistance = 30f;

    private readonly List<VisibleObject> objects = new();

    private Camera cam;
    private float timer;

    private float outlineDistanceSqr;
    private float hideDistanceSqr;

    private void Awake()
    {
        outlineDistanceSqr = outlineDistance * outlineDistance;
        hideDistanceSqr = hideDistance * hideDistance;
    }

    private void Start()
    {
        cam = Camera.main;
                timer = checkInterval;
    }

    public void RegisterObject(VisibleObject obj)
    {
        if (obj != null && !objects.Contains(obj))
            objects.Add(obj);
    }

    public void UnregisterObject(VisibleObject obj)
    {
        objects.Remove(obj);
    }

    private void Update()
    {
        if (cam == null)
            return;

        timer += Time.deltaTime;

        if (timer < checkInterval)
            return;

        timer = 0f;

        Vector3 camPos = cam.transform.position;

        for (int i = objects.Count - 1; i >= 0; i--)
        {
            VisibleObject obj = objects[i];

            if (obj == null)
            {
                objects.RemoveAt(i);
                continue;
            }

            float sqrDistance =
                (camPos - obj.transform.position).sqrMagnitude;

            obj.UpdateVisibility(
                sqrDistance,
                outlineDistanceSqr,
                hideDistanceSqr
            );
        }
    }
}