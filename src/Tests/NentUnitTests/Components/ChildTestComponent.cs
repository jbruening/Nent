using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NentUnitTests.Components
{
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
}