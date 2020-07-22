using System;

namespace PRFramework.Core.Common
{
    public interface IRandomGenerator
    {
        int Next();
        int Next(int minValue, int maxValue);
        int Next(int maxValue);
        double NextDouble();
        void NextBytes(byte[] buffer);
    }

    [Serializable]
    public class RandomGenerator : IRandomGenerator
    {
        private Random _random;

        public RandomGenerator()
        {
            _random = new Random();
        }

        public RandomGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public void NextBytes(byte[] buffer)
        {
            _random.NextBytes(buffer);
        }
    }
}
