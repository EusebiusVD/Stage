using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Admin { get; set; }

        public User(int id, string username, string password, bool admin)
        {
            Id = id;
            Username = username;
            Password = password;
            Admin = admin;
        }
    }
}