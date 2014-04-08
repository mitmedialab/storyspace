using UnityEngine;
using System.Collections;

/// <summary>
/// This class is used to create and initialise the proper FigureGesture implementation based on the current platform the application is being run on
/// </summary>
public class FingerGesturesInitializer : MonoBehaviour 
{
    /// <summary>
    /// Gestures to use when running from the Unity editor
    /// </summary>
	public FingerGestures editorGestures;

    /// <summary>
    /// Gestures to use when running as a standalone desktop application
    /// </summary>
	public FingerGestures desktopGestures;

    /// <summary>
    /// Gestures to use when running on an iOS device
    /// </summary>
	public FingerGestures iosGestures;

    /// <summary>
    /// Gestures to use when running on an Android device
    /// </summary>
	public FingerGestures androidGestures;

	/// <summary>
    /// Whether to keep the FingerGesture instance alive throughout scene loads
	/// </summary>
	public bool makePersistent = true;

	void Awake() 
	{
		if( !FingerGestures.Instance )
		{
			FingerGestures prefab;

			if( Application.isEditor )
			{
				prefab = editorGestures;
			}
			else
			{
#if UNITY_IPHONE
				prefab = iosGestures;
#elif UNITY_ANDROID
				prefab = androidGestures;
#else
				prefab = desktopGestures;
#endif
			}

			Debug.Log( "Creating FingerGestures using " + prefab.name );
			FingerGestures instance = Instantiate( prefab ) as FingerGestures;
			instance.name = prefab.name;
			
			if( makePersistent )
				DontDestroyOnLoad( instance.gameObject );
		}

		Destroy( this.gameObject );
	}
}
