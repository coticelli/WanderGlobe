using WanderGlobe.Models.Custom;
using WanderGlobe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WanderGlobe.Services
{
    public class DreamService : IDreamService
    {
        private readonly ApplicationDbContext _context;

        public DreamService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<DreamDestination>> GetUserWishlistAsync(string userId)
        {
            // Recupera la wishlist dal database
            return await _context.DreamDestinations
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

  public async Task<List<PlannedTrip>> GetUserPlannedTripsAsync(string userId)
        {
            // Recupera i viaggi pianificati dal database
            // *** MODIFICA QUI: Aggiungi Include per la Checklist ***
            System.Diagnostics.Debug.WriteLine($"[DreamService.GetUserPlannedTripsAsync] Recupero PlannedTrips per UserId: {userId} CON Inclusione Checklist.");
            var plannedTrips = await _context.PlannedTrips
                .Include(p => p.Checklist) // <= AGGIUNGI QUESTO!
                .Where(p => p.UserId == userId)
                .ToListAsync();
            System.Diagnostics.Debug.WriteLine($"[DreamService.GetUserPlannedTripsAsync] Trovati {plannedTrips.Count} piani. Il primo piano ha {plannedTrips.FirstOrDefault()?.Checklist?.Count ?? 0} item nella checklist.");
            return plannedTrips;
        }
        
        public Task<List<RecommendedDestination>> GetRecommendationsAsync(string userId)
        {
            // Per le raccomandazioni, potremmo implementare una logica più complessa in futuro
            // Per ora, restituisci una lista vuota per non mostrare località non richieste
            return Task.FromResult(new List<RecommendedDestination>());
        }        // Implementazione di altri metodi
        public async Task<DreamDestination> AddToWishlistAsync(DreamDestination destination)
        {
            // Aggiunge la destinazione alla wishlist nel database
            _context.DreamDestinations.Add(destination);
            await _context.SaveChangesAsync();
            return destination;
        }

// In DreamMap.cshtml.cs

// Definire la classe per la richiesta JSON
public class UpdateVisitedDetailsRequest
{
    public string VisitId { get; set; }
    public string? VisitDate { get; set; } // Ricevuto come stringa YYYY-MM-DD
    public int Rating { get; set; }
    public string? Memories { get; set; }
    public List<string>? Highlights { get; set; }
    public List<string>? TravelCompanions { get; set; }
    // public List<string> PhotoUrls { get; set; } // Se gestisci gli URL delle foto
}

/*
// Handler da implementare (esempio, non funzionante senza modello DB e servizio)
[HttpPost]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> OnPostUpdateVisitedDetailsAsync([FromBody] UpdateVisitedDetailsRequest request)
{
    try
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return new JsonResult(new { success = false, message = "Utente non autenticato." });

        // QUI: Logica per trovare e aggiornare l'entità VisitedCity/UserTripMemory nel database
        // basata su request.VisitId e user.Id.
        // Esempio concettuale:
        // var visitedPlace = await _dbContext.UserTripMemories.FirstOrDefaultAsync(m => m.Id == request.VisitId && m.UserId == user.Id);
        // if (visitedPlace == null) return new JsonResult(new { success = false, message = "Visita non trovata." });
        //
        // if (DateTime.TryParse(request.VisitDate, out var parsedDate))
        // {
        //     visitedPlace.VisitDate = parsedDate;
        // }
        // visitedPlace.Rating = request.Rating;
        // visitedPlace.Memories = request.Memories;
        // visitedPlace.Highlights = request.Highlights ?? new List<string>();
        // visitedPlace.TravelCompanions = request.TravelCompanions ?? new List<string>();
        // visitedPlace.UpdatedAt = DateTime.UtcNow;
        //
        // await _dbContext.SaveChangesAsync();

        System.Diagnostics.Debug.WriteLine($"TODO: Implementare salvataggio backend per VisitId: {request.VisitId}");

        return new JsonResult(new { 
            success = true, 
            message = "Ricordi salvati (simulazione backend).",
            // updatedVisit = new { // Invia i dati aggiornati se necessario
            //    id = visitedPlace.Id,
            //    visitDate = visitedPlace.VisitDate.ToString("yyyy-MM-dd"),
            //    rating = visitedPlace.Rating,
            //    memories = visitedPlace.Memories,
            //    highlights = visitedPlace.Highlights,
            //    travelCompanions = visitedPlace.TravelCompanions
            //}
        });
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Errore in OnPostUpdateVisitedDetailsAsync: {ex.Message}");
        return new JsonResult(new { success = false, message = "Errore server durante il salvataggio dei ricordi." });
    }
}
*/

        public async Task<bool> RemoveFromWishlistAsync(int dreamId, string userId)
        {
            try
            {
                // Trova la destinazione nella wishlist dell'utente
                var destination = await _context.DreamDestinations
                    .FirstOrDefaultAsync(d => d.Id == dreamId && d.UserId == userId);

                if (destination != null)
                {
                    _context.DreamDestinations.Remove(destination);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in RemoveFromWishlistAsync: {ex.Message}");
                return false;
            }
        }


        public async Task<PlannedTrip> CreatePlannedTripAsync(PlannedTrip trip)
        {
            // Aggiunge un nuovo viaggio pianificato al database
            _context.PlannedTrips.Add(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<bool> UpdatePlannedTripAsync(PlannedTrip trip)
        {
            try
            {
                _context.PlannedTrips.Update(trip);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in UpdatePlannedTripAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeletePlannedTripAsync(string tripId, string userId)
        {
            try
            {
                var trip = await _context.PlannedTrips
                    .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

                if (trip != null)
                {
                    _context.PlannedTrips.Remove(trip);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in DeletePlannedTripAsync: {ex.Message}");
                return false;
            }
        }




        public async Task<bool> MarkTripAsVisitedAsync(string tripId, string userId)
        {
            try
            {
                return await DeletePlannedTripAsync(tripId, userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in MarkTripAsVisitedAsync: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> IsCityInUserWishlistAsync(int cityId, string userId)
        {
            try
            {
                // Recupera la città dal database
                var city = await _context.Cities.FindAsync(cityId);
                if (city == null)
                    return false;

                // Controlla se la città è nella wishlist dell'utente
                return await _context.DreamDestinations
                    .AnyAsync(d => d.UserId == userId &&
                             d.CityName.Equals(city.Name, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in IsCityInUserWishlistAsync: {ex.Message}");
                return false;
            }
        }
    }
}