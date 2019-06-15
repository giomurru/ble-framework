using UnityEngine;
using System.Collections;

public class UserInterfaceBehaviour : MonoBehaviour {

	public GameObject activityIndicator;
	private GameObject activityIndicatorInstance;
	
	void OnEnable () {
		AppControllerBehaviour.ShowActivityIndicatorEvent += HandleShowActivityIndicatorEvent;
		AppControllerBehaviour.DestroyActivityIndicatorEvent += HandleDestroyActivityIndicatorEvent;
	}

	void HandleShowActivityIndicatorEvent ()
	{
		ShowActivityIndicator(transform);
	}
	
	void HandleDestroyActivityIndicatorEvent ()
	{
		if (activityIndicatorInstance!=null)
		{
			Destroy(activityIndicatorInstance); 
		}
	}
	
	void OnDisable () 
	{
		AppControllerBehaviour.ShowActivityIndicatorEvent -= HandleShowActivityIndicatorEvent;
		AppControllerBehaviour.DestroyActivityIndicatorEvent -= HandleDestroyActivityIndicatorEvent;
	}
	
	void ShowActivityIndicator(Transform t)
	{
		if (activityIndicatorInstance!=null)
		{
			Destroy(activityIndicatorInstance);
		}
		
		activityIndicatorInstance = Instantiate(activityIndicator, new Vector3 (0,0,0), Quaternion.identity) as GameObject; 
		activityIndicatorInstance.transform.SetParent(transform, false);	
	}
}
