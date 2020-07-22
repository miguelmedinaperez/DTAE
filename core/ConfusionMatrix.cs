using System;
using System.Linq;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.Evaluators
{
    public class BasicEvaluation
    {
        public int TP;
        public int TN;
        public int FP;
        public int FN;

        public double TPrate;
        public double TNrate;
        public double FPrate;
        public double FNrate;

        public double specificity;
        public double sensitivity;
        public double precision;
        public double recall;
        public double Yrate;
    }

    [Serializable]
    public class ConfusionMatrix
    {
        /**
         *       Example of Matrix of Confusion
         * 
         *                      Actual
         * Predicted       |  P  |  N  |
         *               P |  0  |  5  |
         *               N |  1  |  0  |
         *               U |  5  |  1  | <- The last row correspond to the abstentions 
         * 
         * 
         * */

        private readonly string[] lettersposition = Enumerable.Range(0, 26).Select(x => Convert.ToChar('a' + x).ToString()).ToArray();
        private readonly string[] classes;
        public int[,] Matrix { get; private set; }

        public ConfusionMatrix(string[] classes)
        {
            this.classes = classes;
            int numClass = classes.Length;
            Matrix = new int[numClass + 1, numClass];
        }

        public long Rows { get { return Matrix.GetLongLength(0); } }
        public long Columns { get { return Matrix.GetLongLength(1); } }

        public int this[int row, int colum]
        {
            get { return Matrix[row, colum]; }
            set { Matrix[row, colum] = value; }
        }

        public override string ToString()
        {
            string lettersClasses = "";
            for (int i = 0; i < classes.Length; i++)
                lettersClasses += lettersposition[i] + "\t";

            string result = string.Format("=== Confusion Matrix ===\r\n\n{0}<-- classified as\r\n", lettersClasses);
            for (int row = 0; row < Matrix.GetLongLength(0); row++)
            {
                for (int col = 0; col < Matrix.GetLongLength(1); col++)
                    result += string.Format("{0} \t", Matrix[row, col]);
                result = result.Remove(result.LastIndexOfAny(new[] { '\t' }));
                if (row < Matrix.GetLongLength(0) - 1)
                    result += string.Format("\t| {0} = {1}", lettersposition[row], classes[row]);
                if (row == Matrix.GetLongLength(0) - 1)
                    result += "\t| The last row correspond to the abstentions";
                result += "\n";
            }
            return result;
        }

        public static ConfusionMatrix operator +(ConfusionMatrix cm1, ConfusionMatrix cm2)
        {
            if (cm1 == null)
                return cm2;
            if (cm2 == null)
                return cm1;
            if (cm1.Matrix.Length != cm2.Matrix.Length)
                throw new Exception("Matrix No Match");

            int[,] result = (int[,])cm1.Matrix.Clone();

            for (int i = 0; i < cm2.Matrix.GetLongLength(0); i++)
                for (int j = 0; j < cm2.Matrix.GetLongLength(1); j++)
                    result[i, j] += cm2[i, j];

            return new ConfusionMatrix(cm1.classes)
            {
                Matrix = result
            };
        }

        public BasicEvaluation ComputeBasicEvaluation(int positiveClass)
        {
            var basicEvaluation = new BasicEvaluation();
            var N = Matrix.Sum();
            basicEvaluation.TP = Matrix[positiveClass, positiveClass];

            for (int i = 0; i < Matrix.GetLongLength(1); i++)
                basicEvaluation.TN += Matrix[i, i];
            basicEvaluation.TN -= basicEvaluation.TP;

            for (int i = 0; i < Matrix.GetLongLength(1); i++)
                basicEvaluation.FP += Matrix[positiveClass, i];
            basicEvaluation.FP -= basicEvaluation.TP;
            //to add abstentions
            basicEvaluation.FP += Matrix[Matrix.GetLongLength(0) - 1, positiveClass];

            basicEvaluation.FN = N - (basicEvaluation.TP + basicEvaluation.FP + basicEvaluation.TN);

            basicEvaluation.TPrate = basicEvaluation.TP * 1.0 / (basicEvaluation.TP + basicEvaluation.FN);
            basicEvaluation.TNrate = basicEvaluation.TN * 1.0 / (basicEvaluation.TN + basicEvaluation.FP);
            basicEvaluation.FPrate = basicEvaluation.FP * 1.0 / (basicEvaluation.TN + basicEvaluation.FP);
            basicEvaluation.FNrate = basicEvaluation.FN * 1.0 / (basicEvaluation.TP + basicEvaluation.FN);

            basicEvaluation.sensitivity = basicEvaluation.TPrate;
            basicEvaluation.specificity = basicEvaluation.TNrate;

            basicEvaluation.precision = basicEvaluation.TP * 1.0 / (basicEvaluation.TP + basicEvaluation.FP);
            basicEvaluation.recall = basicEvaluation.TPrate;

            basicEvaluation.Yrate = (basicEvaluation.TP + basicEvaluation.FP) * 1.0 / N;

            return basicEvaluation;
        }

    }
}
