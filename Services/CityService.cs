using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models;
using WanderGlobe.Models.Custom;

namespace WanderGlobe.Services
{
    public class CityService : ICityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private IDreamService? _dreamService;

        public CityService(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        // Utilizziamo la lazy-loading per evitare dipendenze circolari
        private IDreamService DreamService
        {
            get
            {
                if (_dreamService == null)
                {
                    _dreamService = _serviceProvider.GetRequiredService<IDreamService>();
                }
                return _dreamService;
            }
        }

        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Include(c => c.Country)
                .ToListAsync();
        }

        public async Task<List<City>> GetCitiesByCountryIdAsync(int countryId)
        {
            return await _context.Cities
                .Where(c => c.CountryId == countryId)
                .ToListAsync();
        }

        public async Task<List<City>> GetCapitalCitiesAsync()
        {
            return await _context.Cities
                .Where(c => c.IsCapital)
                .Include(c => c.Country)
                .ToListAsync();
        }        public async Task<City> GetCityByIdAsync(int cityId)
        {
            return await _context.Cities
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.Id == cityId) ?? new City();
        }

        public async Task<City?> GetCapitalCityByCountryIdAsync(int countryId)
        {
            return await _context.Cities
                .FirstOrDefaultAsync(c => c.CountryId == countryId && c.IsCapital);
        }

        public async Task<List<City>> GetAvailableCitiesForUserAsync(string userId)
        {
            // Ottieni tutti i paesi già visitati dall'utente
            var visitedCountryIds = await _context.VisitedCountries
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.CountryId)
                .ToListAsync();

            // Ottieni tutte le città che non appartengono a paesi già visitati
            return await _context.Cities
                .Include(c => c.Country)
                .Where(c => !visitedCountryIds.Contains(c.CountryId))
                .OrderBy(c => c.Country.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<City>> GetCitiesNotInWishlistAsync(string userId)
        {
            // Otteniamo tutte le città 
            var allCities = await _context.Cities
                .Include(c => c.Country)
                .OrderBy(c => c.Country.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();
            
            // Lista da ritornare
            var availableCities = new List<City>();
            
            // Controlliamo ogni città se è già nella wishlist
            foreach (var city in allCities)
            {
                bool isInWishlist = await DreamService.IsCityInUserWishlistAsync(city.Id, userId);
                
                // Se non è nella wishlist, la aggiungiamo
                if (!isInWishlist)
                {
                    availableCities.Add(city);
                }
            }
            
            return availableCities;
        }

        public async Task<bool> IsCityVisitedByUserAsync(int cityId, string userId)
        {
            var city = await _context.Cities.FindAsync(cityId);
            if (city == null)
                return false;

            // Controlliamo se l'utente ha visitato il paese di questa città
            return await _context.VisitedCountries
                .AnyAsync(vc => vc.CountryId == city.CountryId && vc.UserId == userId);
        }

        public async Task<bool> IsCityInWishlistAsync(int cityId, string userId)
        {
            try
            {
                // Utilizza il DreamService per verificare se la città è nella wishlist
                return await DreamService.IsCityInUserWishlistAsync(cityId, userId);
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public async Task<bool> MarkCityAsVisitedAsync(int cityId, string userId, DateTime visitDate)
        {
            try
            {
                // Ottieni la città
                var city = await _context.Cities.FindAsync(cityId);
                if (city == null)
                    return false;
                
                // Verifica se l'utente ha già visitato questo paese
                var existingVisit = await _context.VisitedCountries
                    .FirstOrDefaultAsync(vc => vc.CountryId == city.CountryId && vc.UserId == userId);
                
                if (existingVisit == null)
                {
                    // Aggiungi il paese alle visite dell'utente
                    _context.VisitedCountries.Add(new VisitedCountry
                    {
                        UserId = userId,
                        CountryId = city.CountryId,
                        VisitDate = visitDate
                    });
                    
                    await _context.SaveChangesAsync();
                }
                
                // Controlla se la città è nella wishlist
                bool isInWishlist = await DreamService.IsCityInUserWishlistAsync(cityId, userId);
                
                if (isInWishlist)
                {
                    // Ottieni la wishlist dell'utente
                    var wishlist = await DreamService.GetUserWishlistAsync(userId);
                    
                    // Cerca la destinazione con lo stesso nome di città o stesso ID
                    var destinationToRemove = wishlist.FirstOrDefault(d => 
                        d.CityName.Equals(city.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (destinationToRemove != null)
                    {
                        await DreamService.RemoveFromWishlistAsync(destinationToRemove.Id, userId);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in MarkCityAsVisitedAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<City>> GetAllCitiesWithCountryAsync()
        {
            try
            {
                // Ottieni tutte le città e includi i dati del paese associato
                var cities = await _context.Cities
                    .Include(c => c.Country)
                    .ToListAsync();
                
                // Filtriamo solo le città con un paese associato valido
                return cities.Where(c => c.Country != null)
                    .OrderBy(c => c.Country.Name)
                    .ThenBy(c => c.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in GetAllCitiesWithCountryAsync: {ex.Message}");
                // In caso di errore, ritorna una lista vuota per evitare di bloccare l'applicazione
                return new List<City>();
            }
        }
    }
}