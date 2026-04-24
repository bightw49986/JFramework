using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using JFramework.StateMachine;

namespace JFramework.Tests.StateMachine
{
    [TestFixture]
    public class SimpleStateMachineTests
    {
        private TestableStateMachine<RecordingState> sm;

        [SetUp]
        public void SetUp()
        {
            sm = new TestableStateMachine<RecordingState>();
            RecordingState.ResetLog();
        }

        [TearDown]
        public void TearDown()
        {
            sm.Dispose();
        }

        // ─── 建構 ───────────────────────────────────────────────────────────────

        [Test]
        public void Constructor_Default_CurrentStateIsNull()
        {
            Assert.IsNull(sm.PublicCurrentState);
        }

        [Test]
        public void Constructor_WithInitialType_ImmediatelyChangesToThatState()
        {
            using var machine = new TestableStateMachine<RecordingState>(typeof(StateA));

            Assert.IsInstanceOf<StateA>(machine.PublicCurrentState);
        }

        [Test]
        public void Constructor_WithInitialArg_ArgIsPassedToInitialize()
        {
            var expectedArg = new object();
            using var machine = new TestableStateMachine<RecordingState>(typeof(StateA), expectedArg);

            Assert.AreEqual(expectedArg, ((StateA)machine.PublicCurrentState).LastInitArg);
        }

        // ─── SetNextState ─────────────────────────────────────────

        [Test]
        public void SetNextState_Immediate_CurrentStateBecomesTargetType()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            Assert.IsInstanceOf<StateA>(sm.PublicCurrentState);
        }

        [Test]
        public void SetNextState_Immediate_ReturnsTrue()
        {
            bool result = sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            Assert.IsTrue(result);
        }

        [Test]
        public void SetNextState_Immediate_InitializeCalledOnNewState()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            Assert.IsTrue(((StateA)sm.PublicCurrentState).InitCalled);
        }

        [Test]
        public void SetNextState_Immediate_TerminatesOldState()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            var stateA = (StateA)sm.PublicCurrentState;

            sm.SetNextState<StateB>(deferToNextStateUpdate: false);

            Assert.IsTrue(stateA.TerminateCalled);
        }

        [Test]
        public void SetNextState_Immediate_TerminateCalledBeforeInitialize()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            RecordingState.ResetLog();

            sm.SetNextState<StateB>(deferToNextStateUpdate: false);

            // Expected: ["StateA.Terminate", "StateB.Init"]
            Assert.AreEqual("StateA.Terminate", RecordingState.CallLog[0]);
            Assert.AreEqual("StateB.Init",      RecordingState.CallLog[1]);
        }

        [Test]
        public void SetNextState_WithArg_ArgPassedToInitialize()
        {
            var expectedArg = new object();

            sm.SetNextState<StateA>(arg: expectedArg, deferToNextStateUpdate: false);

            Assert.AreEqual(expectedArg, ((StateA)sm.PublicCurrentState).LastInitArg);
        }

        [Test]
        public void SetNextState_ToNullType_CurrentStateBecomesNull()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            sm.SetNextState(type: null, deferToNextStateUpdate: false);

            Assert.IsNull(sm.PublicCurrentState);
        }

        [Test]
        public void SetNextState_ToNullType_OldStateTerminated()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            var stateA = (StateA)sm.PublicCurrentState;

            sm.SetNextState(type: null, deferToNextStateUpdate: false);

            Assert.IsTrue(stateA.TerminateCalled);
        }

        [Test]
        public void SetNextState_WrongType_ThrowsStateMachineException()
        {
            // int 不是 RecordingState 的子類別
            Assert.Throws<StateMachineException>(() =>
                sm.SetNextState(typeof(int), deferToNextStateUpdate: false));
        }

        // ─── SetNextState（延遲轉換，預設行為）──────────────────────────────────

        [Test]
        public void SetNextState_Deferred_DoesNotChangeStateBeforeUpdate()
        {
            sm.SetNextState<StateA>(); // deferToNextStateUpdate: true（預設）

            Assert.IsNull(sm.PublicCurrentState);
        }

        [Test]
        public void SetNextState_Deferred_ChangesStateOnNextUpdate()
        {
            sm.SetNextState<StateA>();

            sm.UpdateCurrentState();

            Assert.IsInstanceOf<StateA>(sm.PublicCurrentState);
        }

        // ─── UpdateCurrentState ─────────────────────────────────────────────────

        [Test]
        public void UpdateCurrentState_NoPendingChange_CallsUpdateOnCurrentState()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            var stateA = (StateA)sm.PublicCurrentState;

            sm.UpdateCurrentState();

            Assert.IsTrue(stateA.UpdateCalled);
        }

        [Test]
        public void UpdateCurrentState_NoPendingChange_ArgPassedToUpdate()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            var stateA = (StateA)sm.PublicCurrentState;
            var expectedArg = new object();

            sm.UpdateCurrentState(expectedArg);

            Assert.AreEqual(expectedArg, stateA.LastUpdateArg);
        }

        // ─── 狀態拒絕轉換（CanChangeTo）──────────────────────────────────────────

        [Test]
        public void SetNextState_WhenCurrentStateDenies_ReturnsFalse()
        {
            sm.SetNextState<BlockingState>(deferToNextStateUpdate: false);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("SetNextState\\(\\): failed"));
            bool result = sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            Assert.IsFalse(result);
        }

        [Test]
        public void SetNextState_WhenCurrentStateDenies_StateNotChanged()
        {
            sm.SetNextState<BlockingState>(deferToNextStateUpdate: false);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("SetNextState\\(\\): failed"));
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            Assert.IsInstanceOf<BlockingState>(sm.PublicCurrentState);
        }

        [Test]
        public void SetNextState_WhenSelectiveStateAllowsTarget_Succeeds()
        {
            sm.SetNextState<SelectiveState>(deferToNextStateUpdate: false);

            bool result = sm.SetNextState<StateB>(deferToNextStateUpdate: false);

            Assert.IsTrue(result);
            Assert.IsInstanceOf<StateB>(sm.PublicCurrentState);
        }

        [Test]
        public void SetNextState_WhenSelectiveStateDeniesTarget_ReturnsFalse()
        {
            sm.SetNextState<SelectiveState>(deferToNextStateUpdate: false);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("SetNextState\\(\\): failed"));
            bool result = sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            Assert.IsFalse(result);
        }

        // ─── Lock / Unlock ───────────────────────────────────────────────────────

        [Test]
        public void Lock_WhenUnlocked_ReturnsTrue()
        {
            bool result = sm.Lock(out _);

            Assert.IsTrue(result);
        }

        [Test]
        public void Lock_WhenAlreadyLocked_ReturnsFalse()
        {
            sm.Lock(out _);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Lock\\(\\): failed"));
            bool result = sm.Lock(out _);

            Assert.IsFalse(result);
        }

        [Test]
        public void SetNextState_WhenLocked_ReturnsFalse()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            sm.Lock(out _);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("SetNextState\\(\\): failed"));
            bool result = sm.SetNextState<StateB>(deferToNextStateUpdate: false);

            Assert.IsFalse(result);
        }

        [Test]
        public void SetNextState_WhenLocked_StateNotChanged()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            sm.Lock(out _);

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("SetNextState\\(\\): failed"));
            sm.SetNextState<StateB>(deferToNextStateUpdate: false);

            Assert.IsInstanceOf<StateA>(sm.PublicCurrentState);
        }

        [Test]
        public void UnLock_WithCorrectToken_ReturnsTrue()
        {
            sm.Lock(out var token);

            bool result = sm.UnLock(token);

            Assert.IsTrue(result);
        }

        [Test]
        public void UnLock_WithCorrectToken_AllowsSubsequentStateChange()
        {
            sm.Lock(out var token);
            sm.UnLock(token);

            bool result = sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            Assert.IsTrue(result);
        }

        [Test]
        public void UnLock_WithWrongToken_ReturnsFalse()
        {
            sm.Lock(out _);
            var wrongToken = StateMachineLockToken.Create();

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("UnLock\\(\\): failed"));
            bool result = sm.UnLock(wrongToken);

            Assert.IsFalse(result);
        }

        [Test]
        public void UnLock_WhenNotLocked_ReturnsFalse()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("UnLock\\(\\): failed"));
            bool result = sm.UnLock(StateMachineLockToken.Create());

            Assert.IsFalse(result);
        }

        // ─── Dispose ────────────────────────────────────────────────────────────

        [Test]
        public void Dispose_TerminatesCurrentState()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            var stateA = (StateA)sm.PublicCurrentState;

            sm.Dispose();

            Assert.IsTrue(stateA.TerminateCalled);
        }

        [Test]
        public void Dispose_SetsCurrentStateToNull()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);

            sm.Dispose();

            Assert.IsNull(sm.PublicCurrentState);
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            sm.SetNextState<StateA>(deferToNextStateUpdate: false);
            sm.Dispose();

            Assert.DoesNotThrow(() => sm.Dispose());
        }

        // ─── SimpleStateMachine<TState, TContext> ────────────────────────────────

        [Test]
        public void StateMachineWithContext_OnStateCreated_SetsContextOnState()
        {
            const string expectedContext = "hello-context";
            using var machine = new TestableContextMachine(expectedContext);

            machine.SetNextState(typeof(ContextAwareState), deferToNextStateUpdate: false);

            var state = (ContextAwareState)machine.PublicCurrentState;
            Assert.AreEqual(expectedContext, state.ReceivedContext);
        }
    }
}
