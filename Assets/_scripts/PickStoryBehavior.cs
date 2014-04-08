using UnityEngine;
using System.Collections;

/**
 * scene lets user pick which story scene to go to next
 * 
 * */
public class PickStoryBehavior : MonoBehaviour 
{
	private GameObject tappedBody = null; // holds the object we tapped; null if none	
	//private GameObject[] characters = null; // holds set of characters
	
	/**
	 * Initialize stuff
	 **/
	void OnEnable() 
	{
		// only display relevant story character!
		//this.characters = GameObject.FindGameObjectsWithTag(Constants.TAG_STORY_CHAR);
		
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
		if (this.tappedBody != null && this.tappedBody.tag.Contains(Constants.TAG_STORY_CHAR))
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
		
		// disable growshrink
		GameObject[] chars = GameObject.FindGameObjectsWithTag(Constants.TAG_STORY_CHAR);
		foreach (GameObject k in chars)
		{
			k.GetComponent<GrowShrinkBehavior>().enabled = false;
		}
		
		// select the kid for future scenes			
		// and load the next scene with this kid 
		Debug.Log("attempting to load next scene...");

		// keep recordtrigger object for next scene
		DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_RECORDER)); 

		switch (go.name)
		{
			case Constants.NAME_ICEBERG:
				Debug.Log(">> Loading penguin story scene");
				Application.LoadLevel(Constants.SCENE_ICEBERG); // load next scene
				break;
			case Constants.NAME_TREEMEADOW:
				Debug.Log(">> Loading dragon story scene");
				Application.LoadLevel(Constants.SCENE_TREEMEADOW); // load next scene
				break;
			case Constants.NAME_MARS:
				Debug.Log(">> Loading alien story scene");
				Application.LoadLevel(Constants.SCENE_MARS); // load next scene
				break;
			case Constants.NAME_PINEFOREST:
				Debug.Log (">> Loading squirrel story scene");
				Application.LoadLevel(Constants.SCENE_PINEFOREST);
				break;
			case Constants.NAME_DINOSAUR:
				Debug.Log (">> Loading dinosaur story scene");
				Application.LoadLevel(Constants.SCENE_DINOSAUR);
				break;
			case Constants.NAME_PLAYGROUND:
				Debug.Log (">> Loading playground story scene");
				Application.LoadLevel(Constants.SCENE_PLAYGROUND);
				break;
			case Constants.NAME_HOUSE:
				Debug.Log (">> Loading house story scene");
				Application.LoadLevel(Constants.SCENE_HOUSE);
				break;
			case Constants.NAME_CASTLE:
				Debug.Log (">> Loading castle story scene");
				Application.LoadLevel(Constants.SCENE_CASTLE);
				break;

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
