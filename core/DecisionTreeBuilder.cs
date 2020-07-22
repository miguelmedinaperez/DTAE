using System;
using System.Collections.Generic;
using System.Linq;
using PRFramework.Core.Common;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.DistributionEvaluators;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.DistributionTesters;
using PRFramework.Core.SupervisedClassifiers.DecisionTrees.SplitIteratorProviders;

namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees.Builder
{
    public interface IDecisionTreeBuilder
    {
        IDistributionEvaluator DistributionEvaluator { get; set; }
        IDistributionTester StopCondition { get; set; }
        IPruneTester PruneTester { get; set; }
        bool PruneResult { get; set; }
        Action<IDecisionTreeNode, ISplitIterator, List<SelectorContext>> OnSplitEvaluation { get; set; }
        ISplitIteratorProvider SplitIteratorProvider { get; set; }
        double MinimalInstanceMembership { get; set; }
        double MinimalSplitGain { get; set; }
        int MinimalObjByLeaf { get; set; }
        Func<IDecisionTreeNode, int, int> OnSelectingWhichBetterSplit { get; set; }
        Func<IEnumerable<Tuple<Instance, double>>, InstanceModel, Feature, double[]> InitialDistributionCalculator { get; set; }
        DecisionTree Build(InstanceModel model, IEnumerable<Instance> instances, Feature classFeature);
        DecisionTree Build(InstanceModel model, IEnumerable<Tuple<Instance, double>> objMembership, Feature classFeature);
        void SetOnSelectingFeaturesToConsider(Func<IEnumerable<Feature>, int, IEnumerable<Feature>> func);
        Func<IEnumerable<Feature>, int, IEnumerable<Feature>> OnSelectingFeaturesToConsider { get; set; }
        void SetCanAcceptChildSelector(Func<IChildSelector, int, bool> func);
    }

    [Serializable]
    public class DecisionTreeBuilder : IDecisionTreeBuilder
    {
        public IDistributionEvaluator DistributionEvaluator { get; set; }

        public IDistributionTester StopCondition { get; set; }

        public IPruneTester PruneTester { get; set; }

        public bool PruneResult { get; set; }

        public Action<IDecisionTreeNode, ISplitIterator, List<SelectorContext>> OnSplitEvaluation { get; set; }

        public DecisionTreeBuilder()
        {
            MinimalSplitGain = 1e-30;
            MinimalInstanceMembership = 0.05;
            MinimalObjByLeaf = 2;
            MaxDepth = -1;
            InitialDistributionCalculator = (instances, model, feature) => instances.FindDistribution(feature);
            SplitIteratorProvider = new StandardSplitIteratorProvider();
            StopCondition = new PureNodeStopCondition();
            DistributionEvaluator = new Twoing();
            OnSelectingFeaturesToConsider = (f, level) => f;

        }

        public ISplitIteratorProvider SplitIteratorProvider { get; set; }

        public double MinimalInstanceMembership { get; set; }

        public double MinimalSplitGain { get; set; }

        public int MinimalObjByLeaf { get; set; }

        public Func<IDecisionTreeNode, int, int> OnSelectingWhichBetterSplit { get; set; }
        public Func<IEnumerable<Tuple<Instance, double>>, InstanceModel, Feature, double[]> InitialDistributionCalculator { get; set; }

        public int MaxDepth { get; set; }

        public DecisionTree Build(InstanceModel model, IEnumerable<Instance> instances, Feature classFeature)
        {
            DecisionTree result = Build(model, instances.Select(x => Tuple.Create(x, 1d)), classFeature);
            result.Model = model;
            return result;
        }

        public DecisionTree Build(InstanceModel model, IEnumerable<Tuple<Instance, double>> objMembership,
            Feature classFeature)
        {
            List<SelectorContext> currentContext = new List<SelectorContext>();
            if (MinimalSplitGain <= 0)
                throw new InvalidOperationException("MinimalSplitGain must be positive");
            if (SplitIteratorProvider == null)
                throw new InvalidOperationException("SplitIteratorProvider not defined");

            DecisionTree result = new DecisionTree
            {
                Model = model,
            };

            var filteredObjMembership = objMembership.Where(x => x.Item2 >= MinimalInstanceMembership);

            double[] parentDistribution = InitialDistributionCalculator(filteredObjMembership, model, classFeature);
            result.TreeRootNode = new DecisionTreeNode(parentDistribution);

            FillNode(result.TreeRootNode, model, filteredObjMembership, classFeature, 0, currentContext);

            if (PruneResult && PruneTester != null)
                new DecisionTreePrunner { PruneTester = PruneTester }.Prune(result);

            return result;
        }

        public Func<IChildSelector, int, bool> CanAcceptChildSelector = (x, level) => true;


        private void FillNode(IDecisionTreeNode node, InstanceModel model, IEnumerable<Tuple<Instance, double>> instances,
            Feature classFeature, int level, List<SelectorContext> currentContext)
        {
            if (StopCondition.Test(node.Data, model, classFeature))
                return;
            if (MaxDepth >= 0 && level >= MaxDepth - 1)
                return;
            if (node.Data.Sum() <= MinimalObjByLeaf)
                return;

            int whichBetterToFind = 1;
            if (OnSelectingWhichBetterSplit != null)
                whichBetterToFind = OnSelectingWhichBetterSplit(node, level);
            WiningSplitSelector winingSplitSelector = new WiningSplitSelector(whichBetterToFind)
            {
                CanAcceptChildSelector = this.CanAcceptChildSelector,
            };
            foreach (var feature in OnSelectingFeaturesToConsider(model.Features, level))
                if (feature != classFeature)
                {
                    ISplitIterator splitIterator = SplitIteratorProvider.GetSplitIterator(model, feature, classFeature);
                    if (splitIterator == null)
                        throw new InvalidOperationException(string.Format("Undefined iterator for feature {0}",
                            feature));
                    splitIterator.Initialize(feature, instances);
                    while (splitIterator.FindNext())
                    {
                        double currentGain = DistributionEvaluator.Evaluate(node.Data,
                                                                            splitIterator.CurrentDistribution);
                        if (currentGain >= MinimalSplitGain)
                        {
                            if (OnSplitEvaluation != null)
                                OnSplitEvaluation(node, splitIterator, currentContext);
                            winingSplitSelector.EvaluateThis(currentGain, splitIterator, level);
                        }
                    }
                }
            if (winingSplitSelector.IsWinner())
            {
                IChildSelector maxSelector = winingSplitSelector.WinningSelector;
                node.ChildSelector = maxSelector;
                node.Children = new IDecisionTreeNode[maxSelector.ChildrenCount];
                var instancesPerChildNode =
                    childrenInstanceCreator.CreateChildrenInstances(instances, maxSelector, MinimalInstanceMembership);

                for (int i = 0; i < maxSelector.ChildrenCount; i++)
                {
                    var childNode = new DecisionTreeNode { Parent = node };
                    node.Children[i] = childNode;
                    childNode.Data = winingSplitSelector.WinningDistribution[i];
                    SelectorContext context = null;
                    if (OnSplitEvaluation != null)
                    {
                        context = new SelectorContext
                        {
                            Index = i,
                            Selector = node.ChildSelector,
                        };
                        currentContext.Add(context);
                    }
                    FillNode(childNode, model, instancesPerChildNode[i], classFeature, level + 1, currentContext);
                    if (OnSplitEvaluation != null)
                        currentContext.Remove(context);

                }
            }
        }
        public void SetOnSelectingFeaturesToConsider(Func<IEnumerable<Feature>, int, IEnumerable<Feature>> func)
        {
            OnSelectingFeaturesToConsider = func;
        }

        public Func<IEnumerable<Feature>, int, IEnumerable<Feature>> OnSelectingFeaturesToConsider { get; set; }

        public void SetCanAcceptChildSelector(Func<IChildSelector, int, bool> func)
        {
            CanAcceptChildSelector = func;
        }
        ChildrenInstanceCreator childrenInstanceCreator = new ChildrenInstanceCreator();
    }
}