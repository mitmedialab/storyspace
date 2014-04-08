using UnityEngine;
using System.Collections;

public class GameDirector : MonoBehaviour {
	
	public int foodCount = 15;
	public Transform foodPrefab;
	
	// Use this for initialization
	void Start () {
		for (int i = 0; i < foodCount; i++) {
			Transform t = (Transform)Instantiate(foodPrefab);
			t.position = new Vector3(Random.Range (-500,500), Random.Range(-300,300), t.position.z);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
