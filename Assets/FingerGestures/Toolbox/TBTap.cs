using UnityEngine;
using System.Collections;

/// <summary>
/// ToolBox Tap Component
/// Put this script on any 3D GameObject to detect when they are tapped
/// </summary>
[AddComponentMenu( "FingerGestures/Toolbox/Tap" )]
public class TBTap : TBComponent
{
    public enum TapMode
    {
        SingleTap,
        DoubleTap
    }

    // number of taps required to raise the event
    public TapMode tapMode = TapMode.SingleTap;
    
    public Message message = new Message( "OnTap" );
    public event EventHandler<TBTap> OnTap;

    public bool RaiseTap( int fingerIndex, Vector2 fingerPos )
    {
        FingerIndex = fingerIndex;
        FingerPos = fingerPos;

        if( OnTap != null )
            OnTap( this );

        Send( message );
        return true;
    }
}
