using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using GitIssue.Issues.Json;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    public abstract class ValueTests<TValue> where TValue : IValue
    {
        public new virtual bool Equals(object first, object second)
        {
            bool objEquals = first.Equals(second);
            bool hashEquals = first?.GetHashCode() == second?.GetHashCode();
            Assert.That(objEquals, Is.EqualTo(hashEquals));
            return objEquals;
        }

        public bool HasConverter(Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TValue));
            return converter.CanConvertFrom(type);
        }

        public bool HasConverter<TIn>()
        {
            return this.HasConverter(typeof(TIn));
        }

        public object UseConverter(object input)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TValue));
            if (converter.CanConvertFrom(input.GetType()))
            {
                object? result = converter.ConvertFrom(input);
                if (result != null)
                {
                    return result;
                }
            }

            Assert.Fail($"Failed to convert from {input.GetType()} to {typeof(TValue)}");
            return default(object)!;
        }

        public TValue UseConverter<TIn>(TIn input)
        {
            return (TValue)this.UseConverter((object)input!);
        }
    }

    public abstract class ValueTests<TValue, TBacking> : ValueTests<TValue>
        where TValue : IValue, IValue<TBacking>
    {
        public virtual TBacking GetItem(TValue value)
        {
            return value.Item;
        }
    }

    public abstract class JsonValueTests<TValue> : ValueTests<TValue>
        where TValue : IValue, IJsonValue
    {
        public virtual string ConvertToJson(TValue value)
        {
            return value.ToJson().ToString(Newtonsoft.Json.Formatting.None);
        }
    }

    public abstract class JsonValueTests<TValue, TBacking> : ValueTests<TValue, TBacking>
        where TValue : IValue, IValue<TBacking>, IJsonValue
    {
        public virtual string ConvertToJson(TValue value)
        {
            return value.ToJson().ToString(Newtonsoft.Json.Formatting.None);
        }
    }

    public abstract class ValueTestCases : IEnumerable<TestCaseData>
    {
        public abstract IEnumerator<TestCaseData> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}