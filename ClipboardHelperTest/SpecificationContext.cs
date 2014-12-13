using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardHelperTest
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
