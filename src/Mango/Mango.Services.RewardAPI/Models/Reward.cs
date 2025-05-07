namespace Mango.Services.RewardAPI.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int OrderId { get; set; }
        public DateTime RewardDate { get; set; }
        public int RewardsActivity {get; set; }  // how many points did they receive, which will be basically be the order total

    }
}
