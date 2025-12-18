/****************************************************************************
 * Copyright (c) 2017 xiaojun
 * Copyright (c) 2017 liangxie
 * Copyright (c) 2017 imagicbell
 * Copyright (c) 2018 ~ 2022 liangxie
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

namespace GDFrameworkExtend.UIKit
{
	using UnityEngine;

	/// <summary>
	/// 每个 UIPanel 对应的Data
	/// </summary>
	public interface IUIData
	{
	}

	public class UIPanelData : IUIData
	{
	}
	
	public abstract partial class UIPanel : QMonoBehaviour, IPanel
	{
		public Transform Transform => transform;

		IPanelLoader IPanel.Loader { get; set; }

		public PanelInfo Info { get; set; }

		public PanelState State { get; set; }
		
		protected Transform Root;
		
		protected Transform Ground;
		
		protected Transform Common;

		protected Transform Top;

		protected IUIData mUIData;

		public override IManager Manager => UIManager.Instance;

		protected override void OnBeforeDestroy()
		{
			ClearUIComponents();
		}

		protected virtual void ClearUIComponents()
		{
		}

		public void Init(IUIData uiData = null)
		{
			mUIData = uiData;
			OnInit(uiData);
		}

		public void Open(IUIData uiData = null)
		{
			State = PanelState.Opening;
			OnOpen(uiData);
			OpenAni();
		}

		public override void Hide()
		{
			State = PanelState.Hide;
			HideAni();
			base.Hide();
		}
		


		protected virtual void OnInit(IUIData uiData = null)
		{
			
		}

		protected virtual void GetRelyComponent()
		{
			Root = transform.GetChild(0);
			Ground = Root.Find("Ground");
			Common = Root.Find("Common");
			Top = Root.Find("Top");
		}

		protected virtual void RegisterEvent()
		{
			
		}

		protected virtual void OnOpen(IUIData uiData = null)
		{
			
		}

		/// <summary>
		/// 打开时的动画
		/// </summary>
		protected virtual void OpenAni()
		{
			
		}
		
		/// <summary>
		/// 关闭动画
		/// </summary>
		protected virtual void HideAni()
		{
			
		}

		/// <summary>
		/// avoid override in child class
		/// </summary>
		protected sealed override void OnDestroy()
		{
			base.OnDestroy();
		}

		/// <summary>
		/// 关闭,不允许子类调用
		/// </summary>
		void IPanel.Close(bool destroyed)
		{
			Info.UIData = mUIData;
			mOnClosed?.Invoke();
			mOnClosed = null;
			Hide();
			State = PanelState.Closed;
			OnClose();

			if (destroyed)
			{
				Destroy(gameObject);
			}

			var panelInterface = this as IPanel;
			panelInterface.Loader.Unload();
			UIKit.Config.PanelLoaderPool.RecycleLoader(panelInterface.Loader);
			panelInterface.Loader = null;

			mUIData = null;
		}

		protected void CloseSelf()
		{
			UIKit.ClosePanel(this);
		}

		protected void Back()
		{
			UIKit.Back(name);
		}

		/// <summary>
		/// 必须使用这个
		/// </summary>
		protected abstract void OnClose();

		private System.Action mOnClosed;

		public void OnClosed(System.Action onPanelClosed)
		{
			mOnClosed = onPanelClosed;
		}
	}
}