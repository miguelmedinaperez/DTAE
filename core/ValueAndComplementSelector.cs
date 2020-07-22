using System;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.ChildSelectors
{
    [Serializable]
    public class ValueAndComplementSelector : SingleFeatureSelector, IChildSelector
    {
        public double[] Select(Instance instance)
        {
            if (Feature.FeatureType != FeatureType.Nominal)
                throw new InvalidOperationException("Cannot use value and complement on non-nominal data");
            if (FeatureValue.IsMissing(instance[Feature]))
                return null;
            return (int)instance[Feature] == (int)Value ? new double[] { 1, 0 } : new double[] { 0, 1 };
        }
        public double Value { get; set; }

        public int ChildrenCount { get { return 2; } }

        public string ToString(InstanceModel model, int index)
        {
            return string.Format("{0}{2}{1}", Feature.Name, Feature.ValueToString(Value), 
                index == 0 ? "=" : "<>");
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", Feature.Name, Value);
        }

    }
}