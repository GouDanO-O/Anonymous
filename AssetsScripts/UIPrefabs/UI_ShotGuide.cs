using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GDFramework.Example
{
	public class UI_ShotGuideData : UIPanelData
	{
	}
	public partial class UI_ShotGuide : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UI_ShotGuideData ?? new UI_ShotGuideData();
			// please add init code here
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
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
	}
}
