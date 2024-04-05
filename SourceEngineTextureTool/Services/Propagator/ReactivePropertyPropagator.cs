using System;
using System.Reflection;
using ReactiveUI;

namespace SourceEngineTextureTool.Services.Propagator;

/// <summary>
/// <see cref="ReactivePropertyPropagator{TSource,TPropertyValue}"/> utilizes the behavior of <see cref="ReactiveObject"/> to
/// monitor and update itself with the changes made to another <see cref="ReactiveObject"/>
/// contained in another <see cref="ReactivePropertyPropagator{TSource,TPropertyValue}"/>.
/// These interactions can be chained together to create a sequence, or more simply instantiated via
/// <see cref="ReactivePropertyPropagationSequence{TSource,TPropertyValue}"/>.
/// </summary>
/// <typeparam name="TSource">Any object implementing <see cref="ReactiveObject"/>.</typeparam>
/// <typeparam name="TPropertyValue">Any value or reference type.</typeparam>
public class ReactivePropertyPropagator<TSource, TPropertyValue> where TSource : ReactiveObject
{
    private TSource _target;
    private PropertyInfo _targetPropertyInfo;
    private IObservable<TPropertyValue?> _targetPropertyObserver;

    private IDisposable? _onProviderPropertyChangedSubscription;

    /// <summary>
    /// Creates an entity that can propagate changes observed from another <see cref="ReactivePropertyPropagator"/>
    /// </summary>
    /// <param name="propertyOwner"></param>
    /// <param name="propertyInfo"></param>
    /// <param name="propertyObserver"></param>
    public ReactivePropertyPropagator(
        TSource propertyOwner,
        PropertyInfo propertyInfo,
        IObservable<TPropertyValue?> propertyObserver)
    {
        _target = propertyOwner;
        _targetPropertyInfo = propertyInfo;
        _targetPropertyObserver = propertyObserver;
    }

    public void Deconstruct()
    {
        PropagateFrom(null);
    }

    /// <summary>
    /// Gets this instance's <see cref="Observable"/> that provides it's most recent <see cref="TPropertyValue"/>
    /// </summary>
    public IObservable<TPropertyValue?> Observable
    {
        get => _targetPropertyObserver;
    }

    /// <summary>
    /// Set this entity to apply changes to itself that it observed from elsewhere.
    /// </summary>
    /// <param name="provider">The <see cref="ReactivePropertyPropagator{TSource,TPropertyValue}"/> to watch.</param>
    /// <remarks>Monkey see? Monkey do.</remarks>
    public void PropagateFrom(ReactivePropertyPropagator<TSource, TPropertyValue?>? provider)
    {
        _onProviderPropertyChangedSubscription?.Dispose();

        if (provider is not null)
        {
            _onProviderPropertyChangedSubscription = provider.Observable.Subscribe(newPropertyValue =>
            {
                _targetPropertyInfo.SetValue(_target, newPropertyValue);
            });
        }
    }
}