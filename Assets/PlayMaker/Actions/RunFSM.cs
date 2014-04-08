// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.StateMachine)]
    [Tooltip("Creates an FSM from a saved FSM Template.")]
    public class RunFSM : FsmStateAction
    {
        public FsmTemplateControl fsmTemplateControl = new FsmTemplateControl();

        [UIHint(UIHint.Variable)]
        public FsmInt storeID;

        [Tooltip("Event to send when the FSM has finished (usually because it ran a Finish FSM action).")]
        public FsmEvent finishEvent;

        private Fsm runFsm;

        public override void Reset()
        {
            fsmTemplateControl = new FsmTemplateControl();
            storeID = null;
            runFsm = null;
        }

        /// <summary>
        /// Initialize FSM on awake so it doesn't cause hitches later
        /// </summary>
        public override void Awake()
        {
            if (fsmTemplateControl.fsmTemplate != null)
            {
                runFsm = Fsm.CreateSubFsm(fsmTemplateControl);
            }
        }

        /// <summary>
        /// Forward global events to the sub FSM
        /// </summary>
        public override bool Event(FsmEvent fsmEvent)
        {
            if (runFsm != null && fsmEvent.IsGlobal)
            {
                runFsm.Event(fsmEvent);
            }

            return false;
        }

        /// <summary>
        /// Start the FSM on entering the state
        /// </summary>
        public override void OnEnter()
        {
            if (runFsm == null)
            {
                Finish();
                return;
            }

            runFsm.OnEnable();
            runFsm.Start();

            storeID.Value = fsmTemplateControl.ID;

            CheckIfFinished();
        }

        public override void OnUpdate()
        {
            if (runFsm != null)
            {
                runFsm.Update();                
                CheckIfFinished();
            }
            else
            {
                Finish();
            }
        }

        public override void OnFixedUpdate()
        {
            if (runFsm != null)
            {
                runFsm.FixedUpdate();
                CheckIfFinished();
            }
            else
            {
                Finish();
            }
        }

        public override void OnLateUpdate()
        {
            if (runFsm != null)
            {
                runFsm.LateUpdate();
                CheckIfFinished();
            }
            else
            {
                Finish();
            }
        }

        // TODO: forward all the other monobehaviour events
        // TODO: don't want to lose the collision, trigger, and mouse event optimizations for the sub fsm!

        /// <summary>
        /// Stop the FSM on exiting the state
        /// </summary>
        public override void OnExit()
        {
            if (runFsm != null)
            {
                runFsm.Stop();
            }
        }

        void CheckIfFinished()
        {
            if (runFsm == null || runFsm.Finished)
            {
                Fsm.Event(finishEvent);
                Finish();
            }
        }


    }
}
