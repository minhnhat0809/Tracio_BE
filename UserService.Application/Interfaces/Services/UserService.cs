using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;
public interface IUserService
{
    // queries
    Task<ResponseModel> GetAllUsersAsync(int pageNumber = 1, 
        int rowsPerPage = 10, 
        string? filterField = null, 
        string? filterValue = null,
        string? sortField = null, 
        bool sortDesc = false);
    Task<ResponseModel> GetUserByIdAsync(int userId);
    Task<ResponseModel> GetUserByPropertyAsync(string property);
    // commands
    Task<ResponseModel> UpdateUserAsync(int userId, UpdateUserProfileModel userModel);
    Task<ResponseModel> UpdateUserAvatarAsync(int userId, IFormFile file);
    
    Task<ResponseModel> DeleteAccountAsync(int userId);
    Task<ResponseModel> BanUserAsync(int userId);
    Task<ResponseModel> UnBanUserAsync(int userId);
}
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFirebaseStorageRepository _firebaseStorageRepository;
    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseStorageRepository firebaseStorageRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _firebaseStorageRepository = firebaseStorageRepository;
    }

    /// <summary>
    /// Get all users with pagination, sorting, and filtering
    /// </summary>
    public async Task<ResponseModel> GetAllUsersAsync(
        int pageNumber = 1, 
        int rowsPerPage = 10, 
        string? filterField = null, 
        string? filterValue = null,
        string? sortField = null, 
        bool sortDesc = false)
    {
        // Validate filter field
        if (!string.IsNullOrEmpty(filterField) && !AllowedFilterFields.Contains(filterField))
        {
            return new ResponseModel("error", 400, 
                $"Invalid filter field '{filterField}'. Allowed fields: {string.Join(", ", AllowedFilterFields)}.", null);
        }

        // Validate sort field
        if (!string.IsNullOrEmpty(sortField) && !AllowedSortFields.Contains(sortField))
        {
            return new ResponseModel("error", 400, 
                $"Invalid sort field '{sortField}'. Allowed fields: {string.Join(", ", AllowedSortFields)}.", null);
        }

        // Filtering logic
        Func<IQueryable<User>, IQueryable<User>>? filter = query =>
        {
            if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
            {
                query = query.Where(u => EF.Property<string>(u, filterField).Contains(filterValue));
            }
            return query;
        };

        // Fetch users with filtering, sorting, and pagination
        var users = await _unitOfWork.UserRepository!.GetAllAsync(
            null, pageNumber, rowsPerPage, sortField, sortDesc);

        // Get total user count (without pagination)
        var totalUsers = (await _unitOfWork.UserRepository!.GetAllAsync()).Count();

        // Convert to DTOs
        var userViewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);

        // Response object
        var response = new
        {
            TotalCount = totalUsers,
            PageNumber = pageNumber,
            RowsPerPage = rowsPerPage,
            Users = userViewModel
        };

        return new ResponseModel("success", 200, "Users retrieved successfully", response);
    }



    /// <summary>
    /// Get a user by ID
    /// </summary>
    public async Task<ResponseModel> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.UserRepository!.GetByIdAsync(userId,"");
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        var userModel = _mapper.Map<UserViewModel>(user);
        return new ResponseModel("success", 200, "User retrieved successfully", userModel);
    }

    /// <summary>
    /// Get a user by a specific property (Email, FirebaseId, or PhoneNumber)
    /// </summary>
    public async Task<ResponseModel> GetUserByPropertyAsync(string property)
    {
        var user = await _unitOfWork.UserRepository!.GetUserByPropertyAsync(property);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        var userViewMode = _mapper.Map<UserViewModel>(user);
        return new ResponseModel("success", 200, "User retrieved successfully", userViewMode);
    }

    /// <summary>
    /// Update user details
    /// </summary>
    public async Task<ResponseModel> UpdateUserAsync(int userId, UpdateUserProfileModel userModel)
    {
        var user = await _unitOfWork.UserRepository!.GetByIdAsync(userId);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        _mapper.Map(userModel, user);
        
        await _unitOfWork.UserRepository.UpdateAsync(user);
        return new ResponseModel("success", 200, "User updated successfully", _mapper.Map<UserViewModel>(user));
    }

    /// <summary>
    /// Update user avatar (To be implemented)
    /// </summary>
    public async Task<ResponseModel> UpdateUserAvatarAsync(int userId, IFormFile? file)
    {
        // Fetch user from database
        var user = await _unitOfWork.UserRepository!.GetByIdAsync(userId);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        // Validate if file is provided
        if (file == null || file.Length == 0)
            return new ResponseModel("error", 400, "No valid file provided.", null);

        try
        {
            // ✅ Delete old avatar if it exists
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                bool isDeleted = await _firebaseStorageRepository.DeleteImageByUrlAsync(user.ProfilePicture);
                if (!isDeleted)
                {
                    return new ResponseModel("error", 500, "Failed to delete old avatar.", null);
                }
            }

            // ✅ Generate unique file name
            var fileName = $"avatars/{Guid.NewGuid()}";

            // ✅ Upload new image
            var uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, file, "avatars");
            if (string.IsNullOrEmpty(uploadedUrl))
                return new ResponseModel("error", 500, "Failed to upload new avatar.", null);

            // ✅ Update user profile with new avatar
            user.ProfilePicture = uploadedUrl;
            await _unitOfWork.UserRepository.UpdateAsync(user);

            return new ResponseModel("success", 200, "Avatar updated successfully", _mapper.Map<UserViewModel>(user));
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, $"An unexpected error occurred: {ex.Message}", null);
        }
    }


   
    /// <summary>
    /// Permanently delete a user
    /// </summary>
    public async Task<ResponseModel> DeleteAccountAsync(int userId)
    {
        var user = await _unitOfWork.UserRepository!.GetByIdAsync(userId);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);
        user.IsActive = false;
        await _unitOfWork.UserRepository.UpdateAsync(user);
        return new ResponseModel("success", 200, "User deleted permanently", null);
    }

    /// <summary>
    /// Permanently ban a user [Soft delete a user]
    /// </summary>
    public async Task<ResponseModel> BanUserAsync(int userId)
    {
        var user = await _unitOfWork.UserRepository!.GetByIdAsync(userId);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);
        user.IsActive = false;
        await _unitOfWork.UserRepository.UpdateAsync(user);
        return new ResponseModel("success", 200, "User has been banned", null);
    }
    
    /// <summary>
    /// Permanently ban a user [Soft delete a user]
    /// </summary>
    public async Task<ResponseModel> UnBanUserAsync(int userId)
    {
        var user = await _unitOfWork.UserRepository!.GetByIdAsync(userId);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);
        user.IsActive = true;
        await _unitOfWork.UserRepository.UpdateAsync(user);
        return new ResponseModel("success", 200, "User has been banned", null);
    }
    
    // List of allowed filter and sort fields
    private static readonly HashSet<string> AllowedFilterFields = new()
    {
        "UserName", "Email", "PhoneNumber", "City", "District", "Gender"
    };

    private static readonly HashSet<string> AllowedSortFields = new()
    {
        "UserName", "Email", "Gender", "Weight", "Height", "City", "District"
    };

}
