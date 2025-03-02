using AutoMapper;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs;
using RouteService.Application.DTOs.RouteMediaFiles;
using RouteService.Application.DTOs.Routes;
using RouteService.Domain.Entities;

namespace RouteService.Application.Interfaces.Services;

public interface IRouteMediaFileService
{
    Task<ResponseModel> GetAllRouteMediaFilesAsync(int routeId, int pageIndex, int pageSize, string? sortBy,
        bool sortDesc);
    Task<ResponseModel?> GetRouteMediaFileAsync(int routeId, int pictureId);
    Task<ResponseModel> CreateRouteMediaFileAsync(int routeId, RouteMediaFileRequestModel requestModel);
    Task<ResponseModel> DeleteRouteMediaFileAsync(int routeId, int pictureId);
    Task<ResponseModel> DeleteAllRouteMediaFileAsync(int routeId);
}

public class RouteMediaFileService : IRouteMediaFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RouteMediaFileService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all media files for a specific route
        /// </summary>
        public async Task<ResponseModel>  GetAllRouteMediaFilesAsync(int routeId, int pageIndex, int pageSize, string? sortBy, bool sortDesc)
        {
            var (mediaFiles, totalCount) = await _unitOfWork.RouteMediaFileRepository
                .GetAllAsync( pageIndex, pageSize, sortBy, sortDesc,
                new Dictionary<string, string> { { "RouteId", routeId.ToString() } });

            if (mediaFiles == null)
                return new ResponseModel(null, "No media files found for this route.", false, 404);

            var response = _mapper.Map<List<RouteMediaFileViewModel>>(mediaFiles);
            return new ResponseModel(response, "Media files retrieved successfully.", true, 200);
        }

        /// <summary>
        /// Get a specific media file by ID
        /// </summary>
        public async Task<ResponseModel?> GetRouteMediaFileAsync(int routeId, int pictureId)
        {
            var mediaFile = await _unitOfWork.RouteMediaFileRepository.GetByIdAsync(pictureId);
            if (mediaFile == null || mediaFile.RouteId != routeId)
                return new ResponseModel(null, "Media file not found.", false, 404);

            var response = _mapper.Map<RouteMediaFileViewModel>(mediaFile);
            return new ResponseModel(response, "Media file retrieved successfully.", true, 200);
        }

        /// <summary>
        /// Create a new media file for a route
        /// </summary>
        public async Task<ResponseModel> CreateRouteMediaFileAsync(int routeId, RouteMediaFileRequestModel requestModel)
        {
            // Validate if file is provided
            if (requestModel.MediaFile.Length == 0)
                return new ResponseModel(null, "error", false, 400);
            try
            {
                var routeMediaFile = await _unitOfWork.RouteRepository.GetByFilterAsync(
                    new Dictionary<string, string> { { "RouteId", routeId.ToString() }});
                if (routeMediaFile == null)
                    return new ResponseModel(null, "No media file found for this route.", false, 404);
                // check user exist
                // check location is on route ?
                // add image url
                
               
                // Generate unique file name
                var fileName = Guid.NewGuid().ToString();

                // Upload new image
                var uploadedUrl = await _unitOfWork.FirebaseStorageRepository.UploadImageAsync(
                    fileName, requestModel.MediaFile, 
                    "media-file");
            
                if (string.IsNullOrEmpty(uploadedUrl))
                    return new ResponseModel(null, "Failed to upload new avatar.",false, 500);

                
                var newMediaFile = new RouteMediaFile
                {
                    RouteId = routeId,
                    CyclistId = requestModel.CyclistId,
                    MediaUrl = uploadedUrl,
                    Location = new Point(  // { X, Y, Z }
                            requestModel.Location.Longitude, 
                            requestModel.Location.Latitude, 
                            requestModel.Location.Altitude) 
                        { SRID = 4326 }, 
                    CapturedAt = DateTime.UtcNow,
                    UploadedAt = DateTime.UtcNow
                };

                var createdMedia = await _unitOfWork.RouteMediaFileRepository.CreateAsync(newMediaFile);
                return new ResponseModel(_mapper.Map<RouteMediaFileViewModel>(createdMedia), "Media file created successfully.", true, 201);
            }
            catch (Exception ex)
            {
                return new ResponseModel(null, $"An unexpected error occurred: {ex.Message}",false, 500);
            }
        }

        /// <summary>
        /// Delete a specific media file
        /// </summary>
        public async Task<ResponseModel> DeleteRouteMediaFileAsync(int routeId, int pictureId)
        {
            var mediaFile = await _unitOfWork.RouteMediaFileRepository.GetByIdAsync(pictureId);
            if (mediaFile == null || mediaFile.RouteId != routeId)
                return new ResponseModel(null, "Media file not found.", false, 404);
            
            // ✅ Delete storage image before delete
            if (!string.IsNullOrEmpty(mediaFile.MediaUrl))
            {
                bool isDeleted = await _unitOfWork.FirebaseStorageRepository.DeleteImageByUrlAsync(mediaFile.MediaUrl);
                if (!isDeleted)
                {
                    return new ResponseModel(null, "Failed to delete old avatar.", false,500);
                }
            }                    

            await _unitOfWork.RouteMediaFileRepository.DeleteAsync(pictureId);
            return new ResponseModel(null, "Media file deleted successfully.", true, 200);
        }

        /// <summary>
        /// Delete all media files for a specific route
        /// </summary>
        public async Task<ResponseModel> DeleteAllRouteMediaFileAsync(int routeId)
        {
            try
            {
                var mediaFiles = await _unitOfWork.RouteMediaFileRepository.DeleteAllRouteMediaFilesByRouteIdAsync(routeId);

                if (!mediaFiles.Any()) // Check if no media files were deleted
                {
                    return new ResponseModel(null, "No media files found for the given route.", false, 404);
                }

                return new ResponseModel(null, "All media files deleted successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ResponseModel(null, $"Error deleting media files: {ex.Message}", false, 500);
            }
        }

    }