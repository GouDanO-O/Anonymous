using System;
using UnityEngine;
using UnityEngine.UI;
using GDFrameworkExtend.UIKit;

namespace Core.Game.View
{
	// Generate Id:497927bf-000e-41ee-bdd1-c9983929e20b
	public partial class UI_GameMenuPanel
	{
		public const string Name = "UI_GameMenuPanel";
		
		
		private UI_GameMenuPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public UI_GameMenuPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UI_GameMenuPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UI_GameMenuPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
