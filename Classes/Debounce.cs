using System;
using System.Reactive.Linq;
using Reactive.Bindings;

public class DebounceProperty<T>
{
    public Reactive.Bindings.ReactiveProperty<T> Input { get; }
    public Reactive.Bindings.ReactiveProperty<T> Output { get; }

    public DebounceProperty(TimeSpan time)
    {
        Input = new Reactive.Bindings.ReactiveProperty<T>();
        Output = Input.Sample(time).ToReactiveProperty<T>();
    }

    public DebounceProperty(T value, TimeSpan time)
    {
        Input = new Reactive.Bindings.ReactiveProperty<T>(value);
        Output = Input.Sample(time).ToReactiveProperty<T>();
    }
}