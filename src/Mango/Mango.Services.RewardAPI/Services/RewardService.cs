using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.RewardAPI.Services
{
    public class RewardService : IRewardService 
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public RewardService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }
        public async Task UpdateRewards(RewardMessage rewardMessage)
        {
            try
            {
                Reward reward = new()
                {
                    UserId = rewardMessage.UserId,
                    OrderId = rewardMessage.OrderId,
                    RewardsActivity = rewardMessage.RewardsActivity,
                    RewardDate = DateTime.Now,
                };

                await using var _db = new AppDbContext(_dbOptions);
                await _db.AddAsync(reward);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
