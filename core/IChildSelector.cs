using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    public interface IChildSelector
    {
        double[] Select(Instance instance);
        string ToString(InstanceModel model, int index);
        int ChildrenCount { get; }
    }
}