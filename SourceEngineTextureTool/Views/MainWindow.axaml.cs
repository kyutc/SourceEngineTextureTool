using Avalonia.Controls;
using SourceEngineTextureTool.ViewModels;

namespace SourceEngineTextureTool.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        _SetUpResolutionControlsBindingWorkaround();
    }

    /// <summary>
    /// Bind resolution controls to their associated <see cref="TextureViewModel"/> properties.
    /// </summary>
    /// <remarks>
    /// Bug:
    /// <see cref="https://github.com/AvaloniaUI/Avalonia/issues/10793"/>
    /// The above issue regards an error in which when the value of a <see cref="NumericUpDown"/> text box is cleared, a
    /// null value is propagated to the bound VM property regardless of that property's typing. This issue is cited as
    /// fixed in this PR <see cref="https://github.com/AvaloniaUI/Avalonia/pull/13970"/>, and attempting to clear the
    /// field at runtime results in the default value <code>0</code> being sent, which is captured and ignored in
    /// <see cref="TextureViewModel.TryUpdateTextureResolution"/>. I would think that this would prevent the validation
    /// message from displaying when the field is cleared, but it does not. Setting the binding here does prevent that.
    ///
    /// I don't have any idea where the IBinding.Initiate method is invoked for the binding created via
    /// <code>axaml</code>. I assume the issue is due to the <code>enableDataValidation</code> parameter for that
    /// invocation being set to true by default when its parsed. For whatever reason, that isn't the case when the
    /// binding is set in the code behind.
    /// 
    /// An optimal fix would be to only update the binding when NumericUpDown's control loses focus.
    /// <code>v11.1.0</code> claims to implement this, however the API I need is not public at that point
    /// <see cref="https://github.com/AvaloniaUI/Avalonia/issues/14965"/>.
    /// </remarks>
    private void _SetUpResolutionControlsBindingWorkaround()
    {
        // TextureWidthControl.Bind(NumericUpDown.ValueProperty, new Binding(nameof(TextureViewModel.ResolutionWidth)));
        // TextureHeightControl.Bind(NumericUpDown.ValueProperty, new Binding(nameof(TextureViewModel.ResolutionHeight)));
        // TextureFrameCountControl.Bind(NumericUpDown.ValueProperty, new Binding(nameof(TextureViewModel.FrameCount)));
    }
}