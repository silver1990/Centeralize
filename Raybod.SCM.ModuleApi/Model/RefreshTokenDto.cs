using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.ModuleApi.Model
{
    public class RefreshTokenDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage ="الزامی می باشد")]        
        public string Token { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "الزامی می باشد")]
        public string RefreshToken { get; set; }
    }
}
