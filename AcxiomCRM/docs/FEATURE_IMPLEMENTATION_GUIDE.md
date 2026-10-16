# Feature Implementation Guide

This guide provides a standardized, repeatable blueprint for implementing any new CRM module or extending existing workflows in **AcxiomCRM**.

To demonstrate this process end-to-end, we use a concrete feature example: **Tasks**.

---

## 1. Request Flow & Dependency Direction

When developing a new module, always implement from the core outward:

```
Step 1: Domain (Entity, Enums)
  ↓
Step 2: Infrastructure Persistence (EF Mapping, Migration)
  ↓
Step 3: Application (DTOs, Validators, Service Interface)
  ↓
Step 4: Infrastructure Implementation (Service logic, Ownership scoping, Auditing)
  ↓
Step 5: API Presentation (Controller, DI registration)
  ↓
Step 6: Automated Testing (Unit Tests, Integration Tests)
  ↓
Step 7: React Frontend Integration (API client method, Form & View Components)
```

---

## 2. Worked Example: Implementing the Tasks Feature

### Step 1: Create Domain Entity (`AcxiomCRM.Domain`)

Create `src/AcxiomCRM.Domain/Entities/TaskItem.cs`:
```csharp
using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Domain.Entities;

public class TaskItem
{
    public Guid TaskId { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public bool IsCompleted { get; set; } = false;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
```

---

### Step 2: Configure EF Core Mapping & DbContext (`AcxiomCRM.Infrastructure`)

1. Add `DbSet<TaskItem> Tasks => Set<TaskItem>();` to `ApplicationDbContext.cs`.
2. Create entity mapping in `src/AcxiomCRM.Infrastructure/Persistence/Configurations/TaskConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AcxiomCRM.Domain.Entities;

namespace AcxiomCRM.Infrastructure.Persistence.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.TaskId);
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.AssignedTo).HasMaxLength(100).IsRequired();

        builder.HasOne(t => t.Customer)
               .WithMany()
               .HasForeignKey(t => t.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.AssignedTo);
        builder.HasIndex(t => t.DueDate);
    }
}
```
3. Generate and apply migration:
```bash
dotnet ef migrations add AddTasksModule --project src/AcxiomCRM.Infrastructure --startup-project src/AcxiomCRM.Api
dotnet ef database update --project src/AcxiomCRM.Infrastructure --startup-project src/AcxiomCRM.Api
```

---

### Step 3: Define Application DTOs, Validators & Interface (`AcxiomCRM.Application`)

Create `src/AcxiomCRM.Application/Tasks/DTOs/TaskDtos.cs`:
```csharp
public class TaskDto
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
}

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? AssignedTo { get; set; }
}
```

Create `src/AcxiomCRM.Application/Tasks/Validators/CreateTaskRequestValidator.cs`:
```csharp
using FluentValidation;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date);
    }
}
```

Define `ITaskService.cs` in `src/AcxiomCRM.Application/Tasks/Interfaces/`:
```csharp
public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetTasksAsync(PaginationQuery query, CancellationToken ct = default);
    Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request, CancellationToken ct = default);
}
```

---

### Step 4: Implement Service with Ownership Scope & Auditing (`AcxiomCRM.Infrastructure`)

In `src/AcxiomCRM.Infrastructure/Services/TaskService.cs`:
- Apply role filtering: If `SalesExecutive`, only return tasks where `AssignedTo == currentUserId`.
- Call `_auditService.LogAsync(...)` when a task is created or completed.

---

### Step 5: Implement API Controller (`AcxiomCRM.Api`)

Create `src/AcxiomCRM.Api/Controllers/TasksController.cs`:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IValidator<CreateTaskRequest> _validator;

    public TasksController(ITaskService taskService, IValidator<CreateTaskRequest> validator)
    {
        _taskService = taskService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] PaginationQuery query, CancellationToken ct)
        => Ok(await _taskService.GetTasksAsync(query, ct));

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var val = await _validator.ValidateAsync(request, ct);
        if (!val.IsValid) return ValidationProblem(new ValidationProblemDetails(val.ToDictionary()));
        var res = await _taskService.CreateTaskAsync(request, ct);
        return Ok(res.Data);
    }
}
```

Register `services.AddScoped<ITaskService, TaskService>();` in `AcxiomCRM.Infrastructure/DependencyInjection.cs`.
