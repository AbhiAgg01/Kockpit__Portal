using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Newtonsoft.Json;

namespace KockpitAuthenticator
{
    public static class JwtHandler
    {
        const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
        public static string GenerateToken(string userId, string emailid, string role, string company,
            string schema)
        {
            var payload = new Dictionary<string, object>
            {
                { "UserId", userId },
                { "Email", emailid },
                { "Role", role },
                { "Company", company },
                { "Domain", schema },
            };

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            var token = encoder.Encode(payload, secret);
            return token;
        }

        public static bool ValidateToken(string token, out string msg)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                var provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
                var json = decoder.Decode(token, secret, verify: true);
                msg = string.Empty;
                return true;
            }
            catch (TokenExpiredException)
            {
                msg = "Token has expired";
            }
            catch (SignatureVerificationException)
            {
                msg = "Token has invalid signature";
            }
            return false;
        }

        public static SessionData GetPrincipal(string token)
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            var json = decoder.Decode(token, secret, verify: true);
            return JsonConvert.DeserializeObject<SessionData>(json);
        }
    }
}
