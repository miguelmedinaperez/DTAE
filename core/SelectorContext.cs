namespace PRFramework.Core.SupervisedClassifiers.DecisionTrees
{
    public class SelectorContext
    {
        public IChildSelector Selector { get; set; }
        public int Index { get; set; }
    }
}