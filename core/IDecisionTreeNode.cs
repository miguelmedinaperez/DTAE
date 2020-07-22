using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    public interface IDecisionTreeNode
    {
        IChildSelector ChildSelector { get; set; }
        double[] Data { get; set; }
        IDecisionTreeNode Parent { get; set; }
        IDecisionTreeNode[] Children { get; set; }
        bool IsLeaf { get; }
        string ToString(int ident, InstanceModel model, int digits = -1);
    }
}
