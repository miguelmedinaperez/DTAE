using System;
using System.Collections.Generic;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.SplitIteratorProviders
{
    [Serializable]
    public class BaseSplitIteratorProvider : ISplitIteratorProvider
    {
        public Dictionary<FeatureType, ISplitIterator> Iterators { get; set; }

        public BaseSplitIteratorProvider()
        {
            Iterators = new Dictionary<FeatureType, ISplitIterator>();
        }


        public ISplitIterator GetSplitIterator(InstanceModel model, Feature feature, Feature classFeature)
        {
            ISplitIterator result;
            if (Iterators.TryGetValue(feature.FeatureType, out result))
            {
                result.Model = model;
                result.ClassFeature = classFeature;
                return result;
            }
            return null;
        }
    }
}