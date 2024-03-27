namespace SourceEngineTextureTool.Services.Propagator;

/// <summary>
/// Describes how to propagate changes through a sequence.
/// </summary>
public enum PropagationStrategy
{
    DoNotPropagate,
    FromFirstInSequence,
    FromPreviousInSequence
}