using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SourceEngineTextureTool.Services.Image;

public class ImageImporter
{
    private readonly Dictionary<string, string[]> _importedImages = new();

    /// <summary>
    /// Gets the imported image for the described file.
    /// </summary>
    /// <param name="importedFile">the path to the file to import</param>
    /// <returns>The first image processed from importedFile</returns>
    public string GetImportedImageFromFile(string importedFile)
    {
        return GetImportedImagesFromFile(importedFile).First();
    }

    /// <summary>
    /// Gets the imported images for the described file.
    /// </summary>
    /// <param name="importedFile">the path to the file to import</param>
    /// <returns>One or more images processed from importedFile</returns>
    public string[] GetImportedImagesFromFile(string importedFile)
    {
        string importedImageKey = _CreateUniqueHashFromImportedFile(importedFile);
        if (!_importedImages.ContainsKey(importedImageKey))
        {
            string[] importedImages = ConversionHelper.NormaliseToPng32(importedFile);
            _importedImages.Add(importedImageKey, importedImages);
        }

        return _importedImages[importedImageKey];
    }
    
    /// <summary>
    /// Creates a "unique" hash by creating a string from the importedFile
    /// and its created at/last modified times. 
    /// </summary>
    /// <param name="importedFile">The file to create a hash from</param>
    /// <returns>a probably unique string</returns>
    private string _CreateUniqueHashFromImportedFile(string importedFile)
    {
        return importedFile +
               File.GetCreationTime(importedFile).ToFileTime().GetHashCode().ToString() +
               File.GetLastWriteTime(importedFile).ToFileTime().GetHashCode().ToString();
    }
}