using System;
using System.Collections.Generic;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.Builder
{
    [Serializable]
    public class ChildrenInstanceCreator
    {
        public List<Tuple<Instance, double>>[] CreateChildrenInstances(IEnumerable<Tuple<Instance, double>> instances, 
            IChildSelector selector, double threshold = 0)
        {
            List<Tuple<Instance, double>>[] result = new List<Tuple<Instance, double>>[selector.ChildrenCount];
            for (int i = 0; i < selector.ChildrenCount; i++)
                result[i] = new List<Tuple<Instance, double>>();
            foreach (var tuple in instances)
            {
                double[] selection = selector.Select(tuple.Item1);
                if (selection != null)
                    for (int i = 0; i < selection.Length; i++)
                        if (selection[i] > 0)
                        {
                            double newMembership = selection[i] * tuple.Item2;
                            if (newMembership >= threshold)
                                result[i].Add(Tuple.Create(tuple.Item1, newMembership));
                        }
                

            }
            return result;
        }
    }
}
