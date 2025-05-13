using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models;

namespace WanderGlobe.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadsFolder;

        public PhotoService(ApplicationDbContext context)
        {
            _context = context;
            _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "uploads");
            
            // Ensure directory exists
            Directory.CreateDirectory(_uploadsFolder);
        }
        
        public async Task<List<Photo>> GetPhotosByUserAsync(string userId)
        {
            return await _context.Photos
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.UploadDate)
                .ToListAsync();
        }
        
        public async Task<List<Photo>> GetPhotosByVisitAsync(int visitId)
        {
            // This method is needed for interface compatibility
            return await _context.Photos
                .Where(p => p.TravelJournalCountryId == visitId)
                .OrderByDescending(p => p.UploadDate)
                .ToListAsync();
        }
        
        public async Task<List<Photo>> GetPhotosByVisitAndUserAsync(int countryId, string userId)
        {
            return await _context.Photos
                .Where(p => p.TravelJournalCountryId == countryId && p.TravelJournalUserId == userId)
                .OrderByDescending(p => p.UploadDate)
                .ToListAsync();
        }
        
        public async Task<Photo> GetPhotoByIdAsync(int photoId)
        {
            return await _context.Photos.FindAsync(photoId);
        }
        
        public async Task<Photo> AddPhotoAsync(Photo photo)
        {
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();
            return photo;
        }
        
        public async Task<bool> DeletePhotoAsync(int photoId, string userId)
        {
            var photo = await _context.Photos.FindAsync(photoId);
            if (photo == null || photo.UserId != userId)
                return false;
            
            // Delete the physical file if possible
            if (!string.IsNullOrEmpty(photo.FilePath))
            {
                try
                {
                    string filePath = Path.Combine(
                        Directory.GetCurrentDirectory(), 
                        "wwwroot", 
                        photo.FilePath.TrimStart('/')
                    );
                    
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception)
                {
                    // Log error but continue with DB deletion
                }
            }
            
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;
                
            // Generate a unique filename
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(_uploadsFolder, uniqueFileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            return $"/images/uploads/{uniqueFileName}";
        }
    }
}