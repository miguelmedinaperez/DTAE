using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.DistributionEvaluators
{
    public class Twoing : IDistributionEvaluator
    {
        public double Evaluate(double[] parent, params double[][] children)
        {
            #region preconditions
            if (children.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(children), children, "Twoing need only two child nodes (binary split)");

            if (parent.Sum() == parent.Max())
                return 0;
            #endregion;

            double total = parent.Sum();
            //0.25*pl*pr
            double SL = children[0].Sum();
            double SR = children[1].Sum();
            double twoing = 0.25* SL/total * SR/total;
            
            double aux = 0;
            for (int i = 0; i < parent.Length; i++)
            {
                aux += Math.Abs((children[0][i] / SL) - (children[1][i] / SR));
            }
            twoing *= Math.Pow(aux, 2.0);
            return twoing;
        }
    }
}
