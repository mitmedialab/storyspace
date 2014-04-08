using UnityEngine;
using System.Collections;

/**
 * properties of the story character, such as which direction it starts out facing
 * */
public class RecordTrigger : MonoBehaviour
{
	// are we recording?
	public bool recording = false;
	
	// TODO
	// use this property attached to the record object to track when recording is ON or OFF
	// that way, the property persists across scenes
	// (but will it be a problem if a new version of the object is created every time we enter a scene?)
	// maybe it is by default off, for my purposes?
	// and then by default on during actual story happenings?
}



