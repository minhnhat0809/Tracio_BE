using AutoMapper;
using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces;

namespace UserService.Application.Commands.Handlers;

public class UpdateUserAvatarCommandHandler : IRequestHandler<UpdateUserAvatarCommand, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFirebaseStorageRepository _firebaseStorageRepository;
    public UpdateUserAvatarCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseStorageRepository firebaseStorageRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _firebaseStorageRepository = firebaseStorageRepository;
    }
    public async Task<ResponseModel> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        // Fetch user from database
        if (request.UserId == null) return new ResponseModel("error", 400, "User Id is required", null);
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        // Validate if file is provided
        if (request.Avatar == null || request.Avatar.Length == 0)
            return new ResponseModel("error", 400, "No valid file provided.", null);

        try
        {
            // ✅ Delete old avatar if it exists
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                bool isDeleted = await _firebaseStorageRepository.DeleteImageByUrlAsync(user.ProfilePicture, cancellationToken);
                if (!isDeleted)
                {
                    return new ResponseModel("error", 500, "Failed to delete old avatar.", null);
                }
            }

            // ✅ Generate unique file name
            var fileName = $"avatars/{Guid.NewGuid()}";

            // ✅ Upload new image
            var uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(
                fileName, request.Avatar, 
                "avatars", cancellationToken);
            
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
}