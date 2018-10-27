namespace BetterChat
{
    public class BasicUser
    {
        public string Figure { get; set; }
        public string Username { get; set; }
        public int Id { get; set; }

        public BasicUser(int id, string username, string figure)
        {
            Figure = figure;
            Username = username;
            Id = id;
        }
    }
}
