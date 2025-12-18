using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GDFramework.Example
{
	// Generate Id:8b16e399-38a5-4d25-89c6-2d108bc9d7e2
	public partial class UI_ShotGuide
	{
		public const string Name = "UI_ShotGuide";
		
		
		private UI_ShotGuideData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public UI_ShotGuideData Data
		{
			get
			{
				return mData;
			}
		}
		
		UI_ShotGuideData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UI_ShotGuideData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
