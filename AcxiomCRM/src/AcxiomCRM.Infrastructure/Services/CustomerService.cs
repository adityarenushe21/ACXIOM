using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AcxiomCRM.Application.Common.Exceptions;
using AcxiomCRM.Application.Common.Interfaces;
using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Common.Pagination;
using AcxiomCRM.Application.Customers.DTOs;
using AcxiomCRM.Application.Customers.Interfaces;
using AcxiomCRM.Domain.Constants;
using AcxiomCRM.Domain.Entities;
using AcxiomCRM.Domain.Enums;
using AcxiomCRM.Infrastructure.Persistence;

namespace AcxiomCRM.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ILogger<CustomerService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _logger = logger;
    }

    private IQueryable<Customer> ApplyAuthorizationScope(IQueryable<Customer> query)
    {
        var currentUserId = _currentUserService.UserId ?? string.Empty;

        // Admin and Manager can view all customers
        if (_currentUserService.IsInRole(Roles.Admin) || _currentUserService.IsInRole(Roles.Manager))
        {
            return query;
        }

        // SalesExecutive can only view their own assigned customers
        if (_currentUserService.IsInRole(Roles.SalesExecutive))
        {
            return query.Where(c => c.OwnerId == currentUserId || c.CreatedBy == currentUserId);
        }

        // Fallback: deny all
        return query.Where(_ => false);
    }

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(PaginationQuery query, CustomerStatus? status = null, CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Customers.AsNoTracking();
        baseQuery = ApplyAuthorizationScope(baseQuery);

        if (status.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            baseQuery = baseQuery.Where(c =>
                c.CustomerName.ToLower().Contains(search) ||
                c.CustomerCode.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(search)));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // Sorting
        baseQuery = (query.SortBy?.ToLower(), query.SortDescending) switch
        {
            ("customername", true) => baseQuery.OrderByDescending(c => c.CustomerName),
            ("customername", false) => baseQuery.OrderBy(c => c.CustomerName),
            ("customercode", true) => baseQuery.OrderByDescending(c => c.CustomerCode),
            ("customercode", false) => baseQuery.OrderBy(c => c.CustomerCode),
            ("createddate", false) => baseQuery.OrderBy(c => c.CreatedDate),
            _ => baseQuery.OrderByDescending(c => c.CreatedDate)
        };

        var items = await baseQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                CompanyName = c.CompanyName,
                Address = c.Address,
                City = c.City,
                State = c.State,
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                CreatedBy = c.CreatedBy,
                OwnerId = c.OwnerId
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerDto>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<Result<CustomerDto>> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking().Where(c => c.CustomerId == id);
        query = ApplyAuthorizationScope(query);

        var customer = await query.FirstOrDefaultAsync(cancellationToken);
        if (customer == null)
        {
            // Check if it exists at all to differentiate 404 vs 403
            var exists = await _context.Customers.AnyAsync(c => c.CustomerId == id, cancellationToken);
            if (exists)
            {
                throw new ForbiddenAccessException("You do not have access to view this customer.");
            }
            throw new NotFoundException(nameof(Customer), id);
        }

        var dto = new CustomerDto
        {
            CustomerId = customer.CustomerId,
            CustomerCode = customer.CustomerCode,
            CustomerName = customer.CustomerName,
            Email = customer.Email,
            Phone = customer.Phone,
            CompanyName = customer.CompanyName,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            Status = customer.Status,
            CreatedDate = customer.CreatedDate,
            UpdatedDate = customer.UpdatedDate,
            CreatedBy = customer.CreatedBy,
            OwnerId = customer.OwnerId
        };

        return Result<CustomerDto>.Success(dto);
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        // Enforce uniqueness of CustomerCode and Email
        if (await _context.Customers.AnyAsync(c => c.CustomerCode == request.CustomerCode, cancellationToken))
        {
            throw new ConflictException($"Customer code '{request.CustomerCode}' is already registered.");
        }

        if (await _context.Customers.AnyAsync(c => c.Email == request.Email, cancellationToken))
        {
            throw new ConflictException($"Email '{request.Email}' is already registered for another customer.");
        }

        var currentUserId = _currentUserService.UserId ?? "SYSTEM";
        var assignedOwnerId = request.OwnerId;

        // If SalesExecutive, can only assign to self
        if (_currentUserService.IsInRole(Roles.SalesExecutive) || string.IsNullOrEmpty(assignedOwnerId))
        {
            assignedOwnerId = currentUserId;
        }

        var customer = new Customer
        {
            CustomerCode = request.CustomerCode.Trim(),
            CustomerName = request.CustomerName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Phone = request.Phone.Trim(),
            CompanyName = request.CompanyName?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            Status = CustomerStatus.Active,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = currentUserId,
            OwnerId = assignedOwnerId
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "Create",
            nameof(Customer),
            customer.CustomerId.ToString(),
            null,
            $"Created customer {customer.CustomerCode} ({customer.CustomerName})",
            cancellationToken: cancellationToken);

        var dto = new CustomerDto
        {
            CustomerId = customer.CustomerId,
            CustomerCode = customer.CustomerCode,
            CustomerName = customer.CustomerName,
            Email = customer.Email,
            Phone = customer.Phone,
            CompanyName = customer.CompanyName,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            Status = customer.Status,
            CreatedDate = customer.CreatedDate,
            CreatedBy = customer.CreatedBy,
            OwnerId = customer.OwnerId
        };

        return Result<CustomerDto>.Success(dto);
    }

    public async Task<Result<CustomerDto>> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id, cancellationToken);
        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), id);
        }

        // Ownership authorization check
        var currentUserId = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.SalesExecutive) && customer.OwnerId != currentUserId && customer.CreatedBy != currentUserId)
        {
            throw new ForbiddenAccessException("You can only modify customers assigned to you.");
        }

        // Check Email uniqueness on update
        if (await _context.Customers.AnyAsync(c => c.Email == request.Email && c.CustomerId != id, cancellationToken))
        {
            throw new ConflictException($"Email '{request.Email}' is already registered for another customer.");
        }

        var oldSnapshot = $"{customer.CustomerName}|{customer.Email}|{customer.Status}";

        customer.CustomerName = request.CustomerName.Trim();
        customer.Email = request.Email.Trim().ToLower();
        customer.Phone = request.Phone.Trim();
        customer.CompanyName = request.CompanyName?.Trim();
        customer.Address = request.Address?.Trim();
        customer.City = request.City?.Trim();
        customer.State = request.State?.Trim();
        customer.Status = request.Status;
        customer.UpdatedDate = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.OwnerId) && (_currentUserService.IsInRole(Roles.Admin) || _currentUserService.IsInRole(Roles.Manager)))
        {
            customer.OwnerId = request.OwnerId;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var newSnapshot = $"{customer.CustomerName}|{customer.Email}|{customer.Status}";
        await _auditService.LogAsync(
            currentUserId,
            "Update",
            nameof(Customer),
            customer.CustomerId.ToString(),
            oldSnapshot,
            newSnapshot,
            cancellationToken: cancellationToken);

        var dto = new CustomerDto
        {
            CustomerId = customer.CustomerId,
            CustomerCode = customer.CustomerCode,
            CustomerName = customer.CustomerName,
            Email = customer.Email,
            Phone = customer.Phone,
            CompanyName = customer.CompanyName,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            Status = customer.Status,
            CreatedDate = customer.CreatedDate,
            UpdatedDate = customer.UpdatedDate,
            CreatedBy = customer.CreatedBy,
            OwnerId = customer.OwnerId
        };

        return Result<CustomerDto>.Success(dto);
    }

    public async Task<Result> DeactivateCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id, cancellationToken);
        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), id);
        }

        var currentUserId = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.SalesExecutive) && customer.OwnerId != currentUserId && customer.CreatedBy != currentUserId)
        {
            throw new ForbiddenAccessException("You cannot deactivate customers you do not own.");
        }

        customer.Status = CustomerStatus.Inactive;
        customer.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "Deactivate",
            nameof(Customer),
            customer.CustomerId.ToString(),
            CustomerStatus.Active.ToString(),
            CustomerStatus.Inactive.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
