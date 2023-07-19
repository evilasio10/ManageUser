using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class Usuario
    {
        [Key]
        public int ide_usuario { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }
}
