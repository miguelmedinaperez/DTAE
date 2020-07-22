using System;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.ChildSelectors
{
    [Serializable]
    public class MultipleValuesSelector : SingleFeatureSelector, IChildSelector
    {
        public double[] Select(Instance instance)
        {
            if (Feature.FeatureType != FeatureType.Nominal)
                throw new InvalidOperationException("Cannot use multiple values on non-nominal data");
            if (FeatureValue.IsMissing(instance[Feature]))
                return null;
            int value = (int)instance[Feature];
            int index = Values.IndexOf(x => x == value);
            if (index == -1)
                return null;
            double[] result = new double[ChildrenCount];
            result[index] = 1;
            return result;
        }

        public string ToString(InstanceModel model, int index)
        {
            return string.Format("{0}={1}", Feature.Name, Feature.ValueToString(Values[index]));
        }

        public double[] Values { get; set; }
        public int ChildrenCount { get { return Values.Length; } }

        public override string ToString()
        {
            return string.Format("{0}in[{1}]", Feature.Name, string.Join(",", Values));
        }


    }
}
