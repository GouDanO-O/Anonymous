using Sirenix.OdinInspector;
using UnityEngine;

namespace GDFramework.Guide
{
    [LabelText("教程文本框的位置")]
    public enum EGuideDialogDir
    {
        [LabelText("最上层")] Top,
        [LabelText("中间")] Mid,
        [LabelText("底层")] Bottom
    }

    [LabelText("引导触发类型")]
    public enum EGuideTriggerType
    {
        [LabelText("自动触发(播放完成后自动触发)")] Auto,
        [LabelText("由事件触发")] EventTrigger
    }

    [LabelText("引导遮罩类型")]
    public enum EGuideMaskType
    {
        [LabelText("圆形遮罩")] Circle,
        [LabelText("方形遮罩")] Box
    }

    public class GuideNodeDataConfig : ScriptableObject
    {
        [LabelText("引导ID")] public int id;

        [LabelText("是否展示背景(黑色遮罩)")] public bool willShowBg;

        [Title("新手教程文本")] [MultiLineProperty(5)] [HideLabel]
        public string guideText;

        public EGuideDialogDir guideDialogDir;

        [LabelText("教程叙述人图片")] public Sprite guiderSprite;

        [LabelText("教程叙述人昵称")] public string guiderName;
    }
}