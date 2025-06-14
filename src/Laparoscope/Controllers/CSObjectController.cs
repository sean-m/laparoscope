﻿using Laparoscope.Models;
using Laparoscope.Utils.Authorization;
using McAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using StreamJsonRpc;
using System.IO.Pipes;
using System.Net;

using static Laparoscope.Utils.Authorization.ControllerAuthorizationExtensions;

namespace Laparoscope.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = Global.AuthSchemes, Policy ="HasRoles")]
    public class CSObjectController : Controller
    {
        /// <summary>
        /// Controller for the Get-ADSyncCSObject command. This is used to retreive a single record
        /// from a named connector space. Records are queried by DN only and must specify a valid connector
        /// name. The returned record contains a ConnectedMVObjectId property that can be used to inspect
        /// the corresponding metaverse record.
        ///
        /// Note: records filtered on import may not resolve or may only exist in the connector space and
        /// will not have a ConnectedMVObjectId property.
        /// </summary>
        /// <param name="ConnectorName">
        /// Name of the connector space to query. Can be retreived from /api/Connector.
        /// </param>
        /// <param name="DistinguishedName">
        /// The full distinguished name for the desired connector space record. If querying an
        /// Active Directory connector it may look like this: CN=Super Great Person,CN=Cool Stuff,DC=my,DC=garage.
        /// </param>
        /// <returns>
        /// https://docs.microsoft.com/en-us/azure/active-directory/hybrid/reference-connect-adsync#get-adsynccsobject
        /// </returns>
        [HttpGet]
        public async Task<AadcCSObject?> GetAsync(string ConnectorName, string DistinguishedName)
        {
            // Construct an anonymous object as the Model for IsAuthorized so we can
            // pass in Connector as the context. This will allow the authorization engine
            // to re-use the rules for /api/Connector. If you have rights to view a given
            // connector, there is no reason you shouldn't see it's statistics.
            var roles = HttpContext.User?.RoleClaims();
            if (!Filter.IsAuthorized<dynamic>(new { Name = ConnectorName, ConnectorName = ConnectorName, Identifier = ConnectorName }, "Connector", roles))
            {
                throw new HttpRequestException($"Allow policy not found.", null, HttpStatusCode.Unauthorized);
            }

            if (string.IsNullOrEmpty(ConnectorName) || string.IsNullOrEmpty(DistinguishedName))
            {
                throw new BadHttpRequestException("Must specify ConnectorName and DistinguishedName.");
            }

            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncCSObjectStrict";
                    var result = await jsonRpc.InvokeAsync<AadcCSObject>(function, ConnectorName.Trim(), DistinguishedName.Trim());
                    if (this.IsAuthorized<AadcCSObject>(result))
                    {
                        var uac = result?.Attributes?.FirstOrDefault(a => a.Name.Equals("userAccountControl", StringComparison.CurrentCultureIgnoreCase));
                        if (uac != null)
                        {
                            // Parse out bit-flags from the userAccountControl so it's easier to read
                            string uacString = ParseUac(uac.Values?.FirstOrDefault());
                            if (!string.IsNullOrEmpty(uacString))
                            {
                                uac.Values.Add(uacString);
                            }
                        }
                        return result;
                    }
                }
            }

            return null;
        }

        private string? ParseUac(string? uac) {
            if (string.IsNullOrEmpty(uac)) return null;

            int uacInt = 0;

	        if ((uac?.Length > 2) && (uac?.StartsWith("0x") ?? false)) {
		        uacInt = Convert.ToInt32(uac, 16);
	        }
	        else if ((uac?.Length > 2) && (uac?.StartsWith("0b") ?? false)) {
		        uacInt = Convert.ToInt32(uac, 2);
	        } else {
                int.TryParse(uac, out uacInt);
            }
            return ((UserAccountControl)uacInt).ToString();
        }

        /// <summary>
        /// Flags that control the behavior of the user account.
        /// </summary>
        [Flags()]
        public enum UserAccountControl : int {
            /// <summary>
            /// The logon script is executed.
            ///</summary>
            SCRIPT = 0x00000001,

            /// <summary>
            /// The user account is disabled.
            ///</summary>
            ACCOUNTDISABLE = 0x00000002,

            /// <summary>
            /// The home directory is required.
            ///</summary>
            HOMEDIR_REQUIRED = 0x00000008,

            /// <summary>
            /// The account is currently locked out.
            ///</summary>
            LOCKOUT = 0x00000010,

            /// <summary>
            /// No password is required.
            ///</summary>
            PASSWD_NOTREQD = 0x00000020,

            /// <summary>
            /// The user cannot change the password.
            ///</summary>
            /// <remarks>
            /// Note:  You cannot assign the permission settings of PASSWD_CANT_CHANGE by directly modifying the UserAccountControl attribute.
            /// For more information and a code example that shows how to prevent a user from changing the password, see User Cannot Change Password.
            // </remarks>
            PASSWD_CANT_CHANGE = 0x00000040,

            /// <summary>
            /// The user can send an encrypted password.
            ///</summary>
            ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x00000080,

            /// <summary>
            /// This is an account for users whose primary account is in another domain. This account provides user access to this domain, but not
            /// to any domain that trusts this domain. Also known as a local user account.
            ///</summary>
            TEMP_DUPLICATE_ACCOUNT = 0x00000100,

            /// <summary>
            /// This is a default account type that represents a typical user.
            ///</summary>
            NORMAL_ACCOUNT = 0x00000200,

            /// <summary>
            /// This is a permit to trust account for a system domain that trusts other domains.
            ///</summary>
            INTERDOMAIN_TRUST_ACCOUNT = 0x00000800,

            /// <summary>
            /// This is a computer account for a computer that is a member of this domain.
            ///</summary>
            WORKSTATION_TRUST_ACCOUNT = 0x00001000,

            /// <summary>
            /// This is a computer account for a system backup domain controller that is a member of this domain.
            ///</summary>
            SERVER_TRUST_ACCOUNT = 0x00002000,

            /// <summary>
            /// Not used.
            ///</summary>
            Unused1 = 0x00004000,

            /// <summary>
            /// Not used.
            ///</summary>
            Unused2 = 0x00008000,

            /// <summary>
            /// The password for this account will never expire.
            ///</summary>
            DONT_EXPIRE_PASSWD = 0x00010000,

            /// <summary>
            /// This is an MNS logon account.
            ///</summary>
            MNS_LOGON_ACCOUNT = 0x00020000,

            /// <summary>
            /// The user must log on using a smart card.
            ///</summary>
            SMARTCARD_REQUIRED = 0x00040000,

            /// <summary>
            /// The service account (user or computer account), under which a service runs, is trusted for Kerberos delegation. Any such service
            /// can impersonate a client requesting the service.
            ///</summary>
            TRUSTED_FOR_DELEGATION = 0x00080000,

            /// <summary>
            /// The security context of the user will not be delegated to a service even if the service account is set as trusted for Kerberos delegation.
            ///</summary>
            NOT_DELEGATED = 0x00100000,

            /// <summary>
            /// Restrict this principal to use only Data Encryption Standard (DES) encryption types for keys.
            ///</summary>
            USE_DES_KEY_ONLY = 0x00200000,

            /// <summary>
            /// This account does not require Kerberos pre-authentication for logon.
            ///</summary>
            DONT_REQUIRE_PREAUTH = 0x00400000,

            /// <summary>
            /// The user password has expired. This flag is created by the system using data from the Pwd-Last-Set attribute and the domain policy.
            ///</summary>
            PASSWORD_EXPIRED = 0x00800000,

            /// <summary>
            /// The account is enabled for delegation. This is a security-sensitive setting; accounts with this option enabled should be strictly
            /// controlled. This setting enables a service running under the account to assume a client identity and authenticate as that user to
            /// other remote servers on the network.
            ///</summary>
            TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x01000000,

            /// <summary>
            ///
            /// </summary>
            PARTIAL_SECRETS_ACCOUNT = 0x04000000,

            /// <summary>
            ///
            /// </summary>
            USE_AES_KEYS = 0x08000000
        }
    }
}