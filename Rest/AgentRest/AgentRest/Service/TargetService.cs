using AgentRest.Data;
using AgentRest.Dto;
using AgentRest.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentRest.Service
{
    public class TargetService(ApplicationDbContext context) : ITargetService
    {
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
                Image = targetDto.Photo_url,
                Role = targetDto.Position
            };
            await context.Targets.AddAsync(targetModel);
            await context.SaveChangesAsync();
            TargetModel? target = await context.Targets.FindAsync(targetModel)
                ?? throw new Exception("Something went wrong");
            return new() { Id = target.Id};
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

        public bool IsInValidPosition(int x, int y) => (y > 1000 || x > 1000 || y < 0 || x < 0);

        public async Task<TargetModel> UpdateTargetLocationAsync(long targetId, DirectionDto directionDto)
        {
            TargetModel? target = await GetTargetByIdAsync(targetId);
            var (x, y) = Direction[directionDto.Direction];
            target!.XPosition += x;
            target!.YPosition += y;
            if (IsInValidPosition(target.XPosition, target.YPosition)) 
            { 
                throw new Exception($"The corresponds coordinats: x:{target.XPosition}, y:{target.YPosition} are off the map border"); 
            } 
            await context.SaveChangesAsync();
            return target;
        }

        public async Task<TargetModel> PinTargetAsync(long targetId, LocationDto locationDto)
        {
            TargetModel? target = await GetTargetByIdAsync(targetId);
            target!.XPosition = locationDto.XPosition;
            target.YPosition = locationDto.YPosition;
            context.SaveChanges();
            return target;
        }
    }
}
