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
        public void ChildTest()
        {
            GameObject gobj = null;
            _state.InvokeIfRequired(() => gobj = _state.CreateNewGameObject());
            Helper.WaitUntil(() => gobj != null);
            ChildTestComponent test = null;
            _state.InvokeIfRequired(() =>
            {
                test = gobj.AddComponent<ChildTestComponent>();
            });

            Helper.WaitUntil(() => test != null);
            Helper.WaitUntil(() => test.ChildUpdateCalled);
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

        [TestMethod]
        public void DestroyTest()
        {
            GameObject gobj = null;
            _state.InvokeIfRequired(() => gobj = _state.CreateNewGameObject());
            Helper.WaitUntil(() => gobj != null);
            
            var test = Helper.AddInvoke<TestComponent>(gobj);

            _state.InvokeIfRequired(() => gobj.Destroy());
            Helper.WaitUntil(() => gobj.IsDisposed);
            test.LateUpdateCalled = false;
            Helper.Ensure(() => !test.LateUpdateCalled);
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

            protected bool UpdateCalled;
            protected override void Update()
            {
                Assert.IsTrue(_startCalled);
                UpdateCalled = true;
            }

            public bool LateUpdateCalled;
            protected override void LateUpdate()
            {
                Assert.IsTrue(UpdateCalled);
                LateUpdateCalled = true;
                UpdateCalled = false;
            }
        }

        class ChildTestComponent : TestComponent
        {
            public bool ChildUpdateCalled;
            protected override void Update()
            {
                base.Update();
                Assert.IsTrue(UpdateCalled);
                ChildUpdateCalled = true;
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
