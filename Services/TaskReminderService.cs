using System.Diagnostics;
using Grpc.Core;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using TaskReminderGrpc.Data;
using TaskReminderGrpc.Models;

namespace TaskReminderGrpc.Services;

public class TaskReminderService : TaskReminderGrpcService.TaskReminderGrpcServiceBase
{
    private readonly AppDbContext _dbContext;

    public TaskReminderService(AppDbContext dbContext){
        _dbContext = dbContext;
    }

    public override async Task<CreateTaskReminderResponse> CreateTaskReminder(CreateTaskReminderRequest request, ServerCallContext context){

        if(request.Title == string.Empty || request.Description == string.Empty){
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));
        }

        var taskReminder = new TaskReminder{
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(taskReminder);
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateTaskReminderResponse{
            Id = taskReminder.Id
        });

    }

    public override async Task<ReadTaskReminderResponse> ReadTaskReminder(ReadTaskReminderRequest request, ServerCallContext context){
        if(request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than 0"));

        var taskReminderItem = await _dbContext.taskReminderDbSet.FirstOrDefaultAsync(t => t.Id == request.Id);

        if(taskReminderItem != null){
            return await Task.FromResult(new ReadTaskReminderResponse{
                Id = taskReminderItem.Id,
                Title = taskReminderItem.Title,
                Description = taskReminderItem.Description,
                Status = taskReminderItem.Status
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
    }

    public override async Task<ListAllTaskRemindersResponse> ListAllTaskReminders(ListAllTaskRemindersRequest request, ServerCallContext context){
        var response = new ListAllTaskRemindersResponse();
        var allTaskItems = await _dbContext.taskReminderDbSet.ToListAsync();

        foreach(var taskReminder in allTaskItems){
            response.AllTasks.Add(new ReadTaskReminderResponse{
                Id = taskReminder.Id,
                Title = taskReminder.Title,
                Description = taskReminder.Description,
                Status = taskReminder.Status
            });
        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateTaskReminderResponse> UpdateTaskReminder(UpdateTaskReminderRequest request, ServerCallContext context){
        if(request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object."));
        
        var taskReminderItem = await _dbContext.taskReminderDbSet.FirstOrDefaultAsync(t => t.Id == request.Id);

        if(taskReminderItem == null){
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with Id {request.Id}"));
        }

        taskReminderItem.Title = request.Title;
        taskReminderItem.Description = request.Description;
        taskReminderItem.Status = request.Status;
        
        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new UpdateTaskReminderResponse{
            Id = taskReminderItem.Id
        });
    }

    public override async Task<DeleteTaskReminderResponse> DeleteTaskReminder(DeleteTaskReminderRequest request, ServerCallContext context){
            
        if(request.Id <= 0){
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than 0"));
        }

        var taskReminderItem = await _dbContext.taskReminderDbSet.FirstOrDefaultAsync(t => t.Id == request.Id);

        if(taskReminderItem == null){
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with Id {request.Id}"));
        }

        _dbContext.Remove(taskReminderItem);

        await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new DeleteTaskReminderResponse {
            Id = taskReminderItem.Id
        });
    }
   
}
