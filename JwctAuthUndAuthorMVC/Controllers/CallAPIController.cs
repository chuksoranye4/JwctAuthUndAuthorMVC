using JwctAuthUndAuthorMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwctAuthUndAuthorMVC.Controllers
{
    public class CallAPIController : Controller
    {
        public IActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            //if ((username != "secret") || (password != "secret"))
            //    return View((object)"Login Failed");

            Users loginUser = CreateDummyUsers().Where(p => p.Username == username && p.Password == password).FirstOrDefault();
            if (loginUser == null)
            {
                return View((object)"login failed");
            }

            var claims = new[]{
                new Claim(ClaimTypes.Role,loginUser.Role)
            };
            var accessToken = GenerateJSONWebToken(claims);
            SetJWTCookie(accessToken);

            return RedirectToAction("FlightReservation");

        }


        private string GenerateJSONWebToken(Claim[] claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MynameisJamesBond007"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    issuer: "http://localhost:45610",
                    audience: "http://localhost:45610",
                    expires: DateTime.Now.AddHours(3),
                    signingCredentials: credentials,
                    claims: claims
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private void SetJWTCookie(string accessToken)
        {
            var cookieOption = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(3)
            };

            Response.Cookies.Append("jwtCookie", accessToken, cookieOption);
        }


        [HttpGet]
        public async Task<IActionResult> FlightReservation()
        {
            var jwt = Request.Cookies["jwtCookie"];
            List<Reservation> reservationList = new List<Reservation>();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                using (var response = await httpClient.GetAsync("http://localhost:45610/Reservation"))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        reservationList = JsonConvert.DeserializeObject<List<Reservation>>(apiResponse);
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Index", new { Message = "Please Login again" });
                    }
                }
            }

            return View(reservationList);
        }


        public List<Users> CreateDummyUsers()
        {
            List<Users> userList = new List<Users>
            {
                new Users { Username = "jack", Password = "jack", Role = "Admin" },
                new Users { Username = "donald", Password = "donald", Role = "Manager" },
                new Users { Username = "ketu", Password = "ketu", Role = "Developer" }
            };
            return userList;
        }

    }
}
