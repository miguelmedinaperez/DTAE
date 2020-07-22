using System;
using System.Collections.Generic;
using System.Linq;
using PRFramework.Core.Common;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.ChildSelectors;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.SplitIterators
{
    [Serializable]
    public class ValueAndComplementSplitIterator : ISplitIterator
    {
        public Feature ClassFeature { get; set; }
        public InstanceModel Model { get; set; }

        public double[][] CurrentDistribution { get; set; }

        private int _valueIndex;
        private int _valuesCount;
        private bool _iteratingTwoValues;
        private bool _initialized = false;
        private double[] existingValues;
        private bool _twoValuesIterated;
        private int _numClasses;
        private IEnumerable<Tuple<Instance, double>> _instances { get; set; }
        private Feature _feature;

        private Dictionary<double, double[]> _perValueDistribution;
        private double[] _totalDistribution;
        
        public void Initialize(Feature feature, IEnumerable<Tuple<Instance, double>> instances)
        {
            _instances = instances;
            if (Model == null)
                throw new InvalidOperationException("Model is null");
            if (ClassFeature.FeatureType != FeatureType.Nominal)
                throw new InvalidOperationException("Cannot use this iterator on non-nominal class");
            NominalFeature classFeature = (NominalFeature)ClassFeature;
            if (feature.FeatureType != FeatureType.Nominal)
                throw new InvalidOperationException("Cannot use this iterator on non-nominal feature");
            _numClasses = classFeature.Values.Length;
            _feature = feature;

            _perValueDistribution = new Dictionary<double, double[]>();
            _totalDistribution = new double[_numClasses];
            foreach (var instance in _instances)
            {
                double value = instance.Item1[feature];
                if (FeatureValue.IsMissing(value))
                    continue;

                double[] current;
                if (!_perValueDistribution.TryGetValue(value, out current))
                    _perValueDistribution.Add(value, current = new double[_numClasses]);

                int classIdx = (int)instance.Item1[ClassFeature];
                current[classIdx] += instance.Item2;

                _totalDistribution[classIdx] += instance.Item2;
            }

            CurrentDistribution = new double[2][];

            _valuesCount = _perValueDistribution.Count;
            existingValues = _perValueDistribution.Keys.ToArray();

            _iteratingTwoValues = (_valuesCount == 2);
            _valueIndex = -1;
            _twoValuesIterated = false;
            _initialized = true;
        }

        public bool FindNext()
        {
            if (!_initialized)
                throw new InvalidOperationException("Iterator not initialized");
            if (_valuesCount == _instances.Count())
                return false;
            if (_iteratingTwoValues)
            {
                if (_twoValuesIterated)
                    return false;
                _twoValuesIterated = true;

                CalculateCurrent(_perValueDistribution[existingValues[0]]);
                return true;
            }
            else
            {
                if (_valuesCount < 2 || _valueIndex >= _valuesCount - 1)
                    return false;
                _valueIndex++;
                CalculateCurrent(_perValueDistribution[existingValues[_valueIndex]]);
                return true;
            }
        }

        private void CalculateCurrent(double[] current)
        {
            CurrentDistribution[0] = current;
            CurrentDistribution[1] = _totalDistribution.Substract(current);
        }

        
        public IChildSelector CreateCurrentChildSelector()
        {
            if (_iteratingTwoValues)
            {
                return new MultipleValuesSelector
                           {
                               Feature = _feature,
                               Values = _perValueDistribution.Keys.ToArray(),
                           };
            }
            else
                return new ValueAndComplementSelector
                           {
                               Feature = _feature,
                               Value = existingValues[_valueIndex],
                           };
        }
    }
}
