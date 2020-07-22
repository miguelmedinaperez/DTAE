using System;
using System.Linq;
using PRFramework.Core.DatasetInfo;

namespace PRFramework.Core.Common
{
    [Serializable]
    public class InstanceModel
    {

        public Feature[] Features { get; set;}

        public string RelationName { get; set; }
        public DatasetInformation DatasetInformation { get; set; }

        public Instance CreateInstance()
        {
            Instance value = new Instance(this);
            return value;
        }

        public Instance CreateInstance(params object[] values)
        {
            Instance instance = CreateInstance();
            instance.Initialize(values);
            return instance;
        }

        public Feature this[string featureName]
        {
            get
            {
                Feature feature = Features.FirstOrDefault(x => x.Name == featureName);
                if (feature == null)
                    throw new InvalidOperationException(string.Format("Feature '{0}' does not exists", featureName));
                return feature;
            }
        }

        public Feature this[int featureIdx]
        {
            get
            {
                return Features[featureIdx];
            }
        }
    }

}
