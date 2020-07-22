/*
 * Created by: Miguel Angel Medina Pérez (miguelmedinaperez@gmail.com)
 * Created: 02/01/2019
 * Comments by: Miguel Angel Medina Pérez (miguelmedinaperez@gmail.com)
 */

using PRFramework.Core.Common;
using PRFramework.Core.SupervisedClassifiers.Evaluators.Measures;
using System;
using System.Collections.Generic;
using System.Linq;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees;
using PRFramework.Core.Samplers.Instances;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.Builder;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.DistributionEvaluators;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.DistributionTesters;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.SplitIteratorProviders;

namespace PRFramework.Core.SupervisedClassifiers.EmergingPatterns.Classifiers
{
    [Serializable]
    public class DTAE : ISupervisedClassifier
    {
        public double[] Classify(Instance instance)
        {
            if (!_isTrained)
                throw new InvalidOperationException("Unable to classify: Untrained classifier!");

            double sumNormal = 0, sumOutlier = 0;
            for (int i = 0; i < _validFeatures.Count; i++)
            {
                if (!FeatureValue.IsMissing(instance[_validFeatures[i]]))
                {
                    var actualFeatVal = (int)instance[_validFeatures[i]];
                    var classifierResult = _classifiers[i].Classify(instance);

                    sumNormal += _featuresWeights[i] * _featValuesWeights[i][actualFeatVal] * classifierResult[actualFeatVal];

                    double currentSumOutlier = 0;
                    int countOutlierVotes = 0;
                    for (int j = 0; j < classifierResult.Length; j++)
                    {
                        if (actualFeatVal != j && classifierResult[j] > 0)
                        {
                            currentSumOutlier += _featValuesWeights[i][j] * classifierResult[j];
                            countOutlierVotes++;
                        }
                    }
                    if (countOutlierVotes > 0)
                        sumOutlier += _featuresWeights[i] * currentSumOutlier / countOutlierVotes;
                }
            }

            var totalSum = sumNormal + sumOutlier;
            if (totalSum > 0)
                return new[]
                {
                    sumNormal-sumOutlier
                };

            return new[] { 0.0 };
        }

        public virtual void Train(InstanceModel model, IEnumerable<Instance> instances)
        {
            _classNominalFeature = model.Features[model.Features.Length - 1] as NominalFeature;

            List<Instance> trainingDataset = instances.ToList();

            List<IGrouping<double, Instance>> group = trainingDataset.GroupBy(x => x[_classNominalFeature]).ToList();
            if (group.Count() > 1)
            {
                throw new ArgumentException($"Unable to train {GetType().Name}: The training dataset must contain objects of a single class!");
            }

            foreach (Feature feature in model.Features)
            {
                if (feature != model.ClassFeature())
                {
                    NominalFeature currentClass = feature as NominalFeature;
                    if (currentClass == null)
                    {
                        throw new ArgumentOutOfRangeException($"Unable to train {GetType().Name}: All features must be of type {typeof(NominalFeature)}");
                    }

                    List<Instance> currTrDataset = trainingDataset.FindAll(x => !FeatureValue.IsMissing(x[currentClass]));
                    if (currTrDataset.Count > 5)
                    {
                        List<IGrouping<double, Instance>> grouping = currTrDataset.GroupBy(x => x[currentClass]).ToList();
                        if (grouping.Count(x => x.ToList().Count > 2) >= 2)
                        {
                            int[,] confusionMatrix = EvaluateClassifier(model, currTrDataset, currentClass);

                            double auc = confusionMatrix.ComputeMultiClassAUC();
                            _featuresWeights.Add(auc);
                            _featValuesWeights.Add(ComputeWeightByFeatValue(confusionMatrix));

                            DecisionTreeClassifier classifier = new DecisionTreeClassifier()
                            {
                                DecisionTree = _dtBuilder.Build(model, currTrDataset, currentClass),
                                Model = model
                            };
                            _classifiers.Add(classifier);

                            _validFeatures.Add(currentClass);
                        }
                    }
                }
            }

            if (_classifiers.Count == 0)
            {
                throw new ArgumentException($"Unable to train {GetType().Name}: Not enough variability of the features");
            }

            _isTrained = true;
        }


        private double[] ComputeWeightByFeatValue(int[,] confusionMatrix)
        {
            int rowCount = confusionMatrix.GetLength(0);
            double[] weightByFeatVal = new double[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                weightByFeatVal[i] = Enumerable.Range(0, confusionMatrix.GetLength(1)).Select(x => confusionMatrix[i, x]).Sum();
            }

            double sum = weightByFeatVal.Sum();
            for (var i = 0; i < weightByFeatVal.Length; i++)
            {
                weightByFeatVal[i] /= sum;
            }

            return weightByFeatVal;
        }

        private int[,] EvaluateClassifier(InstanceModel model, List<Instance> dataset, NominalFeature currentClass)
        {
            KFoldStratifiedCrossValidationSampler sampler =
                                        new KFoldStratifiedCrossValidationSampler() { ClassName = currentClass.Name, K = 5 };

            int[,] confusionMatrix = new int[currentClass.Values.Length, currentClass.Values.Length];
            foreach (InstanceSample sample in sampler.Sample(dataset))
            {
                DecisionTreeClassifier classifier = new DecisionTreeClassifier()
                {
                    DecisionTree = _dtBuilder.Build(model, sample.Training, currentClass),
                    Model = model
                };

                int[,] currentConfusionMatrix = classifier.ComputeConfusionMatrix(sample.Test, currentClass);
                for (int j = 0; j < currentClass.Values.Length; j++)
                {
                    for (int k = 0; k < currentClass.Values.Length; k++)
                    {
                        confusionMatrix[j, k] += currentConfusionMatrix[j, k];
                    }
                }
            }

            return confusionMatrix;
        }

        private bool _isTrained = false;

        private NominalFeature _classNominalFeature = null;

        private readonly List<NominalFeature> _validFeatures = new List<NominalFeature>();

        private readonly List<DecisionTreeClassifier> _classifiers = new List<DecisionTreeClassifier>();

        private readonly List<double> _featuresWeights = new List<double>();

        private readonly List<double[]> _featValuesWeights = new List<double[]>();

        private readonly DecisionTreeBuilder _dtBuilder = new DecisionTreeBuilder()
        {
            DistributionEvaluator = new Twoing(),
            StopCondition = new PureNodeStopCondition(),
            SplitIteratorProvider = new StandardSplitIteratorProvider(),
            PruneResult = false,
        };
    }
}
