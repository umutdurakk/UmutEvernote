using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEvernote.Entities
{
    [Table("EvernoteUsers")]
    public class EvernoteUser : MyEntityBase
    {
        [DisplayName("İsim"),StringLength(30)]
        public string Name { get; set; }

        [DisplayName("Soyisim"),StringLength(30)]
        public string Surname { get; set; }

        [DisplayName("Kullanıcı Adı"),Required,StringLength(30)]
        public string Username { get; set; }

        [DisplayName("E-posta"),Required, StringLength(70)]
        public string Email { get; set; }

        [DisplayName("Şifre"),Required, StringLength(25)]
        public string Password { get; set; }

        [Required, StringLength(30),ScaffoldColumn(false)]
        public string ProfileImageFilename { get; set; }//images/user

        [DisplayName("Is Active") ]       
        public bool IsActive { get; set; }

        [Required,ScaffoldColumn(false)]
        public Guid ActivateGuid { get; set; }

        [Required,DisplayName("Is Admin")]
        public bool IsAdmin { get; set; }

        public virtual List<Note> Notes { get; set; }
        public virtual List<Comment> Comments { get; set; }
        public virtual List<Liked> Likes { get; set; }
    }
}
