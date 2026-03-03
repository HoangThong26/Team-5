using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiryDate { get; set; }
<<<<<<< HEAD
=======
        public UserViewDTO User { get; set; }
>>>>>>> c461d4c1f68f0a909524422d02ea522b4ad20704
    }
}
