using System;
using System.Text;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    [Serializable]
    public class DecisionTreeNode : IDecisionTreeNode
    {
        public DecisionTreeNode()
        {
        }

        public DecisionTreeNode(double[] data, IChildSelector selector = null, params IDecisionTreeNode[] children)
        {
            Data = data;
            ChildSelector = selector;
            Children = children;
        }

        public IChildSelector ChildSelector { get; set; }
        public double[] Data { get; set; }

        public IDecisionTreeNode Parent { get; set; }
        public IDecisionTreeNode[] Children { get; set; }

        public bool IsLeaf
        {
            get { return Children == null || Children.Length == 0; }
        }

        public string ToString(int ident, InstanceModel model, int digits = -1)
        {
            // "-[2,3]\n -IntFeature<=4 [1,1]\n -IntFeature>4 [1,2]"
            StringBuilder builder = new StringBuilder();
            builder.Append(Data.ToStringEx(digits));
            if (!IsLeaf)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    IDecisionTreeNode child = Children[i];
                    builder.Append("\n");
                    builder.Append(' ', (ident + 1) * 3); 
                    builder.Append("- ");
                    builder.Append(ChildSelector.ToString(model, i));
                    builder.Append(' ');
                    builder.Append(child.ToString(ident + 1, model)); 
                }
            }
            return builder.ToString();
        }
    }
    
}