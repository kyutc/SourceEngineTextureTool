using System;
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
using SourceEngineTextureTool.Services.Image;
using SourceEngineTextureTool.Services.IO;

namespace SourceEngineTextureTool.Controls;

/// <summary>
/// 
/// </summary>
public class ImportedImageChangedEventArgs : RoutedEventArgs
{
    public ImportedImageChangedEventArgs(RoutedEvent routedEvent, string? oldImportedImage, string? newImportedImage) :
        base(routedEvent)
    {
        OldImportedImage = oldImportedImage;
        NewImportedImage = newImportedImage;
    }

    public string? OldImportedImage { get; }
    public string? NewImportedImage { get; }
}

/// <summary>
/// Template for a control that facilitates importing and displaying images for use within the application.
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

    #region Image Control Styled Properties

    /// <summary>
    /// Exposes the <see cref="AdvancedImage.HeightProperty"/>
    /// </summary>
    public static readonly StyledProperty<double> ImageHeightProperty =
        AvaloniaProperty.Register<DropImageControl, double>(nameof(ImageHeight));

    /// <summary>
    /// Synonymous with the template part<see cref="TP_Image"/>'s Height property
    /// </summary>
    public double ImageHeight
    {
        get => GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    /// <summary>
    /// Exposes the <see cref="AdvancedImage.WidthProperty"/>
    /// </summary>
    public static readonly StyledProperty<double> ImageWidthProperty =
        AvaloniaProperty.Register<DropImageControl, double>(nameof(ImageWidth));

    /// <summary>
    /// Synonymous with the template part<see cref="TP_Image"/>'s width property
    /// </summary>
    public double ImageWidth
    {
        get => GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    /// <summary>
    /// Exposes the <see cref="AdvancedImage.SourceProperty"/>
    /// </summary>
    public static readonly StyledProperty<string?> SourceProperty =
        AdvancedImage.SourceProperty.AddOwner<DropImageControl>();

    /// <summary>
    /// Synonymous with the template part<see cref="TP_Image"/>'s Source property
    /// </summary>
    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    #endregion Image Control Styled Properties

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

    #region Control Properties

    /// <summary>
    /// Defines the <see cref="ImageResolution"/> property.
    /// </summary>
    public static readonly StyledProperty<Resolution?> ImageResolutionProperty =
        AvaloniaProperty.Register<DropImageControl, Resolution?>(nameof(ImageResolution));

    /// <summary>
    /// Gets/sets the <see cref="ImageResolutionProperty"/> for this instance.
    /// </summary>
    public Resolution? ImageResolution
    {
        get => GetValue(ImageResolutionProperty);
        set => SetValue(ImageResolutionProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FrameIndex"/> property.
    /// </summary>
    public static readonly StyledProperty<int?> MipmapOrderProperty =
        AvaloniaProperty.Register<DropImageControl, int?>(nameof(MipmapOrder));

    /// <summary>
    /// Gets/sets the <see cref="FrameIndexProperty"/> for this instance.
    /// </summary>
    public int? MipmapOrder
    {
        get => GetValue(MipmapOrderProperty);
        set => SetValue(MipmapOrderProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FrameIndex"/> property.
    /// </summary>
    public static readonly StyledProperty<int?> FrameIndexProperty =
        AvaloniaProperty.Register<DropImageControl, int?>(nameof(FrameIndex));

    /// <summary>
    /// Gets/sets the <see cref="FrameIndexProperty"/> for this instance.
    /// </summary>
    public int? FrameIndex
    {
        get => GetValue(FrameIndexProperty);
        set => SetValue(FrameIndexProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ImportedImage"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> ImportedImageProperty =
        AvaloniaProperty.Register<DropImageControl, string?>(nameof(ImportedImage),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Gets/sets the <see cref="ImportedImageProperty"/> for this instance.
    /// </summary>
    public string? ImportedImage
    {
        get => GetValue(ImportedImageProperty);
        set => SetValue(ImportedImageProperty, value);
    }

    #endregion Control Properties

    #region Events

    /// <summary>
    /// Defines the <see cref="SourceChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<ImportedImageChangedEventArgs> SourceChangedEvent =
        RoutedEvent.Register<FileDialogService, ImportedImageChangedEventArgs>(nameof(SourceChanged),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when a file is imported
    /// </summary>
    public event EventHandler<ImportedImageChangedEventArgs>? SourceChanged
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
        Command = OpenImageFileDialogCommand;

        if (Classes.Contains(PC_HasImage))
        {
            Image = e.NameScope.Find<AdvancedImage>(TP_Image);
            Image?.Bind(AdvancedImage.SourceProperty, new Binding(nameof(Source)) { Source = this });
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
        var importedImageRepository = App.FetchService<ImageImporter>() ??
                                      throw new Exception("ImportedImageRepository not found.");

        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var file = await fileDialogService.OpenImageFileDialogAsync();
            if (file is null) return;

            var path = Uri.UnescapeDataString(file.Path.AbsolutePath);
            var importedImage = importedImageRepository.GetImportedImageFromFile(path);

            string? oldImportedImage = ImportedImage;
            ImportedImage = importedImage;
            if (oldImportedImage != ImportedImage)
            {
                RaiseEvent(new ImportedImageChangedEventArgs(SourceChangedEvent, oldImportedImage, ImportedImage));
            }
        });
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