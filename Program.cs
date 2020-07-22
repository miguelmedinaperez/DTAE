using PRFramework.Core.Common;
using PRFramework.Core.IO;
using PRFramework.Core.SupervisedClassifiers.EmergingPatterns.Classifiers;
using System;

namespace BaggingRandomMinerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var traininDataset = CsvLoader.Load(@"C:\1tra.csv", out InstanceModel trModel);

            var testingDataset = CsvLoader.Load(@"C:\1tst.csv", out InstanceModel tsModel);

            DTAE classifier = new DTAE();
            classifier.Train(trModel, traininDataset);

            foreach (var obj in testingDataset)
                Console.WriteLine(classifier.Classify(obj)[0]);

            Console.ReadLine();
        }
    }
}
