using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers
{
    public interface ISupervisedClassifier
    {
        double[] Classify(Instance instance);
    }
}
