using UnityEngine;
using System.Collections;

public class FallingBehavior : MonoBehaviour {
	
	public float speed = -50;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float translation = Time.deltaTime * speed;
		
        transform.Translate(0, translation, 0);
		
		// if we reach the bottom of the screen
		if (transform.position.y < -430) 
			randomizeFalling();
	}
	
	void randomizeFalling()
	{
		// randomize starting position
		transform.position = new Vector3(Random.Range (-630,630), Random.Range(400,450), transform.position.z);
		
		// randomize speed
		speed = Random.Range (-50,-200);
	}
}
