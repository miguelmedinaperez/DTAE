using System;
using System.Collections.Generic;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    public interface ISplitIterator
    {
        Feature ClassFeature { get; set; }
        void Initialize(Feature feature, IEnumerable<Tuple<Instance, double>> instances);
        InstanceModel Model { get; set; }
        double[][] CurrentDistribution { get; set; }
        bool FindNext();
        IChildSelector CreateCurrentChildSelector();
    }
}