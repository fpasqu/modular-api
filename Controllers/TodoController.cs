using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using ModularApi.Models;
using ModularApi.Models.Data;
using ModularApi.Models.Data.Entities;
using System.Diagnostics;

namespace ModularApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger _factoryLogger;

        public TodoController(ITodoRepository todoRepository, IMapper mapper, LinkGenerator linkGenerator, ILoggerFactory loggerFactory)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
            _factoryLogger = loggerFactory.CreateLogger("ControllerFactoryLogger");
        }

        [HttpGet("{uid}")]
        public async Task<ActionResult<TodoModel>> Get(string uid)
        {
            //looks for todo with specified id
            var todo = await _todoRepository.GetTodoById(uid);
            //return NotFound if it's null
            if (todo == null)
            {
                _factoryLogger.LogDebug($"(F) Could not find todo with id {uid}");
                return NotFound();
            }
            return Ok(_mapper.Map<TodoModel>(todo));
        }

        [HttpGet]
        public async Task<ActionResult<TodoModel[]>> Get()
        {
            try
            {
                //telemetry
                Activity.Current?.AddEvent(new ActivityEvent("(A) Trying to retrieve todos"));
                var todos = await _todoRepository.GetAllTodos();
                Activity.Current?.AddEvent(new ActivityEvent("(A) Todos from db retrieved successfully"));
                return _mapper.Map<TodoModel[]>(todos);

            }
            catch (Exception)
            {
                //if it fails to get at least one todo, we return a custom status code
                _factoryLogger.LogError("(F) Failed to get todos");
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to get todos");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TodoModel>> Post(TodoModel todoModel)
        {
            //the path is invalid if the name is invalid, so it's a bad request
            var validPath = _linkGenerator.GetPathByAction("Get", "Todo", new { name = todoModel.Name });
            if (string.IsNullOrWhiteSpace(validPath))
            {
                _factoryLogger.LogDebug("(F) Invalid request path");
                return BadRequest("Invalid request path");
            }

            //if this variable is full, it means that it cant be created with that name
            var alreadyExists = await _todoRepository.GetTodoByName(todoModel.Name);
            if (alreadyExists != null)
            {
                _factoryLogger.LogDebug("(F) Todo name already in use");
                return BadRequest("Todo name already in use");
            }

            //adding the todo with mapping
            var todo = _mapper.Map<Todo>(todoModel);
            await _todoRepository.AddTodo(todo);
            if (await _todoRepository.Commit())
            {
                //converting it back to TodoModel for final return
                return Created($"/api/todo/{todo.Name}", _mapper.Map<TodoModel>(todo));
            }
            _factoryLogger.LogError("(F) Failed to post todos");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to post todo");
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<TodoModel>> Put(TodoModel todoModel, string name)
        {
            //check for both values
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(todoModel.Name))
            {
                _factoryLogger.LogDebug("(F) Invalid todo name");
                return BadRequest("Invalid todo name");
            }

            var alreadyExists = await _todoRepository.GetTodoByName(name);
            if (alreadyExists == null)
            {
                return NotFound();
            }

            //maps to new model and saves the changes
            _mapper.Map(todoModel, alreadyExists);
            if (await _todoRepository.Commit())
            {
                return _mapper.Map<TodoModel>(alreadyExists);
            }
            _factoryLogger.LogError("(F) Failed to update todo");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to update todo");
        }

        [HttpDelete("uid/{uid}")]
        public async Task<ActionResult<TodoModel>> Delete(string uid)
        {
            var currentTodo = await _todoRepository.GetTodoById(uid);
            if (currentTodo == null)
            {
                return NotFound();
            }

            await _todoRepository.DeleteTodo(currentTodo);
            if (await _todoRepository.Commit())
            {
                _factoryLogger.LogDebug("(F) Cannot find todo");
                return _mapper.Map<TodoModel>(currentTodo);
            }
            _factoryLogger.LogError("(F) Failed to delete todo");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete todo");
        }

        [HttpDelete("name/{name}")]
        public async Task<ActionResult<TodoModel>> DeleteByName(string name)
        {
            var currentTodo = await _todoRepository.GetTodoByName(name);
            if (currentTodo == null)
            {
                return NotFound();
            }

            await _todoRepository.DeleteTodo(currentTodo);
            if (await _todoRepository.Commit())
            {
                _factoryLogger.LogDebug("(F) Cannot find todo");
                return _mapper.Map<TodoModel>(currentTodo);
            }
            _factoryLogger.LogError("(F) Failed to delete todo");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete todo");
        }

        [HttpPatch("{uid}")]
        public async Task<ActionResult<TodoModel>> Patch(JsonPatchDocument todoModel, string uid)
        {
            var currentTodo = await _todoRepository.GetTodoById(uid);
            if (currentTodo == null)
            {
                _factoryLogger.LogDebug("(F) Cannot find todo");
                return NotFound();
            }

            //applies the new changes of the model to the current one, then saves changes
            todoModel.ApplyTo(currentTodo);
            if (await _todoRepository.Commit())
            {
                return _mapper.Map<TodoModel>(currentTodo);
            }
            _factoryLogger.LogError("(F) Failed to patch todo");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to patch todo");
        }
    }
}