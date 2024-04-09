using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ReactiveUI;

namespace SourceEngineTextureTool.Services.Propagator;

/// <summary>
/// Stores and handles an immutable collection of associated <see cref="ReactivePropertyPropagator{TSource,TPropertyValue}"/>.
/// </summary>
/// <typeparam name="TSource"></typeparam>
/// <typeparam name="TPropertyValue"></typeparam>
public class ReactivePropertyPropagationSequence<TSource, TPropertyValue> : IPropagationSequence
    where TSource : ReactiveObject
{
    private readonly List<ReactivePropertyPropagator<TSource, TPropertyValue>> _reactivePropertyPropagators = new();

    /// <summary>
    /// Creates an immutable series of <see cref="ReactivePropertyPropagator{TSource,TPropertyValue}"/>.
    /// </summary>
    /// <param name="items">The items to propagate to/from.</param>
    /// <param name="fromProperty">The expression to access the property being propagated from.</param>
    /// <param name="toProperty">The expression to access the property being propagated to.</param>
    /// <param name="propagationStrategy">How changes should propagate through the items.</param>
    public ReactivePropertyPropagationSequence(
        IEnumerable<TSource> items,
        Expression<Func<TSource, TPropertyValue>> fromProperty,
        Expression<Func<TSource, TPropertyValue>> toProperty,
        PropagationStrategy propagationStrategy = PropagationStrategy.DoNotPropagate)
    {
        PropertyInfo propertyInfo = _GetPropertyInfo(toProperty);
        foreach (var item in items)
        {
            var observer = item.WhenAnyValue(fromProperty);
            _reactivePropertyPropagators.Add(
                new ReactivePropertyPropagator<TSource, TPropertyValue>(item, propertyInfo, observer));
        }

        if (propagationStrategy is not PropagationStrategy.DoNotPropagate)
        {
            ConfigurePropagationRules(propagationStrategy);
        }
    }

    /// <summary>
    /// Sets the <see cref="PropagationStrategy"/> for this sequence.
    /// </summary>
    /// <param name="propagationStrategy">How changes should propagate through the items.</param>
    public void ConfigurePropagationRules(PropagationStrategy propagationStrategy)
    {
        switch (propagationStrategy)
        {
            case PropagationStrategy.DoNotPropagate:
                _reactivePropertyPropagators.ForEach(rpmo => rpmo.PropagateFrom(null));
                break;
            case PropagationStrategy.FromFirstInSequence:
                _reactivePropertyPropagators.Skip(1).ToList()
                    .ForEach(rpmo => rpmo.PropagateFrom(_reactivePropertyPropagators[0]));
                break;
            case PropagationStrategy.FromPreviousInSequence:
                int index = 0;
                _reactivePropertyPropagators.Skip(1).ToList()
                    .ForEach(rpmo => rpmo.PropagateFrom(_reactivePropertyPropagators[index++]));
                break;
        }
    }

    /// <summary>
    /// Get the <see cref="PropertyInfo"/> of the property described by an expression.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="propertyLambda"></param>
    /// <returns>The member's <see cref="PropertyInfo"/>.</returns>
    /// <remarks>
    /// Since ReactiveUI couldn't create an observable from an expression utilizing reflection, the caller must provide
    /// an expression for accessing the target property. Assignment still requires reflection, however, so a
    /// <see cref="PropertyInfo"/> is still needed. This implementation is a succinct answer from
    /// https://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression
    /// </remarks>
    protected static PropertyInfo _GetPropertyInfo(Expression<Func<TSource, TPropertyValue>> propertyLambda)
    {
        LambdaExpression lambdaExpression = propertyLambda;
        MemberExpression memberExpression = lambdaExpression.Body is UnaryExpression expression
            ? (MemberExpression)expression.Operand
            : (MemberExpression)lambdaExpression.Body;
        return (PropertyInfo)memberExpression.Member;
    }
}