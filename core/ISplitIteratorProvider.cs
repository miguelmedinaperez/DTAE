using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    public interface ISplitIteratorProvider
    {
        ISplitIterator GetSplitIterator(InstanceModel model, Feature feature, Feature classFeature);
    }
}
