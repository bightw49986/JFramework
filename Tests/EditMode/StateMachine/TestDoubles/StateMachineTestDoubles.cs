using System;
using System.Collections.Generic;
using JFramework.StateMachine;

namespace JFramework.Tests.StateMachine
{
    // Exposes protected CurrentState for test assertions
    class TestableStateMachine<TState> : SimpleStateMachine<TState> where TState : class, IState
    {
        public IState PublicCurrentState => CurrentState;

        public TestableStateMachine() : base() { }

        public TestableStateMachine(Type initialStateType, object initialArg = null)
            : base(initialStateType, initialArg) { }
    }

    // Context-aware testable state machine
    class TestableContextMachine : SimpleStateMachine<State<string>, string>
    {
        public IState PublicCurrentState => CurrentState;

        public TestableContextMachine(string context) : base(context) { }
    }

    // Records lifecycle call data and populates a shared log for ordering assertions
    class RecordingState : State
    {
        public static readonly List<string> CallLog = new List<string>();
        public static void ResetLog() => CallLog.Clear();

        public bool InitCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool TerminateCalled { get; private set; }
        public object LastInitArg { get; private set; }
        public object LastUpdateArg { get; private set; }

        protected override void OnInit(object arg)
        {
            InitCalled = true;
            LastInitArg = arg;
            CallLog.Add($"{GetType().Name}.Init");
        }

        protected override void OnUpdate(object arg)
        {
            UpdateCalled = true;
            LastUpdateArg = arg;
            CallLog.Add($"{GetType().Name}.Update");
        }

        protected override void OnTerminated(object arg)
        {
            TerminateCalled = true;
            CallLog.Add($"{GetType().Name}.Terminate");
        }
    }

    class StateA : RecordingState { }
    class StateB : RecordingState { }

    // Denies every outgoing transition
    class BlockingState : RecordingState
    {
        public override bool CanChangeTo(Type targetStateType) => false;
    }

    // Only permits transition to StateB (or null)
    class SelectiveState : RecordingState
    {
        public override bool CanChangeTo(Type targetStateType)
            => targetStateType == null || typeof(StateB).IsAssignableFrom(targetStateType);
    }

    // Carries a string context; exposes it for assertions
    class ContextAwareState : State<string>
    {
        public string ReceivedContext => runnerContext;
    }
}
