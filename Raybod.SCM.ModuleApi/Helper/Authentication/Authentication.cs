using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.ModuleApi.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;

namespace Raybod.SCM.ModuleApi.Helper.Authentication
{
    public class Authentication : IAuthenticate
    {
        private readonly IUserService _userService;
        private readonly TokenManagement _tokenManagement;

        //IHostingEnvironment hostingEnvironmentRoot
        public Authentication(IUserService userService, IOptions<TokenManagement> tokenManagement)
        {
            _userService = userService;
            _tokenManagement = tokenManagement.Value;
        }

        public async Task<object> IsAuthenticatedAsync(SigningApiDto request,string language)
        {
           
            var user = await _userService.SigninWithApiAsync(request);
            if (!user.Succeeded) return user.ToWebApiResultVCore(language);
            var userModel = user.Result;

            var refreshTokenServiceResult = GenerateTokenWithRefreshToken(userModel, request.IsRememberMe);
            return !refreshTokenServiceResult.Succeeded
                ? refreshTokenServiceResult
                : user.ToWebApiResultV3Core(language,token: refreshTokenServiceResult.Result.Token,
                    refreshToken: refreshTokenServiceResult.Result.RefreshToken);
        }

        private ServiceResult<RefreshTokenDto> GenerateTokenWithRefreshToken(UserInfoApiDto userInfo, bool isRememberMe)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                    new Claim(ClaimTypes.Name, userInfo.Id.ToString()),
                    new Claim(ClaimTypes.Surname, userInfo.UserName),
                    new Claim(ClaimTypes.GivenName, userInfo.FirstName + " " + userInfo.LastName),
                    new Claim(ClaimTypes.UserData, userInfo.Image == null ? "" : userInfo.Image),
                    new Claim("UserType", userInfo.UserType.ToString()),
                };
                string token = GenerateToken(claims);
                string refreshToken = GenerateRefreshToken();
                var saveRefreshTokenResult = _userService.SetRefreshToken(userInfo.UserName, refreshToken, isRememberMe);
                if (!saveRefreshTokenResult.Succeeded)
                    return ServiceResultFactory.CreateError(new RefreshTokenDto(), MessageId.InternalError);
                var resDto = new RefreshTokenDto { Token = token, RefreshToken = refreshToken };

                return ServiceResultFactory.CreateSuccess(resDto);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new RefreshTokenDto(), exception);
            }
        }

        public async Task<object> RefreshTokenAsync(string token, string refreshToken)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                if (principal == null)
                    return ServiceResultFactory.CreateError(false, MessageId.TokenNotValid);
                string username = principal.FindFirst(ClaimTypes.Surname).Value;
                var newRefreshToken = GenerateRefreshToken();
                var serviceResult =
                    await _userService.CheckAndSetRefreshTokenAsync(username, refreshToken, newRefreshToken); //retrieve the refresh token from a data store
                if (!serviceResult.Succeeded)
                    return serviceResult;

                var newJwtToken = GenerateToken(principal.Claims);

                return ServiceResultFactory.CreateSuccess(new RefreshTokenDto
                { Token = newJwtToken, RefreshToken = newRefreshToken });
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new RefreshTokenDto(), exception);
            }
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            //LogTxt.log(_hostingEnvironmentRoot.ContentRootPath, "_tokenManagement", JsonConvert.SerializeObject(_tokenManagement));
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                _tokenManagement.Issuer,
                _tokenManagement.Audience,
                claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_tokenManagement.AccessExpiration),
                signingCredentials: credentials
            );
            string token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                string base64string = Convert.ToBase64String(randomNumber);
                return base64string.Replace("+", "");
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience =
                    false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenManagement.Secret)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }

    }
}