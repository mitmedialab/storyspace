// (c) Copyright HutongGames, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Photon")]
	[Tooltip("Joins any available room but will fail if none is currently available.")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W913")]
	public class PhotonNetworkJoinRandomRoom : FsmStateAction
	{

		public override void OnEnter()
		{
			PhotonNetwork.JoinRandomRoom(); 
			
			Finish();
		}
	}
}