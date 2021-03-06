﻿using System;
using System.IO;
using System.Text;
using Anderson.PackageAudit.Infrastructure;
using Anderson.PackageAudit.SharedPipes.Authorization.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Anderson.PackageAudit.Tests.Handlers
{
    public class AuthorizationTests
    {
        private string _token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ik5qazVPVFkzUWpFelFrSTBSRVpDTVRWQlJUVTRPVVpDUWpGR1FUaEROVEl5UXpRek9EbENSZyJ9.eyJpc3MiOiJodHRwczovL2QtbWFnbm94aXVtLmV1LmF1dGgwLmNvbS8iLCJzdWIiOiJhdXRoMHw1YjEzMzA1MjdiOTQ0OTMxZjQwMWE4ODkiLCJhdWQiOlsiaHR0cHM6Ly9kLW1hZ25veGl1bS5jb20vcGFja2FnZXNjYW5uZXIiLCJodHRwczovL2QtbWFnbm94aXVtLmV1LmF1dGgwLmNvbS91c2VyaW5mbyJdLCJpYXQiOjE1Mjg2MDMyNjAsImV4cCI6MTUyODYxMDQ2MCwiYXpwIjoiSnN4b3hET2sydFpicUZZa3V1NzNpd2I2cmxxNVdzNmkiLCJzY29wZSI6Im9wZW5pZCJ9.GHtZVZ3oIKTFsgzjKOR4XBEcRIbM1re-rGyXHaNMWyGgukcDG55LFa_Mhoo-8QteshEIAAFzGOfdDmiZd5TTkpQBsHazqjhZzzXVSo3dOfA2Jx3i2dE42jFMhpAiKYy2sCxCfY7UqdE5y9tW7P-DcXsEFqgt02Y75unsgtKegx50s6jxtGBBvsyyNTPJR_RfXpMDLwPjEVIUmwvh70C6LWH52lYK6NDp06sbWnjhnCfX1u8XS9tr7znbRqb-EItkz64ziZ_pdTKs8RLTLS5NuBd--zPyQv8GO5kEmDH2Ljytcp6jNqidBkxwRRr8GQI5T0KJqZgzJvAjXnKbhV1mEQ";

        private string _unknownAudianceToken = $"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ik5qazVPVFkzUWpFelFrSTBSRVpDTVRWQlJUVTRPVVpDUWpGR1FUaEROVEl5UXpRek9EbENSZyJ9.eyJpc3MiOiJodHRwczovL2QtbWFnbm94aXVtLmV1LmF1dGgwLmNvbS8iLCJzdWIiOiJhdXRoMHw1YjEzMzA1MjdiOTQ0OTMxZjQwMWE4ODkiLCJhdWQiOlsiaHR0cHM6Ly9kLW1hZ25veGl1bS5jb20vcGFja2FnZXNjYW5uZXIiLCJodHRwczovL2QtbWFnbm94aXVtLmV1LmF1dGgwLmNvbS91c2VyaW5mbyJdLCJpYXQiOjE1Mjg2MDMyNjAsImV4cCI6MTUyODYxMDQ2MCwiYXpwIjoiSnN4b3hET2sydFpicUZZa3V1NzNpd2I2cmxxNVdzNmkiLCJzY29wZSI6Im9wZW5pZCJ9.GHtZVZ3oIKTFsgzjKOR4XBEcRIbM1re-rGyXHaNMWyGgukcDG55LFa_Mhoo-8QteshEIAAFzGOfdDmiZd5TTkpQBsHazqjhZzzXVSo3dOfA2Jx3i2dE42jFMhpAiKYy2sCxCfY7UqdE5y9tW7P-DcXsEFqgt02Y75unsgtKegx50s6jxtGBBvsyyNTPJR_RfXpMDLwPjEVIUmwvh70C6LWH52lYK6NDp06sbWnjhnCfX1u8XS9tr7znbRqb-EItkz64ziZ_pdTKs8RLTLS5NuBd--zPyQv8GO5kEmDH2Ljytcp6jNqidBkxwRRr8GQI5T0KJqZgzJvAjXnKbhV1mEQ";

        private string _unknownIssuerToken = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ik5qazVPVFkzUWpFelFrSTBSRVpDTVRWQlJUVTRPVVpDUWpGR1FUaEROVEl5UXpRek9EbENSZyJ9.eyJpc3MiOiJodHRwczovL2QtbWFnbm94aXVtLmV1LmF1dGgwLmNvbS8iLCJzdWIiOiJhdXRoMHw1YjEzMzA1MjdiOTQ0OTMxZjQwMWE4ODkiLCJhdWQiOlsiaHR0cHM6Ly9kLW1hZ25veGl1bS5jb20vcGFja2FnZXNjYW5uZXIiLCJodHRwczovL2QtbWFnbm94aXVtLmV1LmF1dGgwLmNvbS91c2VyaW5mbyJdLCJpYXQiOjE1Mjg2MDMyNjAsImV4cCI6MTUyODYxMDQ2MCwiYXpwIjoiSnN4b3hET2sydFpicUZZa3V1NzNpd2I2cmxxNVdzNmkiLCJzY29wZSI6Im9wZW5pZCJ9.GHtZVZ3oIKTFsgzjKOR4XBEcRIbM1re-rGyXHaNMWyGgukcDG55LFa_Mhoo-8QteshEIAAFzGOfdDmiZd5TTkpQBsHazqjhZzzXVSo3dOfA2Jx3i2dE42jFMhpAiKYy2sCxCfY7UqdE5y9tW7P-DcXsEFqgt02Y75unsgtKegx50s6jxtGBBvsyyNTPJR_RfXpMDLwPjEVIUmwvh70C6LWH52lYK6NDp06sbWnjhnCfX1u8XS9tr7znbRqb-EItkz64ziZ_pdTKs8RLTLS5NuBd--zPyQv8GO5kEmDH2Ljytcp6jNqidBkxwRRr8GQI5T0KJqZgzJvAjXnKbhV1mEQ";

        public void SetEnvironmentVariables(
            string environment = "Test", 
            string issuer = "https://d-magnoxium.eu.auth0.com/", 
            string domain = "d-magnoxium.eu.auth0.com", 
            string audience = "https://d-magnoxium.com/packagescanner",
            string redisConnection = "localhost:32768")
        {
            Environment.SetEnvironmentVariable("FUNCTION_ENVIRONMENT", environment);
            Environment.SetEnvironmentVariable("auth0:issuer", issuer);
            Environment.SetEnvironmentVariable("auth0:domain", domain);
            Environment.SetEnvironmentVariable("auth0:audience", audience);
            Environment.SetEnvironmentVariable("redis:connectionstring", redisConnection);

            StaticClassHelper.Reset(typeof(ConfigurationFactory));
            StaticClassHelper.Reset(typeof(TokenValidationParametersFactory));
        }
          
        [Test]
        public void GivenTokenIsNotPresent_WhenMakingARequestToFunction_ThenFunctionReturnsUnauthorized()
        {
            SetEnvironmentVariables();
            var httpRequest = CreateDefaultHttpRequest();
            IActionResult response = PackageAuditor.Run(httpRequest);

            Assert.That(response, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public void GivenExpiredTokenIsPresent_WhenMakingARequestToFunction_ThenFunctionReturnsUnauthorized()
        {
            SetEnvironmentVariables(environment: "unit test");

            var httpRequest = CreateDefaultHttpRequest();
            httpRequest.Headers.Add("Authorization", _token);
            IActionResult response = PackageAuditor.Run(httpRequest);

            Assert.That(response, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public void GivenUnknownIssuerOnToken_WhenMakingARequestToFunction_ThenFunctionReturnsUnauthorized()
        {
            SetEnvironmentVariables(issuer: "https://example.com/");

            var httpRequest = CreateDefaultHttpRequest();
            httpRequest.Headers.Add("Authorization", _unknownIssuerToken);
            IActionResult response = PackageAuditor.Run(httpRequest);

            Assert.That(response, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public void GivenUnknownAudienceOnToken_WhenMakingARequestToFunction_ThenFunctionReturnsUnauthorized()
        {
            SetEnvironmentVariables(audience: "https://example.com/");

            var httpRequest = CreateDefaultHttpRequest();
            httpRequest.Headers.Add("Authorization", _unknownAudianceToken);
            IActionResult response = PackageAuditor.Run(httpRequest);

            Assert.That(response, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public void GivenValidToken_WhenMakingARequestToFunction_ThenFunctionReturnsUnauthorized()
        {
            SetEnvironmentVariables();

            var httpRequest = CreateDefaultHttpRequest();
            httpRequest.Headers.Add("Authorization", _token);
            IActionResult response = PackageAuditor.Run(httpRequest);

            Assert.That(response, Is.TypeOf<OkObjectResult>());
        }

        private static DefaultHttpRequest CreateDefaultHttpRequest()
        {
            var httpRequest = new DefaultHttpRequest(new DefaultHttpContext());
            var memoryStream = new MemoryStream();
            
            var request = "[{name: \"FluentValidation\", version: \"1.0.1\"}]";
            memoryStream.Write(Encoding.UTF8.GetBytes(request), 0, request.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
            httpRequest.Body = memoryStream;
            return httpRequest;
        }
    }
}
