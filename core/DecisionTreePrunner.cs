using System;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    [Serializable]
    public class DecisionTreePrunner
    {
        public IPruneTester PruneTester { get; set; }

        public void Prune(DecisionTree classifier)
        {
            Prune(classifier.TreeRootNode);
        }

        private bool Prune(IDecisionTreeNode node)
        {
            if (node.IsLeaf)
                return true;
            bool allPrunned = true;
            foreach (IDecisionTreeNode child in node.Children)
            {
                bool childPrunned = Prune(child);
                allPrunned = allPrunned && childPrunned;
            }
            if (allPrunned && PruneTester.CanPrune(node))
            {
                node.ChildSelector = null;
                node.Children = null;
            }
            return false;
        }
    }
}
