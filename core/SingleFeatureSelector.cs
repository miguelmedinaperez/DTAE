using System;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.ChildSelectors
{
    [Serializable]
    public class SingleFeatureSelector 
    {
        public Feature Feature { get; set; }
    }
}