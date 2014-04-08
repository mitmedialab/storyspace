using UnityEngine;
using System.Collections;

public class Snowstorm : MonoBehaviour {
	public int flakeCount = 40;
	public Transform snowflakePrefab;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void StartSnowing()
	{
			for (int i=0; i < flakeCount; i++) 
			{
				Transform t = (Transform)Instantiate(snowflakePrefab, new Vector3(Random.Range(-620,620), 
					Random.Range(400, 500), -5), Quaternion.identity);
				float s = Random.Range(0.3f, 1.3f);
				t.localScale = new Vector3(s,s,s);
				
				FallingBehavior fallingBehavior = t.gameObject.GetComponent<FallingBehavior>();
				fallingBehavior.speed = Random.Range (-50,-200);
				fallingBehavior.enabled = true;
			}
	}
}
