using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Domain.Repositories;
using Abp.Modules.Core.Application.Services.Impl;
using Abp.Modules.Core.Domain.Entities;
using Taskever.Application.Services.Dto;
using Taskever.Domain.Entities;
using Taskever.Domain.Services;

namespace Taskever.Application.Services.Impl
{
    public class TaskService : ITaskService
    {
        private readonly IRepository<Task> _taskRepository;
        private readonly ITaskPrivilegeService _taskPrivilegeService;

        public TaskService(IRepository<Task> taskRepository, ITaskPrivilegeService taskPrivilegeService)
        {
            _taskRepository = taskRepository;
            _taskPrivilegeService = taskPrivilegeService;
        }

        public virtual IList<TaskDto> GetMyTasks()
        {
            //var currentUserId = Thread.CurrentPrincipal.Identity.Name
            return _taskRepository.Query(q => q.Where(task => task.AssignedUser.Id == 1).ToList()).MapIList<Task, TaskDto>();
        }

        public virtual GetTasksOfUserOutput GetTasksOfUser(GetTasksOfUserInput args) //TODO: did not worked with GET, why?
        {
            if (!_taskPrivilegeService.CanSeeTasksOfUser(User.CurrentUserId, args.UserId))
            {
                throw new ApplicationException("Can not see tasks of user");
            }

            return new GetTasksOfUserOutput
                       {
                           Tasks = _taskRepository.Query(q => q.Where(task => task.Tenant.Id == Tenant.CurrentTenantId && task.AssignedUser.Id == args.UserId).ToList()).MapIList<Task, TaskDto>()
                       };
        }

        public virtual TaskDto CreateTask(TaskDto task)
        {
            var taskEntity = task.MapTo<Task>();
            
            //TODO: Automatically set Tenant and Creator User informations!?
            taskEntity.Tenant = new Tenant {Id = Tenant.CurrentTenantId};
            taskEntity.CreatorUser = new User { Id = User.CurrentUserId };
            taskEntity.AssignedUser = new User { Id = User.CurrentUserId };

            _taskRepository.Insert(taskEntity);

            return taskEntity.MapTo<TaskDto>();
        }

        public TaskDto UpdateTask(TaskDto task)
        {
            var taskEntity = task.MapTo<Task>();

            //TODO: Automatically set Tenant and Creator User informations!?
            taskEntity.Tenant = new Tenant { Id = Tenant.CurrentTenantId };
            taskEntity.CreatorUser = new User { Id = User.CurrentUserId };

            _taskRepository.Update(taskEntity);
            return taskEntity.MapTo<TaskDto>();
        }

        public virtual void DeleteTask(int taskId)
        {
            _taskRepository.Delete(taskId);
        }
    }
}