using UnityEngine;
using System.Collections;


/**
 * GrowShrinkBehavior
 * 
 * the object this behavior is attached to will "pulse" its size, enlarging
 * a little and shrinking back to the original size
 * 
 **/
public class GrowShrinkBehavior : MonoBehaviour
{
	
	// scale object down by this much:
	public Vector3 scaleDownBy = new Vector3(1/1.1f,1/1.1f,1/1.1f); 
	// scale object up by this much:
	public Vector3 scaleUpBy = new Vector3(1.1f, 1.1f, 1.1f); 
	private bool scaleDown = false; // false = scale up instead
	public float scaleTime = 0.8f; // time to complete single scaling animation
	public float repeatRate = 0.9f; // time between scaling up, down
	public float timeBeforeStart = 2f; // wait 2s before starting to invoke growshrink()
	
	// Start
	void Start () 
	{
		this.timeBeforeStart = Random.Range(1f,2f);
		
		InvokeRepeating("GrowShrink", this.timeBeforeStart, this.repeatRate);
	}
	
	// On Enable
	void OnEnable() 
	{
	}
	
	void OnDisable()
	{	
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	
	// make the object pulse larger and smaller
	public void GrowShrink() 
	{
		if (gameObject.GetComponent<GrowShrinkBehavior>().enabled
			|| this.scaleDown) // stop after scaling to normal
		{		
			iTween.ScaleBy(gameObject, iTween.Hash("amount", (this.scaleDown ? 
				this.scaleDownBy : this.scaleUpBy), "time", this.scaleTime, 
				"easetype", "easeInOutSine"));
			
			this.scaleDown = !this.scaleDown;
		}
		
	}
	
	/**
	 * scale up once
	 **/
	public void ScaleUpOnce(float time)
	{
		if (!gameObject.GetComponent<GrowShrinkBehavior>().enabled)
		{
			iTween.ScaleBy(gameObject, iTween.Hash("amount", this.scaleUpBy,
				"time", time, "easetype", "easeInOutSine"));
		}
	}
	
	/**
	 * scale down once
	 **/
	public void ScaleDownOnce(float time)
	{
		if (!gameObject.GetComponent<GrowShrinkBehavior>().enabled)
		{
			iTween.ScaleBy(gameObject, iTween.Hash("amount", this.scaleDownBy,
				"time", time, "easetype", "easeInOutSine"));
		}
	}
	
	
}

