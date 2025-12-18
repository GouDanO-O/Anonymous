using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GDFramework.Example
{
	public class UI_TestPanelData : UIPanelData
	{
	}
	public partial class UI_TestPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UI_TestPanelData ?? new UI_TestPanelData();
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
