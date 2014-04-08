// (c) Copyright HutongGames, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Photon")]
	[Tooltip("Get the room we are currently in. If null, we aren't in any room.")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W910")]
	public class PhotonNetworkGetRoomProperties : FsmStateAction
	{

		[UIHint(UIHint.Variable)]
		[Tooltip("True if we are in a room.")]
		public FsmBool isInRoom;
		
		[Tooltip("Send this event if we are in a room.")]
		public FsmEvent isInRoomEvent;
		
		[Tooltip("Send this event if we aren't in any room.")]
		public FsmEvent isNotInRoomEvent;
			
		[ActionSection("room properties")]
		[UIHint(UIHint.Variable)]
		[Tooltip("the name of the room.")]
		public FsmString RoomName;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the number of players inthe room.")]
		public FsmInt playerCount;
		
		
		[UIHint(UIHint.Variable)]
		[Tooltip("The limit of players to this room. This property is shown in lobby, too.\n" +
		 	"If the room is full (players count == maxplayers), joining this room will fail..")]
		public FsmInt maxPlayers;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Defines if the room can be joined. If not open, the room is excluded from random matchmaking. \n" +
			"This does not affect listing in a lobby but joining the room will fail if not open.")]
		public FsmBool open;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("Defines if the room is listed in its lobby.")]
		public FsmBool visible;
		
	
		
		
		public override void Reset()
		{
			
			RoomName = null;
			maxPlayers = null;
			open = null;
			visible = null;
			
			playerCount = 0;
			
			isInRoom = null;
			isInRoomEvent = null;
			isNotInRoomEvent = null;
		}
		
		
		public override void OnEnter()
		{
			getRoomProperties();
			
			Finish();
		}
		
		
		void getRoomProperties()
		{
			Room _room = PhotonNetwork.room;
			bool _isInRoom = _room!=null;
			
			isInRoom.Value = _isInRoom;
			
			if (_isInRoom )
			{
				if (isInRoomEvent!=null)
				{
					Fsm.Event(isInRoomEvent);
				}
			}else{
				
				if (isNotInRoomEvent!=null)
				{
					Fsm.Event(isNotInRoomEvent);
				}
				return;
			}
			
			// we get the room properties
			RoomName.Value = _room.name;
			maxPlayers.Value = _room.maxPlayers;
			open.Value = _room.open;
			visible.Value = _room.visible;
			playerCount.Value = _room.playerCount;
			
			
		}

	}
}