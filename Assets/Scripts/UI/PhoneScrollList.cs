using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Turns a phone list container (a stretch rect holding a VerticalLayoutGroup) into a scrolling list at
/// runtime: the container becomes the masked viewport and rows go into a child "Content" rect.
/// Done in code so the Home scene doesn't need hand edits.
/// </summary>
public static class PhoneScrollList
{
    const string ContentName = "Content";

    /// <summary>Returns the Content transform rows should be instantiated into (creating the scroll setup once).</summary>
    public static Transform Prepare(Transform container)
    {
        var existing = container.Find(ContentName);
        if (existing != null) return existing;

        // Anything left in the container from the scene is placeholder; the rows are always spawned.
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            var child = container.GetChild(i);
            child.SetParent(null);
            Object.Destroy(child.gameObject);
        }

        var go = new GameObject(ContentName, typeof(RectTransform));
        var content = go.GetComponent<RectTransform>();
        content.SetParent(container, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        var layout = go.AddComponent<VerticalLayoutGroup>();
        var old = container.GetComponent<VerticalLayoutGroup>();
        if (old != null)
        {
            layout.padding = old.padding;
            layout.spacing = old.spacing;
            layout.childAlignment = old.childAlignment;
            old.enabled = false;
        }
        // Heights come from each row's LayoutElement so the content height is the real total; widths stay as
        // authored in the prefab (rows are centered, not stretched, or the Pay button hits the phone's edge).
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var viewport = (RectTransform)container;
        if (!container.GetComponent<RectMask2D>()) container.gameObject.AddComponent<RectMask2D>();

        var scroll = container.GetComponent<ScrollRect>();
        if (!scroll) scroll = container.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        scroll.viewport = viewport;
        scroll.content = content;
        return content;
    }

    /// <summary>Makes sure a spawned row has a layout height and scrolls into view when its control is selected.</summary>
    public static void Register(Component row, Selectable selectable)
    {
        if (row == null) return;
        var le = row.GetComponent<LayoutElement>();
        if (le == null) le = row.gameObject.AddComponent<LayoutElement>();
        if (le.preferredHeight <= 0f)
        {
            float h = ((RectTransform)row.transform).sizeDelta.y;
            le.preferredHeight = h > 0f ? h : 80f;
        }
        if (selectable != null && !selectable.GetComponent<ScrollIntoViewOnSelect>())
            selectable.gameObject.AddComponent<ScrollIntoViewOnSelect>();
    }
}
