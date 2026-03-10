using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TaskApp.Application.Models;

public class TaskModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static TaskModel FromEntity(TaskApp.Api.Data.Models.TaskItem entity)
    {
        return new TaskModel
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description ?? string.Empty,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }

    public void CopyTo(Api.Data.Models.TaskItem entity)
    {
        entity.Title = Title;
        entity.Description = Description;
        entity.Status = Status;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public Api.Data.Models.TaskItem ToEntityModel()
    {
        return new Api.Data.Models.TaskItem
        {
            Id = Id,
            Title = Title,
            Description = Description,
            Status = Status,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
