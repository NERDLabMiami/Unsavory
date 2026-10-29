using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>One row in the Phone's Mail list. Spawned from a prefab by PhoneController.</summary>
public class PhoneMessageRow : MonoBehaviour
{
    public Button button;
    public TMP_Text sender;
    public TMP_Text preview;
    public Image avatar;

    public int Index { get; private set; }

    public void Setup(int index, string senderName, string subject, bool read, Action<int> onOpen, Sprite icon = null)
    {
        Index = index;
        if (sender)
        {
            sender.text = senderName;
            sender.fontStyle = read ? FontStyles.Normal : FontStyles.Bold;
        }
        if (preview) preview.text = subject;
        if (avatar && icon) avatar.sprite = icon;
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onOpen(Index));
        }
    }
}
