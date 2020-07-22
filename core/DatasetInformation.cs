using System;
using PRFramework.Core.Common;

namespace PRFramework.Core.DatasetInfo
{
    [Serializable]
    public class DatasetInformation
    {
        public FeatureInformation[] FeatureInformations { get; set; }
        public int ObjectsWithIncompleteData { get; set; }
        public int GlobalAbscenseInformation { get; set; }
    }

    [Serializable]
    public class FeatureInformation
    {
        public Feature Feature { protected get; set; }
        public int MissingValueCount { get; set; }
    }

    [Serializable]
    public class NominalFeatureInformation : FeatureInformation
    {
        public NominalFeature NominalFeature
        {
            get { return Feature as NominalFeature; }
        }
        public double[] Distribution { get; set; }
        public double[] ValueProbability { get; set; }
        public double[] Ratio { get; set; }
    }

    [Serializable]
    public class NumericFeatureInformation : FeatureInformation
    {
        public NumericFeature NumericFeature
        {
            get { return Feature as NumericFeature; }
        }
        public double MinValue;
        public double MaxValue;
    }
}
