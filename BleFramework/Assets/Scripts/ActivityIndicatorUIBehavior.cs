using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActivityIndicatorUIBehavior : MonoBehaviour {

	public Sprite[] animationSprites;
	
	int currentSprite;
	float timeBetweenUpdates;
	// Use this for initialization
	void Start () {
		currentSprite = 0;
		timeBetweenUpdates = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		timeBetweenUpdates += Time.deltaTime;
		if ((timeBetweenUpdates) >= 1.0f/15.0f)
		{
			timeBetweenUpdates = 0.0f;
			transform.GetComponent<Image>().sprite = animationSprites[currentSprite];
			currentSprite++;
			
			if (currentSprite >= animationSprites.Length) currentSprite = 0;
		}
	}
}
