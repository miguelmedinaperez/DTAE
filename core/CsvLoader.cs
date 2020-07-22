using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PRFramework.Core.Common;
using PRFramework.Core.DatasetInfo;

namespace PRFramework.Core.IO
{
    public static class CsvLoader
    {
        public static IEnumerable<Instance> Load(string fileName, out InstanceModel model)
        {
            string[] headers = null;
            List<string[]> matrix = new List<string[]>();

            using (var reader = File.OpenText(fileName))
            {
                string line = reader.ReadLine();
                headers = line.Split(new[] { ',' });
                int colCount = headers.Length;
                while ((line = reader.ReadLine()) != null)
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] thisValue = line.Split(new[] { ',' });
                        if (thisValue.Length != colCount)
                            throw new Exception("Invalid column count at: " + line);
                        matrix.Add(thisValue);
                    }
            }

            MakeHeadersUnique(headers);

            model = new InstanceModel
            {
                Features = new Feature[headers.Length],
                RelationName = TextToId(Path.GetFileNameWithoutExtension(fileName)),
            };
            Instance[] result = new Instance[matrix.Count];
            for (int i = 0; i < matrix.Count; i++)
                result[i] = model.CreateInstance();

            for (int index = 0; index < headers.Length; index++)
            {
                string[] values = matrix.Select(x => x[index]).Distinct().ToArray();

                if (values.All(IsIntOrEmpty) && values.Length <= 6)
                    CreateNomminalFeature(values, headers, index, model, matrix, result);
                else if (values.All(IsDoubleOrEmpty))
                    CreateDoubleFeature(values, headers, index, model, matrix, result);
                else
                    CreateNomminalFeature(values, headers, index, model, matrix, result);
            }

            BaseSerializer.LoadInstancesInformation(model, result);

            return result;
        }

        private static void MakeHeadersUnique(string[] headers)
        {
            HashSet<string> heads = new HashSet<string>();
            for (int i = 0; i < headers.Length; i++)
            {
                while (heads.Contains(headers[i]))
                    headers[i] = headers[i] + "_";
                heads.Add(headers[i]);
            }
        }

        private static string TextToId(string text)
        {
            return Regex.Replace(text, @"[^\w\d-_]", "_");
        }

        private static void CreateNomminalFeature(IEnumerable<string> values, string[] headers, int index, InstanceModel model, List<string[]> matrix,
            Instance[] result)
        {
            string[] casted = values.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().ToArray();
            var newModel = new NominalFeature(TextToId(headers[index]), index)
            {
                Values = casted,
            };
            model.Features[index] = newModel;
            for (int i = 0; i < matrix.Count; i++)
            {
                string asStr = matrix[i][index];
                if (string.IsNullOrWhiteSpace(asStr))
                    asStr = null;
                result[i].SetNominalValue(newModel, asStr);
            }
        }

        private static void CreateDoubleFeature(IEnumerable<string> values, string[] headers, int index, InstanceModel model, List<string[]> matrix,
            Instance[] result)
        {
            double[] casted =
                values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(double.Parse).ToArray();
            var newModel = new DoubleFeature(TextToId(headers[index]), index)
            {
                MinValue = casted.Min(),
                MaxValue = casted.Max(),
            };
            model.Features[index] = newModel;
            for (int i = 0; i < matrix.Count; i++)
            {
                string asStr = matrix[i][index];
                result[i][newModel] = string.IsNullOrWhiteSpace(asStr) ? double.NaN : double.Parse(asStr);
            }
        }

        private static bool IsDoubleOrEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            double result;
            if (double.TryParse(value, out result))
                return true;
            return false;
        }

        private static bool IsIntOrEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            int result;
            if (int.TryParse(value, out result))
                return true;
            return false;
        }
    }
}