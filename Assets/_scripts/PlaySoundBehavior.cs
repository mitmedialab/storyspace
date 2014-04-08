using UnityEngine;
using System.Collections;

/**
 * plays sound when object is tapped
 * 
 **/
public class PlaySoundBehavior : MonoBehaviour
{
	private GameObject tappedBody = null; // holds the object we tapped; null if none
	
	void OnEnable()
    {
        // subscribe to the default global drag gesture events
		FingerGestures.OnFingerTap += HandleFingerGesturesOnFingerTap;
    }
	
	void Start()
	{
	}
	
	void Update()
	{
	}
	
	
	void OnDisable()
    {
        // unsubscribe from the default global drag gesture events
        FingerGestures.OnFingerTap -= HandleFingerGesturesOnFingerTap;
    }

	// handle finger taps
	void HandleFingerGesturesOnFingerTap (int fingerIndex, Vector2 fingerPos)
	{
		// pick object that was under our finger when we tapped
	    this.tappedBody = PickRigidbody( fingerPos );
		
		if (this.tappedBody != null && this.tappedBody.tag.Contains("ObjectWithWord"))	
		{			
			// play audio clip if this game object has a clip to play
			AudioSource auds = this.tappedBody.GetComponent<AudioSource>();
			if (auds != null && auds.clip != null)
			{
				Debug.Log ("playing clip for object " + this.tappedBody.name);
	
				// play the audio clip attached to the game object
				this.tappedBody.audio.Play();	
			}
		}

	}
 
	// utility method to raycast into the scene from the input screen position,
	// looking for a rigidbody
	GameObject PickRigidbody( Vector2 screenPos )
	{
	    Ray ray = Camera.main.ScreenPointToRay( screenPos );
	    RaycastHit hit;
		
	    if( !Physics.Raycast( ray, out hit)) //, Mathf.Infinity ) )
	        return null;
	    return hit.collider.gameObject;
	}
}


