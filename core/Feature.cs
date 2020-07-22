using System;
using System.Globalization;
using System.Linq;
using PRFramework.Core.DatasetInfo;

namespace PRFramework.Core.Common
{
    [Serializable]
    public abstract class Feature : IEquatable<Feature>
    {
        protected Feature(string name, int index)
        {
            if (index < 0)
                throw new ArgumentException("Invalid argument: Index cannot be negative");
            Name = name;
            Index = index;
            HasMissingValues = false;
        }

        public string Name { get; set; }
        public bool HasMissingValues { get; set; }

        public abstract FeatureType FeatureType { get; }
        public FeatureInformation FeatureInformation { get; set; }
        public abstract double Parse(string value);

        public int Index { get; set; }

        public abstract bool IsOrdered { get; }

        public string ToString(double value)
        {
            string strValue = FeatureValue.IsMissing(value) ? "?" : ValueToString(value);
            return string.Format("{0}={1}", Name, strValue);
        }

        public abstract string ValueToString(double value);

        public virtual string ValueToStringUnformatted(double value)
        {
            return ValueToString(value);
        }

        public bool Equals(Feature other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Feature) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(Feature left, Feature right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(null, right)) return false;
            return left.Name == right.Name;
        }

        public static bool operator !=(Feature left, Feature right)
        {
            if (ReferenceEquals(left, right)) return false;
            if (ReferenceEquals(null, right)) return true;
            return left.Name != right.Name;
        }
    }

    [Serializable]
    public abstract class CategoricalFeature : Feature
    {
        public CategoricalFeature(string name, int index) : base(name, index)
        {
        }

        public string[] Values { get; set; }

        public override double Parse(string value)
        {
            int idx = Values.IndexOf(x => x == value);
            if (idx == -1)
                throw new ArgumentException(
                    string.Format("Feature value '{0}' is not present in the definition of feature {1}",
                                  value, Name));
            return idx;
        }

        public override string ToString()
        {
            //"'AFeature'['A','B','C']"
            return string.Format("'{0}'[{1}]", Name, string.Join(",", Values.Select(x => "'" + x + "'").ToArray()));
        }

        public override string ValueToString(double value)
        {
            if (double.IsNaN(value))
                return "?";
            return "'" + Values[(int) value] + "'";
        }
    }

    [Serializable]
    public class NominalFeature : CategoricalFeature
    {
        public NominalFeature(string name, int index) : base(name, index)
        {
        }

        public override FeatureType FeatureType
        {
            get { return FeatureType.Nominal; }
        }

        public override bool IsOrdered { get { return false; } }
    }
    
    [Serializable]
    public class OrdinalFeature : CategoricalFeature
    {
        public OrdinalFeature(string name, int index)
            : base(name, index)
        {
        }

        public override FeatureType FeatureType
        {
            get { return FeatureType.Ordinal; }
        }

        public override bool IsOrdered { get { return true; } }

    }
   

    [Serializable]
    public abstract class NumericFeature : Feature
    {
        protected NumericFeature(string name, int index) : base(name, index)
        {
        }

        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public override string ToString()
        {
            if (!double.IsNaN(MinValue) || !double.IsNaN(MaxValue))
                return string.Format("'{0}'[{1}-{2}]", Name, MinValue, MaxValue);
            else
                return string.Format("'{0}'", Name);
        }
        public override bool IsOrdered { get { return true; } }

    }

    [Serializable]
    public class IntegerFeature : NumericFeature
    {
        public IntegerFeature(string name, int index) : base(name, index)
        {
        }

        public override FeatureType FeatureType
        {
            get { return FeatureType.Integer; }
        }

        public override double Parse(string value)
        {
            return int.Parse(value);
        }

        public override string ValueToString(double value)
        {
            if (double.IsNaN(value))
                return "?";
            return ((int) value).ToString();
        }

    }

    [Serializable]
    public class DoubleFeature : NumericFeature
    {
        public DoubleFeature(string name, int index) : base(name, index)
        {
        }

        public override FeatureType FeatureType
        {
            get { return FeatureType.Double; }
        }

        public override double Parse(string value)
        {
            return double.Parse(value, CultureInfo.CreateSpecificCulture("en-US"));
        }

        public override string ValueToString(double value)
        {
            if (double.IsNaN(value))
                return "?";

            //var str = value.ToString();
            //if (str.Length - str.IndexOf('.') > 14)
            //{
            //    int i = str.Length - 2;
            //    for (; i > 3 && str[i] == str[i - 1]; i--) ;
            //    str = str.Substring(0, i + 1);
            //}

            return value.ToString();
        }

        public override string ValueToStringUnformatted(double value)
        {
            if (double.IsNaN(value))
                return "?";
            return value.ToString();
        }
    }
        
    public enum FeatureType
    {
        Integer,
        Nominal,
        Double,
        Fuzzy,
        Ordinal,
    }
}