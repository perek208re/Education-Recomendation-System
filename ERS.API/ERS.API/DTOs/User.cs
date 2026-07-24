namespace ERS.API.DTOs
{
    public class User
    {
        public string UserId { get; set; }
        public List<string> SkillsToLearn { get; set; }
        public List<string> LearnedSkills { get; set; }
    }
}
