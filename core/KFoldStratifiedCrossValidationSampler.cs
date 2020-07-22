using System;
using System.Collections.Generic;
using System.Linq;
using PRFramework.Core.Common;

namespace PRFramework.Core.Samplers.Instances
{
    [Serializable]
    public class KFoldStratifiedCrossValidationSampler : BaseSampler, ISampler
    {
        public int K { get; set; }
        public int Seed { get; set; }
        public string ClassName { get; set; }

        public KFoldStratifiedCrossValidationSampler()
        {
            Seed = 1;
            ClassName = "class";
        }

        public List<InstanceSample> Sample(IEnumerable<Instance> instances)
        {
            var splits = CalculateSplits(instances, K);
            List<InstanceSample> result = new List<InstanceSample>();
            for (int i = 0; i < splits.Length; i++)
            {
                var newSample = GetSampleForIndex(splits, i);
                result.Add(newSample);
            }
            return result;
        }

        private static InstanceSample GetSampleForIndex(List<Instance>[] splits, int i)
        {
            List<Instance> split = splits[i];
            var newSample = new InstanceSample
                                {
                                    Name = i.ToString(),
                                    Test = new List<Instance>(split),
                                    Training = new List<Instance>()
                                };
            for (int j = 0; j < splits.Length; j++)
                if (i != j)
                    newSample.Training.AddRange(splits[j]);
            return newSample;
        }

        private List<Instance>[] CalculateSplits(IEnumerable<Instance> instances, int k)
        {
            IRandomGenerator randomGenerator = new RandomGenerator(Seed);
            List<Instance>[] splits = new List<Instance>[k];
            for (int i = 0; i < k; i++)
                splits[i] = new List<Instance>();

            var clonned = new List<Instance>(instances);
            var classFeature = clonned[0].Model[ClassName];
            var clonnedGRouped = clonned.GroupBy(x => x[classFeature]).ToList();

            int splitIdx = 0;
            foreach (var group in clonnedGRouped)
            {
                var instInGroup = group.ToList();
                while (instInGroup.Count > 0)
                {
                    int selectIdx = randomGenerator.Next(instInGroup.Count());
                    splits[splitIdx].Add(instInGroup.ElementAt(selectIdx));
                    instInGroup.RemoveAt(selectIdx);
                    splitIdx = (splitIdx + 1) % k;
                }
            }
            return splits;
        }

        public InstanceSample GetSample(IEnumerable<Instance> instances, string sampleId)
        {
            var splits = CalculateSplits(instances, K);
            int index = int.Parse(sampleId);
            return GetSampleForIndex(splits, index);
        }
    }
}