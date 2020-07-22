/*
 * Created by: Bárbara Cervantes (bcervantesg@gmail.com) and Miguel Angel Medina Pérez (miguelmedinaperez@gmail.com)
 * Created: 12/04/2017
 * Comments by: Miguel Angel Medina Pérez (miguelmedinaperez@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.Evaluators.Measures
{
    public static class AUCCalculatorExtensions
    {
        public static double ComputeTwoClassAUC(this BasicEvaluation basicEvaluation)
        {
            double positives = basicEvaluation.TP + basicEvaluation.FN;
            double negatives = basicEvaluation.TN + basicEvaluation.FP;
            var tprate = positives > 0.0 ? basicEvaluation.TP / positives : 1.0;
            var fprate = negatives > 0.0 ? basicEvaluation.TN / negatives : 1.0;
            return (tprate + fprate) / 2.0;
        }

        public static double ComputeMultiClassAUC(this int[,] confusionMatrix)
        {
            var eval = new BasicEvaluation();
            for (int i = 0; i < confusionMatrix.GetLength(0); i++)
            {
                eval.TP += confusionMatrix[i, i];
                for (int j = 0; j < confusionMatrix.GetLength(0); j++)
                    if (i != j)
                    {
                        eval.FN += confusionMatrix[i, j];
                        eval.FP += confusionMatrix[j, i];

                        eval.TN += confusionMatrix[j, j];
                    }

            }
            return ComputeTwoClassAUC(eval);
        }

        public static double ComputeMultiClassKappa(this int[,] confusionMatrix)
        {
            double sumAUC = 0.0;
            int count = 0;
            for (int i = 0; i < confusionMatrix.GetLength(0); i++)
            {
                for (int j = i + 1; j < confusionMatrix.GetLength(0); j++)
                {
                    double a = confusionMatrix[i, i];
                    double c = confusionMatrix[j, i];
                    //double c = confusionMatrix[i, j];
                    double b = confusionMatrix[i, j];
                    //double b = confusionMatrix[j, i];
                    double d = confusionMatrix[j, j];

                    double p_0 = (a + d) / (a + b + c + d);
                    double p_yes = (a + b) * (a + c) / ((a + b + c + d) * (a + b + c + d));
                    double p_no = (c + d) * (b + d) / ((a + b + c + d) * (a + b + c + d));
                    double p_e = p_yes + p_no;

                    sumAUC += (p_e < 1) ? (p_0 - p_e) / (1 - p_e) : p_0;
                    count++;
                }
            }
            return sumAUC / count;
        }

        public static double ComputeAccuracy(this int[,] confusionMatrix)
        {
            double sumTP = 0.0;
            for (int i = 0; i < confusionMatrix.GetLength(0); i++)
                sumTP += confusionMatrix[i, i];

            return 1.0 * sumTP / confusionMatrix.Sum();
        }

        public static double ComputeMultiClassPrecision(this int[,] confusionMatrix)
        {
            double sumPrecision = 0.0;
            int count = 0;
            for (int i = 0; i < confusionMatrix.GetLength(0); i++)
            {
                for (int j = i + 1; j < confusionMatrix.GetLength(0); j++)
                {
                    sumPrecision += 1.0 * confusionMatrix[i, i] / (confusionMatrix[i, i] + confusionMatrix[j, i]);
                    count++;
                }
            }
            return sumPrecision / count;
        }

        public static double ComputeMultiClassRecall(this int[,] confusionMatrix)
        {
            double sumPrecision = 0.0;
            int count = 0;
            for (int i = 0; i < confusionMatrix.GetLength(0); i++)
            {
                for (int j = i + 1; j < confusionMatrix.GetLength(0); j++)
                {
                    sumPrecision += 1.0 * confusionMatrix[i, i] / (confusionMatrix[i, i] + confusionMatrix[i, j]);
                    count++;
                }
            }
            return sumPrecision / count;
        }

        public static double ComputeMultiClassF1(this int[,] confusionMatrix)
        {
            double sumPrecision = 0.0;
            int count = 0;
            for (int i = 0; i < confusionMatrix.GetLength(0); i++)
            {
                for (int j = i + 1; j < confusionMatrix.GetLength(0); j++)
                {
                    sumPrecision += 2.0 * confusionMatrix[i, i] / (2.0 * confusionMatrix[i, i] + confusionMatrix[j, i] + confusionMatrix[i, j]);
                    count++;
                }
            }
            return sumPrecision / count;
        }

        public static Tuple<double, double, double, double, double> TestClassifier(this ISupervisedClassifier classifier, IEnumerable<Instance> testingDataset, Feature classFeature)
        {
            int errors = 0;
            var instanceList = testingDataset.ToList();
            int[] trueLabel = new int[instanceList.Count];
            int[] predicted = new int[instanceList.Count];
            for (int i = 0; i < instanceList.Count; i++)
            {
                Instance instance = testingDataset.ElementAt<Instance>(i);
                double[] classification_result = classifier.Classify(instance);
                trueLabel[i] = (int)instance[classFeature];
                predicted[i] = classification_result.ArgMax<double>();
                if (trueLabel[i] != predicted[i])
                {
                    errors++;
                }
            }

            // Computing confusion matrix
            int classCount = classFeature.ClassValues().Length;
            int[,] confusionMatrix = new int[classCount, classCount];
            for (int i = 0; i < trueLabel.Length; i++)
            {
                confusionMatrix[trueLabel[i], predicted[i]]++;
            }

            double acc = 100.0 * (instanceList.Count - errors) / instanceList.Count;
            double auc = ComputeMultiClassAUC(confusionMatrix);
            double precision = ComputeMultiClassPrecision(confusionMatrix);
            double recall = ComputeMultiClassRecall(confusionMatrix);
            double f1 = ComputeMultiClassF1(confusionMatrix);

            return new Tuple<double, double, double, double, double>(acc, auc, precision, recall, f1);
        }

        public static int[,] ComputeConfusionMatrix(this ISupervisedClassifier classifier, IEnumerable<Instance> testingDataset, Feature classFeature)
        {
            int errors = 0;
            var instanceList = testingDataset.ToList();
            int[] trueLabel = new int[instanceList.Count];
            int[] predicted = new int[instanceList.Count];
            for (int i = 0; i < instanceList.Count; i++)
            {
                Instance instance = testingDataset.ElementAt<Instance>(i);
                double[] classification_result = classifier.Classify(instance);
                trueLabel[i] = (int)instance[classFeature];
                predicted[i] = classification_result.ArgMax<double>();
                if (trueLabel[i] != predicted[i])
                {
                    errors++;
                }
            }

            // Computing confusion matrix
            int classCount = classFeature.ClassValues().Length;
            int[,] confusionMatrix = new int[classCount, classCount];
            for (int i = 0; i < trueLabel.Length; i++)
            {
                confusionMatrix[trueLabel[i], predicted[i]]++;
            }

            return confusionMatrix;
        }
    }
}
