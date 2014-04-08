using UnityEngine;
using System.Collections;

/**
 * pick which session to play for the SR2 study or SR3 study
 * */
public class PickSession : MonoBehaviour 
{
	private GameObject tappedBody = null; // holds the object we tapped; null if none

	
	/**
	 * Initialize stuff
	 **/
	void OnEnable() 
	{
		// subscribe to the default global drag gesture events
		FingerGestures.OnFingerTap += HandleFingerGesturesOnFingerTap;
		FingerGestures.OnFingerDown += HandleFingerGesturesOnFingerDown;
	}
	
	void OnDestroy()
	{
		// unsubscribe from the default global drag gesture events
        FingerGestures.OnFingerTap -= HandleFingerGesturesOnFingerTap;
		FingerGestures.OnFingerDown -= HandleFingerGesturesOnFingerDown;

	}
	
	/**
	 * update 
	 **/
	void Update() 
	{
		// if user presses escape or 'back' button on android, exit program
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit ();
	}
	
	/**
	 * handle finger down events 
	 **/
	void HandleFingerGesturesOnFingerDown (int fingerIndex, Vector2 fingerPos)
	{
		CheckTouch(fingerIndex, fingerPos);
	}

	/**
     * handle finger taps  - pass to general highlight-on-touch function
	 **/
	void HandleFingerGesturesOnFingerTap (int fingerIndex, Vector2 fingerPos)
	{		
	 	CheckTouch(fingerIndex, fingerPos);	
	}
	
	/**
	 * check if next button was pressed on touch
	 **/
	void CheckTouch (int fingerIndex, Vector2 fingerPos)
	{
		// pick object that was under our finger when we tapped
	    this.tappedBody = PickRigidbody( fingerPos );

		// if the tapped object is a story character, play the character's animation 
		// & select the character, then move to next scene for character's story
		if (this.tappedBody != null && (this.tappedBody.tag.Contains(Constants.TAG_SESSION) 
			|| this.tappedBody.tag.Contains(Constants.TAG_SR3)))
		{
			Debug.Log ("tapped: " + this.tappedBody);
			
			// play sound and animation for the kid,
			// then load the next scene with this kid
			AnimateAndLoadNext(this.tappedBody);			
		}
				
	}
	
	/**
	 * animates object -- plays sound, ?
	 **/
	void AnimateAndLoadNext(GameObject go)
	{
		// Running SR3 (2nd MOS study) if session selected was tagged with SR3 tag
		// Running SR2 (long-term study) if session selected was tagged otherwise
		Constants.SR3 = (this.tappedBody.tag.Contains(Constants.TAG_SR3));		
		int sessionNum = System.Int32.Parse(go.name);
		Constants.currentSession = sessionNum;

		Debug.Log ("Loading start scene for session " + Constants.currentSession
			+ " in study " + (Constants.SR3 ? "SR3" : "SR2"));

		// load first scene for the selected session
		// actually, first scene is always the pre "start" scene
		DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
		Application.LoadLevel (Constants.SCENE_START);
	}
	
	
	
	/** utility method to raycast into the scene from the input screen position,
	 * looking for a rigidbody
	 **/
	GameObject PickRigidbody( Vector2 screenPos )
	{
	    Ray ray = Camera.main.ScreenPointToRay( screenPos );
	    RaycastHit hit;
		
	    if( !Physics.Raycast( ray, out hit))
	        return null;
		
	    return hit.collider.gameObject;
	}
	
}
