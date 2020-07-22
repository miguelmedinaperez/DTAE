namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    public interface IPruneTester
    {
        bool CanPrune(IDecisionTreeNode node);
    }
}
