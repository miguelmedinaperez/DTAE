using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PRFramework.Core.Common
{
    public static class LinqExtensions
    {
        public static void CopyToExceptIndex(this Array source, Array dest, int index)
        {
            Array.ConstrainedCopy(source, 0, dest, 0, index);
            Array.ConstrainedCopy(source, index + 1, dest, index, dest.Length - index);
        }

        public static int IndexOf<TSource>(this IEnumerable<TSource> source, System.Func<TSource, bool> predicate)
        {
            int idx = 0;
            foreach (TSource element in source)
            {
                if (predicate(element))
                    return idx;
                idx++;
            }
            return -1;
        }

        public static int ArgMax<TSource>(this IEnumerable<TSource> source)
        {
            if (!source.Any())
                return -1;
            TSource maxValue = source.Max();
            return source.IndexOf(x => x.Equals(maxValue));
        }

        public static int ArgMax<TSource,TDest>(this IEnumerable<TSource> source, Func<TSource,TDest> select)
        {
            if (!source.Any())
                return -1;
            TDest maxValue = source.Select(select).Max();
            return source.IndexOf(x => x.Equals(maxValue));
        }

        public static int ArgMin<TSource>(this IEnumerable<TSource> source)
        {
            if (!source.Any())
                return -1;
            TSource minValue = source.Min();
            return source.IndexOf(x => x.Equals(minValue));
        }

        public static double[] MultiplyBy(this IEnumerable<double> arr, double value)
        {
            return arr.Select(x => x * value).ToArray();
        }

        public static double[] DividedBy(this IEnumerable<double> arr, double value)
        {
            return arr.Select(x => x / value).ToArray();
        }

        public static double[] AddTo(this double[] arr1, double[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new InvalidOperationException("Cannot add arrays of different lengths");
            var length = arr1.Length;
            double[] result = new double[length];
            for (int i = 0; i < length; i++)
                result[i] = arr1[i] + arr2[i];
            return result;
        }
        public static void AccumulateAdd(this double[] arr1, double[] arr2)
        {
            for (int i = 0; i < arr1.Length; i++)
                arr1[i] = arr1[i] + arr2[i];
        }

        public static bool IsEqual(this double[] arr1, double[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new InvalidOperationException("Cannot add arrays of different lengths");
            var length = arr1.Length;
            for (int i = 0; i < length; i++)
                if (arr1[i] != arr2[i])
                    return false;
            return true;
        }

        public static double[] MultiplyBy(this double[] arr1, double[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new InvalidOperationException("Cannot add arrays of different lengths");
            var length = arr1.Length;
            double[] result = new double[length];
            for (int i = 0; i < length; i++)
                result[i] = arr1[i] * arr2[i];
            return result;
        }

        public static double[] Substract(this double[] arr1, double[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new InvalidOperationException("Cannot add arrays of different lengths");
            var length = arr1.Length;
            double[] result = new double[length];
            for (int i = 0; i < length; i++)
                result[i] = arr1[i] - arr2[i];
            return result;
        }

        public static double[] DividedBy(this double[] arr1, double[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new InvalidOperationException("Cannot add arrays of different lengths");
            var length = arr1.Length;
            double[] result = new double[length];
            for (int i = 0; i < length; i++)
                result[i] = arr1[i] / arr2[i];
            return result;
        }

        public static double[][] CloneArray(this double[][] source)
        {
            double[][] result = new double[source.GetLength(0)][];
            for (int index = 0; index < source.Length; index++)
            {
                double[] value = source[index];
                result[index] = (double[])value.Clone();
            }
            return result;
        }

        public static double[] FindDistribution(this IEnumerable<Instance> source, Feature classFeature)
        {
            if (classFeature.FeatureType != FeatureType.Nominal)
                throw new InvalidOperationException("Cannot find distribution for non-nominal class");
            double[] result = new double[((NominalFeature)classFeature).Values.Length];
            foreach (Instance instance in source)
                if (!FeatureValue.IsMissing(instance[classFeature]))
                {
                    int value = (int)instance[classFeature];
                    result[value]++;
                }
            return result;
        }
        public static double[] FindDistribution(this IEnumerable<Tuple<Instance, double, double>> source, Feature classFeature)
        {
            return FindDistribution(source.Select(x => Tuple.Create(x.Item1, x.Item2)), classFeature);
        }

        public static double[] FindDistribution(this IEnumerable<Tuple<Instance, double>> source, Feature classFeature)
        {
            if (classFeature.FeatureType != FeatureType.Nominal)
                throw new InvalidOperationException("Cannot find distribution for non-nominal class");
            double[] result = new double[((NominalFeature)classFeature).Values.Length];
            foreach (var tuple in source)
                if (!FeatureValue.IsMissing(tuple.Item1[classFeature]))
                {
                    int value = (int)tuple.Item1[classFeature];
                    result[value] += tuple.Item2;
                }
            return result;
        }

        public static string ToStringEx(this IEnumerable<double> values, int digits = -1, string separator=",")
        {
            if (values == null)
                return "null";
            if (digits >= 0)
                return string.Format("[{0}]", string.Join(separator, values.Select(x => x.ToString("f" + digits))));
            else
                return string.Format("[{0}]", string.Join(separator, values));
        }

        public static void AddInstance(this IList<Instance> source, InstanceModel model, params object[] values)
        {
            Instance instance = model.CreateInstance();
            instance.Initialize(values);
            source.Add(instance);
        }

        public static IEnumerable<Tuple<Instance, double>> CreateMembershipTuple(this IEnumerable<Instance> instances)
        {
            return instances.Select(x => Tuple.Create(x, 1d));
        }

        public static string Repeat(this char c, int count)
        {
            return (new StringBuilder()).Append(c, count).ToString();
        }
        
        public static int Sum(this int[,] source)
        {
            int result = 0;
            for (int i = 0; i < source.GetLongLength(0); i++)
                for (int j = 0; j < source.GetLongLength(1); j++)
                    result += source[i, j];

            return result;
        }

        public static string[] NominalFeatureValues(this Feature feature)
        {
            var asNominal = feature as NominalFeature;
            if (asNominal == null)
                throw new ArgumentException("Feature is not nominal");
            return asNominal.Values;
        }
    }
}