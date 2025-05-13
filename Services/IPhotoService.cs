using WanderGlobe.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WanderGlobe.Services
{
    public interface IPhotoService
    {
        Task<List<Photo>> GetPhotosByUserAsync(string userId);
        Task<List<Photo>> GetPhotosByVisitAsync(int visitId);
        Task<List<Photo>> GetPhotosByVisitAndUserAsync(int countryId, string userId);
        Task<Photo> GetPhotoByIdAsync(int photoId);
        Task<Photo> AddPhotoAsync(Photo photo);
        Task<bool> DeletePhotoAsync(int photoId, string userId);
        Task<string> SaveImageAsync(IFormFile file);
    }
}