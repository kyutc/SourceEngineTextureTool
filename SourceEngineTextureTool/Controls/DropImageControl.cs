using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using SourceEngineTextureTool.Models;
using SourceEngineTextureTool.Services.IO;

namespace SourceEngineTextureTool.Controls;

/// <summary>
/// 
/// </summary>
public class SourceChangedEventArgs : RoutedEventArgs
{
    public SourceChangedEventArgs(RoutedEvent routedEvent, string? oldSource, string? newSource) : base(routedEvent)
    {
        OldSource = oldSource;
        NewSource = newSource;
    }

    public string? OldSource { get; }
    public string? NewSource { get; }
}

/// <summary>
/// Template for a control that 
/// </summary>
[TemplatePart(TP_Button, typeof(Button))]
[TemplatePart(TP_Image, typeof(AdvancedImage))]
[PseudoClasses(PC_HasImage)]
public partial class DropImageControl : TemplatedControl
{
    private const string TP_Button = "PART_Button";
    private const string TP_Image = "PART_Image";

    private const string PC_HasImage = ":hasimage";

    public bool HasImage => Classes.Contains(PC_HasImage);

    #region Direct Properties

    // public static readonly DirectProperty<TextureWorkspaceControl, Resolution?> ResolutionProperty =
    //     AvaloniaProperty.RegisterDirect<TextureWorkspaceControl, Resolution?>(
    //         nameof(Resolution),
    //         twc => 
    //         );

    public Resolution? Resolution
    {
        get => _resolution;
        set => _resolution = value;
    }

    private Resolution? _resolution;
        
    #endregion
    
    #region Source Shared Styled Property

    /// <summary>
    /// Defines the <see cref="AdvancedImage.SourceProperty"/>
    /// </summary>
    public static readonly StyledProperty<string?> SourceProperty =
        AdvancedImage.SourceProperty.AddOwner<TextureWorkspaceControl>();
    
    /// <summary>
    /// Synonymous with the template part<see cref="TP_Image"/>'s Source property
    /// </summary>
    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    #endregion Source Shared Styled Property

    #region Command Shared Styled Property

    /// <summary>
    /// Defines the <see cref="Button.Command"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        Button.CommandProperty.AddOwner<DropImageControl>();

    /// <summary>
    /// Synonymous to the template part <see cref="TP_Button"/>'s <see cref="Button.Command"/> property.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    #endregion Command Shared Styled Property
    
    #region Events
    
    /// <summary>
    /// Defines the <see cref="SourceChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<SourceChangedEventArgs> SourceChangedEvent =
        RoutedEvent.Register<FileDialogService, SourceChangedEventArgs>(nameof(SourceChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when a file is imported
    /// </summary>
    public event EventHandler<SourceChangedEventArgs>? SourceChanged
    {
        add => AddHandler(SourceChangedEvent, value);
        remove => RemoveHandler(SourceChangedEvent, value);
    }

    #endregion Events
    
    public DropImageControl()
        : this(null)
    {
    }

    public DropImageControl(string? imageSource)
    {
        SetupDragAndDrop();
        Source = imageSource;
    }

    #region Overrides

    public Button? Button;
    public AdvancedImage? Image;
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Button = e.NameScope.Find<Button>(TP_Button);
        Button?.Bind(Button.CommandProperty, new Binding(nameof(Command)) { Source = this });

        if (Classes.Contains(PC_HasImage))
        {
            Image = e.NameScope.Find<AdvancedImage>(TP_Image);
            Image?.Bind(AdvancedImage.SourceProperty, new Binding(nameof(Source)) { Source = this });
            Command = null; // Todo: Flyout w/ options
        }
        else
        {
            Command = OpenImageFileDialogCommand;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourceProperty)
        {
            string? source = change.GetNewValue<string?>();

            if (!String.IsNullOrWhiteSpace(source))
            {
                PseudoClasses.Set(PC_HasImage, true);
            }
            else
            {
                PseudoClasses.Set(PC_HasImage, false);
            }
        }
    }
    
    #endregion Overrides

    [RelayCommand]
    private Task OpenImageFileDialog()
    {
        var fileDialogService = App.FetchService<IFileDialogService>() ??
                                     throw new Exception("FileDialogService not found.");

        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var file = await fileDialogService.OpenImageFileDialogAsync();
            if (file is null) return;
            
            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), file.Path.AbsolutePath); //new Uri(Directory.GetCurrentDirectory()).MakeRelativeUri(file.Path)
            UpdateImage(path);
        });
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    private void UpdateImage(string? uri)
    {
        // Todo: Service that validates a string as relative URI or (eventually) URL
        // Todo: Clean up existing preview (if null string only do this part)
        // Todo: Initialize DropImage w/ new uri
        // Todo: Set AdvancedImage source
        string? oldSource = Source;
        Source = uri;
        if (Source != oldSource)
        {
            RaiseEvent(new SourceChangedEventArgs(SourceChangedEvent, oldSource, Source));
        }
    }

    private void SetupDragAndDrop()
    {
        // PointerPressed += BeginDrag;
        // AddHandler(DragDrop.DragOverEvent, DragOver);
        // AddHandler(DragDrop.DropEvent, Drop);
    }

    // /// <summary>
    // /// Constructs and attaches a dataobject to the pointer event generated from click-dragging from this control.
    // /// </summary>
    // /// <param name="e"></param>
    // private async void BeginDrag(object? sender, PointerEventArgs? e)
    // {
    //     
    //     var source = e.Source;
    //     if (String.IsNullOrWhiteSpace(Source)
    //         || source is not AdvancedImage image
    //         || !image.Equals(Image))
    //     {
    //         Console.WriteLine($"{e.Source} cannot be dragged.");
    //         return;
    //     }
    //     
    //     Console.WriteLine($"Beginning drag event for {e.Source}");
    //     
    //     var dragData = new DataObject();
    //     dragData.Set(DataFormats.Text, Source);
    //
    //     var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy) switch
    //     {
    //         DragDropEffects.Copy => $"{Source} copied.",
    //         DragDropEffects.None => $"Operation cancelled",
    //         _ => "Huhh?"
    //     };
    //     
    //     Console.WriteLine($"Result of DragDrop operation on {GetType().FullName}: {result}");
    // }
    
    // private void DragOver(object? sender, DragEventArgs e)
    // {
    //     Console.WriteLine("Dragging over");
    //     if (e.Source is not StyledElement styledElement) return;
    //     // if (Equals(styledElement, this))
    //     // {
    //         Console.WriteLine($"Dragging over {styledElement.Name}");
    //         e.DragEffects &= DragDropEffects.Copy;
    //     // }
    // }
    
    // private void Drop(object? sender, DragEventArgs e)
    // {
    //     Console.WriteLine($"Dropping over {sender}");
    //     if (Equals(e.Source, this) || Equals(e.Source, Image))
    //     {
    //         e.DragEffects &= DragDropEffects.Copy;
    //     }
    //
    //     if (e.Data.Contains(DataFormats.Text))
    //     {
    //         Console.WriteLine("Updating Source");
    //         UpdateImage(e.Data.GetText());
    //     }
    // }
    
}