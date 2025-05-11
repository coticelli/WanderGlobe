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
        }        public async Task<List<DreamDestination>> GetUserWishlistAsync(string userId)
        {
            // Recupera la wishlist dal database
            return await _context.DreamDestinations
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<PlannedTrip>> GetUserPlannedTripsAsync(string userId)
        {
            // Recupera i viaggi pianificati dal database
            return await _context.PlannedTrips
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }        public Task<List<RecommendedDestination>> GetRecommendationsAsync(string userId)
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

        public async Task<bool> RemoveFromWishlistAsync(int destinationId, string userId)
        {
            try
            {
                // Trova la destinazione nella wishlist dell'utente
                var destination = await _context.DreamDestinations
                    .FirstOrDefaultAsync(d => d.Id == destinationId && d.UserId == userId);
                
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
        }        public async Task<PlannedTrip> CreatePlannedTripAsync(PlannedTrip trip)
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

        public async Task<bool> DeletePlannedTripAsync(int tripId, string userId)
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
        }        public async Task<bool> MarkTripAsVisitedAsync(int tripId, string userId)
        {
            try
            {
                // In una implementazione completa, potremmo trasformare un viaggio pianificato
                // in un viaggio visitato, oppure creare un record nella tabella apposita
                // Per ora, semplicemente eliminiamo il viaggio pianificato
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