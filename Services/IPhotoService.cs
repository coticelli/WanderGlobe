// Folder: Services
// File: IPhotoService.cs
using Microsoft.AspNetCore.Http; // For IFormFile
using System.Collections.Generic;
using System.Threading.Tasks;
using WanderGlobe.Models; // For Photo and PhotoViewModel

namespace WanderGlobe.Services
{
    public interface IPhotoService
    {
        Task<List<PhotoViewModel>> GetPhotosByUserAsync(string userId); // Changed to ViewModel for consistency
        
        // This method seems to imply visitId is a CountryId due to its usage in your PhotoService.
        // If it's truly for a "visit" which now means VisitedCity, its signature and implementation need to change.
        // For now, keeping it as is but flagging for review based on your actual needs for "GetPhotosByVisitAsync".
        Task<List<PhotoViewModel>> GetPhotosByCountryVisitAsync(int countryId, string userId); // Renamed for clarity

        Task<PhotoViewModel?> GetPhotoViewModelByIdAsync(int photoId, string userId); // To get a single photo view model for a user
        
        // AddPhotoAsync now takes specific parameters needed to create and link the photo.
        // The IFormFile is handled by the PageModel/Controller and path is passed.
        Task<PhotoViewModel?> CreatePhotoRecordAsync(string userId, int? visitedCityId, string fileName, string url, string? caption);

        Task<bool> DeletePhotoAsync(int photoId, string userId);

        // SaveImageAsync is more of a utility, typically called by PageModel/Controller or within CreatePhotoRecordAsync.
        // It's fine here, but often PageModels handle file saving and pass the URL/path to the service.
        // Task<string> SaveImageAsync(IFormFile file, string userId, int? visitedCityId); // Made more specific

        // New method to get photos for a specific visited city
        Task<List<PhotoViewModel>> GetPhotosForVisitedCityAsync(int visitedCityId, string userId);
    }
}