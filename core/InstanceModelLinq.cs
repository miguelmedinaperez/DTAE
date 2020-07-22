using System.Linq;
using PRFramework.Core.Common;

namespace PRFramework.Core.SupervisedClassifiers
{
    public static class InstanceModelLinq
    {
        public static Feature ClassFeature(this InstanceModel model)
        {
            return model.Features.FirstOrDefault(x => x.Name.ToLower() == "class");
        }

        public static string[] ClassValues(this Feature feature)
        {
            return (feature as NominalFeature).Values;
        }
    }
}
