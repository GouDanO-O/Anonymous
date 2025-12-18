using Core.Game.Procedure;
using GDFramework.Procedure;
using GDFrameworkCore;
using UnityEngine;
using UnityEngine.UI;
using GDFrameworkExtend.UIKit;

namespace Core.Game.View
{
	public class UI_GameMenuPanelData : UIPanelData
	{
	}
	public partial class UI_GameMenuPanel : UIPanel,ICanGetSystem,ICanSendEvent
	{
		protected Transform GameMenuButtonRoot;
		
		protected Button StartGameButton;

		protected Button ContinueButton;

		protected Button EditorButton;

		protected Button SettingButton;

		protected Button ExitGameButton;
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UI_GameMenuPanelData ?? new UI_GameMenuPanelData();
			// please add init code here

			GetRelyComponent();
			RegisterEvent();
		}

		protected override void GetRelyComponent()
		{
			base.GetRelyComponent();
			GameMenuButtonRoot = Common.Find("GameMenuButtonRoot");

			StartGameButton = GameMenuButtonRoot.Find("StartGameButton").GetComponent<Button>();
			ContinueButton = GameMenuButtonRoot.Find("ContinueButton").GetComponent<Button>();
			EditorButton = GameMenuButtonRoot.Find("EditorButton").GetComponent<Button>();
			SettingButton = GameMenuButtonRoot.Find("SettingButton").GetComponent<Button>();
			ExitGameButton = GameMenuButtonRoot.Find("ExitGameButton").GetComponent<Button>();
		}

		protected override void RegisterEvent()
		{
			base.RegisterEvent();
			StartGameButton.onClick.AddListener(StartGame);
			ContinueButton.onClick.AddListener(ContinueGame);
			EditorButton.onClick.AddListener(EditorMod);
			SettingButton.onClick.AddListener(OpenSetting);
			ExitGameButton.onClick.AddListener(ExitGame);
		}

		protected bool CheckWillShowContinueButton()
		{
			return GameManager.Instance.IsNewGame();
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
			OpenMenuCheck();
		}

		protected void OpenMenuCheck()
		{
			if (CheckWillShowContinueButton())
			{
				ContinueButton.GetComponent<Image>().color = Color.gray;
			}
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

		protected void StartGame()
		{
			this.SendEvent(new SChangeProcedureEvent(typeof(GameSceneProcedure)));
		}

		//TODO 后续有存档功能时,可以进行读取存档游戏
		protected void ContinueGame()
		{
			
		}

		protected void EditorMod()
		{
			CloseSelf();
		}

		protected void OpenSetting()
		{
			
		}

		protected void ExitGame()
		{
			Application.Quit();
		}

		public IArchitecture GetArchitecture()
		{
			return GameMain.Interface;
		}
	}
}
