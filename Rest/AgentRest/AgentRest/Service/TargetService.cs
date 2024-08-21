using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class TargetService(ApplicationDbContext context) : ITargetService
    {
        public async Task<TargetModel> CreateTargetAsync(TargetDto targetDto)
        {
            TargetModel? targetModel = new()
            {
                Name = targetDto.Name,
                Image = targetDto.Photo_url,
                Role = targetDto.Position
            };
            await context.Targets.AddAsync(targetModel);
            await context.SaveChangesAsync();
            return targetModel;
        }

        public async Task<List<TargetModel>> GetAllTargetsAsync() =>
            await context.Targets.ToListAsync() ?? [];

        public async Task<TargetModel?> GetTargetByIdAsync(long id) =>
            await context.Targets.Where(t => t.Id == id).FirstOrDefaultAsync() 
            ?? throw new Exception("Could not found the the target by the given id");

        public async Task<TargetModel> UpdateTargetAsync(int targetId, TargetModel targetModel)
        {
            TargetModel? target = await GetTargetByIdAsync(targetId);
            target!.Name = targetModel.Name;
            target!.Image = targetModel.Image;
            target.Role = targetModel.Role;
            target.XPosition = targetModel.XPosition;
            target.YPosition = targetModel.YPosition;
            target.TargetStatus = targetModel.TargetStatus;
            await context.SaveChangesAsync();
            return target;
        }
    }
}
