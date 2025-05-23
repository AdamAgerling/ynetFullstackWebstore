﻿using AutoMapper;
using Labb2Webb.Models;
using Labb2Webb.Repositories;
using Labb2Webb.Services;
using Labb2Webb.Shared.DTOs;
using Labb2Webb.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Labb2Webb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private JwtTokenGenerator _tokenGenerator { get; set; }
        public AuthController(IConfiguration configuration, ICustomerRepository customerRepository, IMapper mapper, JwtTokenGenerator tokenGenerator)
        {
            _configuration = configuration;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _tokenGenerator = tokenGenerator;
        }

        [HttpGet("email/{email}", Name = "GetCustomerByEmail")]
        public async Task<ActionResult<Customer>> GetCustomerByEmail(string email)
        {
            var customer = await _customerRepository.GetByEmailAsync(email);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateCustomerDto createCustomerDto)
        {
            if (createCustomerDto == null)
            {
                return BadRequest("Invalid registration data");
            }

            var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            createCustomerDto.FirstName = textInfo.ToTitleCase(createCustomerDto.FirstName.ToLower());
            createCustomerDto.LastName = textInfo.ToTitleCase(createCustomerDto.LastName.ToLower());


            var customer = _mapper.Map<Customer>(createCustomerDto);
            customer.Email = createCustomerDto.Email.ToLower();

            customer.Role = Role.User;

            var passwordHasher = new PasswordHasher<Customer>();
            customer.PasswordHash = passwordHasher.HashPassword(customer, createCustomerDto.Password);

            await _customerRepository.AddCustomerAsync(customer);

            var cusomerDto = _mapper.Map<CustomerDto>(customer);

            return CreatedAtAction("GetCustomerByEmail", new { email = customer.Email }, cusomerDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var customer = await _customerRepository.GetByEmailAsync(login.Email);
            if (customer == null)
            {
                return Unauthorized("This user does not exist.");
            }

            var passwordHasher = new PasswordHasher<Customer>();
            var result = passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, login.Password);


            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid Password.");
            }

            var token = _tokenGenerator.GenerateJwtToken(customer);
            var fullName = $"{customer.FirstName} {customer.LastName}";

            var response = new LoginResponseDto
            {
                Token = token,
                FullName = fullName
            };

            return Ok(response);
        }
    }
}