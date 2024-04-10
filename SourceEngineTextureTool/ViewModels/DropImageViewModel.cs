using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;

namespace SourceEngineTextureTool.ViewModels;

public class DropImageViewModel : ViewModelBase
{
    /// <summary>
    /// Gets/sets this viewmodel's DropImage.
    /// </summary>
    public DropImage DropImage { get; }
    
    public byte? MipmapOrder
    {
        get => DropImage.MipmapOrder;
        set => DropImage.MipmapOrder = value;
    }
    
    public Resolution? TargetResolution
    {
        get => DropImage.TargetResolution;
        set => DropImage.TargetResolution = value;
    }

    public ushort? FrameIndex
    {
        get => DropImage.FrameIndex;
        set => DropImage.FrameIndex = value;
    }

    /// <summary>
    /// Gets/sets the <see cref="DropImage.ImportedImage"/> property.
    /// Defaults to <see cref="DefaultImage"/> if set to null.
    /// </summary>
    public string? ImportedImage
    {
        get => DropImage.ImportedImage;
        set
        {
            if (value is null)
            {
                UseDefaultImage = true;
            }
            else if (value != DefaultImage)
            {
                UseDefaultImage = false;
            }

            this.RaisePropertyChanging();
            DropImage.ImportedImage = UseDefaultImage ? DefaultImage : value;
            this.RaisePropertyChanged();

            if (!UsePreviewImage)
            {
                CurrentlyDisplayedImage = ImportedImage;
            }
        }
    }

    /// <summary>
    /// Gets/sets the <see cref="DropImage.PreviewImage"/> property.
    /// </summary>
    public string? PreviewImage
    {
        get => DropImage.PreviewImage;
        private set
        {
            this.RaisePropertyChanging();
            DropImage.PreviewImage = value;
            this.RaisePropertyChanged();

            if (UsePreviewImage)
            {
                CurrentlyDisplayedImage = PreviewImage;
            }
        }
    }

    /// <summary>
    /// Gets/sets the default image to use for <see cref="ImportedImage"/>.
    /// </summary>
    [Reactive] public string? DefaultImage
    {
        get => _defaultImage;
        set
        {
            this.RaiseAndSetIfChanged(ref _defaultImage, value);
            if (UseDefaultImage)
            {
                ImportedImage = DefaultImage;
            }
        }
    }

    private string? _defaultImage;

    /// <summary>
    /// Gets/sets whether to display the unprocessed imported image or the rendered preview.
    /// </summary>
    [Reactive] public bool UsePreviewImage { get; set; }

    /// <summary>
    /// Gets/sets whether changes to the <see cref="DefaultImage"/> should be automatically applied to <see cref="ImportedImage"/>
    /// </summary>
    [Reactive] public bool UseDefaultImage { get; set; }

    /// <summary>
    /// Gets the image to display.
    /// </summary>
    [Reactive] public string? CurrentlyDisplayedImage { get; set; }

    public DropImageViewModel(DropImage dropImage)
    {
        DropImage = dropImage;
        
        UseDefaultImage = true;

        this.WhenAnyValue(divm => divm.DefaultImage)
            .Skip(1)
            .Subscribe(_ =>
            {
                if (UseDefaultImage)
                {
                    ImportedImage = DefaultImage;
                }
            });

        this.WhenAnyValue(divm => divm.ImportedImage)
            .Skip(1)
            .Subscribe(_ => _UpdatePreviewImage());

        this.WhenAnyValue(divm => divm.UsePreviewImage)
            .Skip(1)
            .Subscribe(
                newUsePreviewImage => CurrentlyDisplayedImage = newUsePreviewImage ? PreviewImage : ImportedImage);
    }

    public void _UpdatePreviewImage()
    {
        // Todo: ImportedImage was changed, meaning we need a new PreviewImage
        string newPreviewImage = "";

        // Todo: Update preview image
        //  PreviewImage = ConversionHelper.CreatePreview(
        //             // ImportedImage,
        //             // ImageResolution.Width,
        //             // ImageResolution.Height,
        //             // how do I get project settings from here // Todo: Service that registers global settings. 
        //             // );

        PreviewImage = newPreviewImage;
    }
}