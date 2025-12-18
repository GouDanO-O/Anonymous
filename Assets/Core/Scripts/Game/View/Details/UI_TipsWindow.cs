using UnityEngine;
using UnityEngine.UI;
using GDFrameworkExtend.UIKit;
using TMPro;
using UnityEngine.Events;

namespace Core.Game.View
{
	public class UI_TipsWindowData : UIPanelData
	{
		public UnityAction SureAction;

		public UnityAction CancelAction;

		public string TipsString;

		public string CancelString;

		public string SureString;
	}
	public partial class UI_TipsWindow : UIPanel
	{
		protected TextMeshProUGUI TipsText;
		
		protected TextMeshProUGUI CancelText;

		protected TextMeshProUGUI SureText;
		
		protected Button CancelButton;
		
		protected Button SureButton;
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UI_TipsWindowData ?? new UI_TipsWindowData();
			// please add init code here
			Transform window = transform.Find("Window");
			TipsText = window.Find("TipsText").GetComponent<TextMeshProUGUI>();
			CancelButton = window.Find("CancelButton").GetComponent<Button>();
			SureButton = window.Find("SureButton").GetComponent<Button>();
			
			CancelText = CancelButton.transform.Find("CancelText").GetComponent<TextMeshProUGUI>();
			SureText = SureButton.transform.Find("SureText").GetComponent<TextMeshProUGUI>();
			
			CancelButton.onClick.AddListener(ClickCancelButton);
			SureButton.onClick.AddListener(ClickSureButton);
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
			TipsText.text = mData.TipsString;
			CancelText.text = mData.CancelString;
			SureText.text = mData.SureString;
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}

		/// <summary>
		/// 点击确认按钮
		/// </summary>
		private void ClickSureButton()
		{
			mData.SureAction?.Invoke();
			CloseSelf();
		}

		/// <summary>
		/// 点击取消按钮
		/// </summary>
		private void ClickCancelButton()
		{
			mData.CancelAction?.Invoke();
			CloseSelf();
		}
	}
}
