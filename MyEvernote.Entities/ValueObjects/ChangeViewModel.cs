using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEvernote.Entities.ValueObjects
{
    public class ChangeViewModel
    {
        [DisplayName("Kullanıcı adı"),
          Required(ErrorMessage = "{0} alanı boş geçilemez"),
          StringLength(25, ErrorMessage = "{0}  max 25 karakter içermeli")]
        public string Username { get; set; }

        [DisplayName("E-posta"),
        Required(ErrorMessage = "{0} alanı boş Geçilmez"),
        StringLength(70, ErrorMessage =
       "{0}  max 70 karakter içermeli"),
        EmailAddress(ErrorMessage = "{0} Alanı için geçerli bir e-posta girin")]
        public string Email { get; set; }

        [DisplayName("Şifre"),
        Required(ErrorMessage = "{0} alanı boş Geçilmez"),
        StringLength(25, ErrorMessage = "{0}  max 25 karakter içermeli")]
        public string Password { get; set; }

        [DisplayName("Şifre Tekrar"),
        Required(ErrorMessage = "{0} alanı boş Geçilmez"),
        StringLength(25, ErrorMessage = "{0}  max 25 karakter içermeli"),
        Compare("Password", ErrorMessage = "Şifreler uyuşmuyor")]
        public string RePassword { get; set; }

        public Guid getid { get; set; }
    }
}
