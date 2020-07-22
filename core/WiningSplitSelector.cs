using System;
using System.Collections.Generic;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.Builder
{
    public class WiningSplitSelector
    {
        private readonly int _whichBetterToFind;

        public IChildSelector WinningSelector
        {
            get
            {
                int index = Math.Min(_whichBetterToFind - 1, list.Count - 1);
                return list[index].Item2;
            }
        }

        public double[][] WinningDistribution
        {
            get
            {
                int index = Math.Min(_whichBetterToFind - 1, list.Count - 1);
                return list[index].Item3;
            }
        }

        private List<Tuple<double, IChildSelector, double[][]>> list =
            new List<Tuple<double, IChildSelector, double[][]>>();

        public double minStoredValue = double.MinValue;

        public WiningSplitSelector(int whichBetterToFind = 1)
        {
            if (whichBetterToFind <= 0)
                throw new ArgumentException("WhichBetterToFind must be positive");
            _whichBetterToFind = whichBetterToFind;
        }

        public bool EvaluateThis(double currentGain, ISplitIterator splitIterator, int level)
        {
            if (list.Count < _whichBetterToFind || currentGain > minStoredValue)
            {
                IChildSelector currentChildSelector = splitIterator.CreateCurrentChildSelector();
                if (CanAcceptChildSelector(currentChildSelector, level))
                {
                    list.Add(Tuple.Create(currentGain, currentChildSelector,
                                          splitIterator.CurrentDistribution.CloneArray()));
                    list.Sort(comparer);
                    if (list.Count > _whichBetterToFind)
                        list.RemoveAt(_whichBetterToFind);
                    int index = Math.Min(_whichBetterToFind - 1, list.Count - 1);
                    minStoredValue = list[index].Item1;
                    return true;
                }
            }
            return false;
        }

        public Func<IChildSelector, int, bool> CanAcceptChildSelector = (x,level) => true;

        public bool IsWinner()
        {
            return list.Count > 0;
        }

        private class CustomComparer : IComparer<Tuple<double, IChildSelector, double[][]>>
        {
            public int Compare(Tuple<double, IChildSelector, double[][]> x, Tuple<double, IChildSelector, double[][]> y)
            {
                return Math.Sign(y.Item1 - x.Item1);
            }
        }
        private static CustomComparer comparer = new CustomComparer();
    }
}