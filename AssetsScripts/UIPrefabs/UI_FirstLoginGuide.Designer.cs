using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GDFramework.Example
{
	// Generate Id:3bd25f25-67f0-4c3b-882d-99dacf316e64
	public partial class UI_FirstLoginGuide
	{
		public const string Name = "UI_FirstLoginGuide";
		
		
		private UI_FirstLoginGuideData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public UI_FirstLoginGuideData Data
		{
			get
			{
				return mData;
			}
		}
		
		UI_FirstLoginGuideData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UI_FirstLoginGuideData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
