using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nent;
using NentUnitTests.Components;

namespace NentUnitTests
{
    [TestClass]
    public class StateTests
    {
        private GameState _state;
        private GameObject _gobj;

        [TestInitialize]
        public void TestStartup()
        {
            Debug.logger = new TestLogger();
            _state = new GameState();
            _state.StartOnOtherThread();

            _gobj = Helper.CreateInvoke(_state);
        }
        [TestCleanup]
        public void TestCleanup()
        {
            _state.Stop();
        }

        [TestMethod]
        public void CreateTest()
        {
            var test = Helper.AddInvoke<TestComponent>(_gobj);

            Helper.WaitUntil(() => test != null);
            Helper.WaitUntil(() => test.LateUpdateCalled);
        }

        [TestMethod]
        public void ChildTest()
        {
            var test = Helper.AddInvoke<ChildTestComponent>(_gobj);

            Helper.WaitUntil(() => test != null);
            Helper.WaitUntil(() => test.ChildUpdateCalled);
        }

        [TestMethod]
        public void YieldTest()
        {
            var test = Helper.AddInvoke<TestYielder>(_gobj);

            Helper.WaitUntil(() => test != null);
            Assert.IsTrue(test.BeforeStuff);
            Assert.IsFalse(test.AfterStuff);
            Helper.WaitUntil(() => test.AfterStuff, 5);
        }

        [TestMethod]
        public void EnabledTest()
        {
            var test = Helper.AddInvoke<TestComponent>(_gobj);

            Helper.WaitUntil(() => test.LateUpdateCalled);
            _state.InvokeIfRequired(() =>
            {
                test.Enabled = false;
                test.LateUpdateCalled = false;
            });
            Helper.WaitUntil(() => !test.LateUpdateCalled);
            _state.InvokeIfRequired(() =>
            {
                test.Enabled = true;
            });
            Helper.WaitUntil(() => test.LateUpdateCalled);
        }

        [TestMethod]
        public void DestroyTest()
        {
            var test = Helper.AddInvoke<TestComponent>(_gobj);

            _state.InvokeIfRequired(_gobj.Destroy);
            Helper.WaitUntil(() => _gobj.IsDisposed);
            test.LateUpdateCalled = false;
            Helper.Ensure(() => !test.LateUpdateCalled);
        }
    }
}
