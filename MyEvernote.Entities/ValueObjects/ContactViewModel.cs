using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEvernote.Entities.ValueObjects
{
    public class ContactViewModel
    {
        [DisplayName("E-posta"),
        Required(ErrorMessage = "{0} alanı boş Geçilmez"),
        StringLength(70, ErrorMessage =
       "{0}  max 70 karakter içermeli"),
        EmailAddress(ErrorMessage = "{0} Alanı için geçerli bir e-posta girin")]
        public string Email { get; set; }

        [DisplayName("Kullanıcı adı"),
           Required(ErrorMessage = "{0} alanı boş Geçilmez"),
           StringLength(25, ErrorMessage = "{0}  max 25 karakter içermeli")]
        public string Username { get; set; }

        [DisplayName("Not Metni"), Required, StringLength(2000)]
        public string Text { get; set; }
    }
}
