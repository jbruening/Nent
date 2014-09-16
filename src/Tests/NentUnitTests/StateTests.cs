using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nent;

namespace NentUnitTests
{
    [TestClass]
    public class StateTests
    {
        private GameState _state;

        [TestInitialize]
        public void TestStartup()
        {
            Debug.logger = new TestLogger();
            _state = new GameState();
            _state.StartOnOtherThread();
        }
        [TestCleanup]
        public void TestCleanup()
        {
            _state.Stop();
        }

        [TestMethod]
        public void CreateTest()
        {
            GameObject gobj = null;
            _state.InvokeIfRequired(() => gobj = _state.CreateNewGameObject());
            Helper.WaitUntil(() => gobj != null);
            TestComponent test = null;
            _state.InvokeIfRequired(() =>
            {
                test = gobj.AddComponent<TestComponent>();
            });

            Helper.WaitUntil(() => test != null);
            Helper.WaitUntil(() => test.LateUpdateCalled);
        }

        class TestComponent : Component
        {
            private bool _awakeCalled = false;
            protected override void Awake()
            {
                _awakeCalled = true;
            }

            private bool _startCalled = false;
            protected override void Start()
            {
                Assert.IsTrue(_awakeCalled);
                _startCalled = true;
            }

            private bool _updateCalled = false;
            protected override void Update()
            {
                Assert.IsTrue(_startCalled);
                _updateCalled = true;
            }

            public bool LateUpdateCalled = false;
            protected override void LateUpdate()
            {
                Assert.IsTrue(_updateCalled);
                LateUpdateCalled = true;
                _updateCalled = false;
            }
        }
    }
}
