using System.Collections.Generic;
using System.Linq;
using PRFramework.Core.Common;
using PRFramework.Core.DatasetInfo;
using System;

namespace PRFramework.Core.IO
{
    public class BaseSerializer
    {
        public static void LoadInstancesInformation(InstanceModel model, IEnumerable<Instance> instances)
        {
            foreach (var feature in model.Features)
            {
                LoadFeatureInformation(feature, model, instances);
            }

            FillDatasetInformation(model, instances);
        }

        public static void LoadFeatureInformation(Feature feature, InstanceModel model, IEnumerable<Instance> instances, bool fillDatasetInformation = false)
        {
            if (feature is CategoricalFeature)
            {
                var len = ((CategoricalFeature)feature).Values.Length;
                double[] valuesCount = new double[len];

                for (int i = 0; i < len; i++)
                    valuesCount[i] = instances.Count(x => x[feature] == i && !FeatureValue.IsMissing(x[feature]));

                var valuesmissing = instances.Select(x => x[feature]).Count(FeatureValue.IsMissing);
                var valueProbability = valuesCount.Select(x => x / (valuesCount.Sum() * 1.0)).ToArray();
                var ratio = valuesCount.Select(x => x / (valuesCount.Min() * 1F)).ToArray();

                feature.FeatureInformation = new NominalFeatureInformation()
                {
                    Distribution = valuesCount,
                    MissingValueCount = valuesmissing,
                    ValueProbability = valueProbability,
                    Ratio = ratio,
                    Feature = feature,
                };
            }
            else if (feature is NumericFeature)
            {
                var nonMissingValues = instances.Where(x => !FeatureValue.IsMissing(x[feature])).Select(x => x[feature]).ToArray();
                var valuesmissing = instances.Count() - nonMissingValues.Length;
                double max, min;

                if (nonMissingValues.Length > 0)
                {
                    max = nonMissingValues.Max();
                    min = nonMissingValues.Min();
                }
                else
                {
                    max = 0;
                    min = 0;
                }

                feature.FeatureInformation = new NumericFeatureInformation
                {
                    MissingValueCount = valuesmissing,
                    MaxValue = max,
                    MinValue = min,
                    Feature = feature,
                };
            }

            if (fillDatasetInformation)
                FillDatasetInformation(model, instances);
        }

        public static void FillDatasetInformation(InstanceModel model, IEnumerable<Instance> instances)
        {
            var datasetInformation = new DatasetInformation();

            int objWithIncompleteData = instances.Count(instance => model.Features.Any(feature => FeatureValue.IsMissing(instance[feature])));
            datasetInformation.FeatureInformations = model.Features.Select(feature => feature.FeatureInformation).ToArray();
            datasetInformation.ObjectsWithIncompleteData = objWithIncompleteData;
            datasetInformation.GlobalAbscenseInformation = model.Features.Sum(feature => feature.FeatureInformation.MissingValueCount);
            model.DatasetInformation = datasetInformation;
        }
    }
}
