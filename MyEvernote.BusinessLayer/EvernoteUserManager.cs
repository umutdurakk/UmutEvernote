using MyEvernote.Common.Helpers;
using MyEvernote.DataAccessLayer.EntityFramework;
using MyEvernote.Entities;
using MyEvernote.Entities.Messages;
using MyEvernote.Entities.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyEvernote.BusinessLayer.Results;
using MyEvernote.BusinessLayer.Abstract;

namespace MyEvernote.BusinessLayer
{
    public class EvernoteUserManager:ManagerBase<EvernoteUser>
    {  
        public BusinessLayerResult<EvernoteUser> RegisterUser(RegisterViewModel data)
        {
            //Kullanıcı username kontrolü
            //kullanıcı e-posta kontrolü
            //kayıt işlemi
            //Aktivasyon e-postası gönderimi
            EvernoteUser user= Find(x => x.Username == data.Username || x.Email == data.Email);
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            if (user !=null)
            {
                if (user.Username==data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAlreadyExists,
                        "Kullanıcı adı kayıtlı");
                }
                if (user.Username == data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAlreadyExists,
                        "E-posta kayıtlı");
                }
            }
            else
            {
                int dbResult = base.Insert(new EvernoteUser()
                {
                    Name=data.Name,
                    Surname=data.Surname,
                    Username = data.Username,
                    Email = data.Email,
                    ProfileImageFilename = "user_boy.png",
                    Password = data.Password,
                    ActivateGuid = Guid.NewGuid(),                  
                    IsActive=false,
                    IsAdmin=false,
                    
                }) ;
                if (dbResult >0)
                {
                   res.Result= Find(x => x.Email == data.Email && x.Username == data.Username);

                    string siteUri = ConfigHelper.Get <string>("SiteRootUri");
                    string activateUri = $"{siteUri}/Home/UserActivate/{res.Result.ActivateGuid}";
                    string body = $"Merhaba {res.Result.Username};<br><br> Hesabınızı aktifleştirmek için <a href ='{activateUri}'" + "target='_blank'>tıklayınız</a>.";

                    MailHelper.SendMail(body,res.Result.Email,"MyEvernote Hesap Aktifleştirme");
                
                }
            }

            return res;
        }

        public BusinessLayerResult<EvernoteUser> GetUserById(int id)
        {

            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result = Find(x => x.Id == id);
            if (res.Result==null)
            {
                res.AddError(ErrorMessageCode.UserNotFound, "kullanıcı bulunamadı");
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> LoginUser(LoginViewModel data)
        {
            //Giriş kontrolü
            //hesap aktive edilmiş mi

            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result =  Find(x => x.Username == data.Username && x.Password == data.Password);
            
            if (res.Result !=null)
            {
                if (!res.Result.IsActive)
                {
                    res.AddError(Entities.Messages.ErrorMessageCode.UserIsNotActive,
                           "Kullanıcı aktifleşmemiştir");
                    res.AddError(Entities.Messages.ErrorMessageCode.CheckYourEmail,
                          "E-postanızı kontrol edin");
   
                }
                
            }
            else
            {
                res.AddError(Entities.Messages.ErrorMessageCode.UsernameOrPassWrong,
                          "Kullanıcı adı veya şifre uyuşmuyor");
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> UpdateProfile(EvernoteUser data)
        {
            EvernoteUser db_user = Find(x => x.Id != data.Id && (x.Username == data.Username || x.Email == data.Email));

            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            if (db_user !=null && db_user.Id !=data.Id)
            {
                if (db_user.Username==data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAlreadyExists, "Kullanıcı adı kayıtlı.");
                }
                if (db_user.Email==data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAlreadyExists, "E-posta adresi kayıtlı");
                }
                return res;
            }
            res.Result = Find(x => x.Id == data.Id);
            res.Result.Email = data.Email;
            res.Result.Name = data.Name;
            res.Result.Surname = data.Surname;
            res.Result.Password = data.Password;
            res.Result.Username = data.Username;
            if (string.IsNullOrEmpty(data.ProfileImageFilename) == false)
            {
                res.Result.ProfileImageFilename = data.ProfileImageFilename;
            }
            if (base.Update(res.Result)==0)
            {
                res.AddError(ErrorMessageCode.ProfilCouldNotUpdate, "Profil güncellenemedi.");
            }
            return res;

        }

        public BusinessLayerResult<EvernoteUser> RemoveUserById(int id)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            EvernoteUser user = Find(x => x.Id == id);

            if (user != null)
            {         
                if (Delete(user)==0)
                {
                    res.AddError(ErrorMessageCode.UserCouldNotRemove, "Kullanıcı silinemedi");
                    return res;
                } 
            
            }
            else
            {
              res.AddError(ErrorMessageCode.UserNotFound, "Kullanıcı bulunamadı");
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> ActivateUser(Guid ActivateId)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result =Find(x => x.ActivateGuid==ActivateId);

            if (res.Result !=null)
            {
                if (res.Result.IsActive)
                {
                    res.AddError(ErrorMessageCode.UserAlreadyActive, "kullanıcı zaten aktif edilmiş.");
                    return res;
                }

                res.Result.IsActive = true;

               Update(res.Result);


            }
            else
            {
                res.AddError(ErrorMessageCode.ActivateIdDoesNotExists, "kullanıcı bulunamadı.");
            }
            return res;
        }
 
        public override int Delete(EvernoteUser user)
        {
            
            NoteManager noteManager = new NoteManager();
            LikedManager likedManager = new LikedManager();
            CommentManager commentManager = new CommentManager();
           
            if (user.Notes.ToList()!=null)
            {
                foreach (Note note in user.Notes.ToList())
                {
                      noteManager.Delete(note);
                    
                }

            }
            
            if (user.Comments.ToList()!=null)
            {
             foreach (Comment com in user.Comments.ToList())
                {   
                     commentManager.Delete(com);
   
                }  
            }
            if (user.Likes.ToList()!=null)
            {  
                foreach (Liked li in user.Likes.ToList())
                {
                
                 likedManager.Delete(li);
                
               
                }

            }
            
            return base.Delete(user);
        }
         
        //method hiding
        public new BusinessLayerResult<EvernoteUser> Insert(EvernoteUser data)
        {
           
            EvernoteUser user = Find(x => x.Username == data.Username || x.Email == data.Email);
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();

            res.Result = data;

            if (user != null)
            {
                if (user.Username == data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAlreadyExists, "Kullanıcı adı kayıtlı.");
                }

                if (user.Email == data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAlreadyExists, "E-posta adresi kayıtlı.");
                }
            }
            else
            {
                res.Result.ProfileImageFilename = "user_boy.png";
                res.Result.ActivateGuid = Guid.NewGuid();

                if (base.Insert(res.Result) == 0)
                {
                    res.AddError(ErrorMessageCode.UserCouldNotInserted, "Kullanıcı eklenemedi.");
                }
            }

            return res;
        }

        public new BusinessLayerResult<EvernoteUser> Update(EvernoteUser data)
        {
               EvernoteUser db_user = Find(x => x.Id != data.Id && (x.Username == data.Username || x.Email == data.Email));
             
               BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
             
              // res.Result = data;
               
               if (db_user != null && db_user.Id != data.Id)
               {
                   if (db_user.Username == data.Username)
                   {
                       res.AddError(ErrorMessageCode.UsernameAlreadyExists, "Kullanıcı adı kayıtlı.");
                   }
                   if (db_user.Email == data.Email)
                   {
                       res.AddError(ErrorMessageCode.EmailAlreadyExists, "E-posta adresi kayıtlı");
                   }
                   return res;
               }
                res.Result = Find(x => x.Id == data.Id);
                res.Result.Email = data.Email;
                res.Result.Name = data.Name;
                res.Result.Surname = data.Surname;
                res.Result.Password = data.Password;
                res.Result.Username = data.Username;
                res.Result.IsActive = data.IsActive;
                res.Result.IsAdmin = data.IsAdmin;
                if (base.Update(res.Result) == 0)
                {
                    res.AddError(ErrorMessageCode.UserCouldNotInserted, "Profil güncellenemedi.");
                }
                return res;
        }

        public BusinessLayerResult<EvernoteUser> Rememberpass(EvernoteUser usser)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result=Find(x => x.Email == usser.Email && x.Username == usser.Username);
            string siteUri = ConfigHelper.Get<string>("SiteRootUri");
            string activateUri = $"{siteUri}/Home/Change/{res.Result.ActivateGuid}";
            string body = $"Merhaba {res.Result.Username};<br><br> Hesabınızın şifresini değiştirmek için  için <a href ='{activateUri}'" + "target='_blank'>tıklayınız</a>.";

            MailHelper.SendMail(body, res.Result.Email, "MyEvernote  Şifre değiştirme.");
            return res;
        }

        public BusinessLayerResult<EvernoteUser> Changes(ChangeViewModel data)
        {

            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result = Find(x => x.Username == data.Username && x.Email == data.Email && x.ActivateGuid == data.getid);

            if (res.Result != null)
            {  
                    res.Result.Password = data.Password;

            }
            else
            {
                res.AddError(ErrorMessageCode.ActivateIdDoesNotExists, "kullanıcı bulunamadı.");
            }
            if (base.Update(res.Result) == 0)
            {
                res.AddError(ErrorMessageCode.ProfilCouldNotUpdate, "Profil güncellenemedi.");
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> Contacts(ContactViewModel conn)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result = Find(x => x.Email == conn.Email && x.Username == conn.Username);
            if (res.Result== null )
            {
                     res.AddError(ErrorMessageCode.UserNotFound, "Kullanıcı kayıtlı değil.");
                
                return res;
            }

            string body =( $"kullanıcı adı " + conn.Username + "\n  E-posta " + conn.Email  + "\n  Mesaj: " + conn.Text);

            MailHelper.SendMail(body, "adavsevernote@outlook.com", "Şikayet/İstek");
            return res;
        }

    }
}
