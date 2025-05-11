using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using WanderGlobe.Models;
using WanderGlobe.Models.Custom;
using WanderGlobe.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WanderGlobe.Pages
{
    public class CityFilterTestModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICityService _cityService;
        private readonly IDreamService _dreamService;

        public CityFilterTestModel(
            UserManager<ApplicationUser> userManager,
            ICityService cityService,
            IDreamService dreamService)
        {
            _userManager = userManager;
            _cityService = cityService;
            _dreamService = dreamService;
        }

        public List<City> AllCities { get; set; } = new List<City>();
        public List<City> VisitedCities { get; set; } = new List<City>();
        public List<City> FilteredCities { get; set; } = new List<City>();
        public List<DreamDestination> UserWishlist { get; set; } = new List<DreamDestination>();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRefreshFiltersAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostMarkAsVisitedAsync(int destinationId)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = _userManager.GetUserId(User);
            
            try
            {
                // Prima otteniamo i dettagli della destinazione
                var userWishlist = await _dreamService.GetUserWishlistAsync(userId);
                var wishlistItem = userWishlist.FirstOrDefault(d => d.Id == destinationId);
                
                if (wishlistItem == null)
                {
                    TempData["ErrorMessage"] = "Destinazione non trovata nella wishlist.";
                    return RedirectToPage();
                }
                
                // Cerchiamo la città nel database
                var cities = await _cityService.GetAllCitiesAsync();
                var city = cities.FirstOrDefault(c => 
                    c.Name.Equals(wishlistItem.CityName, StringComparison.OrdinalIgnoreCase) &&
                    c.Country.Name.Equals(wishlistItem.CountryName, StringComparison.OrdinalIgnoreCase));
                
                if (city == null)
                {
                    TempData["ErrorMessage"] = $"Non è stato possibile trovare la città {wishlistItem.CityName} nel database.";
                    return RedirectToPage();
                }
                
                // Segna la città come visitata e rimuovila dalla wishlist
                var result = await _cityService.MarkCityAsVisitedAsync(city.Id, userId, DateTime.Now);
                
                if (result)
                {
                    TempData["SuccessMessage"] = $"La città {wishlistItem.CityName} è stata segnata come visitata e rimossa dalla wishlist.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Si è verificato un errore durante il salvataggio.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Si è verificato un errore: {ex.Message}";
            }
            
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkRandomAsVisitedAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = _userManager.GetUserId(User);
            
            try
            {
                // Ottieni la wishlist dell'utente
                var userWishlist = await _dreamService.GetUserWishlistAsync(userId);
                
                if (!userWishlist.Any())
                {
                    TempData["ErrorMessage"] = "La wishlist è vuota, non ci sono città da marcare come visitate.";
                    return RedirectToPage();
                }
                
                // Seleziona una destinazione casuale
                var random = new Random();
                int randomIndex = random.Next(0, userWishlist.Count);
                var randomDestination = userWishlist[randomIndex];
                
                // Cerca la città nel database
                var cities = await _cityService.GetAllCitiesAsync();
                var city = cities.FirstOrDefault(c => 
                    c.Name.Equals(randomDestination.CityName, StringComparison.OrdinalIgnoreCase) &&
                    c.Country.Name.Equals(randomDestination.CountryName, StringComparison.OrdinalIgnoreCase));
                
                if (city == null)
                {
                    TempData["ErrorMessage"] = $"Non è stato possibile trovare la città {randomDestination.CityName} nel database.";
                    return RedirectToPage();
                }
                
                // Segna la città come visitata
                var result = await _cityService.MarkCityAsVisitedAsync(city.Id, userId, DateTime.Now);
                
                if (result)
                {
                    TempData["SuccessMessage"] = $"La città {randomDestination.CityName} è stata segnata come visitata e rimossa dalla wishlist.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Si è verificato un errore durante il salvataggio.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Si è verificato un errore: {ex.Message}";
            }
            
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Carica tutte le città
                AllCities = await _cityService.GetAllCitiesAsync();
                
                // Carica la wishlist dell'utente
                UserWishlist = await _dreamService.GetUserWishlistAsync(userId);
                
                // Ottieni le città visitate
                var visitedCities = new List<City>();
                foreach (var city in AllCities)
                {
                    bool isVisited = await _cityService.IsCityVisitedByUserAsync(city.Id, userId);
                    if (isVisited)
                    {
                        visitedCities.Add(city);
                    }
                }
                VisitedCities = visitedCities;
                
                // Ottieni le città filtrate per la selezione
                var citiesNotInWishlist = await _cityService.GetCitiesNotInWishlistAsync(userId);
                var filteredCities = new List<City>();
                
                foreach (var city in citiesNotInWishlist)
                {
                    bool isVisited = await _cityService.IsCityVisitedByUserAsync(city.Id, userId);
                    if (!isVisited)
                    {
                        filteredCities.Add(city);
                    }
                }
                FilteredCities = filteredCities;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Si è verificato un errore: {ex.Message}";
            }
        }
    }
}
