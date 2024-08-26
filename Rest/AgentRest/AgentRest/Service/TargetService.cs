using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using AgentRest.Service;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class TargetService(ApplicationDbContext context, IServiceProvider serviceProvider) : ITargetService
    {
        private IAgentService agentService => serviceProvider.GetRequiredService<IAgentService>();
        private IMissionService missionService => serviceProvider.GetRequiredService<IMissionService>();

        private readonly Dictionary<string, (int, int)> Direction = new()
        {
            {"n", (0, 1)},
             {"s", (0, -1)},
             {"e", (-1, 0)},
             {"w", (1, 0)},
             {"ne", (-1, 1)},
             {"nw", (1, 1)},
             {"se", (-1, -1)},
             {"sw", (1, -1)}
        };

        public async Task<IdDto> CreateTargetAsync(TargetDto targetDto)
        {
            TargetModel? targetModel = new()
            {
                Name = targetDto.Name,
                Image = targetDto.PhotoUrl,
                Role = targetDto.Position
            };
            await context.Targets.AddAsync(targetModel);
            await context.SaveChangesAsync();
            var target = await context.Targets
                .Where(t => t.Name == targetModel.Name)
                .Where(t => t.Image == targetModel.Image)
                .FirstOrDefaultAsync();
            return new() { Id = target!.Id };
        }

        public async Task DeleteTargetAsync(long targetId)
        {
            TargetModel? target = await GetTargetByIdAsync(targetId);
            context.Targets.Remove(target!);
            await context.SaveChangesAsync();
        }

        public async Task<List<TargetModel>> GetAllTargetsAsync() =>
            await context.Targets.AnyAsync()
            ? await context.Targets.ToListAsync()
            : [];

        public async Task<TargetModel?> GetTargetByIdAsync(long id) =>
            await context.Targets.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new Exception("Could not found the target by the given id");

        public async Task<TargetModel> UpdateTargetAsync(long targetId, TargetModel targetModel)
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

        private bool IsInvalidPosition(int x, int y) => (y > 1000 || x > 1000 || y < 0 || x < 0);

        public async Task<TargetModel> MoveTargetAsync(long targetId, DirectionDto directionDto)
        {
            TargetModel? target = await GetTargetByIdAsync(targetId);
            var (x, y) = Direction[directionDto.Direction];
            target!.XPosition += x;
            target!.YPosition += y;
            if (IsInvalidPosition(target.XPosition, target.YPosition))
            {
                throw new Exception($"The corresponds coordinats: x:{target.XPosition}, y:{target.YPosition} are off the map borders");
            }
            await context.SaveChangesAsync();

            List<AgentModel> closestAgents = await agentService.GetAvailableAgentsAsync(target);
            if (closestAgents.Count > 0)
            {
                var createMissionsTasks = closestAgents.Select(agent => missionService.CreateMissionAsync(target, agent));
                await Task.WhenAll(createMissionsTasks);
            }
            return target;
        }

        public async Task<TargetModel> PinTargetAsync(long targetId, LocationDto locationDto)
        {
            TargetModel? target = await GetTargetByIdAsync(targetId);
            target!.XPosition = locationDto.x;
            target.YPosition = locationDto.y;
            await context.SaveChangesAsync();

            var closestAgents = await agentService.GetAvailableAgentsAsync(target) ?? [];
            if (closestAgents.Count != 0)
            {
                var missions = closestAgents.Select((a) => new MissionModel() {  AgentId = a.Id, TargetId = targetId});
                await context.Missions.AddRangeAsync(missions);
                await context.SaveChangesAsync();
                
            }
            return target;
        }

        public async Task<List<TargetModel>> GetAvailableTargestAsync(AgentModel agent) => 
            await context.Targets.AnyAsync()
            ? await context.Targets
                   .Where(t => t.TargetStatus == TargetStatus.Alive)
                   .Where(t => Math.Sqrt(Math.Pow(agent.XPosition - t.XPosition, 2)
                    + Math.Pow(agent.YPosition - t.YPosition, 2)) < 200)
                   .ToListAsync()
            : [];
    }
}
