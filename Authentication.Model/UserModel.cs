using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Model
{
    public class UserModel
    {
        public int PortalId { get; set; }
        public string PortalName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string MobileNumber { get; set; }
        public List<RoleModel> Roles { get; set; }
    }
}
