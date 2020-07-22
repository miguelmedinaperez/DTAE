namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    public interface IDistributionEvaluator
    {
        double Evaluate(double[] parent, params double[][] children);
    }
}