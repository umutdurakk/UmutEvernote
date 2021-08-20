using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyEvernote.Entities.ValueObjects
{
    public class LoginViewModel
    {
            [DisplayName("Kullanıcı adı"),
            Required(ErrorMessage = "{0} alanı boş Geçilmez"),
            StringLength(25,ErrorMessage = "{0}  max 25 karakter içermeli")]
        public string Username { get; set; }

            [DisplayName("Şifre"),
            Required(ErrorMessage = "{0} alanı boş Geçilmez"),
            DataType(DataType.Password),
            StringLength(25, ErrorMessage = "{0}  max 25 karakter içermeli")]
        public string Password { get; set; }
    }
}