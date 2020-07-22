using System;
using System.Linq;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.DistributionTesters
{
    [Serializable]
    public class PureNodeStopCondition : IDistributionTester
    {
        public bool Test(double[] distribution, InstanceModel model, Feature classFeature)
        {
            bool stop = distribution.Max() == distribution.Sum();
            return stop;
        }
    }
}