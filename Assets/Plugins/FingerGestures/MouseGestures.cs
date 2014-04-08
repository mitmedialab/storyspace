using UnityEngine;
using System.Collections;

/// <summary>
/// The desktop implementation of FingerGestures, using mouse device input
/// </summary>
public class MouseGestures : FingerGestures
{
    // Number of mouse buttons to track
    public int maxMouseButtons = 3;

    protected override void Start()
    {
        base.Start();
    }

    public override int MaxFingers
    {
        get { return maxMouseButtons; }
    }

    protected override FingerGestures.FingerPhase GetPhase( Finger finger )
    {
        int button = finger.Index;

        // did we just press it?
        if( Input.GetMouseButtonDown( button ) )
            return FingerPhase.Began;

        // mouse button down?
        if( Input.GetMouseButton( button ) )
        {
            // find out if the mouse has moved since last update
            Vector3 delta = GetPosition( finger ) - finger.Position;

            if( delta.sqrMagnitude < 1.0f )
                return FingerPhase.Stationary;

            return FingerPhase.Moved;
        }

        // did we just release the button?
        if( Input.GetMouseButtonUp( button ) )
            return FingerPhase.Ended;
        
        return FingerPhase.None;
    }

    protected override Vector2 GetPosition( Finger finger )
    {
        return Input.mousePosition;
    }
}
