using System.Collections;
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

        [TestMethod]
        public void YieldTest()
        {
            GameObject gobj = null;
            _state.InvokeIfRequired(() => gobj = _state.CreateNewGameObject());
            Helper.WaitUntil(() => gobj != null);
            TestYielder test = null;
            _state.InvokeIfRequired(() =>
            {
                test = gobj.AddComponent<TestYielder>();
            });
            Helper.WaitUntil(() => test != null);
            Assert.IsTrue(test.BeforeStuff);
            Assert.IsFalse(test.AfterStuff);
            Helper.WaitUntil(() => test.AfterStuff, 5);
        }

        class TestComponent : Component
        {
            private bool _awakeCalled;
            protected override void Awake()
            {
                _awakeCalled = true;
            }

            private bool _startCalled;
            protected override void Start()
            {
                Assert.IsTrue(_awakeCalled);
                _startCalled = true;
            }

            private bool _updateCalled;
            protected override void Update()
            {
                Assert.IsTrue(_startCalled);
                _updateCalled = true;
            }

            public bool LateUpdateCalled;
            protected override void LateUpdate()
            {
                Assert.IsTrue(_updateCalled);
                LateUpdateCalled = true;
                _updateCalled = false;
            }
        }

        class TestYielder : Component
        {
            protected override void Awake()
            {
                StartCoroutine(DoStuff());
            }

            private IEnumerator DoStuff()
            {
                BeforeStuff = true;
                yield return new WaitForSeconds(GameState, 1);
                AfterStuff = true;
            }

            public bool AfterStuff { get; set; }
            public bool BeforeStuff { get; set; }
        }
    }
}
