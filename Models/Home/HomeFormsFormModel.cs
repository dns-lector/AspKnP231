using Microsoft.AspNetCore.Mvc;

namespace AspKnP231.Models.Home
{
    public class HomeFormsFormModel
    {
        [FromForm(Name = "user-name")]
        public String UserName { get; set; } = null!;
        
        [FromForm(Name = "user-email")]
        public String UserEmail { get; set; } = null!;        
        
        [FromForm(Name = "user-birthdate")]
        public DateTime UserBirthdate { get; set; }
        
        [FromForm(Name = "user-login")]
        public String UserLogin { get; set; } = null!;

        [FromForm(Name = "user-password")]
        public String UserPassword { get; set; } = null!;        

        [FromForm(Name = "user-repeat")]
        public String UserRepeat { get; set; } = null!;

        [FromForm(Name = "user-button")]
        public String UserButton { get; set; } = null!;
    }
}
