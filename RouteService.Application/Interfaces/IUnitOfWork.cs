namespace RouteService.Application.Interfaces;

public interface IUnitOfWork
{
    IRouteRepository RouteRepository { get; }
    IRouteBookmarkRepository RouteBookmarkRepository { get; }
    IRouteCommentRepository RouteCommentRepository { get; }
    IRouteMediaFileRepository RouteMediaFileRepository { get; }
    IReactionRepository ReactionRepository { get; }
    IFirebaseStorageRepository FirebaseStorageRepository { get; }
}