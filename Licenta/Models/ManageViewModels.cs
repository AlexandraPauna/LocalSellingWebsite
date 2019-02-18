using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Licenta.Models.Data;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Licenta.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Parola curenta")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Parola prea scurta!", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Parola Noua")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirma parola noua")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Parola nu corespunde!")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangeProfileViewModel
    {
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Prenume imposibil!")]
        [DataType(DataType.Text)]
        [Display(Name = "Prenume")]
        public string FirstName { get; set; }

        [StringLength(40, MinimumLength = 2, ErrorMessage = "Nume imposibil!")]
        [DataType(DataType.Text)]
        [Display(Name = "Nume")]
        public string LastName { get; set; }

        [StringLength(30, ErrorMessage = "Nume Utilizator prea lung!")]
        [DataType(DataType.Text)]
        [Display(Name = "Nume Utilizator")]
        [Remote("UserAlreadyExistsAsync", "Manage", ErrorMessage = "Numele de Utilizator exista deja!")]
        public string UserName { get; set; }

        [Display(Name = "Oras")]
        public int CityId { get; set; }
        public virtual City City { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Numar Telefon")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Poza Profil")]
        public byte[] UserPhoto { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}