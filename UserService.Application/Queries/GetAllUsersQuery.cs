namespace UserService.Application.Queries;

using MediatR;
using UserService.Application.DTOs.ResponseModel;


public class GetAllUsersQuery : IRequest<ResponseModel>
{
    public int PageNumber { get; set; } = 1;
    public int RowsPerPage { get; set; } = 10;
    public string? FilterField { get; set; }
    public string? FilterValue { get; set; }
    public string? SortField { get; set; }
    public bool SortDesc { get; set; }
}
