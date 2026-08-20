namespace Galaxy.Lol.Domain.Model.ReadModels
{

    public class RoleDistributionReadModel
    {
        public string Role { get; set; } = string.Empty;
        public int Total { get; set; }
        public double AverageDifficulty { get; set; }
        public int InFreeRotation { get; set; }
    }

    public class MasteryByRoleReadModel
    {
        public string Role { get; set; } = string.Empty;
        public int Champions { get; set; }
        public long TotalPoints { get; set; }
        public int MaxLevel { get; set; }
    }
}
