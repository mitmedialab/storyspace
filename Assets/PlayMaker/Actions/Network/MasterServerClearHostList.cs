// (c) Copyright HutongGames, LLC 2010-2012. All rights reserved.

#if !(UNITY_FLASH || UNITY_NACL)

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Clear the host list which was received by MasterServer Request Host List")]
	public class MasterServerClearHostList : FsmStateAction
	{
		
		public override void OnEnter()
		{
			MasterServer.ClearHostList();
			
			Finish();
			
		}
	}
}

#endif