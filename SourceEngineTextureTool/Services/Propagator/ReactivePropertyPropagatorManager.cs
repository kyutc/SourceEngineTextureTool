using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ReactiveUI;

namespace SourceEngineTextureTool.Services.Propagator;

/// <summary>
/// Aggregate manager of <see cref="IPropagationSequence"/>.
/// Assists with instantiating a <see cref="ReactivePropertyPropagationSequence{TSource,TPropertyValue}"/>
/// from a homogenous list.
/// </summary>
public class ReactivePropertyPropagatorManager
{
    private readonly List<IPropagationSequence> _reactivePropertyPropagatorSequences = new();
    private PropagationStrategy _propagationStrategy = PropagationStrategy.FromFirstInSequence;

    /// <summary>
    /// Gets/Sets the <see cref="PropagationStrategy"/> for all propagation sequences managed by this instance.
    /// </summary>
    public PropagationStrategy PropagationStrategy
    {
        get => _propagationStrategy;
        set
        {
            _propagationStrategy = value;
            _reactivePropertyPropagatorSequences.ForEach(rpps => rpps.ConfigurePropagationRules(PropagationStrategy));
        }
    }

    /// <summary>
    /// Dereferences all <see cref="IPropagationSequence"/> managed by this instance.
    /// </summary>
    public void Clear()
    {
        _reactivePropertyPropagatorSequences.Clear();
    }

    /// <summary>
    /// Creates and stores a <see cref="ReactivePropertyPropagationSequence{TSource,TPropertyValue}"/> from the
    /// provided list and expression.
    /// </summary>
    /// <param name="items">The items to watch.</param>
    /// <param name="property">The expression used to access the property to propagate.</param>
    /// <typeparam name="TSource">Subclass of <see cref="ReactiveObject"/>.</typeparam>
    /// <typeparam name="TPropertyValue">The type of the property being propagated.</typeparam>
    public void InitializePropertyPropagationSequence<TSource, TPropertyValue>(IEnumerable<TSource> items,
        Expression<Func<TSource, TPropertyValue>> property)
        where TSource : ReactiveObject
    {
        if (items.Count() < 2)
        {
            return; // At least 2 needed for a sequence
        }

        var newReactivePropertyPropagators =
            new ReactivePropertyPropagationSequence<TSource, TPropertyValue>(items, property, PropagationStrategy);
        _reactivePropertyPropagatorSequences.Add(newReactivePropertyPropagators);
    }
}