// Questo file serve per diagnosticare problemi con la lista delle città
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WanderGlobe.Data;
using WanderGlobe.Models;
using WanderGlobe.Services;
using Microsoft.AspNetCore.Identity;
using WanderGlobe.Models.Custom;

namespace WanderGlobe.Pages
{
    public class DiagnosticModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDreamService _dreamService;
        private readonly ICityService _cityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiagnosticModel(
            ApplicationDbContext context, 
            IDreamService dreamService, 
            ICityService cityService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _dreamService = dreamService;
            _cityService = cityService;
            _userManager = userManager;
        }        public List<City> AllCities { get; set; } = new List<City>();
        public List<City> CapitalCities { get; set; } = new List<City>();
        public List<Country> AllCountries { get; set; } = new List<Country>();
        public List<DreamDestination> UserWishlist { get; set; } = new List<DreamDestination>();
        public List<City> FilteredCities { get; set; } = new List<City>();
        public string? ErrorMessage { get; set; }
        
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Carica tutte le città
                AllCities = await _context.Cities
                    .Include(c => c.Country)
                    .ToListAsync();
                
                // Carica tutte le capitali
                CapitalCities = await _context.Cities
                    .Include(c => c.Country)
                    .Where(c => c.IsCapital)
                    .ToListAsync();
                    
                // Carica tutti i paesi
                AllCountries = await _context.Countries.ToListAsync();
                
                // Ottieni l'utente corrente
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Ottieni la wishlist dell'utente
                    UserWishlist = await _dreamService.GetUserWishlistAsync(user.Id);
                    
                    // Ottieni le città filtrate (non visitate e non in wishlist)
                    var cities = new List<City>();
                    foreach (var city in AllCities)
                    {
                        bool isVisited = await _cityService.IsCityVisitedByUserAsync(city.Id, user.Id);
                        bool isInWishlist = await _cityService.IsCityInWishlistAsync(city.Id, user.Id);
                        
                        if (!isVisited && !isInWishlist)
                        {
                            cities.Add(city);
                        }
                    }
                    FilteredCities = cities;
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Si è verificato un errore: {ex.Message}";
                return Page();
            }
        }
    }
}
