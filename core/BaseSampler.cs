using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PRFramework.Core.Common;

namespace PRFramework.Core.Samplers.Instances
{
    [Serializable]
    public class BaseSampler
    {
        private Dictionary<string, string> DecodeParameters(string parameters, char separator)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            var allParams = parameters.Split(new char[] { separator });
            foreach (string param in allParams)
            {
                var match = Regex.Match(param, "(.*)=(.*)");
                if (match.Success)
                    result.Add(match.Groups[1].Value.Trim(),
                               match.Groups[2].Value.Trim());
            }
            return result;
        }

        private string EncodeParameters()
        {
            var comps = new List<string>();
            foreach (var info in GetType().GetProperties())
            {
                string propName = info.Name;
                if (propName != "State")
                    comps.Add(string.Format("{0}={1}", propName,
                                            ObjectPropertyHelper.GetPropertyValue(this, propName)));
            }
            return string.Join("_", comps.ToArray());
        }

        public string State
        {
            get {
                return EncodeParameters();
            }
            set
            {
                var parameters = DecodeParameters(value, '_');
                foreach (var pair in parameters)
                    ObjectPropertyHelper.SetPropertyValue(this, pair.Key, pair.Value);
            }
        }
    }
}