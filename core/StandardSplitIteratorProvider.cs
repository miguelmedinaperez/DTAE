using System;
using System.Collections.Generic;
using PRFramework.Core.Common;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.SplitIterators;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.SplitIteratorProviders
{
    [Serializable]
    public class StandardSplitIteratorProvider : BaseSplitIteratorProvider
    {

        public StandardSplitIteratorProvider()
        {
            Iterators[FeatureType.Integer] = new OrderedFeatureSplitIterator
            {
                CuttingStrategy = CuttingStrategy.OnPoint,
            };
            Iterators[FeatureType.Double] =
                          new OrderedFeatureSplitIterator
                          {
                              CuttingStrategy = CuttingStrategy.OnPoint,
                          };
            Iterators[FeatureType.Nominal] =
                          new ValueAndComplementSplitIterator();
                          {
                          };
            Iterators[FeatureType.Ordinal] =
                          new OrderedFeatureSplitIterator
                          {
                              CuttingStrategy = CuttingStrategy.OnPoint,
                          };
        }

        
    }
}
