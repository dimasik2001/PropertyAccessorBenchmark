using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using PropertyAccessorBenchmark;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

[Config(typeof(Config))]
[SimpleJob(RuntimeMoniker.Net70)]
[SimpleJob(RuntimeMoniker.Net48)]
[MemoryDiagnoser]
public class PropertyAccessorBenchmarks
{
    private readonly SampleClass _sampleObject = new SampleClass { Property = "Initial Value" };
    private readonly NonStaticPropertyAccessor<SampleClass> _nonStaticPropertyAccessor= new NonStaticPropertyAccessor<SampleClass>();
    private readonly Type _type = typeof(SampleClass);
    private readonly string Value = "Value";
    private const string NameOfProperty = nameof(SampleClass.Property);
    //[Benchmark]
    public object GetProperty()
    {
        return _sampleObject.Property;
    }

   // [Benchmark(Baseline = true)]
    public void SetProperty()
    {
        _sampleObject.Property = Value;
    }
   // [Benchmark]
    public object GetPropertyAccessorOld()
    {
        return PropertyAccessorOld.GetProperty(_sampleObject, NameOfProperty);
    }

   // [Benchmark]
    public void SetPropertyAccessorOld()
    {
        PropertyAccessorOld.SetProperty(_sampleObject, NameOfProperty, Value);
    }

    [Benchmark]
    public object GetPropertyAccessorStaticGeneric()
    {
        return StaticGenericPropertyAccessor<SampleClass>.GetProperty(_sampleObject, NameOfProperty);
    }

    [Benchmark]
    public void SetPropertyAccessorStaticGeneric()
    {
        StaticGenericPropertyAccessor<SampleClass>.SetProperty(_sampleObject, NameOfProperty, Value);
    }
    [Benchmark]
    public object GetPropertyAccessorNonStatic()
    {
        return _nonStaticPropertyAccessor.GetProperty(_sampleObject, NameOfProperty);
    }

    [Benchmark]
    public void SetPropertyAccessorNonStatic()
    {
        _nonStaticPropertyAccessor.SetProperty(_sampleObject, NameOfProperty, Value);
    }
    [Benchmark]
    public object GetPropertyAccessorUpdated()
    {
        return PropertyAccessorUpdated.GetProperty(_sampleObject, NameOfProperty);
    }

    [Benchmark]
    public void SetPropertyAccessorUpdated()
    {
        PropertyAccessorUpdated.SetProperty(_sampleObject, NameOfProperty, Value);
    }
    //[Benchmark]
    public object GetPropertyReflection()
    {
        return _sampleObject.GetType().GetProperty(NameOfProperty).GetValue(_sampleObject);
    }

   // [Benchmark]
    public void SetPropertyReflection()
    {
        _sampleObject.GetType().GetProperty(NameOfProperty).SetValue(_sampleObject, Value);
    }
   // [Benchmark]
    public object GetPropertyReflectionCached()
    {
        return _type.GetProperty(NameOfProperty).GetValue(_sampleObject);
    }

    //[Benchmark]
    public void SetPropertyReflectionCached()
    {
        _type.GetProperty(NameOfProperty).SetValue(_sampleObject, Value);
    }
}

public class SampleClass
{
    public virtual string Property { get; set; }
}
public class SampleChild : SampleClass
{
    public override string Property { get => "overrided " + base.Property; set => base.Property = value + " overrided"; }
    public int Age { get; set; }
}

public class Config : ManualConfig
{
    public Config()
    {
        AddLogger(ConsoleLogger.Default);
        SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
    }
}

class Program
{
    static void Main()
    {
        //Test();
        //Test2();
        BenchmarkRunner.Run<PropertyAccessorBenchmarks>();
        Console.ReadLine();
    }

    private static void Test2()
    {
        var parent = new SampleClass();
        var child = new SampleChild();
        var propertyName = nameof(SampleClass.Property);
        var accessor = new NonStaticPropertyAccessor<SampleClass>();
        accessor.SetProperty(child, propertyName, "set child");
        Console.WriteLine(accessor.GetProperty(child, propertyName));
    }

    private static void Test()
    {
        var obj = new SampleClass();
        var accessor = new NonStaticPropertyAccessor<SampleClass>();
        var propertyName = nameof(SampleClass.Property);
        PropertyAccessorOld.SetProperty(obj, propertyName, "set old");
        Console.WriteLine(PropertyAccessorOld.GetProperty(obj, propertyName));

        StaticGenericPropertyAccessor<SampleClass>.SetProperty(obj, propertyName, "set static generic");
        Console.WriteLine(StaticGenericPropertyAccessor<SampleClass>.GetProperty(obj, propertyName));

        accessor.SetProperty(obj, propertyName, "set non static");
        Console.WriteLine(accessor.GetProperty(obj, propertyName));

    }
}