// Folder: Services
// File: IVisitedCityService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WanderGlobe.Models; // Or WanderGlobe.ViewModels if VisitedCityViewModel is there

namespace WanderGlobe.Services
{
    public interface IVisitedCityService
    {
        /// <summary>
        /// Gets all visited city records for a specific user, mapped to ViewModels.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of VisitedCityViewModel objects.</returns>
        Task<List<VisitedCityViewModel>> GetVisitedCitiesForUserAsync(string userId);

        /// <summary>
        /// Adds a new visited city record for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cityId">The ID of the city visited.</param>
        /// <param name="visitDate">The date of the visit.</param>
        /// <param name="notes">Optional notes about the visit.</param>
        /// <returns>The ID of the newly created VisitedCity record, or 0 if failed.</returns>
        Task<int> AddVisitedCityAsync(string userId, int cityId, DateTime visitDate, string? notes);

        /// <summary>
        /// Updates an existing visited city record.
        /// </summary>
        /// <param name="visitedCityRecordId">The unique ID of the VisitedCity record to update.</param>
        /// <param name="userId">The ID of the user (for verification).</param>
        /// <param name="newVisitDate">The new date of the visit.</param>
        /// <param name="newNotes">The new notes for the visit.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        Task<bool> UpdateVisitedCityAsync(int visitedCityRecordId, string userId, DateTime newVisitDate, string? newNotes);

        /// <summary>
        /// Removes a specific visited city record.
        /// </summary>
        /// <param name="visitedCityRecordId">The unique ID of the VisitedCity record to remove.</param>
        /// <param name="userId">The ID of the user (for verification).</param>
        /// <returns>True if removal was successful, false otherwise.</returns>
        Task<bool> RemoveVisitedCityAsync(int visitedCityRecordId, string userId);

        /// <summary>
        /// Gets a single VisitedCityViewModel by its record ID, ensuring it belongs to the user.
        /// </summary>
        /// <param name="visitedCityRecordId">The ID of the VisitedCity record.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A VisitedCityViewModel or null if not found or not authorized.</returns>
        Task<VisitedCityViewModel?> GetVisitedCityByIdAsync(int visitedCityRecordId, string userId);
    }
}