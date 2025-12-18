#if ES3_TMPRO && ES3_UGUI

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 添加到存档槽位的组件，允许它被选择、删除和取消删除。
/// </summary>
public class ES3Slot : MonoBehaviour
{
    [Tooltip("包含槽位名称的文本标签。")] public TMP_Text nameLabel;
    [Tooltip("包含槽位最后更新时间戳的文本标签。")] public TMP_Text timestampLabel;

    [Tooltip("如果showConfirmationIfExists为true时显示的确认对话框。")]
    public GameObject confirmationDialog;

    // 此槽位所属的管理器。这由创建它的管理器设置。
    public ES3SlotManager mgr;

    [Tooltip("选择此槽位的按钮。")] public Button selectButton;
    [Tooltip("删除此槽位的按钮。")] public Button deleteButton;
    [Tooltip("撤销删除此槽位的按钮。")] public Button undoButton;

    // 此槽位是否已被标记为删除。
    public bool markedForDeletion = false;

    #region 初始化和清理

    // 参见Unity文档获取更多信息：https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html
    public virtual void OnEnable()
    {
        // 添加按钮点击监听器。
        selectButton.onClick.AddListener(TrySelectSlot);
        deleteButton.onClick.AddListener(MarkSlotForDeletion);
        undoButton.onClick.AddListener(UnmarkSlotForDeletion);
    }

    // 参见Unity文档获取更多信息：https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html
    public virtual void OnDisable()
    {
        // 移除所有按钮点击监听器。
        selectButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        undoButton.onClick.RemoveAllListeners();

        // 如果此槽位被标记为删除，删除它。
        if (markedForDeletion)
            DeleteSlot();
    }

    #endregion

    #region 选择方法

    // 当选择槽位按钮被按下时调用。
    protected virtual void TrySelectSlot()
    {
        // 如果需要，管理确认对话框。
        if (mgr.showConfirmationIfExists)
        {
            if (confirmationDialog == null)
                Debug.LogError("此ES3SelectSlot组件的confirmationDialog字段尚未在检查器中设置。", this);

            // 如果我们正在覆盖存档槽位，显示确认对话框。
            if (ES3.FileExists(GetSlotPath()))
            {
                // 显示对话框。
                confirmationDialog.SetActive(true);
                // 为确认按钮注册事件。
                confirmationDialog.GetComponent<ES3SlotDialog>().confirmButton.onClick
                    .AddListener(OverwriteThenSelectSlot);
                return;
            }
        }

        SelectSlot();
    }

    // 选择槽位并在适用时调用选择后事件。
    public virtual void SelectSlot()
    {
        // 如果确认对话框打开，隐藏它。
        confirmationDialog?.SetActive(false);

        // 设置自动保存使用的路径。
        ES3SlotManager.selectedSlotPath = GetSlotPath();

        // 当Easy Save方法使用的默认路径。
        ES3Settings.defaultSettings.path = ES3SlotManager.selectedSlotPath;

        // 如果我们指定了用户选择槽位后要调用的事件，调用它。
        mgr.onAfterSelectSlot?.Invoke();

        // 如果我们指定了用户选择槽位后要加载的场景，加载它。
        if (!string.IsNullOrEmpty(mgr.loadSceneAfterSelectSlot))
            SceneManager.LoadScene(mgr.loadSceneAfterSelectSlot);
    }

    #endregion

    #region 删除方法

    // 标记槽位为待删除并显示撤销按钮。
    protected virtual void MarkSlotForDeletion()
    {
        markedForDeletion = true;
        // 使撤销按钮可见并隐藏删除按钮。
        undoButton.gameObject.SetActive(true);
        deleteButton.gameObject.SetActive(false);
    }

    // 取消标记槽位为待删除并再次显示删除按钮。
    protected virtual void UnmarkSlotForDeletion()
    {
        markedForDeletion = false;
        // 使撤销按钮不可见并显示删除按钮。
        undoButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(true);
    }

    // 删除槽位的现有数据然后选择它。
    protected virtual void OverwriteThenSelectSlot()
    {
        DeleteSlot();
        // 创建新槽位。
        var newSlot = mgr.CreateNewSlot(nameLabel.text);
        // 选择新槽位。
        newSlot.SelectSlot();
    }

    // 删除存档槽位。
    public virtual void DeleteSlot()
    {
        // 从磁盘和缓存中删除链接到此槽位的文件。
        ES3.DeleteFile(GetSlotPath(), new ES3Settings(ES3.Location.Cache));
        ES3.DeleteFile(GetSlotPath(), new ES3Settings(ES3.Location.File));
        // 销毁此槽位。
        Destroy(this.gameObject);
    }

    #endregion

    #region 实用方法

    // 获取具有给定槽位名称的槽位的相对文件路径。
    public virtual string GetSlotPath()
    {
        // 从管理器获取槽位路径。
        return mgr.GetSlotPath(nameLabel.text);
    }

    // 将此槽位移动到槽位列表ScrollView的顶部。
    public void MoveToTop()
    {
        transform.SetSiblingIndex(1);
    }

    #endregion
}
#endif