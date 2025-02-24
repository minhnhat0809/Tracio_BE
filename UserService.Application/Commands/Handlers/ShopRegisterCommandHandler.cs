using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces;
using AutoMapper;
using FirebaseAdmin.Auth;
using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Domain.Entities;

namespace UserService.Application.Commands.Handlers;


public class ShopRegisterCommandHandler : IRequestHandler<ShopRegisterCommand, ResponseModel>
{
    private readonly IUnitOfWork _repository;
    private readonly IMapper _mapper;
    private readonly IFirebaseStorageRepository _firebaseStorageRepository;
    private readonly IFirebaseAuthenticationRepository _firebaseAuthenticationRepository;
    
    public ShopRegisterCommandHandler(IUnitOfWork repository, IMapper mapper, IFirebaseStorageRepository firebaseStorageRepository, IFirebaseAuthenticationRepository firebaseAuthenticationRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _firebaseStorageRepository = firebaseStorageRepository;
        _firebaseAuthenticationRepository = firebaseAuthenticationRepository;
    }

    public async Task<ResponseModel> Handle(ShopRegisterCommand? registerModel, CancellationToken cancellationToken)
    {
        if (registerModel == null)
            return new ResponseModel("error", 400, "Request data is missing.", null);

        if (string.IsNullOrEmpty(registerModel.RegisterModel?.FirebaseUid) || string.IsNullOrEmpty(registerModel.RegisterModel.Email))
            return new ResponseModel("error", 400, "Firebase UID and Email are required.", null);

        // Validate and upload avatar if provided (reuse the same logic)
        string uploadedUrl = string.Empty;
        if (registerModel.RegisterModel.AvatarFile != null)
        {
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(registerModel.RegisterModel.AvatarFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                return new ResponseModel("error", 400, "Only JPG, JPEG, or PNG files are allowed.", null);

            try
            {
                var fileName = $"avatars/{Guid.NewGuid()}";
                
                uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(
                    fileName, 
                    registerModel.RegisterModel.AvatarFile, 
                    "avatars",
                    cancellationToken);
                
                if (string.IsNullOrEmpty(uploadedUrl))
                    return new ResponseModel("error", 500, "Failed to upload avatar.", null);
            }
            catch (Exception ex)
            {
                return new ResponseModel("error", 500, "Error uploading avatar.", ex.Message);
            }
        }

        // Check for duplicate user
        var existingUser = await _repository.UserRepository.GetUserByMultiplePropertiesAsync(
            registerModel.RegisterModel.Email, registerModel.RegisterModel.FirebaseUid, registerModel.RegisterModel.PhoneNumber);
        if (existingUser != null)
            return new ResponseModel("error", 409, "A user with this email, phone, or Firebase UID already exists.", null);

        // Get Firebase user and validate email/phone
        try
        {
            var firebaseUser = await _firebaseAuthenticationRepository.GetFirebaseUserByUidAsync(registerModel.RegisterModel.FirebaseUid, cancellationToken);
            if (!firebaseUser.EmailVerified)
                return new ResponseModel("error", 404, "Email is not verified. Please verify your email before registering.", null);
            if(firebaseUser.Email != registerModel.RegisterModel.Email)
                return new ResponseModel("error", 404, "Email does not match with Firebase record.", null);
            if (!string.IsNullOrEmpty(registerModel.RegisterModel.PhoneNumber))
            {
                if (firebaseUser.PhoneNumber != registerModel.RegisterModel.PhoneNumber)
                    return new ResponseModel("error", 400, "Phone number does not match with Firebase record.", null);
                if (!firebaseUser.PhoneNumber.StartsWith("+"))
                    return new ResponseModel("error", 400, "Invalid phone number format. Ensure the number is in E.164 format.", null);
            }
        }
        catch (FirebaseAuthException fbaEx)
        {
            return new ResponseModel("error", 500, "Firebase authentication error.", fbaEx.Message);
        }

        // Create the User object
        var newUser = new User
        {
            FirebaseId = registerModel.RegisterModel.FirebaseUid,
            Email = registerModel.RegisterModel.Email,
            UserName = registerModel.RegisterModel.UserName,
            ProfilePicture = uploadedUrl,
            PhoneNumber = registerModel.RegisterModel.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Role = BitConverter.GetBytes(128) // shop role 
        };

        try
        {
            var createdUser = await _repository.UserRepository.CreateAsync(newUser);
            if (createdUser != null)
            {
                int roleInt = BitConverter.ToInt32(createdUser.Role, 0);
                string roleName = (roleInt == 256) ? "cyclist" : "shop_owner";
                var claims = new Dictionary<string, object>
                {
                    { "role", roleName },
                    { "custom_id", createdUser.UserId }
                };
                await _firebaseAuthenticationRepository.SetCustomClaimsAsync(createdUser.FirebaseId, claims, cancellationToken);
            }
            return new ResponseModel("success", 201, "User created successfully.", _mapper.Map<UserViewModel>(newUser));
        }
        catch (Exception dbEx)
        {
            return new ResponseModel("error", 500, "Database error occurred while creating user.", dbEx.Message);
        }
    }
}
