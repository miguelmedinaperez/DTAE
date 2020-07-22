using System.Collections.Generic;
using PRFramework.Core.Common;

namespace PRFramework.Core.Samplers.Instances
{
    public class InstanceSample
    {
        public string Name { get; set; }
        public List<Instance> Training { get; set; }
        public List<Instance> Test { get; set; }
    }
}
