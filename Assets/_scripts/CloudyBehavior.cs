using UnityEngine;
using System.Collections;

public class CloudyBehavior : MonoBehaviour {
	public int cloudCount = 12;
	public Transform cloudPrefab;
	
	private int r1 = -640;
	private int r2 = -320;
	private int r3 = 0;
	private int r4 = 320;
	private int r5 = 640;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void CreateClouds()
	{
		StartCoroutine(CreateCloudsHelper());
	}
	
	IEnumerator CreateCloudsHelper()
	{
			
			for (int i=0; i < cloudCount; i++) 
			{
				Transform t;
				
				if (i < cloudCount / 4)
				{
					t = (Transform)Instantiate(cloudPrefab, 
						new Vector3(Random.Range(r1,r2), Random.Range(290, 380), -6), 
						Quaternion.identity);
				}
				else if (i < cloudCount * 2/4)
				{
					t = (Transform)Instantiate(cloudPrefab, 
						new Vector3(Random.Range(r2,r3), Random.Range(300, 370), -6), 
						Quaternion.identity);
				}
				else if (i < cloudCount * 3/4)
				{
					t = (Transform)Instantiate(cloudPrefab, 
						new Vector3(Random.Range(r3,r4), Random.Range(300, 370), -6), 
						Quaternion.identity);
				}
				else
				{
					t = (Transform)Instantiate(cloudPrefab, 
						new Vector3(Random.Range(r4,r5), Random.Range(300, 370), -6), 
						Quaternion.identity);
				}
				
				
				float s = Random.Range(0.6f, 1.2f);
				t.localScale = new Vector3(s,s,s);
				
				yield return new WaitForSeconds(Random.Range (.2f,.8f));
			}
	}
}

