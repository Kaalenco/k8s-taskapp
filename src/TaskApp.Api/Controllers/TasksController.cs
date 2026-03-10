using Microsoft.AspNetCore.Mvc;

using TaskApp.Application.Models;
using TaskApp.Application.Services;

namespace TaskApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
     private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasks() => Ok(await _taskService.GetAllTasks());

    // GET: api/tasks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskModel>> GetTask(int id)
    {
        var task = await _taskService.GetTask(id);
        if (task == null) return NotFound();
        return task;
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskModel>> CreateTask(TaskModel task)
    {
        var created = await _taskService.CreateTask(task);
        return created!=null
            ?  CreatedAtAction(nameof(GetTask), new { id = created.Id }, created)
            : NoContent();
    }

    // PUT: api/tasks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskModel task)
    {
        task.Id = id;
        var result = await _taskService.UpdateTask(id, task);
        return ResultToActionResult(result);
    }

    // DELETE: api/tasks/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id) => ResultToActionResult(await _taskService.DeleteTask(id));

    private IActionResult ResultToActionResult(UpdateResult result)
    {
        return result switch
        {
            UpdateResult.BadData => BadRequest(),
            UpdateResult.NotFound => NotFound(),
            UpdateResult.NoChanges => NoContent(),
            UpdateResult.Success => NoContent(),
            _ => StatusCode(500)
        };
    }
}
