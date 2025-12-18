#if ES3_TMPRO && ES3_UGUI

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// 附加到创建槽位按钮的脚本，用于管理槽位创建。
/// </summary>
public class ES3CreateSlot : MonoBehaviour
{
    [Tooltip("用于弹出'创建槽位'对话框的按钮。")] public Button createButton;
    [Tooltip("创建槽位对话框的ES3SlotDialog组件")] public ES3SlotDialog createDialog;
    [Tooltip("创建槽位对话框的TMP_Text输入文本字段。")] public TMP_InputField inputField;

    [Tooltip("此创建槽位对话框所属的ES3SlotManager。")]
    public ES3SlotManager mgr;

    protected virtual void OnEnable()
    {
        // 根据槽位管理器中的设置，是否应该显示或隐藏此创建槽位按钮。
        gameObject.SetActive(mgr.showCreateSlotButton);
        // 使创建槽位按钮弹出创建槽位对话框。
        createButton.onClick.AddListener(ShowCreateSlotDialog);
        // 为确认按钮添加监听器。
        createDialog.confirmButton.onClick.AddListener(TryCreateNewSlot);
    }

    protected virtual void OnDisable()
    {
        // 确保文本字段为下次使用而清空。
        inputField.text = string.Empty;
        // 移除所有监听器。
        createButton.onClick.RemoveAllListeners();
        createDialog.confirmButton.onClick.RemoveAllListeners();
    }

    // 当创建新槽位按钮被按下时调用。
    protected void ShowCreateSlotDialog()
    {
        // 使对话框可见并激活。
        createDialog.gameObject.SetActive(true);
        // 设置输入字段为活动状态，这样玩家不需要点击它就能输入名称。
        inputField.Select();
        inputField.ActivateInputField();
    }

    // 当创建新槽位对话框中的创建按钮被按下时调用。
    public virtual void TryCreateNewSlot()
    {
        // 如果用户没有指定名称，抛出错误。
        // 注意不需要对名称进行其他验证，因为这是通过TMP_InputField组件中的REGEX处理的。
        if (string.IsNullOrEmpty(inputField.text))
        {
            mgr.ShowErrorDialog("您必须为您的存档槽位指定一个名称");
            return;
        }

        // 获取我们尝试创建的槽位的文件路径。
        var slotPath = mgr.GetSlotPath(inputField.text);

        // 如果具有此名称的槽位已经存在，要求用户输入不同的名称，
        // 或者如果槽位被标记为删除，删除它的文件以便可以创建这个槽位。
        if (ES3.FileExists(slotPath))
        {
            // 检查是否存在具有此名称且已被标记为删除的槽位。
            var slotMarkedForDeletion = mgr.slots.Select(go => go.GetComponent<ES3Slot>()).FirstOrDefault(slot =>
                mgr.GetSlotPath(slot.nameLabel.text) == slotPath && slot.markedForDeletion);

            // 如果没有具有此路径且被标记为删除的槽位，强制用户选择另一个名称。
            if (slotMarkedForDeletion == null)
            {
                mgr.ShowErrorDialog("已存在具有此名称的槽位。请选择不同的名称。");
                return;
            }
            // 否则，删除槽位以便可以从头创建。
            else
                slotMarkedForDeletion.DeleteSlot();
        }

        // 创建槽位。
        var slot = mgr.CreateNewSlot(inputField.text);
        // 清空输入字段，这样当我们重新打开时值不会在那里。
        inputField.text = "";
        // 隐藏对话框。
        createDialog.gameObject.SetActive(false);

        // 如果我们指定了用户创建槽位后要调用的事件，调用它。
        mgr.onAfterCreateSlot?.Invoke();
    }
}

#endif