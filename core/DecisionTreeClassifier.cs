using System;
using System.Linq;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    [Serializable]
    public class DecisionTreeClassifier : ISupervisedClassifier
    {
        public DecisionTreeClassifier()
        {
            
        }

        public DecisionTreeClassifier(DecisionTree decisionTree)
        {
            DecisionTree = decisionTree;
            Model = decisionTree.Model;
        }

        public DecisionTree DecisionTree { get; set; }
        public InstanceModel Model { get; set; }

        public double[] Classify(Instance instance)
        {
            var classification = ClassifyInstance(DecisionTree.TreeRootNode, instance, 1);
            return classification.MultiplyBy(1 / classification.Sum());
        }

        public double[] RawClassify(Instance instance)
        {
            return ClassifyInstance(DecisionTree.TreeRootNode, instance, 1);
        }

        private double[] ClassifyInstance(IDecisionTreeNode node, Instance instance, double instanceMembership)
        {
            if (node.IsLeaf)
                return node.Data.MultiplyBy(instanceMembership);
            double[] childrenSelection = node.ChildSelector.Select(instance);
            if (childrenSelection != null)
            {
                if (childrenSelection.Length != node.Children.Length)
                    throw new IndexOutOfRangeException("Child index is out of range");
                double[] result = null;
                for (int i = 0; i < childrenSelection.Length; i++)
                {
                    double selection = childrenSelection[i];
                    if (selection > 0)
                    {
                        IDecisionTreeNode child = node.Children[i];
                        double[] childValue = ClassifyInstance(child, instance, instanceMembership);
                        if (result != null)
                            result = result.AddTo(childValue);
                        else
                            result = childValue;
                    }
                }
                return result;
            }
            else
            {
                double[] result = null;
                double totalNodeMembership = node.Data.Sum();
                for (int i = 0; i < node.Children.Length; i++)
                {
                        IDecisionTreeNode child = node.Children[i];
                        double childMembership = node.Children[i].Data.Sum();
                        double[] childValue = ClassifyInstance(child, instance, childMembership / totalNodeMembership * instanceMembership);
                        if (result != null)
                            result = result.AddTo(childValue);
                        else
                            result = childValue;
                }
                return result;    
            }
        }
    }
}
