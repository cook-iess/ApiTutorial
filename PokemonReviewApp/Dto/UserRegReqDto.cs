using System.ComponentModel.DataAnnotations;

namespace PokemonReviewApp.Dto
{
    public class UserRegReqDto
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
