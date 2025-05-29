// Folder: Services
// File: PhotoService.cs
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment (if saving files here)
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models;
using Microsoft.Extensions.Logging; // For logging

namespace WanderGlobe.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment; // Inject if saving files within this service
        private readonly ILogger<PhotoService> _logger;

        public PhotoService(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<PhotoService> logger)
        {
            _context = context;
            _environment = environment; // Used for constructing file paths
            _logger = logger;
        }
        
        public async Task<List<PhotoViewModel>> GetPhotosByUserAsync(string userId)
        {
            return await _context.Photos
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.UploadDate)
                .Select(p => new PhotoViewModel
                {
                    Id = p.Id,
                    Url = p.Url,
                    Caption = p.Caption
                })
                .ToListAsync();
        }
        
        // This method was using TravelJournalCountryId. If "visitId" here means CountryId,
        // it's better to rename. If it means VisitedCityId, it needs to be rewritten.
        public async Task<List<PhotoViewModel>> GetPhotosByCountryVisitAsync(int countryId, string userId)
        {
            // This implementation assumes photos might still be linked via TravelJournal fields.
            // If all photos are linked via VisitedCityId, this method might be obsolete or different.
            return await _context.Photos
                .Where(p => p.TravelJournalCountryId == countryId && p.UserId == userId) // Assuming userId check is implicit or intended
                .OrderByDescending(p => p.UploadDate)
                .Select(p => new PhotoViewModel
                {
                    Id = p.Id,
                    Url = p.Url,
                    Caption = p.Caption
                })
                .ToListAsync();
        }

        // --- NEW METHOD ---
        public async Task<List<PhotoViewModel>> GetPhotosForVisitedCityAsync(int visitedCityId, string userId)
        {
            return await _context.Photos
                .Where(p => p.VisitedCityId == visitedCityId && p.UserId == userId)
                .OrderByDescending(p => p.UploadDate)
                .Select(p => new PhotoViewModel
                {
                    Id = p.Id,
                    Url = p.Url ?? "", // Ensure Url is not null for the ViewModel
                    Caption = p.Caption
                })
                .ToListAsync();
        }
        
        public async Task<PhotoViewModel?> GetPhotoViewModelByIdAsync(int photoId, string userId)
        {
            return await _context.Photos
                .Where(p => p.Id == photoId && p.UserId == userId)
                .Select(p => new PhotoViewModel
                {
                    Id = p.Id,
                    Url = p.Url,
                    Caption = p.Caption
                })
                .FirstOrDefaultAsync();
        }
        
        // Changed from AddPhotoAsync(Photo photo) to be more specific
        public async Task<PhotoViewModel?> CreatePhotoRecordAsync(string userId, int? visitedCityId, string fileName, string url, string? caption)
        {
            var newPhoto = new Photo
            {
                UserId = userId,
                VisitedCityId = visitedCityId, // Link to the specific city visit
                FileName = fileName,
                Url = url,
                Caption = caption,
                UploadDate = DateTime.UtcNow
            };

            try
            {
                _context.Photos.Add(newPhoto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Photo record created with ID {PhotoId} for user {UserId}, linked to VisitedCityId {VisitedCityId}", newPhoto.Id, userId, visitedCityId);
                return new PhotoViewModel { Id = newPhoto.Id, Url = newPhoto.Url, Caption = newPhoto.Caption };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating photo record for user {UserId}", userId);
                return null;
            }
        }
        
        public async Task<bool> DeletePhotoAsync(int photoId, string userId)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == userId);
            if (photo == null)
            {
                _logger.LogWarning("DeletePhotoAsync: Photo with ID {PhotoId} not found for user {UserId}", photoId, userId);
                return false;
            }
            
            // Attempt to delete the physical file
            if (!string.IsNullOrEmpty(photo.Url))
            {
                try
                {
                    // Assuming URL is like "/images/user_photos/USER_ID/visit_VISIT_ID/filename.jpg"
                    string webRootPath = _environment.WebRootPath;
                    string fullFilePath = Path.Combine(webRootPath, photo.Url.TrimStart('/'));
                    
                    if (File.Exists(fullFilePath))
                    {
                        File.Delete(fullFilePath);
                        _logger.LogInformation("Successfully deleted physical file: {FilePath}", fullFilePath);
                    }
                    else
                    {
                        _logger.LogWarning("Physical file not found for deletion: {FilePath}", fullFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting physical file for photo ID {PhotoId}: {FilePath}", photo.Id, photo.Url);
                    // Log error but continue with DB deletion
                }
            }
            
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted photo record ID {PhotoId} for user {UserId}", photoId, userId);
            return true;
        }

        // SaveImageAsync is often handled in the PageModel/Controller that receives IFormFile.
        // If PhotoService handles it, it needs IWebHostEnvironment.
        // The PageModel's OnPostUploadPhotoAsync is a good place for file saving logic.
        // This method is removed as its responsibility is better placed in the PageModel/Controller
        // which then calls CreatePhotoRecordAsync with the resulting URL.
        // public async Task<string> SaveImageAsync(IFormFile file) { ... }
    }
}