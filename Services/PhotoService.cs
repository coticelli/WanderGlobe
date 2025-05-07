using Microsoft.EntityFrameworkCore;
using WanderGlobe.Data;
using WanderGlobe.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Models.Custom;

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

            // Assicura che la directory esista
            Directory.CreateDirectory(_uploadsFolder);
        }

        public async Task<List<Photo>> GetPhotosByUserAsync(string userId)
        {
            // In un'applicazione reale, recupereremmo i dati dal database
            return new List<Photo>();
        }

        public async Task<List<Photo>> GetPhotosByVisitAsync(int visitId)
        {
            // In un'applicazione reale, recupereremmo i dati dal database
            return new List<Photo>();
        }

        public async Task<Photo> GetPhotoByIdAsync(int photoId)
        {
            // In un'applicazione reale, recupereremmo i dati dal database
            return new Photo();
        }

        public async Task<Photo> AddPhotoAsync(Photo photo)
        {
            // In un'applicazione reale, salveremmo nel database
            // Invece di adattare proprietà inesistenti, creiamo un nuovo oggetto Photo
            // con le proprietà che esistono effettivamente nel modello
            var newPhoto = new Photo
            {
                Id = photo.Id,
                TravelJournalId = 0,  // Imposta un valore predefinito o ottienilo da un metodo helper
                Caption = photo.Caption ?? "No caption" // Usa la proprietà Caption se esiste
                // Non assegnare proprietà che non esistono
            };

            return newPhoto;
        }

        public async Task<bool> DeletePhotoAsync(int photoId, string userId)
        {
            // In un'applicazione reale, elimineremmo dal database
            return true;
        }

        // Metodo helper per salvare un'immagine e ottenere il percorso
        public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

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