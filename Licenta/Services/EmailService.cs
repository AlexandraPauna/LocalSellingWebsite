using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using BDPR.Common.Exceptions;
using BDPR.Common.Services;

namespace Licenta.Services
{

        /// <summary>
        /// Implementeaza diferite metode legate de trimiterea de email-uri.
        /// </summary>
        public class EmailService : IEmailService
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
            public bool IsValidEmailAddress(string emailAddress)
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
                    throw new InvalidEmailAddressException("EmailService: toAddresses is null or empty.");
                }

                foreach (var toAddress in toAddresses)
                {
                    if (!IsValidEmailAddress(toAddress))
                    {
                        throw new InvalidEmailAddressException($"EmailService: toAddress is not a valid email address: '{toAddress}'.");
                    }
                }

                if (!IsValidEmailAddress(fromAddress))
                {
                    throw new InvalidEmailAddressException($"EmailService: fromAddress is not a valid email address: '{fromAddress}'.");
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

            private static string DomainMapper(Match match)
            {
                var idn = new IdnMapping();
                var domainName = match.Groups[2].Value;

                domainName = idn.GetAscii(domainName);

                return match.Groups[1].Value + domainName;
            }
        }
    }

