using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordRepository _passwordRepository;

        public EmployeeService(IUnitOfWork unitOfWork, IPasswordRepository passwordRepository)
        {
            _unitOfWork = unitOfWork;
            _passwordRepository = passwordRepository;
        }

        public async Task<PagedResult<EmployeeResponse>> GetEmployeesAsync(EmployeeSearchRequest request)
        {
            var employees = await _unitOfWork.employeeRepository.GetEmployeesAsync(
                request.Keyword,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            var totalItems = await _unitOfWork.employeeRepository.GetTotalEmployeesAsync(request.Keyword);

            return new PagedResult<EmployeeResponse>
            {
                Items = employees.Select(MapToResponse).ToList(),
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
            };
        }

        public async Task<EmployeeResponse> GetEmployeeByIdAsync(string id)
        {
            var employee = await _unitOfWork.employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
                return null;

            return MapToResponse(employee);
        }

        public async Task<EmployeeResponse> CreateEmployeeAsync(EmployeeCreateRequest request)
        {
            if (await _unitOfWork.employeeRepository.IsUsernameExistsAsync(request.Username))
                throw new ValidationException("The inputted account is already existed, please choose another account name");

            if (await _unitOfWork.employeeRepository.IsEmailExistsAsync(request.Email))
                throw new ValidationException("Email already exists");

            if (await _unitOfWork.employeeRepository.IsIdentityNumberExistsAsync(request.IdentityNumber))
                throw new ValidationException("Identity number already exists");

            var employee = new User
            {
                Userid = Guid.NewGuid().ToString(),
                Username = request.Username,
                Fullname = request.Fullname,
                Identitynumber = request.IdentityNumber,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                Password = _passwordRepository.HashPassword(request.Password),
                Joindate = DateTime.Now,
                Roleid = 3, // Employee role
                Status = 1, // Active
                Birthdate = DateOnly.FromDateTime(request.DateOfBirth),
                Gender = request.Sex ? 1 : 0 // 1 for male, 0 for female
            };

            var createdEmployee = await _unitOfWork.employeeRepository.CreateEmployeeAsync(employee);
            return MapToResponse(createdEmployee);
        }

        public async Task<EmployeeResponse> UpdateEmployeeAsync(string id, EmployeeUpdateRequest request)
        {
            var employee = await _unitOfWork.employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
                throw new ValidationException("Employee not found");

            // Ensure we're only updating employees (role ID 3)
            if (employee.Roleid != 3)
                throw new ValidationException("Cannot update non-employee user");

            // Check if email is already used by another employee
            if (await _unitOfWork.employeeRepository.IsEmailExistsAsync(request.Email) &&
                employee.Email != request.Email)
                throw new ValidationException("Email already exists");

            // Check if identity number is already used by another employee
            if (await _unitOfWork.employeeRepository.IsIdentityNumberExistsAsync(request.IdentityNumber) &&
                employee.Identitynumber != request.IdentityNumber)
                throw new ValidationException("Identity number already exists");

            // Update basic information
            employee.Fullname = request.Fullname;
            employee.Identitynumber = request.IdentityNumber;
            employee.Email = request.Email;
            employee.Phone = request.Phone;
            employee.Address = request.Address;
            employee.Birthdate = DateOnly.FromDateTime(request.DateOfBirth);
            employee.Gender = request.Sex ? 1 : 0;
            employee.Roleid = 3; // Ensure role ID is preserved

            // Update password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                employee.Password = _passwordRepository.HashPassword(request.Password);
                //employee.Password = _passwordHasher.HashPassword(request.Password);
            }

            var updatedEmployee = await _unitOfWork.employeeRepository.UpdateEmployeeAsync(employee);
            return MapToResponse(updatedEmployee);
        }

        public async Task DeleteEmployeeAsync(string id)
        {
            var employee = await _unitOfWork.employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
                throw new ValidationException("Employee not found");

            if (employee.Roleid != 3)
                throw new ValidationException("Cannot delete non-employee user");

            await _unitOfWork.employeeRepository.DeleteEmployeeAsync(id);
        }

        private EmployeeResponse MapToResponse(User user)
        {
            return new EmployeeResponse
            {
                UserId = user.Userid,
                Username = user.Username,
                Fullname = user.Fullname,
                IdentityNumber = user.Identitynumber,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role?.Name,
                JoinDate = user.Joindate ?? DateTime.Now,
                Birthdate = user.Birthdate,
                Gender = user.Gender == 1,
                Image = user.Image
            };
        }
    }
}
