using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if ES3_TMPRO && ES3_UGUI

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ES3SlotManager : MonoBehaviour
{
    [Tooltip("当我们选择一个已经存在的槽位时是否显示确认提示。")]
    public bool showConfirmationIfExists = true;
    [Tooltip("是否显示创建新槽位按钮。")]
    public bool showCreateSlotButton = true;
    [Tooltip("当用户创建新的存档槽位时是否自动创建一个空的存档文件。这将使用默认设置创建，所以如果您使用ES3Settings对象，应该将此设置为false。")]
    public bool autoCreateSaveFile = false;
    [Tooltip("创建槽位后是否应该选择该存档槽位。")]
    public bool selectSlotAfterCreation = false;

    [Space(16)]

    [Tooltip("用户选择槽位后要加载的场景名称。")]
    public string loadSceneAfterSelectSlot;

    [Space(16)]

    [Tooltip("选择槽位后调用的事件，但在加载由loadSceneAfterSelectSlot指定的场景之前。")]
    public UnityEvent onAfterSelectSlot;

    [Tooltip("用户创建槽位后调用的事件，但尚未选择该槽位。")]
    public UnityEvent onAfterCreateSlot;

    [Space(16)]

    [Tooltip("我们要存储存档文件的子文件夹。如果这是相对路径，它将相对于Application.persistentDataPath。")]
    public string slotDirectory = "slots/";
    [Tooltip("我们要为存档文件使用的扩展名。")]
    public string slotExtension = ".es3";

    [Space(16)]

    [Tooltip("我们将实例化以创建槽位的模板。")]
    public GameObject slotTemplate;
    [Tooltip("创建新槽位的对话框。")]
    public GameObject createDialog;
    [Tooltip("向用户显示错误的对话框。")]
    public GameObject errorDialog;

    // 已选择槽位的相对路径，如果没有选择则为null。
    public static string selectedSlotPath = null;

    // 已创建的槽位列表。
    public List<GameObject> slots = new List<GameObject>();

    // 如果文件没有时间戳，它将返回此DateTime。
    static DateTime falseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    // 参见Unity文档获取更多信息：https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html
    protected virtual void OnEnable()
    {
        // 停用槽位模板使其不可见。
        slotTemplate.SetActive(false);
        // 如果需要，销毁任何现有槽位并重新开始。
        DestroySlots();
        // 如果存在，创建我们的存档槽位。
        InstantiateSlots();
    }

    // 查找存档槽位文件并为每个文件实例化一个存档槽位。
    protected virtual void InstantiateSlots()
    {
        // 用于存储我们的存档槽位的列表，以便我们可以对它们进行排序。
        List<(string Name, DateTime Timestamp)> slots = new List<(string Name, DateTime Timestamp)>();

        // 如果没有槽位要加载，则不执行任何操作。
        if (!ES3.DirectoryExists(slotDirectory))
            return;

        // 将每个槽位放入List中，以便我们可以对它们进行排序。
        foreach (var file in ES3.GetFiles(slotDirectory))
        {
            // 获取槽位名称，即不带扩展名的文件名。
            var slotName = Path.GetFileNameWithoutExtension(file);
            // 获取时间戳，以便我们可以向用户显示此信息并使用它来对槽位进行排序。
            var timestamp = ES3.GetTimestamp(GetSlotPath(slotName)).ToLocalTime();
            // 将数据添加到槽位列表中。
            slots.Add((Name: slotName, Timestamp: timestamp));
        }

        // 现在按时间戳对槽位进行排序。
        slots = slots.OrderByDescending(x => x.Timestamp).ToList();

        // 现在创建槽位。
        foreach (var slot in slots)
            InstantiateSlot(slot.Name, slot.Timestamp);
    }

    // 使用给定的槽位名称和时间戳实例化单个存档槽位。
    public virtual ES3Slot InstantiateSlot(string slotName, DateTime timestamp)
    {
        // 创建我们槽位的实例。
        var slot = Instantiate(slotTemplate, slotTemplate.transform.parent);

        // 将其添加到我们的槽位列表中。
        slots.Add(slot);

        // 确保我们将其激活，因为模板将处于非活动状态。
        slot.SetActive(true);

        var es3SelectSlot = slot.GetComponent<ES3Slot>();
        es3SelectSlot.nameLabel.text = slotName.Replace('_', ' ');

        // 如果文件没有时间戳，则不显示时间戳。
        if (timestamp == falseDateTime)
            es3SelectSlot.timestampLabel.text = "";
        // 否则，设置时间戳的标签。
        else
            es3SelectSlot.timestampLabel.text = $"{timestamp.ToString("yyyy-MM-dd")}\n{timestamp.ToString("HH:mm:ss")}";

        return es3SelectSlot;
    }

    // 通过在UI中实例化并在必要时为其创建存档文件来创建新槽位。
    public virtual ES3Slot CreateNewSlot(string slotName)
    {
        // 获取当前时间戳。
        var creationTimestamp = DateTime.Now;
        // 在UI中创建槽位。
        var slot = InstantiateSlot(slotName, creationTimestamp);
        // 将槽位移动到列表顶部。
        slot.MoveToTop();

        // 如果启用了选项，则自动为存档槽位创建文件。
        if (autoCreateSaveFile)
            ES3.SaveRaw("{}", GetSlotPath(slotName));

        // 如果需要，选择槽位。
        if (selectSlotAfterCreation)
            slot.SelectSlot();

        // 将滚动视图滚动到列表顶部。
        ScrollToTop();

        return slot;
    }

    // 显示向用户显示错误的对话框。
    public virtual void ShowErrorDialog(string errorMessage)
    {
        errorDialog.transform.Find("Dialog Box/Message").GetComponent<TMP_Text>().text = errorMessage;
        errorDialog.SetActive(true);
    }

    #region 实用方法

    // 销毁所有已创建的槽位，但不删除其底层存档文件。
    protected virtual void DestroySlots()
    {
        foreach (var slot in slots)
            Destroy(slot);
        slots.Clear();
    }

    // 获取具有给定槽位名称的槽位的相对文件路径。
    public virtual string GetSlotPath(string slotName)
    {
        // 我们在此时将任何空白字符转换为下划线以使文件更具可移植性。
        return slotDirectory + Regex.Replace(slotName, @"\s+", "_") + slotExtension;
    }

    // 滚动到槽位列表的顶部。
    public void ScrollToTop()
    {
        transform.Find("Scroll View").GetComponent<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 1f;
    }
    #endregion
}
#endif


#if UNITY_EDITOR
// 管理创建槽位的上下文菜单项。
public class ES3SlotMenuItems : MonoBehaviour
{
    [MenuItem("GameObject/Easy Save 3/Add Save Slots to Scene", false, 33)]
    [MenuItem("Assets/Easy Save 3/Add Save Slots to Scene", false, 33)]
    [MenuItem("Tools/Easy Save 3/Add Save Slots to Scene", false, 150)]
    public static void AddSaveSlotsToScene()
    {
#if !ES3_TMPRO || !ES3_UGUI
        EditorUtility.DisplayDialog("无法创建存档槽位", "必须在Window > Package Manager中安装'TextMeshPro'和'Unity UI'包才能使用Easy Save的槽位功能。", "确定");
#else
        var mgr = AddSlotsToScene();
        mgr.gameObject.name = "Save Slots Canvas";
        mgr.transform.parent.gameObject.name = "Save Slots";
        mgr.showConfirmationIfExists = true;
        mgr.showCreateSlotButton = true;
        AddEventSystemToSceneIfNotExists();
#endif
    }

    [MenuItem("GameObject/Easy Save 3/Add Load Slots to Scene", false, 33)]
    [MenuItem("Assets/Easy Save 3/Add Load Slots to Scene", false, 33)]
    [MenuItem("Tools/Easy Save 3/Add Load Slots to Scene", false, 150)]
    public static void AddLoadSlotsToScene()
    {
#if !ES3_TMPRO || !ES3_UGUI
        EditorUtility.DisplayDialog("无法创建存档槽位", "必须在Window > Package Manager中安装'TextMeshPro'和'Unity UI'包才能使用Easy Save的槽位功能。", "确定");
#else
        var mgr = AddSlotsToScene();
        mgr.gameObject.name = "Load Slots Canvas";
        mgr.transform.parent.gameObject.name = "Load Slots";
        mgr.showConfirmationIfExists = false;
        mgr.showCreateSlotButton = false;
        mgr.GetComponentInChildren<ES3CreateSlot>().gameObject.SetActive(false);
        AddEventSystemToSceneIfNotExists();
#endif
    }

#if ES3_TMPRO && ES3_UGUI

    static void AddEventSystemToSceneIfNotExists()
    {
#if UNITY_2022_3_OR_NEWER
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
#else
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
#endif
        {
            GameObject eventSystemGameObject = new GameObject("EventSystem");
            eventSystemGameObject.AddComponent<EventSystem>();
            eventSystemGameObject.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemGameObject, "Created EventSystem");
        }
    }

    static ES3SlotManager AddSlotsToScene()
    {
        if (!SceneManager.GetActiveScene().isLoaded)
            EditorUtility.DisplayDialog("无法将管理器添加到场景", "无法将存档槽位添加到场景，因为当前没有打开的场景。", "确定");

        var pathToEasySaveFolder = ES3Settings.PathToEasySaveFolder();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathToEasySaveFolder + "Scripts/Save Slots/Easy Save Slots Canvas.prefab");
        var instance = (GameObject)Instantiate(prefab);
        Undo.RegisterCreatedObjectUndo(instance, "Added Save Slots to Scene");

        return instance.GetComponentInChildren<ES3SlotManager>();
    }
#endif
}
#endif