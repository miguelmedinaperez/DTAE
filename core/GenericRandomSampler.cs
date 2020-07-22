using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PRFramework.Core.Common;

namespace PRFramework.Core.Samplers
{
    public class RandomSamplerWithReplacement<T>
    {
        public IEnumerable<T> GetSample(IEnumerable<T> population, int sampleCount)
        {
            var populationList = population.ToList();
            var sample = new List<T>();
            for (int i = 0; i < sampleCount; i++)
                sample.Add(populationList[randomGenerator.Next(populationList.Count)]);

            return sample;
        }

        private static Random randomGenerator = new Random();
    }

    public class RandomSamplerWithoutReplacement<T>
    {
        public IEnumerable<T> GetSample(IEnumerable<T> population, int sampleCount)
        {
            var populationList = population.ToList();
            if (populationList.Count <= sampleCount)
                throw new ArgumentOutOfRangeException("sampleCount", "You cannot sample without replacement more objects than the size of the population.");

            var sample = new HashSet<T>();
            while (sample.Count < sampleCount)
                sample.Add(populationList[randomGenerator.Next(populationList.Count)]);

            return sample;
        }

        private static Random randomGenerator = new Random();
    }
}
