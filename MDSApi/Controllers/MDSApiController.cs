using MDSApi.Authentication;
using MDSApi.Models;
using MDSWcf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MDSApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MDSApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        private readonly Services.IMDSServices _services;
        private readonly IConfiguration _configuration;
        private readonly ServiceClient _clientProxy;

        public MDSApiController(UserManager<ApplicationUser> userManager,
                              Services.IMDSServices services,
                              IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            _services = services;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = _userManager.Users.FirstOrDefault(s => s.UserName == model.Username.ToUpper() && s.Domain == model.Domain.ToUpper());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Bad Useraname or Password");
            }

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }
        
        [HttpPost]
        [Route("GetAllEntities")]
        public List<string> GetAllEntities(RequestModel req)
        {
            string _targetURL = _configuration["Mds:ServiceUrl"];
            string _defaultPassword = _configuration["Mds:Credentials:Password"];
            //string _domain = _services.GetUserDomain(req.domain);
            string _domain = req.domain;
            string _username = _services.GetUsername(req.userName);
            ServiceClient _clientProxy = GetClientProxy(_domain, _username, _defaultPassword, _targetURL);
            return _services.GetAllEntities(_clientProxy, req.modelName, req.versionName);
        }

        [HttpPost]
        [Route("GetModel")]
        public Identifier GetModel(RequestModel req)
        {
            string _targetURL = _configuration["Mds:ServiceUrl"];
            string _defaultPassword = _configuration["Mds:Credentials:Password"];
            string _domain = _services.GetUserDomain(req.domain);
            string _username = _services.GetUsername(req.userName);
            ServiceClient _clientProxy = GetClientProxy(_domain, _username, _defaultPassword, _targetURL);
            return _services.GetModel(_clientProxy, req.modelName, req.versionName);
        }


        [HttpPost]
        [Route("GetEntityData")]
        public ActionResult<object> GetEntityData(RequestModel req)
        {
            string _targetURL = _configuration["Mds:ServiceUrl"];
            string _defaultPassword = _configuration["Mds:Credentials:Password"];
            string _domain = _services.GetUserDomain(req.domain);
            string _username = _services.GetUsername(req.userName);
            ServiceClient _clientProxy = GetClientProxy(_domain, _username, _defaultPassword, _targetURL);
            return _services.GetEntityData(_clientProxy, req.modelName, req.versionName, req.entity);
        }

        private ServiceClient GetClientProxy(string targetURL)
        {
            // Create an endpoint address using the URL.
            EndpointAddress endptAddress = new EndpointAddress(targetURL);
            var wsBinding = new WSHttpBinding();
            wsBinding.Security.Mode = SecurityMode.Transport;
            wsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            wsBinding.MaxReceivedMessageSize = 2147483647;
            return new ServiceClient(wsBinding, endptAddress);
        }

        private ServiceClient GetClientProxy(string domain, string username, string password, string targetURL)
        {
            ServiceClient client = GetClientProxy(targetURL);
            client.ClientCredentials.Windows.ClientCredential.Domain = domain;
            client.ClientCredentials.Windows.ClientCredential.UserName = username;
            client.ClientCredentials.Windows.ClientCredential.Password = password;
            return client;
        }
    }
}
