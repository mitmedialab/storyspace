using UnityEngine;
using System.Collections;

/// <summary>
/// Tap gesture: single press and release gestures at the same location
/// </summary>
[AddComponentMenu( "FingerGestures/Gesture Recognizers/Tap" )]
public class TapGestureRecognizer : AveragedGestureRecognizer
{
    /// <summary>
    /// Event fired when a tap occurs (if RequiredTaps is 0) or when the exact number of RequiredTaps has been reached
    /// </summary>
    public event EventDelegate<TapGestureRecognizer> OnTap;

    /// <summary>
    /// How far the finger can move from its initial position without making the gesture fail
    /// </summary>
    public float MoveTolerance = 5.0f;

    /// <summary>
    /// Maximum amount of time the fingers can be held down without failing the gesture. Set to 0 for infinite duration.
    /// </summary>
    public float MaxDuration = 0;

    protected override void OnBegin( FingerGestures.IFingerList touches )
    {
        Position = touches.GetAveragePosition();
        StartPosition = Position;
    }

    protected override GestureState OnActive( FingerGestures.IFingerList touches )
    {
        if( touches.Count != RequiredFingerCount )
        {
            // all fingers lifted - fire the tap event
            if( touches.Count == 0 )
            {
                RaiseOnTap();
                return GestureState.Recognized;
            }

            // either lifted off some fingers or added some new ones
            return GestureState.Failed;
        }

        // check if the gesture timed out
        if( MaxDuration > 0 && ElapsedTime > MaxDuration )
            return GestureState.Failed;
        
        // check if finger moved too far from start position
        float sqrDist = Vector3.SqrMagnitude( touches.GetAveragePosition() - StartPosition );
        if( sqrDist >= MoveTolerance * MoveTolerance )
            return GestureState.Failed;
        
        return GestureState.InProgress;
    }

    protected void RaiseOnTap()
    {
        if( OnTap != null )
            OnTap( this );
    }
}
