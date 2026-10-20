using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Unsavory/Setup Phone: wires the Phone in the open scene (PhoneController, ModalFocus, button hooks) and
/// turns the Rent / Message template rows into prefabs. Safe to re-run. Does not save the scene.
/// </summary>
public static class PhoneSetup
{
    const string BillPrefabPath = "Assets/Prefabs/Phone Bill.prefab";
    const string MessagePrefabPath = "Assets/Prefabs/Phone Message.prefab";

    [MenuItem("Unsavory/Setup Phone")]
    static void Run()
    {
        var phone = FindPhone();
        if (phone == null) { Debug.LogError("[PhoneSetup] Couldn't find Overlay Canvas/Canvases/Phone in the open scene."); return; }

        Transform T(string path)
        {
            var t = phone.transform.Find(path);
            if (t == null) Debug.LogError("[PhoneSetup] Missing child: Phone/" + path);
            return t;
        }

        var home = T("Home Screen"); var bills = T("Bills"); var mail = T("Mail");
        var billsList = T("Bills/Bills"); var rent = T("Bills/Bills/Rent");
        var messages = T("Mail/Messages"); var message = T("Mail/Messages/Message");
        var view = T("Mail/View Message");
        if (home == null || bills == null || mail == null || billsList == null || messages == null || view == null) return;

        var controller = GetOrAdd<PhoneController>(phone);
        var modal = GetOrAdd<ModalFocus>(phone);
        modal.manualNavigation = true; // PhoneController sets the navigation chain itself
        modal.allowCancel = false; // PhoneController handles Cancel itself (step back, not "Close")

        // --- Prefabs from the template rows ---
        if (rent != null && PrefabUtility.GetCorrespondingObjectFromSource(rent.gameObject) == null)
        {
            var bill = GetOrAdd<BillScript>(rent.gameObject);
            bill.titleUI = Find<TMP_Text>(rent, "Category");
            bill.dueUI = Find<TMP_Text>(rent, "Due Date");
            bill.payButton = Find<Button>(rent, "Pay");
            rent.gameObject.tag = "Bill";
            ClearAndAdd(bill.payButton.onClick, bill, "payBill");
            var saved = PrefabUtility.SaveAsPrefabAssetAndConnect(rent.gameObject, BillPrefabPath, InteractionMode.AutomatedAction);
            controller.billPrefab = saved.GetComponent<BillScript>();
        }
        else controller.billPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BillPrefabPath)?.GetComponent<BillScript>();

        if (message != null && PrefabUtility.GetCorrespondingObjectFromSource(message.gameObject) == null)
        {
            var row = GetOrAdd<PhoneMessageRow>(message.gameObject);
            row.button = message.GetComponent<Button>();
            row.sender = Find<TMP_Text>(message, "Sender");
            row.preview = Find<TMP_Text>(message, "Preview");
            var saved = PrefabUtility.SaveAsPrefabAssetAndConnect(message.gameObject, MessagePrefabPath, InteractionMode.AutomatedAction);
            controller.messagePrefab = saved.GetComponent<PhoneMessageRow>();
        }
        else controller.messagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MessagePrefabPath)?.GetComponent<PhoneMessageRow>();

        // --- Controller references ---
        controller.homeScreen = home.gameObject;
        controller.billsScreen = bills.gameObject;
        controller.mailScreen = mail.gameObject;
        controller.messagesList = messages.gameObject;
        controller.viewMessage = view.gameObject;
        controller.homeFirst = Find<Button>(home, "Bills");
        controller.viewMessageBack = Find<Button>(view, "Header");
        controller.billsBack = Find<Button>(bills, "Header");
        controller.mailBack = Find<Button>(mail, "Header");
        controller.homeMail = Find<Button>(home, "Mail");
        controller.billsContainer = billsList;
        controller.messagesContainer = messages;
        controller.viewInfo = Find<TMP_Text>(view, "Information");
        controller.viewBody = Find<TMP_Text>(view, "Body");

        // --- Button hooks ---
        ClearAndAdd(Find<Button>(home, "Bills").onClick, controller, "OpenBills");
        ClearAndAdd(Find<Button>(home, "Mail").onClick, controller, "OpenMail");
        ClearAndAdd(Find<Button>(bills, "Header").onClick, controller, "Back");
        ClearAndAdd(Find<Button>(mail, "Header").onClick, controller, "Back");
        ClearAndAdd(Find<Button>(view, "Header").onClick, controller, "Back");

        // --- Cellphone opens the Phone instead of the old Bill Panel ---
        foreach (var hi in Object.FindObjectsByType<HomeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (hi.action != HomeInteractable.HomeAction.OpenBills) continue;
            hi.billsModalRoot = phone;
            EditorUtility.SetDirty(hi);
        }

        // Draw the whole Overlay Canvas above the apartment's Foreground sprites (Cellphone, letter, Bowl Front).
        var canvas = phone.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas = canvas.rootCanvas;
            canvas.overrideSorting = false;
            canvas.sortingLayerName = "Foreground";
            canvas.sortingOrder = 200;
            EditorUtility.SetDirty(canvas);
        }

        // Closed until the Cellphone opens it (otherwise ModalFocus grabs focus on scene start).
        phone.SetActive(false);

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(modal);
        EditorSceneManager.MarkSceneDirty(phone.scene);
        Debug.Log("[PhoneSetup] Phone wired. Save the scene (Cmd+S).");
    }

    static GameObject FindPhone()
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name != "Phone" || !t.gameObject.scene.IsValid() || t.Find("Home Screen") == null) continue;
            return t.gameObject;
        }
        return null;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    static T Find<T>(Transform root, string name) where T : Component
    {
        var t = root.Find(name);
        if (t == null) { Debug.LogError($"[PhoneSetup] {root.name}/{name} not found"); return null; }
        var c = t.GetComponent<T>();
        if (c == null) Debug.LogError($"[PhoneSetup] {root.name}/{name} has no {typeof(T).Name}");
        return c;
    }

    static void ClearAndAdd(UnityEvent evt, Object target, string method)
    {
        for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(evt, i);
        var mi = target.GetType().GetMethod(method, System.Type.EmptyTypes);
        UnityEventTools.AddVoidPersistentListener(evt, (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), target, mi));
    }
}
