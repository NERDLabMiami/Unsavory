using System.Collections.Generic;
using UnityEngine;

public class OrderQueueLayout2D : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform parent; // spawningGround (orders parent + front-slot anchor)

    [Header("Queue Layout (World Units)")]
    public float spacingWorldX = 1.25f;  // distance between orders in world units (to the right)
    public float z = 0f;

    [Header("Spawn (Off-screen Right)")]
    [Tooltip("How far beyond the right screen edge to spawn, in world units.")]
    public float offscreenMarginWorld = 1.5f;

    [Header("Motion")]
    public float followSpeed = 14f; // larger = snappier

    bool _warnedClamp;

    // Keeps the slot on screen whatever the camera, window shape or scene anchor.
    Vector3 ClampIntoView(Vector3 world)
    {
        if (!cam) cam = Camera.main;
        if (!cam) return world;

        Vector3 v = cam.WorldToViewportPoint(world);
        float cx = Mathf.Clamp(v.x, 0.1f, 0.8f);
        float cy = Mathf.Clamp(v.y, 0.15f, 0.88f);
        if (Mathf.Approximately(cx, v.x) && Mathf.Approximately(cy, v.y)) return world;

        if (!_warnedClamp)
        {
            _warnedClamp = true;
            Debug.LogWarning($"[OrderQueueLayout2D] order anchor was off-screen at viewport ({v.x:0.00}, {v.y:0.00}); clamped to ({cx:0.00}, {cy:0.00}). Move the anchor in the scene.");
        }
        Vector3 clamped = cam.ViewportToWorldPoint(new Vector3(cx, cy, Mathf.Abs(v.z)));
        clamped.z = z;
        return clamped;
    }

    public Vector3 FrontSlotWorld
    {
        get
        {
            if (parent != null)
            {
                var p = parent.position;
                p.z = z;
                return ClampIntoView(p);
            }

            // Fallback if parent missing: use a reasonable on-screen point
            if (!cam) cam = Camera.main;
            var a = cam ? cam.ViewportToWorldPoint(new Vector3(0.75f, 0.82f, 0f)) : Vector3.zero;
            a.z = z;
            return a;
        }
    }

    public Vector3 OffscreenRightWorld
    {
        get
        {
            if (!cam) cam = Camera.main;
            if (!cam)
                return FrontSlotWorld + Vector3.right * 10f;

            // Compute the world x at the right edge of the camera view, then add margin
            float y = FrontSlotWorld.y;
            Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, 0f));
            float x = rightEdge.x + offscreenMarginWorld;

            return new Vector3(x, y, z);
        }
    }

    // Call this from CurrentOrderScript every frame
    public void Layout(List<GameObject> activeOrders)
    {
        if (!cam) cam = Camera.main;
        if (activeOrders == null) return;

        Vector3 anchorWorld = FrontSlotWorld;

        for (int i = 0; i < activeOrders.Count; i++)
        {
            var go = activeOrders[i];
            if (!go) continue;

            // Parent under spawning ground so moving spawningGround moves the whole queue
            if (parent && go.transform.parent != parent)
                go.transform.SetParent(parent, true);

            Vector3 target = anchorWorld + new Vector3(spacingWorldX * i, 0f, 0f);

            float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
            go.transform.position = Vector3.Lerp(go.transform.position, target, t);
        }
    }
}
