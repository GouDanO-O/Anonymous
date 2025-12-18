using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GDFramework.Example
{
	// Generate Id:1e495d39-31c5-43b4-ac86-7ad6ba3f4c7c
	public partial class UI_TestPanel
	{
		public const string Name = "UI_TestPanel";
		
		
		private UI_TestPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public UI_TestPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UI_TestPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UI_TestPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
