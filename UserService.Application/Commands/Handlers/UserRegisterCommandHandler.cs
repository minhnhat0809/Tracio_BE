
namespace UserService.Application.Commands.Handlers;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FirebaseAdmin.Auth;
using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Domain.Entities;

public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, ResponseModel>
{
    private readonly IUnitOfWork _repository;
    private readonly IMapper _mapper;
    private readonly IFirebaseStorageRepository _firebaseStorageRepository;

    public UserRegisterCommandHandler(IUnitOfWork repository, IMapper mapper, IFirebaseStorageRepository firebaseStorageRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _firebaseStorageRepository = firebaseStorageRepository;
    }

    public async Task<ResponseModel> Handle(UserRegisterCommand? registerModel, CancellationToken cancellationToken)
    {
        try
        {
            // Validate required fields
            if (registerModel == null)
            {
                return new ResponseModel("error", 400, "Request data is missing.", null);
            }

            if (string.IsNullOrEmpty(registerModel.RegisterModel?.FirebaseUid))
            {
                return new ResponseModel("error", 400, "Firebase UID is required.", null);
            }

            if (string.IsNullOrEmpty(registerModel.RegisterModel.Email))
            {
                return new ResponseModel("error", 400, "Email is required.", null);
            }
            
            if (registerModel.RegisterModel.AvatarFile != null)
            {
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(registerModel.RegisterModel.AvatarFile.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                {
                    return new ResponseModel("error", 400, "File Error: Only JPG, JPEG, or PNG files are allowed.", null);
                }

            }

            // Check if the user already exists
            var existingUser = await _repository.UserRepository.GetUserByMultiplePropertiesAsync(
                registerModel.RegisterModel.Email, registerModel.RegisterModel.FirebaseUid, registerModel.RegisterModel.PhoneNumber
            );

            if (existingUser != null)
            {
                return new ResponseModel("error", 409, "A user with this email, phone, or Firebase UID already exists.", null);
            }

            // Get Firebase User
            try
            {
                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(registerModel.RegisterModel.FirebaseUid, cancellationToken);
                if (firebaseUser == null)
                {
                    return new ResponseModel("error", 404, "No user found in Firebase with the given UID.", null);
                }
                // check email verify
                if (!firebaseUser.EmailVerified)
                {
                    return new ResponseModel("error", 404, "Email is not verified. Please verify your email before registering.", null);
                }
                // check phone verify
                if (!string.IsNullOrEmpty(registerModel.RegisterModel.PhoneNumber))
                {
                    if (firebaseUser.PhoneNumber != registerModel.RegisterModel.PhoneNumber)
                    {
                        return new ResponseModel("error", 400, "Phone number does not match with Firebase record.", null);
                    }

                    if (!firebaseUser.PhoneNumber.StartsWith("+")) // Kiểm tra định dạng E.164
                    {
                        return new ResponseModel("error", 400, "Invalid phone number format. Ensure the number is in E.164 format.", null);
                    }
                }
            }
            catch (FirebaseAuthException firebaseEx)
            {
                return new ResponseModel("error", 500, "Firebase authentication error.", firebaseEx.Message);
            }

            
            
            // Upload avatar if provided
            string uploadedUrl = "";
            if (registerModel.RegisterModel.AvatarFile != null)
            {
                try
                {
                    var fileName = $"avatars/{Guid.NewGuid()}"; // Unique filename
                    uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, registerModel.RegisterModel.AvatarFile, "avatars");

                    if (string.IsNullOrEmpty(uploadedUrl))
                    {
                        return new ResponseModel("error", 500, "Failed to upload avatar.", null);
                    }
                }
                catch (Exception storageEx)
                {
                    return new ResponseModel("error", 500, "Error uploading avatar.", storageEx.Message);
                }
            }

            // Define Cyclist Role as Binary (100000000 = 256 in decimal)
            int cyclistRole = 256;
            byte[] roleBytes = BitConverter.GetBytes(cyclistRole);

            // Create a new User object
            var newUser = new User
            {
                FirebaseId = registerModel.RegisterModel.FirebaseUid,
                Email = registerModel.RegisterModel.Email,
                UserName = registerModel.RegisterModel.UserName,
                ProfilePicture = uploadedUrl,
                PhoneNumber = registerModel.RegisterModel.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = roleBytes
            };

            // Create the User in the Database
            try
            {
                var user = await _repository.UserRepository.CreateAsync(newUser);
                
                if (user != null)
                {
                    // Convert role from byte[] to int
                    int roleInt = BitConverter.ToInt32(user.Role, 0);
                    string roleName = (roleInt == 256) ? "cyclist" : "shop_owner";

                    var claims = new Dictionary<string, object>
                    {
                        { "role", roleName },
                        { "custom_id", user.UserId }
                    };
                    await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.FirebaseId, claims, cancellationToken);
                }

                return new ResponseModel("success", 201, "User created successfully.", _mapper.Map<UserViewModel>(newUser));
            }
            catch (Exception dbEx)
            {
                return new ResponseModel("error", 500, "Database error occurred while creating user.", dbEx.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UserRegister] Unexpected error: {ex.Message}");

            return new ResponseModel("error", 500, "An unexpected error occurred.", "Internal Server Error.");
        }
    }
}

