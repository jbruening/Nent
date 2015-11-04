using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nent;

namespace NentUnitTests.Components
{
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
}