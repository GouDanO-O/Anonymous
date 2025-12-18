using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GDFramework.Example
{
	public class UI_FirstLoginGuideData : UIPanelData
	{
	}
	public partial class UI_FirstLoginGuide : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UI_FirstLoginGuideData ?? new UI_FirstLoginGuideData();
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
