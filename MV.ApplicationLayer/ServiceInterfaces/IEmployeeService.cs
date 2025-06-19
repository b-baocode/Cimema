using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IEmployeeService
    {
        Task<PagedResult<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request);
        Task<EmployeeResponse> GetEmployeeByIdAsync(string id);
        Task<EmployeeResponse> CreateEmployeeAsync(EmployeeCreateRequest request);
        Task<EmployeeResponse> UpdateEmployeeAsync(string id, EmployeeUpdateRequest request);
        Task DeleteEmployeeAsync(string id);
    }
}
