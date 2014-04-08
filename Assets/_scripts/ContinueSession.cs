using UnityEngine;
using System.Collections;

/**
 * pause between scenes, in case there's a need to do so (e.g., to administer a
 * vocab test), with a 'continue' button to advance
 * */
public class ContinueSession : MonoBehaviour 
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
		if (this.tappedBody != null && this.tappedBody.tag.Contains(Constants.TAG_SESSION))
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
		Debug.Log("attempting to load next scene...");
		Debug.Log ("current scene: " + Constants.currentScene);

		// load next scene for the selected session
		Constants.currentScene += 1;
		
		// still doing this session's stories (SR2 - long-term story study)
		if (!Constants.SR3 && 
			Constants.currentScene < Constants.SESSION_STORY_ORDERS[Constants.currentSession-1].Length)
		{
			Debug.Log ("trying to load ... " + Constants.currentSession + " " + Constants.currentScene);
			DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
			Application.LoadLevel (Constants.SESSION_STORY_ORDERS[Constants.currentSession-1][Constants.currentScene]);		
		}
		
		// still doing this session's stories (SR3 - short-term MOS study)
		else if (Constants.SR3 &&
			Constants.currentScene < Constants.SR3_STORY_ORDERS[Constants.currentSession-1].Length)
		{
			Debug.Log ("trying to load ... " + Constants.currentSession + " " + Constants.currentScene);
			DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
			Application.LoadLevel(Constants.SR3_STORY_ORDERS[Constants.currentSession-1][Constants.currentScene]);
		}
		
		else // otherwise, at end, return to start
		{
			Constants.currentScene = -1;
			DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
			Application.LoadLevel (Constants.SCENE_INIT);					
		}
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
