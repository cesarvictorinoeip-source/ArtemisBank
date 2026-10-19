namespace ArtemisBank.Core.Application.DTOs
{
    public class SaveUserDto
    {
        public string Id { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string IdCard { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string UserType { get; set; } = "";
        public decimal InitialAmount { get; set; }
        public decimal AdditionalAmount { get; set; }
    }
}