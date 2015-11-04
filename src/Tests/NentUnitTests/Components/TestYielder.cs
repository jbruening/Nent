using System.Collections;
using Nent;

namespace NentUnitTests.Components
{
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
