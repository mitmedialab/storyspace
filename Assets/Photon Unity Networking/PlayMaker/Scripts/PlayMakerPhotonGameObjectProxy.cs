// (c) Copyright HutongGames, LLC 2010-2012. All rights reserved.

using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

/// <summary>
/// This component is required on gameObjects and prefabs you are planning to instanciate over the Photon network.
/// 
/// It receives the OnPhotonInstantiate message when instanciated and forward it as an Event for Fsm component attached this gameObject and all its childrens.
/// 
/// It also verifies the proper setup for fsm components on that gameObject that have networked synched variables:
/// *It assumes like for the Unity networking that you have a PhotonView observing the fsm.
/// *I insert at runtime a bridge ( PlayMakerPhotonView ) that goes inbetween the fsm and the photonView. This is required because the fsmComponent doesn't implement photon networking nativly
/// ( that is not implementing OnPhotonInstantiate() nor OnPhotonSerializeView
/// It can be set up manually of course, else Iwill do it for the user at runtime when the gameObject is instanciated. 
/// Note: for gameObject sitting in the hierarchy when starting, the check is also happening but it's directly call within the "PlayMaker photon proxy" mandatory prefab
/// </summary>
public class PlayMakerPhotonGameObjectProxy : MonoBehaviour {
	
	/// <summary>
	/// output in the console activities of the various elements.
	/// Is taken form the debug value of the "PlayMaker Photon Proxy"
	/// </summary>
	bool debug = true;
	
	/// <summary>
	/// The fsm proxy used to send the "OnPhotonInstantiate" event to Fsm.
	/// </summary>
	PlayMakerFSM fsmProxy;
		
		
	[ContextMenu("Help")]
	public void help ()
	{
	    Application.OpenURL ("https://hutonggames.fogbugz.com/default.asp?W990");
	}
	
	
	// get the Playmaker Photon proxy fsm.
	void Awake () {
	
		// get the photon proxy for Photon Fsm Proxy to send event.
		GameObject go = GameObject.Find("PlayMaker Photon Proxy");
		
		if (go == null )
		{
			Debug.LogError("Working with photon network require that you add a 'PlayMaker Photon Proxy' component to the gameObject. You can do so from the menu 'PlayMaker Photon/components/Add photon proxy to scene'");
			return;
		}
		
		// get the proxy to set the debug flag.
	 	PlayMakerPhotonProxy _proxy = go.GetComponent<PlayMakerPhotonProxy>();
		if (_proxy!=null)
		{
			debug = _proxy.debug;
		}
		
		// get the Fsm for reference when sending events.
		fsmProxy = go.GetComponent<PlayMakerFSM>();
		if (fsmProxy==null)
		{
			return;
		}
		
		_proxy.SanitizeGameObject(this.gameObject);
		
	}// Awake
	
	#region Photon Messages
	
	/// <summary>
	/// compose this message to dispatch the associated global Fsm Event. 
	/// TOFIX: The problem is, It's called before Fsm are instanciated, so I had to implement a slight delay...
	/// </summary>
	void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (debug) {
			Debug.Log ("PLayMaker Photon proxy:OnPhotonInstantiate "+info.sender.name);
		}
		
	
		if (fsmProxy==null)
		{
			return;
		}
	
		// TOFIX: if we found a better solution, I am all up for it... How stable this can be if framerate is low for example?
		Invoke("sendPhotonInstantiationFsmEvent",0.1f);
		
	}// OnPhotonInstantiate
	
	
	/// <summary>
	/// Sends the photon instantiation fsm event to ALL Fsm of the instantiated gameObject AND its children.
	/// </summary>
	void sendPhotonInstantiationFsmEvent()
	{
		if (debug) {
			Debug.Log("sending PHOTON INSTANTIATE event to "+this.gameObject.name);
		}
		// set the target to be this gameObject.
		FsmOwnerDefault goTarget = new FsmOwnerDefault();
		goTarget.GameObject = new FsmGameObject();
		goTarget.GameObject.Value = this.gameObject;
		goTarget.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
		
       // send the event to this gameObject and all its children
		FsmEventTarget eventTarget = new FsmEventTarget();
		eventTarget.excludeSelf = false;
		eventTarget.target = FsmEventTarget.EventTarget.GameObject;
		eventTarget.gameObject = goTarget;
		eventTarget.sendToChildren = true;
		
		// create the event.
		FsmEvent fsmEvent = new FsmEvent("PHOTON / PHOTON INSTANTIATE");
	
		// send the event
		fsmProxy.Fsm.Event(eventTarget,fsmEvent.Name); // TOFIX: doesn't work if using simply fsmEvent
		
	}// sendPhotonInstantiationFsmEvent
	
	#endregion
}
