using System.Collections.Generic;
using PRFramework.Core.Common;

namespace PRFramework.Core.Samplers.Instances
{
    public interface ISampler
    {
        List<InstanceSample> Sample(IEnumerable<Instance> instances);
        InstanceSample GetSample(IEnumerable<Instance> instances, string samplerId);
        string State { get; set; }
        int Seed { get; set; }
    }
}