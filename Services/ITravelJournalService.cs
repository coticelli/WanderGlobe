using WanderGlobe.Models.Custom;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public interface ITravelJournalService
    {
        Task<List<TimelineEntry>> GetTimelineByUserAsync(string userId, string sort = "desc");
        Task<List<int>> GetVisitedYearsAsync(string userId);
        Task<bool> AddJournalNoteAsync(TimelineNote note);
        Task<bool> AddPhotoAsync(int countryId, string userId, string caption, string imageUrl);
    }
}