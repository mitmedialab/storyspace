using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for multi-finger gestures that require individual access to each finger position (as opposed to averaged position)
/// </summary>
public abstract class MultiFingerGestureRecognizer : GestureRecognizer
{
    Vector2[] pos;
    Vector2[] startPos;
    
    /// <summary>
    /// Initial finger positions (one per active finger/touch)
    /// </summary>
    protected Vector2[] StartPosition
    {
        get { return startPos; }
        set { startPos = value; }
    }

    /// <summary>
    /// Current finger positions (one per active finger/touch)
    /// </summary>
    protected Vector2[] Position
    {
        get { return pos; }
        set { pos = value; }
    }

    protected override void Start()
    {
        base.Start();
        OnFingerCountChanged( GetRequiredFingerCount() );
    }

    protected void OnFingerCountChanged( int fingerCount )
    {
        StartPosition = new Vector2[fingerCount];
        Position = new Vector2[fingerCount];
    }

    /// <summary>
    /// Number of touches used by the gesture
    /// </summary>
    public int RequiredFingerCount
    {
        get { return GetRequiredFingerCount(); }
    }

    /// <summary>
    /// Get the position of the finger at the given index
    /// </summary>
    /// <param name="index">index of the finger to retrieve</param>
    public Vector2 GetPosition( int index )
    {
        return pos[index];
    }

    /// <summary>
    /// Get the initial position of the finger at the given index
    /// </summary>
    /// <param name="index">index of the finger to retrieve</param>
    public Vector2 GetStartPosition( int index )
    {
        return startPos[index];
    }
}
