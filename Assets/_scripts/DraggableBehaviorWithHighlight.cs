using UnityEngine;
 
public class DraggableBehaviorWithHighlight : MonoBehaviour 
{
	// game objects
	private GameObject highlight = null; // light for highlighting objects
	private GameObject draggedBody; // holds the object we're currently dragging, or null if none
	
    void OnEnable()
    {
        // subscribe to the default global drag gesture events
        FingerGestures.OnDragBegin += FingerGestures_OnDragBegin;
        FingerGestures.OnDragMove += FingerGestures_OnDragMove;
        FingerGestures.OnDragEnd += FingerGestures_OnDragEnd;
		
		// set up highlight
		this.highlight = GameObject.FindGameObjectWithTag(Constants.TAG_LIGHT);
		if (this.highlight != null)
		{
			this.highlight.SetActive(false);
			Debug.Log("Got light: " + this.highlight.name);
		}
		else
		{
			Debug.Log("ERROR: No light found");
		}

    }
 
    void OnDisable()
    {
        // unsubscribe from the default global drag gesture events
        FingerGestures.OnDragBegin -= FingerGestures_OnDragBegin;
        FingerGestures.OnDragMove -= FingerGestures_OnDragMove;
        FingerGestures.OnDragEnd -= FingerGestures_OnDragEnd;
    }
     
    void FingerGestures_OnDragBegin( Vector2 fingerPos, Vector2 startPos )
    {
         // pick object that was under our finger when we started dragging
	    draggedBody = PickRigidbody( startPos );
		Debug.Log("start drag");

	 
    }
 
	void FingerGestures_OnDragMove( Vector2 fingerPos, Vector2 delta )
	{
	    if( draggedBody )
	    {
	        // convert the finger position to world position
	        float distToCamera = Mathf.Abs( draggedBody.transform.position.z - 
				Camera.main.transform.position.z );
	        Vector3 fingerWorldPos = Camera.main.ScreenToWorldPoint( 
				new Vector3( fingerPos.x, fingerPos.y, distToCamera) );
	 
	        // center the object on our moving finger
	        this.draggedBody.transform.position = fingerWorldPos;
			this.highlight.transform.position = fingerWorldPos;
			
			
	    }
	}
     
	void FingerGestures_OnDragEnd( Vector2 fingerPos )
	{
	    if( draggedBody )
	    {
	        // we are no longer dragging the object
	        draggedBody = null;
	    }
	}
	
	// utility method to raycast into the scene from the input screen position, looking for a rigidbody
	GameObject PickRigidbody( Vector2 screenPos )
	{
	    Ray ray = Camera.main.ScreenPointToRay( screenPos );
	    RaycastHit hit;

	    if( !Physics.Raycast( ray, out hit ) )
	        return null;
		
		Debug.Log("hit " + hit.collider.gameObject.name);
	    return hit.collider.gameObject;
	}
}