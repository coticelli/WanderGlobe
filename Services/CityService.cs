// Services/CityService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Aggiungi logger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models;
// Rimuovi using WanderGlobe.Models.Custom; se non più necessario

namespace WanderGlobe.Services
{
    public class CityService : ICityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private IDreamService? _dreamService; // Considera se questa dipendenza è ancora necessaria qui o se può essere gestita altrove
        private readonly ILogger<CityService> _logger;

        public CityService(ApplicationDbContext context, IServiceProvider serviceProvider, ILogger<CityService> logger)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private IDreamService DreamService => _dreamService ??= _serviceProvider.GetRequiredService<IDreamService>();

        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Include(c => c.Country)
                .OrderBy(c => c.Country.Name).ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<City>> GetCitiesByCountryIdAsync(int countryId)
        {
            return await _context.Cities
                .Where(c => c.CountryId == countryId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<City>> GetCapitalCitiesAsync()
        {
            return await _context.Cities
                .Where(c => c.IsCapital)
                .Include(c => c.Country)
                .OrderBy(c => c.Country.Name)
                .ToListAsync();
        }
        
        public async Task<City?> GetCityByIdAsync(int cityId) // Modificato
        {
            if (cityId <= 0) return null;
            return await _context.Cities
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.Id == cityId);
        }

        public async Task<City?> GetCapitalCityByCountryIdAsync(int countryId)
        {
            return await _context.Cities
                .Include(c => c.Country) // Includi Country per consistenza, anche se non strettamente necessario solo per il check
                .FirstOrDefaultAsync(c => c.CountryId == countryId && c.IsCapital);
        }
        
        public async Task<City?> GetCapitalCityAsync(int countryId) // Alias, già presente
        {
             return await GetCapitalCityByCountryIdAsync(countryId);
        }


        public async Task<List<City>> GetAvailableCitiesForUserAsync(string userId)
        {
            // Città che l'utente NON ha ancora visitato (secondo la nuova tabella VisitedCities)
            var visitedCityIds = await _context.VisitedCities
                .Where(vc => vc.UserId == userId)
                .Select(vc => vc.CityId)
                .Distinct()
                .ToListAsync();

            return await _context.Cities
                .Include(c => c.Country)
                .Where(c => !visitedCityIds.Contains(c.Id))
                .OrderBy(c => c.Country.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<City>> GetCitiesNotInWishlistAsync(string userId)
        {
            var dreamCityIds = await _context.DreamDestinations
                                     .Where(dd => dd.UserId == userId && dd.CityId.HasValue)
                                     .Select(dd => dd.CityId.Value)
                                     .Distinct()
                                     .ToListAsync();
            
            return await _context.Cities
                .Include(c => c.Country)
                .Where(c => !dreamCityIds.Contains(c.Id))
                .OrderBy(c => c.Country.Name).ThenBy(c => c.Name)
                .ToListAsync();
        }


        public async Task<List<City>> GetAllCitiesWithCountryAsync()
        {
            return await _context.Cities
                .Include(c => c.Country) // Assicurati che Country sia sempre caricato
                .Where(c => c.Country != null) // Filtra se per caso ci fossero città orfane
                .OrderBy(c => c.Country!.Name) // Usa ! se sei sicuro che Country non sarà null dopo il filtro
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        // --- IMPLEMENTAZIONE NUOVI METODI PER VISITEDCITY ---

        public async Task AddVisitedCityAsync(VisitedCity visitedCity)
        {
            if (visitedCity == null)
            {
                throw new ArgumentNullException(nameof(visitedCity));
            }

            // Opzionale: verifica se la città esiste (anche se la FK dovrebbe gestirlo)
            var cityExists = await _context.Cities.AnyAsync(c => c.Id == visitedCity.CityId);
            if (!cityExists)
            {
                _logger.LogError($"Tentativo di aggiungere visita per CityId non esistente: {visitedCity.CityId}");
                throw new ArgumentException($"La città con ID {visitedCity.CityId} non esiste.");
            }

            // Verifica se l'utente ha già visitato questa specifica città in questa specifica data
            // Potresti voler una logica diversa (es. solo UserId e CityId se una città può essere visitata una sola volta)
            bool alreadyVisitedOnDate = await HasUserVisitedCityOnDateAsync(visitedCity.UserId, visitedCity.CityId, visitedCity.VisitDate);
            if (alreadyVisitedOnDate)
            {
                _logger.LogWarning($"L'utente {visitedCity.UserId} ha già registrato una visita per la città {visitedCity.CityId} in data {visitedCity.VisitDate.ToShortDateString()}.");
                throw new ArgumentException("Hai già registrato una visita per questa città in questa data.");
            }

            _context.VisitedCities.Add(visitedCity);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Aggiunta VisitedCity per utente {visitedCity.UserId}, Città ID {visitedCity.CityId}, Data {visitedCity.VisitDate.ToShortDateString()}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Errore DB durante l'aggiunta di VisitedCity: {ex.InnerException?.Message ?? ex.Message}");
                throw new Exception("Errore durante il salvataggio della visita della città nel database.", ex);
            }
        }

        public async Task<List<VisitedCity>> GetVisitedCitiesByUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<VisitedCity>();

            return await _context.VisitedCities
                .Where(vc => vc.UserId == userId)
                .Include(vc => vc.City)                 // Includi i dettagli della Città
                    .ThenInclude(city => city.Country) // Includi il Paese della Città
                .OrderByDescending(vc => vc.VisitDate)
                .ToListAsync();
        }

        public async Task<bool> HasUserVisitedCityOnDateAsync(string userId, int cityId, DateTime visitDate)
        {
            // Compara solo la parte Data, ignorando l'ora, se necessario
            var visitDateOnly = visitDate.Date;
            return await _context.VisitedCities
                .AnyAsync(vc => vc.UserId == userId && 
                                vc.CityId == cityId && 
                                vc.VisitDate.Date == visitDateOnly);
        }
        
        public async Task<VisitedCity?> GetVisitedCityByIdAsync(int visitedCityId)
        {
            return await _context.VisitedCities
                .Include(vc => vc.City) // Includi la città per info se necessario prima della rimozione
                    .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(vc => vc.Id == visitedCityId);
        }


        public async Task RemoveVisitedCityAsync(int visitedCityId)
        {
            var visitedCity = await _context.VisitedCities.FindAsync(visitedCityId);
            if (visitedCity != null)
            {
                _context.VisitedCities.Remove(visitedCity);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Rimossa VisitedCity ID: {visitedCityId}");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, $"Errore DB durante la rimozione di VisitedCity ID: {visitedCityId}");
                    throw new Exception("Errore durante la rimozione della visita della città dal database.", ex);
                }
            }
            else
            {
                 _logger.LogWarning($"Tentativo di rimuovere VisitedCity non esistente con ID: {visitedCityId}");
            }
        }
        
        public async Task<bool> IsCityVisitedByUserAsync(int cityId, string userId)
        {
            return await _context.VisitedCities.AnyAsync(vc => vc.CityId == cityId && vc.UserId == userId);
        }
        
        public async Task<bool> IsCityInWishlistAsync(int cityId, string userId)
        {
            return await _context.DreamDestinations.AnyAsync(d => d.CityId == cityId && d.UserId == userId);
        }
        
        public async Task<bool> MarkCityAsVisitedAsync(int cityId, string userId, DateTime visitDate)
        {
            try
            {
                var visitedCity = new VisitedCity
                {
                    UserId = userId,
                    CityId = cityId,
                    VisitDate = visitDate
                };
                
                await AddVisitedCityAsync(visitedCity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in MarkCityAsVisitedAsync for CityId {cityId}, UserId {userId}");
                return false;
            }
        }
    }
}