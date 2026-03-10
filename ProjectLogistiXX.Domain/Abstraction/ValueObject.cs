using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.ValueObjects
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public bool Equals(ValueObject? other)
        {
            if (other is null || GetType() != other.GetType())
            {
                return false; 
            }
            
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ValueObject)
            {
                return false; 
            }
            return Equals((ValueObject)obj);
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(17, (current, obj) => current * 31 + (obj?.GetHashCode() ?? 0));
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null && right is null)
            {
                return true; 
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !(left == right);
        }
    }
}
