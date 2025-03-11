using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserLogin.DTO;
using UserLogin.Models;

namespace UserLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;

        //Inject UserManager
        public AccountController(UserManager<ApplicationUser> UserManager, IConfiguration config)
        {
            userManager = UserManager;
            this.config = config;
        }
        [HttpPost("Register")]//Post api/Account/Register  -> literal 
        public async Task<IActionResult> Register(RegisterDto UserFromRequest)
        {
            //Validation
            if (ModelState.IsValid)
            {
                //Save DB
                ApplicationUser user = new ApplicationUser();
                //Mapping
                user.UserName = UserFromRequest.UserName;
                user.Email = UserFromRequest.Email;
                IdentityResult result =
                await userManager.CreateAsync(user, UserFromRequest.Password);
                if (result.Succeeded)
                {
                    return Ok("Create");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("Password", error.Description);
                    }
                    return BadRequest(ModelState);
                }

            }
            return BadRequest(ModelState);
        }
        [HttpPost("Login")]//Post api/Account/Login
        public async Task<IActionResult> Login(LoginDto userFromRequest)
        {
            if (ModelState.IsValid)
            {
                //check
                ApplicationUser userFromDb = await userManager.FindByNameAsync(userFromRequest.UserName);
                if (userFromDb != null)
                {
                    bool found = await userManager.CheckPasswordAsync(userFromDb, userFromRequest.Password);
                    if (found == true)
                    {

                        //generate token <==
                        List<Claim> UserClaims = new List<Claim>();

                        //Token Generated id change (JWT Predefiened Claims)
                        UserClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        UserClaims.Add(new Claim(ClaimTypes.NameIdentifier, userFromDb.Id));
                        UserClaims.Add(new Claim(ClaimTypes.Name, userFromDb.UserName));
                        var UserRoles = await userManager.GetRolesAsync(userFromDb);
                        foreach (var roleName in UserRoles)
                        {
                            UserClaims.Add(new Claim(ClaimTypes.Role, roleName));
                        }
                        SymmetricSecurityKey SignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecritKey"]));
                        SigningCredentials signingCred =
                            new SigningCredentials(SignInKey, SecurityAlgorithms.HmacSha256);

                        //Design Token
                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            issuer: config["JWT:IssuerIP"],
                            audience: config["JWT:AudienceIP"], //consumer -> angular 
                            expires: DateTime.Now.AddHours(2),
                            claims: UserClaims,
                            signingCredentials: signingCred
                            );
                        //generate token response 
                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                            expiration = DateTime.Now.AddHours(2)
                            //mytoken.ValidTo
                        });
                    }

                }
                ModelState.AddModelError("Username", "Username OR Password Invalid");

            }
            return BadRequest(ModelState);
        }
    
    }
}
