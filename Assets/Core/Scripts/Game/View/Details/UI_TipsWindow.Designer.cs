using System;
using UnityEngine;
using UnityEngine.UI;
using GDFrameworkExtend.UIKit;

namespace Core.Game.View
{
	// Generate Id:05ae0d31-075d-462f-b32a-fb2899d191ca
	public partial class UI_TipsWindow
	{
		public const string Name = "UI_TipsWindow";
		
		
		private UI_TipsWindowData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public UI_TipsWindowData Data
		{
			get
			{
				return mData;
			}
		}
		
		UI_TipsWindowData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UI_TipsWindowData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
