﻿using JwctAuthUndAuthorMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwctAuthUndAuthorMVC.Controllers
{
    public class jQueryApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public string Index(string key, string person)
        {
            string tokenString = GenerateJSONWebToken(key, person);
            return tokenString;
        }

        private string GenerateJSONWebToken(string key, string person)
        {
            var claims = new[] {
                new Claim("Name", person)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
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

        [HttpPost]
        public IActionResult Reservation([FromBody] List<Reservation> rList)
        {
            return PartialView("Reservation", rList);
        }
    }
}
