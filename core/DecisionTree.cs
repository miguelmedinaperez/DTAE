using System;
using System.Linq;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    [Serializable]
    public class DecisionTree
    {
        public IDecisionTreeNode TreeRootNode { get; set; }
        public InstanceModel Model { get; set; }

        public int Size
        {
            get
            {
                return TreeRootNode != null ? ComputeSizeTree(TreeRootNode) : 0;
            }
        }

        public int Leaves
        {
            get
            {
                return TreeRootNode != null ? ComputeLeaves(TreeRootNode) : 0;
            }
        }

        public string ToString(int digits = -1)
        {
            return string.Format("Database:\t{0}\n\n{1}\n\nNumber of Leaves:\t{2}\n\nSize of the tree:\t{3}", Model.RelationName, TreeRootNode.ToString(0, Model, digits), Leaves, Size);
        }

        public override string ToString()
        {
            return ToString(-1);
        }

        private int ComputeLeaves(IDecisionTreeNode decisionTree)
        {
            return decisionTree.IsLeaf ? 1 : decisionTree.Children.Sum(childs => ComputeLeaves(childs));
        }

        private int ComputeSizeTree(IDecisionTreeNode decisionTree)
        {
            return decisionTree.Children == null
                       ? 0
                       : decisionTree.Children.Select(ComputeSizeTree).Concat(new[] { 0 }).Max() + 1;
        }
    }
}