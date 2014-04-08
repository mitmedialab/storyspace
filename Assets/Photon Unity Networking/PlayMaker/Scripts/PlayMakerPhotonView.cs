// (c) Copyright HutongGames, LLC 2010-2012. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Play maker photon variable types. Simply for better code and being able to reference conveniently the variables types to stream.
/// Used in PlayMakerPhotonFsmVariableDefinition
/// TODO: expand to support all possible types.
/// </summary>
using HutongGames.PlayMaker;

	public enum PlayMakerPhotonVarTypes
	{
		Bool,
		Int,
		Float,
		String,
		Vector3,
	}


/// <summary>
/// PlayMaker photon synch proxy.
/// This behavior implements the underlying data serialization necessary to synchronize data for a given Fsm Component having some of its variables checked for network synching.
/// This behavior is also observed by a PhotonView. This is mandatory for the serialization over the photon network to happen.
/// the required set up described above is NOT YET checked. 
/// TODO: implement runtime or editor time verification that the set up is right. The problem is I can't find a way to get notified about instanciation so I can't run a check on
/// a gameObject created by an instanciating from the network.
/// </summary>
public class PlayMakerPhotonView : MonoBehaviour {
	
	
	
	/// <summary>
	/// The fsm Component being observed. 
	/// We implement the setter to set up FsmVariable as soon as possible, 
	/// else the photonView will miss it and create errors as we start streaming the fsm vars too late ( we have to do it before the Start() )
	/// </summary>
	public PlayMakerFSM observed
	{
		set{
			_observed = value;
			SetUpFsmVariableToSynch();
		}
		
		get{
			return _observed;
		}
	}
	
	private PlayMakerFSM _observed;
	
	/// <summary>
	/// Holds all the variables references to read from and write to during serialization.
	/// </summary>
	private ArrayList variableLOT;
	
	
	/// <summary>
	/// call base
	/// </summary>	
	private void Awake()
    {
		variableLOT = new ArrayList();

    }// Awake

	/// <summary>
	/// Sets up fsm variable caching for synch.
	/// It scans the observed Fsm for all variable checked for network synching, and store the required info about them using PlayMakerPhotonFsmVariableDefinition
 	/// It store all these variables in  variableLOT to be iterated during stream read and write.
 	/// TODO: implement all possible variables to synch.
	/// </summary>
	private void SetUpFsmVariableToSynch()
	{
		
		// fill the variableLOT with all the networksynched Fsmvariables.
	
		// int
		foreach(FsmInt fsmInt in  observed.FsmVariables.IntVariables)
		{
			if (fsmInt.NetworkSync){
				Debug.Log ("network synched fsmInt: '"+fsmInt.Name +"' in fsm:'" +observed.Fsm.Name+"' in gameObject:'"+observed.gameObject.name+"'");
			
				PlayMakerPhotonFsmVariableDefinition varDef = new PlayMakerPhotonFsmVariableDefinition();
				varDef.name = fsmInt.Name;
				varDef.type = PlayMakerPhotonVarTypes.Int;
				varDef.FsmIntPointer = fsmInt;
				variableLOT.Add(varDef);
			}
		}
	
		// float
		foreach(FsmFloat fsmFloat in  observed.FsmVariables.FloatVariables)
		{
			if (fsmFloat.NetworkSync){
				Debug.Log ("network synched FsmFloat: '"+fsmFloat.Name +"' in fsm:'" +observed.Fsm.Name+"' in gameObject:'"+observed.gameObject.name+"'");
			
				PlayMakerPhotonFsmVariableDefinition varDef = new PlayMakerPhotonFsmVariableDefinition();
				varDef.name = fsmFloat.Name;
				varDef.type = PlayMakerPhotonVarTypes.Float;
				varDef.FsmFloatPointer = fsmFloat;
				variableLOT.Add(varDef);
			}
		}
	
	
		// vector3
		foreach(FsmVector3 fsmVector3 in  observed.FsmVariables.Vector3Variables)
		{
			if (fsmVector3.NetworkSync){
				Debug.Log ("network synched fsmVector3: '"+fsmVector3.Name +"' in fsm:'" +observed.Fsm.Name+"' in gameObject:'"+observed.gameObject.name+"'");
			
				PlayMakerPhotonFsmVariableDefinition varDef = new PlayMakerPhotonFsmVariableDefinition();
				varDef.name = fsmVector3.Name;
				varDef.type = PlayMakerPhotonVarTypes.Vector3;
				varDef.FsmVector3Pointer = fsmVector3;
				variableLOT.Add(varDef);
			}
		}
		
	}// SetUpFsmVariableToSynch
	 
	
	#region serialization
	
	/// <summary>
	/// The serialization required for playmaker integration. This is transparent for the user.
	/// 1: Add the "PlaymakerPhotonView" component to observe this fsm,
    /// 2: Add a "photonView" component to observe that "PlaymakerPhotonView" component
    /// 3: Check "network synch" in the Fsm variables you want to synch over the network, 
    /// 
    /// This setup is required For each Fsm. So if on one GameObject , several Fsm wants to sync variables, 
    /// you need a "PlaymakerPhotonView" and "PhotonView" setup for each.
    /// 
	/// TODO: this might very well need improvment or reconsideration. AT least some editor or runtime check and helpers.
	/// I am thinking of letting the user only add a "photonView" observing the fsm, and at runtime insert the "PlaymakerPhotonView" in between.
	/// But I can't run this check when an instanciation occurs as I have no notifications of this.
	/// 
	/// </summary>
	/// <param name='stream'>
	/// Stream of data
	/// </param>
	/// <param name='info'>
	/// Info.
	/// </param>
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.isWriting)
        {
			
			// go through the Look up table and send in order.
			foreach( PlayMakerPhotonFsmVariableDefinition varDef in variableLOT )
       		{
				
				switch (varDef.type)
				{
					case PlayMakerPhotonVarTypes.Bool:
						stream.SendNext(varDef.FsmBoolPointer.Value); 
						break;
					case PlayMakerPhotonVarTypes.Int:
						stream.SendNext(varDef.FsmIntPointer.Value); 
						break;
					case PlayMakerPhotonVarTypes.Float:
						stream.SendNext(varDef.FsmFloatPointer.Value); 
						break;
					case PlayMakerPhotonVarTypes.String:
						stream.SendNext(varDef.FsmStringPointer.Value); 
						break;
					case PlayMakerPhotonVarTypes.Vector3:
					//Debug.Log("sending: "+varDef.FsmVector3Pointer.Name +" : "+varDef.FsmVector3Pointer.Value );
						stream.SendNext(varDef.FsmVector3Pointer.Value); 
						break;
				}	
       		}

        }else{	
			
			// go through the Look up table and read in order.
			foreach( PlayMakerPhotonFsmVariableDefinition varDef in variableLOT )
       		{
				switch (varDef.type)
				{
					case PlayMakerPhotonVarTypes.Bool:			
						varDef.FsmBoolPointer.Value = (bool)stream.ReceiveNext();
						break;
					case PlayMakerPhotonVarTypes.Int:
						varDef.FsmIntPointer.Value = (int)stream.ReceiveNext();
						break;
					case PlayMakerPhotonVarTypes.Float:
						varDef.FsmFloatPointer.Value = (float)stream.ReceiveNext();
						break;
					case PlayMakerPhotonVarTypes.String:
						varDef.FsmStringPointer.Value = (string)stream.ReceiveNext();
						break;
					case PlayMakerPhotonVarTypes.Vector3:
						Vector3 tmp = (Vector3)stream.ReceiveNext();
						varDef.FsmVector3Pointer.Value = tmp;
					//	Debug.Log("receiving: "+varDef.FsmVector3Pointer.Name +" : "+tmp + " : "+varDef.FsmVector3Pointer.Value);
						break;
				}	
       		}

        }// reading or writing
    }
	
	#endregion
	
}

/// <summary>
/// Allow a convenient description of the Fsm variable that needs streaming.  
/// Also let the reference be cached instead of accessed everytime. Make the stream function easier to script and potenitaly help performances hopefully.
/// </summary>
public class PlayMakerPhotonFsmVariableDefinition
{

	/// <summary>
	/// The name of the Fsm Variable. Within a given Fsm, variables names have to be unique, so we are ok.
	/// </summary>
	public string name;
	
	/// <summary>
	/// Store the type conviniently instead of messing with type during the streaming.
	/// </summary>
	public PlayMakerPhotonVarTypes type;
	
	/// <summary>
	/// The fsm bool pointer. Set Only if type correspond. This is for convenient caching without loosing the type.
	/// </summary>
	public FsmBool FsmBoolPointer;
	
	/// <summary>
	/// The fsm bool pointer. Set Only if type correspond. This is for convenient caching without loosing the type.
	/// </summary>
	public FsmInt FsmIntPointer;
	
	/// <summary>
	/// The fsm bool pointer. Set Only if type correspond. This is for convenient caching without loosing the type.
	/// </summary>
	public FsmFloat FsmFloatPointer;
	
	/// <summary>
	/// The fsm bool pointer. Set Only if type correspond. This is for convenient caching without loosing the type.
	/// </summary>
	public FsmString FsmStringPointer;
	
	/// <summary>
	/// The fsm bool pointer. Set Only if type correspond. This is for convenient caching without loosing the type.
	/// </summary>
	public FsmVector3 FsmVector3Pointer;
}
