using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Keeps the list row containing this control inside its ScrollRect's viewport when it gets d-pad selection.</summary>
public class ScrollIntoViewOnSelect : MonoBehaviour, ISelectHandler
{
    const float Margin = 6f;

    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(Reveal());
    }

    IEnumerator Reveal()
    {
        yield return null; // rows may have just been spawned; let the layout run first
        Canvas.ForceUpdateCanvases();

        var scroll = GetComponentInParent<ScrollRect>();
        if (scroll == null || scroll.content == null || scroll.viewport == null) yield break;

        // The row is the direct child of Content that holds this control.
        Transform t = transform;
        while (t != null && t.parent != scroll.content) t = t.parent;
        var row = t as RectTransform;
        if (row == null) yield break;

        var content = scroll.content;
        var view = scroll.viewport.rect;
        float maxScroll = Mathf.Max(0f, content.rect.height - view.height);
        float y = content.anchoredPosition.y;

        if (row.GetSiblingIndex() == 0) y = 0f;
        else if (row.GetSiblingIndex() == content.childCount - 1) y = maxScroll;
        else
        {
            var b = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, row);
            if (b.min.y < view.yMin + Margin) y += view.yMin + Margin - b.min.y;
            else if (b.max.y > view.yMax - Margin) y -= b.max.y - (view.yMax - Margin);
        }

        scroll.StopMovement();
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Clamp(y, 0f, maxScroll));
    }
}
