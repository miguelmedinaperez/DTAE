using System;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.ChildSelectors
{
    [Serializable]
    public class CutPointSelector : SingleFeatureSelector, IChildSelector
    {
        public double[] Select(Instance instance)
        {
            if (Feature.FeatureType == FeatureType.Nominal)
                throw new InvalidOperationException("Cannot use cutpoint on nominal data");
            if (FeatureValue.IsMissing(instance[Feature]))
                return null;
            return instance[Feature] <= CutPoint ? new double[] { 1, 0 } : new double[] { 0, 1 };
        }

        public int ChildrenCount { get { return 2; } }

        public double CutPoint { get; set; }

        public string ToString(InstanceModel model, int index)
        {
            return string.Format("{0}{2}{1}", Feature.Name, Feature.ValueToString(CutPoint),
                index == 0 ? "<=" : ">");
        }

        public override string ToString()
        {
            return string.Format("{0}<={1}", Feature.Name, CutPoint);
        }
    }
}
