using System;
using System.Collections.Generic;
using System.Linq;
using PRFramework.Core.Common;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.ChildSelectors;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.Builder;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.SplitIterators
{
    public enum CuttingStrategy {OnPoint, CenterBetweenPoints}

    [Serializable]
    public class OrderedFeatureSplitIterator : ISplitIterator
    {
        public OrderedFeatureSplitIterator()
        {
            CuttingStrategy = CuttingStrategy.OnPoint;
        }

        public Feature ClassFeature { get; set; }
        public InstanceModel Model { get; set; }
        public CuttingStrategy CuttingStrategy { get; set; }
        public double[][] CurrentDistribution { get; set; }

        public double CurrentValue { get { return _selectorFeatureValue; } }

        private double _lastClassValue;
        private Feature _feature;
        private double _selectorFeatureValue;
        private bool _initialized = false;
        private Tuple<Instance, double>[] sorted;
        private int _currentIndex;
        private IEnumerable<Tuple<Instance, double>> _instances { get; set; }

        public void Initialize(Feature feature, IEnumerable<Tuple<Instance, double>> instances)
        {
            _initialized = true;
            _instances = instances;

            if (Model == null)
                throw new InvalidOperationException("Model is null");
            if (ClassFeature.FeatureType != FeatureType.Nominal)
                throw new InvalidOperationException("Cannot use this iterator on non-nominal class");
            if (!feature.IsOrdered)
                throw new InvalidOperationException("Cannot use this iterator on non-ordered feature");
            _feature = feature;
            CurrentDistribution = new double[2][];

            sorted = _instances.Where(x => !FeatureValue.IsMissing(x.Item1[feature])).OrderBy(x => x.Item1[feature]).ToArray();

            CurrentDistribution[0] = new double[(ClassFeature as NominalFeature).Values.Length];
            CurrentDistribution[1] = sorted.FindDistribution(ClassFeature);
            if (sorted.Length == 0)
                return;

            _currentIndex = -1;
            _lastClassValue = FindNextClass(0);

        }
        
        public bool FindNext()
        {
            if (!_initialized)
                throw new InvalidOperationException("Iterator not initialized");
            if (_currentIndex >= sorted.Length - 1)
                return false;

            for (_currentIndex = _currentIndex + 1; _currentIndex < sorted.Length - 1; _currentIndex++)
            {
                var instance = sorted[_currentIndex].Item1;
                int objClass = (int)instance[ClassFeature];
                CurrentDistribution[0][objClass] += sorted[_currentIndex].Item2;
                CurrentDistribution[1][objClass] -= sorted[_currentIndex].Item2;
                if (instance[_feature] != sorted[_currentIndex + 1].Item1[_feature])
                {
                    double nextClassValue = FindNextClass(_currentIndex + 1);
                    if (_lastClassValue != nextClassValue || (_lastClassValue == -1 && nextClassValue == -1))
                    {
                        if (CuttingStrategy == CuttingStrategy.OnPoint)
                            _selectorFeatureValue = instance[_feature];
                        else
                            _selectorFeatureValue = (instance[_feature] + sorted[_currentIndex + 1].Item1[_feature]) / 2;
                        _lastClassValue = nextClassValue;
                        validateDistribution();
                        return true;
                    }
                }
            }
            return false;
        }

        private double FindNextClass(int index)
        {
            double currentClass = sorted[index].Item1[ClassFeature];
            double currentValue = sorted[index].Item1[_feature];
            index++;
            while (index < sorted.Length && currentValue == sorted[index].Item1[_feature])
            {
                if (sorted[index].Item1[ClassFeature] != currentClass)
                    return -1;
                index++;
            }
            return currentClass;
        }

        private bool validateDistribution()
        {
            ChildrenInstanceCreator childrenInstanceCreator = new ChildrenInstanceCreator();
            var instancesPerChildNode = childrenInstanceCreator.CreateChildrenInstances(_instances, CreateCurrentChildSelector(), 0.05);

            double[] dist0 = instancesPerChildNode[0].FindDistribution(ClassFeature);
            double[] dist1 = instancesPerChildNode[1].FindDistribution(ClassFeature);


            for (int i = 0; i < dist0.Length; i++)
            {
                if (CurrentDistribution[0][i] != dist0[i] && CurrentDistribution[1][i] != dist0[i])
                    return false;
                if (CurrentDistribution[0][i] != dist1[i] && CurrentDistribution[1][i] != dist1[i])
                    return false;
            }
            return true;
        }


        public IChildSelector CreateCurrentChildSelector()
        {
            return new CutPointSelector()
                       {
                           CutPoint = _selectorFeatureValue,
                           Feature = _feature,
                       };
        }
    }
}
