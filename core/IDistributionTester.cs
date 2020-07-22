using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    public interface IDistributionTester
    {
        bool Test(double[] distribution, InstanceModel model, Feature classFeature);
    }

}