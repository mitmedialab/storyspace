using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// Defines an animation frame and associated data.
/// </summary>
public class tk2dSpriteAnimationFrame
{
	/// <summary>
	/// The sprite collection.
	/// </summary>
	public tk2dSpriteCollectionData spriteCollection;
	/// <summary>
	/// The sprite identifier.
	/// </summary>
	public int spriteId;
	
	/// <summary>
	/// When true will trigger an animation event when this frame is displayed
	/// </summary>
	public bool triggerEvent = false;
	/// <summary>
	/// Custom event data (string)
	/// </summary>
	public string eventInfo = "";
	/// <summary>
	/// Custom event data (int)
	/// </summary>
	public int eventInt = 0;
	/// <summary>
	/// Custom event data (float)
	/// </summary>
	public float eventFloat = 0.0f;
	
	public void CopyFrom(tk2dSpriteAnimationFrame source)
	{
		CopyFrom(source, true);
	}

	public void CopyTriggerFrom(tk2dSpriteAnimationFrame source)
	{
		triggerEvent = source.triggerEvent;
		eventInfo = source.eventInfo;
		eventInt = source.eventInt;
		eventFloat = source.eventFloat;		
	}

	public void ClearTrigger()
	{
		triggerEvent = false;
		eventInt = 0;
		eventFloat = 0;
		eventInfo = "";
	}
	
	public void CopyFrom(tk2dSpriteAnimationFrame source, bool full)
	{
		spriteCollection = source.spriteCollection;
		spriteId = source.spriteId;
		
		if (full) CopyTriggerFrom(source);
	}
}

[System.Serializable]
/// <summary>
/// Sprite Animation Clip contains a collection of frames and associated properties required to play it.
/// </summary>
public class tk2dSpriteAnimationClip
{
	/// <summary>
	/// Name of animation clip
	/// </summary>
	public string name = "Default";
	
	/// <summary>
	/// Array of frames
	/// </summary>
	public tk2dSpriteAnimationFrame[] frames = null;
	
	/// <summary>
	/// FPS of clip
	/// </summary>
	public float fps = 30.0f;
	
	/// <summary>
	/// Defines the start point of the loop when <see cref="WrapMode.LoopSection"/> is selected
	/// </summary>
	public int loopStart = 0;
	
	/// <summary>
	/// Wrap mode for the clip
	/// </summary>
	public enum WrapMode
	{
		/// <summary>
		/// Loop indefinitely
		/// </summary>
		Loop,
		
		/// <summary>
		/// Start from beginning, and loop a section defined by <see cref="tk2dSpriteAnimationClip.loopStart"/>
		/// </summary>
		LoopSection,
		
		/// <summary>
		/// Plays the clip once and stops at the last frame
		/// </summary>
		Once,
		
		/// <summary>
		/// Plays the clip once forward, and then once in reverse, repeating indefinitely
		/// </summary>
		PingPong,
		
		/// <summary>
		/// Simply choses a random frame and stops
		/// </summary>
		RandomFrame,
		
		/// <summary>
		/// Starts at a random frame and loops indefinitely from there. Useful for multiple animated sprites to start at a different phase.
		/// </summary>
		RandomLoop,
		
		/// <summary>
		/// Switches to the selected sprite and stops.
		/// </summary>
		Single
	};
	
	/// <summary>
	/// The wrap mode.
	/// </summary>
	public WrapMode wrapMode = WrapMode.Loop;

	public void CopyFrom(tk2dSpriteAnimationClip source)
	{
		name = source.name;
		if (source.frames == null) 
		{
			frames = null;
		}
		else
		{
			frames = new tk2dSpriteAnimationFrame[source.frames.Length];
			for (int i = 0; i < frames.Length; ++i)
			{
				if (source.frames[i] == null)
				{
					frames[i] = null;
				}
				else
				{
					frames[i] = new tk2dSpriteAnimationFrame();
					frames[i].CopyFrom(source.frames[i]);
				}
			}
		}
		fps = source.fps;
		loopStart = source.loopStart;
		wrapMode = source.wrapMode;
		if (wrapMode == tk2dSpriteAnimationClip.WrapMode.Single && frames.Length > 1)
		{
			frames = new tk2dSpriteAnimationFrame[] { frames[0] };
			Debug.LogError(string.Format("Clip: '{0}' Fixed up frames for WrapMode.Single", name));
		}
	}

	public void Clear()
	{
		name = "";
		frames = new tk2dSpriteAnimationFrame[0];
		fps = 30.0f;
		loopStart = 0;
		wrapMode = WrapMode.Loop;
	}

	public bool Empty
	{
		get { return name.Length == 0 || frames == null || frames.Length == 0; }
	}
}

[AddComponentMenu("2D Toolkit/Backend/tk2dSpriteAnimation")]
/// <summary>
/// Holds a collection of clips
/// </summary>
public class tk2dSpriteAnimation : MonoBehaviour 
{
	/// <summary>
	/// Array of <see cref="tk2dSpriteAnimationClip">clips</see>
	/// </summary>
	public tk2dSpriteAnimationClip[] clips;
	
	/// <summary>
	/// Resolves an animation clip by name and returns a unique id.
	/// </summary>
	/// <returns> Unique Animation Clip Id. </returns>
	/// <param name='name'>Case sensitive clip name, as defined in <see cref="tk2dSpriteAnimationClip"/>. </param>
	public int GetClipIdByName(string name)
	{
		for (int i = 0; i < clips.Length; ++i)
			if (clips[i].name == name) return i;
		return -1;
	}

	/// <summary>
	/// Resolves an animation clip by name and returns a reference to it
	/// </summary>
	/// <returns> tk2dSpriteAnimationClip reference, null if not found </returns>
	/// <param name='name'>Case sensitive clip name, as defined in <see cref="tk2dSpriteAnimationClip"/>. </param>
	public tk2dSpriteAnimationClip GetClipByName(string name)
	{
		for (int i = 0; i < clips.Length; ++i)
			if (clips[i].name == name) return clips[i];
		return null;
	}	
}
