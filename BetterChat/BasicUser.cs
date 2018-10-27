using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
