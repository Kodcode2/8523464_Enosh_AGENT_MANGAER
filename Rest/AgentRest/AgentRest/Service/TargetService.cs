using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using AgentRest.Service;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class TargetService(IServiceProvider serviceProvider) : ITargetService
    {
        private IAgentService agentService => serviceProvider.GetRequiredService<IAgentService>();
        private IMissionService missionService => serviceProvider.GetRequiredService<IMissionService>();

        private ApplicationDbContext context = DbContextFactory.CreateDbContext(serviceProvider);

        // Direction mapping for moving targets
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

        // Create a new target asynchronously
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

        // Get all targets asynchronously
        public async Task<List<TargetModel>> GetAllTargetsAsync() =>
            await context.Targets.AnyAsync()
            ? await context.Targets.ToListAsync()
            : [];

        // Get a target by ID asynchronously
        public async Task<TargetModel?> GetTargetByIdAsync(long id) =>
            await context.Targets.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new Exception("Could not found the target by the given id");

        // Check if the position is outside valid range
        private bool IsInvalidPosition(int x, int y) => (y > 1000 || x > 1000 || y < 0 || x < 0);

        // Move target based on direction asynchronously
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

            // Create new missions for closest agents
            List<AgentModel> closestAgents = await agentService.GetAvailableAgentsAsync(target);
            if (closestAgents.Count > 0)
            {
                var newMissions = closestAgents.Select(agent => missionService.CreateMissionModel(target, agent)).ToList();
                await context.Missions.AddRangeAsync(newMissions);
                await context.SaveChangesAsync();
            }
            return target;
        }

        // Pin target to a specific location asynchronously
        public async Task<TargetModel> PinTargetAsync(long targetId, LocationDto locationDto)
        {
            TargetModel? target = await GetTargetByIdAsync(targetId);
            target!.XPosition = locationDto.x;
            target.YPosition = locationDto.y;

            // Create missions for closest agents
            var closestAgents = await agentService.GetAvailableAgentsAsync(target) ?? [];
            if (closestAgents.Count != 0)
            {
                var missions = closestAgents.Select((a) => new MissionModel() {  AgentId = a.Id, TargetId = targetId, RemainingTime = EvaluateRemainingTime(target, a)});
                await context.Missions.AddRangeAsync(missions);
                
            }
            await context.SaveChangesAsync();
            return target;
        }

        // Evaluate remaining time for agent to reach target
        private double EvaluateRemainingTime(TargetModel target, AgentModel a) =>
            missionService.MeasureDistance(target, a) / 5;
    }
}
