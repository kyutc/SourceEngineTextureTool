using System.ComponentModel.DataAnnotations;

namespace SourceEngineTextureTool.Services.Propagator;

/// <summary>
/// Describes how to propagate changes through a sequence.
/// </summary>
public enum PropagationStrategy
{
    [Display(Name = "Do not generate mipmaps")] DoNotPropagate,
    [Display(Name = "The largest mipmap")] FromFirstInSequence,
    [Display(Name = "The previous mipmap")] FromPreviousInSequence
}