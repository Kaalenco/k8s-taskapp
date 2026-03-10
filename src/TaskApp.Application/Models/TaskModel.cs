using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TaskApp.Application.Models;

public class TaskModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static TaskModel FromEntity(TaskApp.Api.Data.Models.TaskItem entity)
    {
        return new TaskModel
        {
            Id = entity.Id,
            Name = entity.Title,
            Description = entity.Description ?? string.Empty,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }

    public void CopyTo(Api.Data.Models.TaskItem entity)
    {
        entity.Title = Name;
        entity.Description = Description;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public Api.Data.Models.TaskItem ToEntityModel()
    {
        return new Api.Data.Models.TaskItem
        {
            Id = Id,
            Title = Name,
            Description = Description,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
