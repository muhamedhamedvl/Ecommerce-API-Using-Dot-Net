namespace WebApiEcomm.Core.Entites.Dtos
{
    public record UserDto
    {
        public string Email { get; init; }
        public string DisplayName { get; init; }
        public string Token { get; init; }
        public IList<string> Roles { get; init; }
    }
}
