/*
 * Created by: Miguel Angel Medina Pérez (miguelmedinaperez@gmail.com)
 * Created: 11/10/2016
 * Comments by: Miguel Angel Medina Pérez (miguelmedinaperez@gmail.com)
 */

using PRFramework.Core.SupervisedClassifiers;
using System.Collections.Generic;
using System.Linq;

namespace PRFramework.Core.Common
{
    /// <summary>
    ///     A wrapper of <see cref="Instance"/> which implements <see cref="IVector"/> interface.
    /// </summary>
    public class InstanceVector : IVector
    {
        public double this[int index]
        {
            get { return _innerInstance.Values[index]; }
            set { _innerInstance.Values[index] = value; }
        }

        public int Length => _features.Count;

        public static implicit operator Instance(InstanceVector vector)
        {
            return vector._innerInstance;
        }

        public InstanceVector(Instance innerInstance)
        {
            _innerInstance = innerInstance;
            var classFeature = _innerInstance.Model.ClassFeature();
            _features = _innerInstance.Model.Features.Where(f => f != classFeature).ToList();
        }
        
        private readonly Instance _innerInstance;
        private readonly List<Feature> _features;
    }
}
