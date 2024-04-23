using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;
using SourceEngineTextureTool.Models.Settings;
using SourceEngineTextureTool.Services.Image;

namespace SourceEngineTextureTool.ViewModels;

public class DropImageViewModel : ViewModelBase
{
    /// <summary>
    /// Gets/sets this viewmodel's DropImage.
    /// </summary>
    public DropImage DropImage { get; }

    [Reactive] public Sett Settings { get; set; }

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
        }
    }

    /// <summary>
    /// Gets/sets the <see cref="DropImage.PreviewImage"/> property.
    /// Updated when <see cref="ImportedImage"/> is set or <see cref="Settings"/> are changed.
    /// </summary>
    public string? PreviewImage
    {
        get => DropImage.PreviewImage;
    }

    /// <summary>
    /// Gets/sets the default image to use when <see cref="ImportedImage"/> is null. 
    /// </summary>
    [Reactive] public string? DefaultImage { get; set; }

    private string? _defaultImage;

    /// <summary>
    /// Gets/sets whether to display the unprocessed imported image or the rendered preview.
    /// </summary>
    [Reactive] public bool UsePreviewImage { get; set; }

    /// <summary>
    /// Gets/sets whether changes to the <see cref="DefaultImage"/> should be automatically applied to <see cref="ImportedImage"/>.
    /// </summary>
    [Reactive] public bool UseDefaultImage { get; set; }

    /// <summary>
    /// Gets/sets the image to display.
    /// </summary>
    [Reactive] public string? CurrentlyDisplayedImage { get; set; }

    public IObservable<bool> HasConvertedImage { get; }

    public DropImageViewModel(DropImage dropImage)
    {
        DropImage = dropImage;
        UseDefaultImage = true;
        HasConvertedImage = this.WhenAnyValue(divm => divm.PreviewImage)
            .Select(_ => DropImage.ConvertedImage is not null);

        // This service definition gets the DataContext of the MainWindow in order to get the ProjectSettingsViewModel.
        // This is a direct, non-invasive way to ensure that all DropImageViewModels in the current view know which
        // image should be displayed.
        var currentProjectSettingsViewModel = App.FetchService<ProjectSettingsViewModel>();
        currentProjectSettingsViewModel.WhenAnyValue(psvm => psvm.EnableCompiledTexturePreview)
            .Subscribe(newUsePreviewImage => UsePreviewImage = newUsePreviewImage);

        // When project settings are changed update the preview
        currentProjectSettingsViewModel.WhenAnyValue(psvm => psvm.SettSettings)
            .Subscribe(newSettSettings =>
            {
                Settings = newSettSettings;
                _UpdatePreviewImage();
            });

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
            .Subscribe(newImportedImage =>
            {
                _UpdatePreviewImage();

                if (!UsePreviewImage)
                {
                    CurrentlyDisplayedImage = ImportedImage;
                }
            });

        this.WhenAnyValue(divm => divm.PreviewImage)
            .Subscribe(_ =>
            {
                if (UsePreviewImage)
                {
                    CurrentlyDisplayedImage = PreviewImage;
                }
            });

        this.WhenAnyValue(divm => divm.UsePreviewImage)
            .Skip(1)
            .Subscribe(
                newUsePreviewImage => CurrentlyDisplayedImage = newUsePreviewImage ? PreviewImage : ImportedImage);
    }

    /// <summary>
    /// Generate the <see cref="PreviewImage"/> for this <see cref="DropImage"/>.
    /// </summary>
    public void _UpdatePreviewImage()
    {
        this.RaisePropertyChanging(nameof(PreviewImage));

        // If not all Crunch operations are implemented, exceptions here will break the app.
        try
        {
            ConversionHelper.Render(DropImage, Settings);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        this.RaisePropertyChanged(nameof(PreviewImage));
    }
}