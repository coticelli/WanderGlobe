// This file should be placed in your Models folder (e.g., WanderGlobe/Models/PhotoViewModel.cs)
// or if you have a dedicated ViewModels folder, place it there and adjust the namespace.

namespace WanderGlobe.Models // Or WanderGlobe.ViewModels if you use that folder
{
    /// <summary>
    /// A ViewModel representing the data needed to display a photo,
    /// typically in a gallery or list.
    /// </summary>
    public class PhotoViewModel
    {
        /// <summary>
        /// The unique identifier of the photo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The web-accessible URL of the photo.
        /// Should be initialized to prevent null issues if non-nullable in your context.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// An optional caption for the photo.
        /// </summary>
        public string? Caption { get; set; }

        // You can add more properties here if your views need them, for example:
        // public DateTime UploadDate { get; set; }
        // public string UploaderUserName { get; set; }
        // public bool IsCurrentUserOwner { get; set; }
    }
}