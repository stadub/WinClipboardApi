using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipbordHelperTest
{
    public abstract class SpecificationContext
    {
        [TestInitialize]
        public void Init()
        {
            this.Given();
            this.When();
        }

        public virtual void Given() { }
        public virtual void When() { }
    }
}
