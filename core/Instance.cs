using System;
using System.Collections.Generic;
using System.Linq;

namespace PRFramework.Core.Common
{
    [Serializable]
    public class Instance
    {
        public InstanceModel Model { get; private set; }

        public double[] Values { get; private set; }


        public Instance(InstanceModel model)
        {
            Model = model;
            Values = new double[model.Features.Length];
        }

        public double this[Feature feature]
        {
            get { return Values[feature.Index]; }
            set
            {
                if (!double.IsNaN(value))
                {
                    if (feature.FeatureType == FeatureType.Integer && value != Math.Round(value))
                        throw new InvalidCastException(string.Format("Cannot assign {0} to an integer feature", value));
                    if (feature.FeatureType == FeatureType.Nominal && value >= ((NominalFeature)feature).Values.Length)
                        throw new InvalidCastException(string.Format("Value {0} is out of nominal value indexes", value));
                }
                Values[feature.Index] = value;
            }
        }
        public IEnumerable<double> this[IEnumerable<Feature> features]
        {
            get { return features.Select(x => Values[x.Index]); }
        }
        public double this[string featureName]
        {
            get { return this[Model[featureName]]; }
            set { this[Model[featureName]] = value; }
        }

        public void SetNominalValue(Feature feature, string value)
        {
            if (feature.FeatureType != FeatureType.Nominal)
                throw new ArgumentException(
                    string.Format("Cannot change nominal value for non-nominal feature. Feature={0}, Value={1}",
                                  feature.Name, value));
            if (value == null)
            {
                this[feature] = FeatureValue.Missing;
                return;
            }
            int valueIndex = ((NominalFeature)feature).Values.IndexOf(x => x == value);
            if (valueIndex == -1)
                throw new ArgumentException(string.Format("Unexisting value to set. Feature={0}, Value={1}", 
                    feature.Name, value));
            this[feature] = valueIndex;
        }

        internal void Initialize (params object[] values)
        {
            if (values == null)
            {
                Initialize(new object[] {null});
                return;
            }
            for (int i = 0; i < values.Length; i++)
            {
                var feature = Model.Features[i];
                if (values[i] == null)
                    this[feature] = FeatureValue.Missing;
                else
                {
                    if (feature.FeatureType == FeatureType.Nominal && values[i] is string)
                        SetNominalValue(feature, (string) values[i]);
                    else
                        this[feature] = Convert.ToDouble(values[i]);
                }
            }
        }

        public override string ToString()
        {
            return string.Join(",", Model.Features.Select((t, i) => t.ToString(Values[i])).ToArray());
        }
    }

    [Serializable]
    public static class FeatureValue
    {
        public const double Missing = double.NaN;

        public static bool IsMissing (double value)
        {
            return double.IsNaN(value);
        }
        public static bool IsMissing(IEnumerable<double> values)
        {
            return values.Any(p => double.IsNaN(p));
        }
    }
}