using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class StoryspaceMainInteraction : MonoBehaviour 
{
	// game objects
	private GameObject highlight = null; // light for highlighting objects
	private GameObject tappedBody = null; // holds the object we tapped; null if none
	private GameObject draggedBody = null; // holds the object we're currently dragging, or null if none
	private GameObject draggedBodyTwo = null; // holds second dragged object, or null if none
	
	// constants
	private const int dragRadius = 180; // for two-finger drag
	private const int searchStep = 20; // for two-finger drag search for nearby objects
	
	// flags
	private bool snowing = false; // is it snowing?
	private bool canstopsnow = false; // can only stop snow after it has started snowing
	//private bool flipped = false; // for dragging
	private bool recordOn = true; // record actions if on
	private bool allowTouch = true; // allow touch actions
	private bool loadPlaybacks = true; // load playback files
	
	// record and playback
	private RecordAndPlayback rap = new RecordAndPlayback();
	
	// tcp communications
	private TCPClient clientSocket = null;
	
	// actions for main thread
	readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();
	
	// to make sure we never get a duplicate of ourselves... kind of a hack :(
	//private static StoryspaceMainInteraction smi = null;
	// TODO if we try resetting
	
	// TODO use the following if we try resetting to session select page
	/**
	 * when we awake, make sure there are no duplicates of ourselves
	 * we want only one instance of this class. weird stuff happens with our
	 * TCP communications if we are deleted/recreated or if there are two.
	 * */
	/*void Awake()
	{
		// is there already this class in existence?
		if (smi == null)
		{
			smi = this;	// if not, it'll be us
			DontDestroyOnLoad(gameObject); // keep us around
		}
		else
		{
			Destroy(this); // already a class, destroy us and use the other
		}
	}*/
	
	/**
	 * Initialize stuff
	**/
	void OnEnable() 
	{
		Debug.Log("Initializing...");
		
		// disable back button if the initial scene was the "choose current session"
		// rather than "pick a story!" ??
		//GameObject.FindGameObjectWithTag(Constants.TAG_RESET).SetActive(false);
		// TODO
		
		// subscribe to the default global tap/down gesture events
		FingerGestures.OnFingerTap += HandleFingerGesturesOnFingerTap;
		FingerGestures.OnFingerDown += HandleFingerGesturesOnFingerDown;
		
		// subscribe to the default global drag gesture events
        FingerGestures.OnDragBegin += FingerGestures_OnDragBegin;
        FingerGestures.OnDragMove += FingerGestures_OnDragMove;
        FingerGestures.OnDragEnd += FingerGestures_OnDragEnd;
		
		// subscribe to default global two-finger drag gesture events
		FingerGestures.OnTwoFingerDragBegin += HandleFingerGesturesOnTwoFingerDragBegin;
		FingerGestures.OnTwoFingerDragMove += HandleFingerGesturesOnTwoFingerDragMove;
		FingerGestures.OnTwoFingerDragEnd += HandleFingerGesturesOnTwoFingerDragEnd;
		
		// set up record/playback
		if (this.recordOn)
		{
			this.rap.SetupRecordingFile();
		}
		if (this.loadPlaybacks)
		{
			Debug.Log("----------Start loading playbacks----------");		
			this.rap.LoadAllPlaybackFiles();
			Debug.Log("----------Done loading playbacks!----------");
		}
		
		// set up light
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
		
		
		// For testing playback:
		//StartCoroutine(this.rap.PlaybackFile("playbackfilename"));
		
		// set up tcp client...
		// note: does not attempt to reconnect if connection fails
		if (this.clientSocket == null)
		{
			this.clientSocket = new TCPClient();
			this.clientSocket.run (Constants.TCP_IP);
			this.clientSocket.receivedMsgEvent += 
				new ReceivedMessageEventHandler(HandleClientSocketReceivedMsgEvent);
		}
		
	}

	/**
	 * on destroy
	 **/
	void OnDestroy()
	{
		// close reader
		if (this.recordOn)
			this.rap.CloseRecordingFile();
		
		// unsubscribe from the default global tap/down gesture events
        FingerGestures.OnFingerTap -= HandleFingerGesturesOnFingerTap;
		FingerGestures.OnFingerDown -= HandleFingerGesturesOnFingerDown;
		
		// unsubscribe from the default global drag gesture events
        FingerGestures.OnDragBegin -= FingerGestures_OnDragBegin;
        FingerGestures.OnDragMove -= FingerGestures_OnDragMove;
        FingerGestures.OnDragEnd -= FingerGestures_OnDragEnd;
		
		FingerGestures.OnTwoFingerDragBegin -= HandleFingerGesturesOnTwoFingerDragBegin;
		FingerGestures.OnTwoFingerDragMove -= HandleFingerGesturesOnTwoFingerDragMove;
		FingerGestures.OnTwoFingerDragEnd -= HandleFingerGesturesOnTwoFingerDragEnd;
		
		// unsubscribe from received message events
		this.clientSocket.receivedMsgEvent -= HandleClientSocketReceivedMsgEvent;
		
		Debug.Log("destroyed storyspacemaininteraction");
	}
	
	/**
	 * update 
	 **/
	void Update() 
	{
		// if user presses escape or 'back' button on android, exit program
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit ();
		
		// dispatch stuff on main thread
		while (ExecuteOnMainThread.Count > 0)
		{
			ExecuteOnMainThread.Dequeue().Invoke();	
		}
	}
	
	/**
	 * things to do whenever a level is loaded
	 * this class persists across loads
	 * */
	void OnLevelWasLoaded(int level)
	{
		Debug.Log ("Loaded level " + level);
		
		// set up light for this scene
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
		
		// disable any arrows
		GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_ARROW);
		foreach (GameObject go in gos)
		{
			go.SetActive(false); // disable
		}
	}
		
	#region finger events
	
	/**
	 * handle on drag begin events
	 **/
	void FingerGestures_OnDragBegin( Vector2 fingerPos, Vector2 startPos )
    {
         // pick object that was under our finger when we started dragging
	    this.draggedBody = PickRigidbody( startPos );
		Debug.Log("start drag " + (this.draggedBody == null ? "null" : this.draggedBody.name));
				
		// record drag start
		if (this.recordOn && this.draggedBody != null)
		{
			this.rap.RecordAction(this.draggedBody.name, "DragBegin",
				this.draggedBody.transform.position,
				System.DateTime.Now.ToLongTimeString()
				 + "." + System.DateTime.Now.Millisecond);	
		}
	}

	/**
	 * handle on drag move events
	 **/
	void FingerGestures_OnDragMove( Vector2 fingerPos, Vector2 delta )
	{
		// if the dragged object is a StoryCharacter, highlight it and drag
	    if( this.draggedBody != null && (this.draggedBody.tag.Contains(Constants.TAG_STORY_CHAR)))
	    {	
			if (this.allowTouch)
			{	
				// convert the finger position to world position
		        float distToCamera = Mathf.Abs( draggedBody.transform.position.z - 
					Camera.main.transform.position.z );
		        Vector3 fingerWorldPos = Camera.main.ScreenToWorldPoint( 
					new Vector3( fingerPos.x, fingerPos.y, distToCamera) );
				
				// check if on screen
				if (fingerWorldPos.x > Constants.RIGHT_SIDE)
					fingerWorldPos.x = Constants.RIGHT_SIDE;
				else if (fingerWorldPos.x < Constants.LEFT_SIDE)
					fingerWorldPos.x = Constants.LEFT_SIDE;
				if (fingerWorldPos.y > Constants.TOP_SIDE)
					fingerWorldPos.y = Constants.TOP_SIDE;
				else if (fingerWorldPos.y < Constants.BOTTOM_SIDE)
					fingerWorldPos.y = Constants.BOTTOM_SIDE;
		 
				
		        // center the object on our moving finger
		        this.draggedBody.transform.position = fingerWorldPos;
				// move highlighting light and set active
				LightOn (1, fingerWorldPos);
				// the "flipped" thing was an attempt to make objects always face the
				// direction that they are being dragged - never finished implementing
				// or debugging it; didn't work quite right initially.
				// But I left all the code here, commented out, in case I do want to
				// implement that sometime, as it might be a cool feature.
				//
				// and make sure object is facing correct direction
				/*if (!this.flipped) 
				{
					// need to add StoryCharacterProperties to objects if we use this
					Debug.Log("ADD STORYCHARACTERPROPERTIES TO OBJECTS");
					this.flipped = true;
					CheckDirectionX(this.draggedBody, delta.x); // NOT RELIABLE...
				}*/
			
				// record drag 
				if (this.recordOn)
				{
					this.rap.RecordAction(this.draggedBody.name, "DragMove",
						this.draggedBody.transform.position,
						System.DateTime.Now.ToLongTimeString()
						 + "." + System.DateTime.Now.Millisecond);	
				}
			} // end if allow touch
			
	    }		
	}
	
     /**
	 * handle on drag end events
	 **/
	void FingerGestures_OnDragEnd( Vector2 fingerPos )
	{
	    if( this.draggedBody)
	    {
			// record drag end
			if (this.recordOn && this.draggedBody != null)
			{
				this.rap.RecordAction(this.draggedBody.name, "DragEnd",
					this.draggedBody.transform.position,
					System.DateTime.Now.ToLongTimeString()
					 + "." + System.DateTime.Now.Millisecond);	
			}
				
	        // we are no longer dragging the object
	        this.draggedBody = null;
			//this.flipped = false;
			LightOff (1);
	    }
	}
	
	/**
	 * handle on two-finger drag begin events
	 **/
	void HandleFingerGesturesOnTwoFingerDragBegin (Vector2 fingerPos, Vector2 startPos)
	{
		// pick object that was under our finger when we started dragging
	    this.draggedBody = PickRigidbody( startPos );
		
		// if any other objects within a given radius, drag both
		this.draggedBodyTwo = PickNearby(startPos);
		
		Debug.Log("start two-finger drag " + 
			(this.draggedBody == null ? "null" : this.draggedBody.name) + " & "
			+ (this.draggedBodyTwo == null ? "null" : this.draggedBodyTwo.name));
		
		// record drag starts
		if (this.recordOn && this.draggedBody != null)
		{
			this.rap.RecordAction(this.draggedBody.name, "DragBegin",
				this.draggedBody.transform.position,
				System.DateTime.Now.ToLongTimeString()
				 + "." + System.DateTime.Now.Millisecond);	
		}
		
		// record drag start
		if (this.recordOn && this.draggedBodyTwo != null)
		{
			this.rap.RecordAction(this.draggedBodyTwo.name, "DragBegin",
				this.draggedBodyTwo.transform.position,
				System.DateTime.Now.ToLongTimeString()
				 + "." + System.DateTime.Now.Millisecond);	
		}
		
	}
	
	/**
	 * handle on two-finger drag move events
	 **/
	void HandleFingerGesturesOnTwoFingerDragMove (Vector2 fingerPos, Vector2 delta)
	{
		// if the dragged object is a StoryCharacter, drag
	    if( this.draggedBody != null && (this.draggedBody.tag.Contains(Constants.TAG_STORY_CHAR)))
	    {
			if (this.allowTouch)
			{
		        // move object by delta-drag (how much finger moved in that direction)
				this.draggedBody.transform.position = new Vector3(
					this.draggedBody.transform.position.x + delta.x,
					this.draggedBody.transform.position.y + delta.y,
					this.draggedBody.transform.position.z);
				// and make sure object is facing correct direction
				//CheckDirectionX(this.draggedBody, delta.x);
			
				// record drag 
				if (this.recordOn)
				{
					this.rap.RecordAction(this.draggedBody.name, "DragMove",
						this.draggedBody.transform.position,
						System.DateTime.Now.ToLongTimeString()
						 + "." + System.DateTime.Now.Millisecond);	
				}
			}
	    }	
		
		// if the dragged object is a StoryCharacter, drag
	    if( this.draggedBodyTwo != null && (this.draggedBodyTwo.tag.Contains(Constants.TAG_STORY_CHAR)))
	    {
			if (this.allowTouch)
			{
				// move object by delta-drag (how much finger moved in that direction)
				this.draggedBodyTwo.transform.position = new Vector3(
					this.draggedBodyTwo.transform.position.x + delta.x,
					this.draggedBodyTwo.transform.position.y + delta.y,
					this.draggedBodyTwo.transform.position.z);
				// and make sure object is facing correct direction
				//CheckDirectionX(this.draggedBodyTwo, delta.x);
			
				// record drag 
				if (this.recordOn)
				{
					this.rap.RecordAction(this.draggedBodyTwo.name, "DragMove",
						this.draggedBodyTwo.transform.position,
						System.DateTime.Now.ToLongTimeString()
						 + "." + System.DateTime.Now.Millisecond);	
				}
			}
	    }	
		
		if (this.allowTouch)
		{
			// move highlighting light and set active
			float distToCamera = Mathf.Abs( draggedBody.transform.position.z
				- Camera.main.transform.position.z );
		    Vector3 fingerWorldPos = Camera.main.ScreenToWorldPoint(
				new Vector3( fingerPos.x, fingerPos.y, distToCamera) );
			LightOn (1, fingerWorldPos);
		}
		
	}
	
	/**
	 * handle on two-finger drag end events
	 **/
	void HandleFingerGesturesOnTwoFingerDragEnd (Vector2 fingerPos)
	{
		 if( this.draggedBody || this.draggedBodyTwo)
	    {
			// record drag end
			if (this.recordOn && this.draggedBody != null)
			{
				this.rap.RecordAction(this.draggedBody.name, "DragEnd",
					this.draggedBody.transform.position,
					System.DateTime.Now.ToLongTimeString()
					 + "." + System.DateTime.Now.Millisecond);	
			}
			// record drag end
			if (this.recordOn && this.draggedBodyTwo != null)
			{
				this.rap.RecordAction(this.draggedBodyTwo.name, "DragEnd",
					this.draggedBodyTwo.transform.position,
					System.DateTime.Now.ToLongTimeString()
					 + "." + System.DateTime.Now.Millisecond);	
			}
			
			
	        // we are no longer dragging the objects
	        this.draggedBody = null;
			this.draggedBodyTwo = null;
			LightOff (1);
	    }
	}
	
	/**
	 * handle finger down events - pass to general highlight-on-touch function
	 **/
	void HandleFingerGesturesOnFingerDown (int fingerIndex, Vector2 fingerPos)
	{
		CheckTouch(fingerIndex, fingerPos, "FingerDown");
	}

	/**
     * handle finger taps  - pass to general highlight-on-touch function
	 **/
	void HandleFingerGesturesOnFingerTap (int fingerIndex, Vector2 fingerPos)
	{		
	 	CheckTouch(fingerIndex, fingerPos, "Tap");	
	}
	
	/**
	 * check if next button was pressed on touch
	 **/
	void CheckTouch (int fingerIndex, Vector2 fingerPos, string tag)
	{
		// pick object that was under our finger when we tapped
    	this.tappedBody = PickRigidbody( fingerPos );
		Debug.Log("touch " + (this.tappedBody == null ? "null" : this.tappedBody.name));
	
		// if the tapped object is a StoryCharacter, highlight it
		// highlight only one character at a time (radio button style)
		if (this.tappedBody != null && (this.tappedBody.tag.Contains(Constants.TAG_STORY_CHAR)
			|| this.tappedBody.tag.Contains(Constants.TAG_ARROW)))
		{
			Debug.Log ("highlighting " + this.tappedBody.name);
			// record tap
			if (this.recordOn)
			{
				this.rap.RecordAction(this.tappedBody.name, tag,
					this.tappedBody.transform.position,
					System.DateTime.Now.ToLongTimeString()
					 + "." + System.DateTime.Now.Millisecond);	
			}
			
			if (this.allowTouch)
			{
				// move highlighting light and set active
				LightOn (1, this.tappedBody.transform.position);
			}
		}
		
		// if the tapped object was the snowflake behind the sun
		if (this.tappedBody != null && this.tappedBody.tag.Contains(Constants.TAG_STORM_TRIGGER))
		{
			// record tap
			if (this.recordOn)
			{
				this.rap.RecordAction(this.tappedBody.name, tag,
					this.tappedBody.transform.position,
					System.DateTime.Now.ToLongTimeString()
					 + "." + System.DateTime.Now.Millisecond);	
			}
			if (this.allowTouch)
			{
				// make clouds appear & start snowing
				if (! this.snowing) StartCoroutine(CloudsAndSnow());
				else if (this.canstopsnow) StartCoroutine(StopSnow());
			}
		}
		
		// if it was the reset object for resetting
		// (i.e., going back to the "story select" scene or forward to the "continue" scene)
		if (this.tappedBody != null && this.tappedBody.tag.Contains(Constants.TAG_ARROW))
		{
			// record tap
			if (this.recordOn)
			{
				this.rap.RecordAction(this.tappedBody.name, tag,
					this.tappedBody.transform.position,
					System.DateTime.Now.ToLongTimeString()
					 + "." + System.DateTime.Now.Millisecond);	
			}
			if (this.allowTouch)
			{
				// touched the back arrow - go back to the beginning!
				ReturnToBeginning();
			}
		}
		// don't allow record trigger to be toggle - always record!
		//
		// uncomment if you want the record trigger to be active (but it didn't
		// always work right - sometimes one tap registered as two, so you'd trigger
		// record to go on and then off again from the same tap)
		//
		// if you don't want to record, you can set the "recordOn" flag to false
		//
		/*if (this.tappedBody != null && this.tappedBody.tag.Contains(Constants.TAG_RECORDER))
		{
			// toggle record
			this.recordOn = !this.recordOn;
			Debug.Log( " ------ " + (this.recordOn ? "now recording!" : "stopped recording") + " ------ ");
			
			// record tap
			if (this.recordOn)
			{
				this.rap.RecordAction(this.tappedBody.name, tag,
					this.tappedBody.transform.position,
					System.DateTime.Now.ToLongTimeString()
					 + "." + System.DateTime.Now.Millisecond);	
			}
		}*/
	}
	
	/**
	 * when dragging, check which direction on the x-axis the object is being dragged
	 * then change the object's rotation accordingly
	 * TODO - did not finish implementing/debugging this. But left this here because
	 * sometime it might be cool for objects to always face the direction they're being
	 * dragged...
	 * */
	void CheckDirectionX(GameObject go, float deltaX)
	{
		// for this to work, need to mark which direction everything starts in...
		// that's in StoryCharacterProperties
		// or make everything face the same direction (would need all sprite images
		// to be facing right or something - not hard, but a little time consuming)
		
		if (go != null)
		{
			
			// if dragging left, make sure object faces left
			if (deltaX < 0) // negative is moving left: face left
			{
				// then object.transform = transform.x, transform.y + 180, transform.z
				// move object by delta-drag (how much finger moved in that direction)
				go.transform.Rotate(0, 
					(go.GetComponent<StoryCharacterProperties>().facingRight ? 0 : 180),
					0);
			}
			// if dragging right, make sure object faces right
			else if (deltaX > 0) // positive is moving right: face right
			{
				go.transform.Rotate(0, 
					(go.GetComponent<StoryCharacterProperties>().facingRight ? -180 : 0),
					0);
			}
			// if deltaX is zero, not moving left or right, don't change rotation.	
		}
	}
	
	#endregion
	
	#region misc helper functions
	
	/**
	 * start snow
	 **/
	IEnumerator CloudsAndSnow()
	{
		this.snowing = true; // set snowing flag

		// find director
		GameObject director = GameObject.FindGameObjectWithTag(Constants.TAG_GAME_DIRECTOR);
		if (director != null)
		{
			director.GetComponent<CloudyBehavior>().enabled = true;
			director.GetComponent<CloudyBehavior>().CreateClouds(); // make clouds
			Debug.Log("making clouds");
			
			yield return new WaitForSeconds(director.GetComponent<CloudyBehavior>().cloudCount / 2f);

			director.GetComponent<Snowstorm>().enabled = true;
			director.GetComponent<Snowstorm>().StartSnowing(); // start snowing
			Debug.Log ("starting snow");
			
			yield return new WaitForSeconds(4f);
			
			this.canstopsnow = true; // can stop snow now that it's snowing
			Debug.Log("snowing ... can stop now");
		}
	}
	
	/**
	 * stop snow
	 * */
	IEnumerator StopSnow()
	{
		this.canstopsnow = false; // can only stop snow when it's snowing
		Debug.Log("stopping snow");
		
		// destroy snowflakes and clouds
		DestroyClonedObjectsByTag(new string[] { Constants.TAG_SNOWFLAKE, Constants.TAG_CLOUD });
		
		yield return new WaitForSeconds(1f);
		this.snowing = false; // set snowing flag
		Debug.Log("not snowing anymore!");
		
	}
		
	/**
	 * destroy objects with the specified tags
	 **/
	void DestroyClonedObjectsByTag(string[] tags)
	{
		// destroy objects with the specified tags
		foreach (string tag in tags)
		{
			GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
			foreach (GameObject go in objs)
			{
				if (go.name.Contains("Clone"))
				{
					Debug.Log ("destroying " + go.name);
					Destroy(go);
				}
			}
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
	
	/**
	 * find a nearby rigidbody within some radius
	 **/
	GameObject PickNearby( Vector2 screenPos )
	{
		Ray ray;
		RaycastHit hit;
		
		// check for nearby objects within dragRadius
		for (int x = (int) screenPos.x-dragRadius; x < (int) screenPos.x+dragRadius; x+=searchStep)
		{
			for (int y = (int) screenPos.y-dragRadius; y < (int) screenPos.y+dragRadius; y+=searchStep)
			{
				ray = Camera.main.ScreenPointToRay( new Vector2(x,y) );	
	    		if(!Physics.Raycast( ray, out hit))
					continue; // did not find object
				else
				{
					Debug.Log("found possible nearby: " + hit.collider.gameObject.name);
					if (!hit.collider.gameObject.name.Equals(this.draggedBody.name)
						&& hit.collider.gameObject.tag.Contains(Constants.TAG_STORY_CHAR))					
	        			return hit.collider.gameObject; // return found object
				}
			}
		}
		
		return null; // did not find any nearby objects
	}
	
	
	/**
	 * sets light object active in the specified position and with the specified scale
	 **/
	public void LightOn(Vector3 posn)
	{
		LightOn (1, posn);
	}
	public void LightOn(int scaleBy, Vector3 posn)
	{
		if (this.highlight != null)
		{
			this.highlight.SetActive(true);
			this.highlight.transform.position = new Vector3(posn.x,posn.y,posn.z+1);
			Vector3 sc = this.highlight.transform.localScale;
			sc.x *= scaleBy;
			this.highlight.transform.localScale = sc;
		}
		else
		{
			Debug.Log ("Tried to turn light on ... but light is null!");
		}
	}

	/**
	 * deactivates light, returns to specified scale
	 **/	
	public void LightOff ()
	{
		LightOff (1);
	}
	public void LightOff(int scaleBy)
	{
		if (this.highlight != null)
		{
			Vector3 sc = this.highlight.transform.localScale;
			sc.x /= scaleBy;
			this.highlight.transform.localScale = sc;
	
			this.highlight.SetActive(false); // turn light off
		}
		else
		{
			Debug.Log ("Tried to turn light off ... but light is null!");
		}
	}
	
		#endregion

	
	/**
	 * received command from remote operator - process and deal with message
	 * */
	void HandleClientSocketReceivedMsgEvent (object sender, String msg)
	{
		Debug.Log ("!! COMMAND received from remote operator: " + msg);
		
		// split the message into tokens with spaces as delimiter
		// FYI: split returns an empty string if the delimiter is found at 
		// the beg or end of the string being split, so we trim first
		string[] tokens = msg.Trim().Split();
		
		// process first token to determine which message type this is
        // if there is a second token, this is the message argument
		switch ( tokens[0] )
		{
			case Constants.PLAYBACK:
				// argument for playback command is the filename
				if (tokens.Length >=2)
				{
					StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
					() => {  StartCoroutine(this.rap.PlaybackFile(tokens[1])); });
				}
				else
				{
					Debug.Log ("ERROR [TCP Handle Message]: did not have playback token in message!");	
				}
				break;
				
			case Constants.SCENE_GO_BACK:
				this.GoBackAScene();
				break;
				
			case Constants.SCENE_ADVANCE:
				this.AdvanceScene();
				break;
			
			case Constants.SWITCH_TURN:
				// 	switch whose turn -- argument is "CHILD" or "ROBOT"
				// disable or enable touch events appropriately
				// if it's the child's turn, allow touch events; otherwise,
				// robot turn, disable touch events, only allow playback actions
				if (tokens.Length >=2)
				{	
					this.allowTouch = (tokens[1].Contains(Constants.CHILD_TURN) ? true : false);
					// and reset/reload the level
					this.ReloadScene();
				}
				else
				{
					Debug.Log ("ERROR [TCP Handle Message]: did not have CHILD/ROBOT token in message!");	
				}
				break;
			
			case Constants.DISABLE_TOUCH:
				// disable touch events from user (only allow playback actions)
				this.allowTouch = false; 
				break;
				
			case Constants.ENABLE_TOUCH:
				// enable touch events from user
				this.allowTouch = true;
				break;
				
			case Constants.RELOAD_LEVEL:
				// reload the current level
				// e.g., when the robot's turn starts, want all characters back in their
				// starting configuration for use with automatic playbacks
				this.ReloadScene();
				break;
				
			case Constants.PID:
				// argument for PID command is the current PID
				if (tokens.Length >=2)
				{
					// receive PID from teleop; use to set recording filename!
					this.rap.SetupRecordingFile(tokens[1] + "-tablet-"); // TODO
				}
				else
				{
					Debug.Log ("ERROR [TCP Handle Message]: did not have PID token in message!");	
				}
				break;
			case Constants.RETURN_TO_BEG:
				// load session selection scene
				//this.ReturnToSessionSelection(); //TODO if we try resetting to session select
				break;
			
		}
	}
	
	/**
	 * load initial scene - this function usually called after the 'back' arrow
	 * is touched by the user to go back a scene
	 **/
	void ReturnToBeginning()
	{
		Debug.Log(">> Loading scene");
		
		// load first scene for the selected session...
		// goes to wait scene right now - not the beginning?
		// at the moment, the 'return to session selection' function goes back to
		// the beginning instead - this just advances to the wait scene, and from
		// there, to whatever session scene is next
		// ... that's so the session stuff can be tested and used manually, without
		// a teleop remotely sending 'advance scene' commands - repurposed the back
		// arrow to be a 'continue' arrow.
		DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
		Application.LoadLevel (Constants.SCENE_WAIT);		
	}
	
	/**
	 * go to previous scene
	 * 
	 * */
	void AdvanceScene()
	{
		Constants.currentScene += 1; // forward a scene
		
		// and load that:
		// SR2
		if (!Constants.SR3 && // if SR2 study
			Constants.currentScene < Constants.SESSION_STORY_ORDERS[Constants.currentSession-1].Length)
		{
			// since we sometimes get here from *not* the main thread, such as after the 
			// teleop sends an "advance" command and the message is handled, we want to 
			// queue our advance scene action for execution on the main thread
			StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
			() => { DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
						Application.LoadLevel(Constants.SESSION_STORY_ORDERS[
					Constants.currentSession-1][Constants.currentScene]); });
		}
		else if (!Constants.SR3 && 
			Constants.currentScene >= Constants.SESSION_STORY_ORDERS[Constants.currentSession-1].Length)
		{
			// if we're at the end, go to the wait scene
			StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
			() => { DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
				Constants.currentScene = -1; // reset current scene counter
				Application.LoadLevel(Constants.SCENE_WAIT);  });
		}
		
		// SR3 - advance to a scene based on SR3 story orders
		else if (Constants.SR3 &&
			Constants.currentScene < Constants.SR3_STORY_ORDERS[Constants.currentSession-1].Length)
		{
			StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
			() => { DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
					Application.LoadLevel(Constants.SR3_STORY_ORDERS[
					Constants.currentSession-1][Constants.currentScene]); });
		}
	}
	
	/**
	 * go back a scene
	 * 
	 * */
	void GoBackAScene()
	{
		Constants.currentScene -= 1; // back a scene
		// and load that if it exists:
		// SR2
		if (!Constants.SR3 && // if SR2 study
			Constants.currentScene >= 0) //Constants.SESSION_STORY_ORDERS[Constants.currentSession-1].Length)
		{
			// since we sometimes get here from *not* the main thread, such as after the 
			// teleop sends an "go back" command and the message is handled, we want to 
			// queue our go back a scene action for execution on the main thread
			StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
			() => { DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
				Application.LoadLevel(Constants.SESSION_STORY_ORDERS[
					Constants.currentSession-1][Constants.currentScene]);  });
		}	
		else if (Constants.currentScene < 0)
		{
			// if we're at the beginning, go to the wait scene
			StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
			() => { DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
				Constants.currentScene = -1; // reset current scene counter
				Application.LoadLevel(Constants.SCENE_WAIT);  });
		}	
		
		//SR3
		else if (Constants.SR3 &&
			Constants.currentScene >= Constants.SR3_STORY_ORDERS[Constants.currentSession-1].Length)
		{
			StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
			() => { DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
				Application.LoadLevel(Constants.SR3_STORY_ORDERS[
				Constants.currentSession-1][Constants.currentScene]);  });
		}
	}
	
	/**
	 * reload scene
	 * 
	 * */
	void ReloadScene()
	{
		Debug.Log("Reloading current level...");
		// since we sometimes get here from *not* the main thread, such as after the 
		// teleop sends an "reload" command and the message is handled, we want to 
		// queue our reload scene action for execution on the main thread
		StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
		() => { DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
			Application.LoadLevel(Application.loadedLevel); });
	}
		
	/**
	 * return to session selection scene
	 * */
	void ReturnToSessionSelection()
	{
		Debug.Log("Returning to session selection");
		// since we sometimes get here from *not* the main thread, such as after the 
		// teleop sends an "return" command and the message is handled, we want to 
		// queue our return scene action for execution on the main thread
		StoryspaceMainInteraction.ExecuteOnMainThread.Enqueue(
		() => { 
			// session selection page has the persisting object, do something about it later
			DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Constants.TAG_PERSIST));
			Application.LoadLevel(Constants.SCENE_INIT); });
		
	}
}


