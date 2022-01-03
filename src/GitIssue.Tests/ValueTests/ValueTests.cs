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
        public bool HasConverter(Type type)
        {
            var converter = TypeDescriptor.GetConverter(typeof(TValue));
            return converter.CanConvertFrom(type);
        }

        public bool HasConverter<TIn>()
        {
            return HasConverter(typeof(TIn));
        }

        public object UseConverter(object input)
        {
            var converter = TypeDescriptor.GetConverter(typeof(TValue));
            if (converter.CanConvertFrom(input.GetType()))
                return converter.ConvertFrom(input);
            Assert.Fail($"Failed to convert from {input.GetType()} to {typeof(TValue)}");
            return default!;
        }

        public TValue UseConverter<TIn>(TIn input)
        {
            return (TValue)UseConverter((object)input!);
        }

        public new virtual bool Equals(object first, object second)
        {
            var objEquals = first.Equals(second);
            var hashEquals = first?.GetHashCode() == second?.GetHashCode();
            Assert.That(objEquals, Is.EqualTo(hashEquals));
            return objEquals;
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
            return GetEnumerator();
        }
    }
}