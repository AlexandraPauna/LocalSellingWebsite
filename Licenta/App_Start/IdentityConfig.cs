using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Licenta.Common.Entities;
using Licenta.DataAccess;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Licenta
{
    public class EmailService : IIdentityMessageService
    {
        private const string EmailHost = "smtp.gmail.com";
        private const int EmailPort = 587;
        private const string EmailUsername = "site.anunturi.licenta@gmail.com"; // se va inlocui cu numele contului de gmail
        private const string EmailPassword = "Parola@1234"; // se va inlocui cu parola contului de gmail

        /// <summary>
        /// Verifica daca un email este sau nu valid.
        /// </summary>
        /// <param name="emailAddress">Adresa de email de verificat</param>
        /// <returns>True daca este valida, false daca nu.</returns>
        private bool IsValidEmailAddress(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return false;
            }

            try
            {
                emailAddress = Regex.Replace(emailAddress, @"(@)(.+)$", DomainMapper);
            }
            catch
            {
                return false;
            }

            return Regex.IsMatch(
                emailAddress,
                @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Trimite un email unui singur destinatar.
        /// </summary>
        /// <param name="toAddress">Adresa destinatarului</param>
        /// <param name="fromAddress">Adresa expeditorului</param>
        /// <param name="fromDisplayName">Titlul expeditorului</param>
        /// <param name="subject">Subiectul emailului</param>
        /// <param name="content">Continutul emailului in format html</param>
        /// <returns>Task</returns>
        public async Task SendEmailAsync(string toAddress, string fromAddress, string fromDisplayName, string subject, string content)
        {
            await SendEmailAsync(new[] { toAddress }, fromAddress, fromDisplayName, subject, content);
        }

        /// <summary>
        /// Trimite un email mai multor destinatari.
        /// </summary>
        /// <param name="toAddresses">Adresesle destinatarilor</param>
        /// <param name="fromAddress">Adresa expeditorului</param>
        /// <param name="fromDisplayName">Numele expeditorului</param>
        /// <param name="subject">Subiectul emailului</param>
        /// <param name="content">Continutul emailului in format html</param>
        /// <returns>Task</returns>
        public async Task SendEmailAsync(string[] toAddresses, string fromAddress, string fromDisplayName, string subject, string content)
        {
            if (toAddresses == null || toAddresses.Length == 0)
            {
                throw new Exception("EmailService: toAddresses is null or empty.");
            }

            foreach (var toAddress in toAddresses)
            {
                if (!IsValidEmailAddress(toAddress))
                {
                    throw new Exception($"EmailService: toAddress is not a valid email address: '{toAddress}'.");
                }
            }

            if (!IsValidEmailAddress(fromAddress))
            {
                throw new Exception($"EmailService: fromAddress is not a valid email address: '{fromAddress}'.");
            }

            var mail = new MailMessage
            {
                IsBodyHtml = true,
                Subject = subject,
                From = new MailAddress(fromAddress, fromDisplayName),
                Body = content,
            };

            foreach (var toAddress in toAddresses)
            {
                mail.To.Add(new MailAddress(toAddress));
            }

            // Se creeaza clientul SMTP
            var smtpClient = new SmtpClient
            {
                Host = EmailHost,
                Port = EmailPort,
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(EmailUsername, EmailPassword)
            };

            // Trimiterea de mail
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await smtpClient.SendMailAsync(mail);
                scope.Complete();
            }
        }

        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }

        private static string DomainMapper(Match match)
        {
            var idn = new IdnMapping();
            var domainName = match.Groups[2].Value;

            domainName = idn.GetAscii(domainName);

            return match.Groups[1].Value + domainName;
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> store) : base(store)
        {
        }
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var roleStore = new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>());
            return new ApplicationRoleManager(roleStore);
        }
    }
}
