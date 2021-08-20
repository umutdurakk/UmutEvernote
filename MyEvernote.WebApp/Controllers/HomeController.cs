using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyEvernote.BusinessLayer;
using MyEvernote.BusinessLayer.Results;
using MyEvernote.Common.Helpers;
using MyEvernote.Entities;
using MyEvernote.Entities.Messages;
using MyEvernote.Entities.ValueObjects;
using MyEvernote.WebApp.Filters;
using MyEvernote.WebApp.Models;
using MyEvernote.WebApp.ViewModels;

namespace MyEvernote.WebApp.Controllers
{
   
    public class HomeController : Controller
    {
        private NoteManager noteManager = new NoteManager();
        private CategoryManager categoryManager = new CategoryManager();
        EvernoteUserManager evernoteUserManager = new EvernoteUserManager();
        LikedManager likedManager = new LikedManager();

        public ActionResult Index()
        {
            //if (CurrentSession.User!=null)
            //{
            //    var notes = likedManager.ListQueryable().Include("LikedUser")
            //     .Include("Note").Where(x => x.LikedUser.Id ==
            // CurrentSession.User.Id).Select(x => x.Note).Include("Category").Include("Owner")
            //     .OrderByDescending(x => x.ModifiedOn);
                

            //    return View(notes.ToList());
            //}

            return View(noteManager.ListQueryable().Where(x => x.IsDraft == false).OrderByDescending(x => x.ModifiedOn).ToList());
            //return View(nm.GetAllNoteQueryble().OrderByDescending(x => x.ModifiedOn).ToList());

        }
        public ActionResult ByCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Category cat = categoryManager.Find(x=>x.Id==id.Value);
            // if (cat == null)
            //{
            // return HttpNotFound();
            //  }

            List<Note> notes = noteManager.ListQueryable().Where(x => x.IsDraft == false && x.CategoryId == id).OrderByDescending(
                x => x.ModifiedOn).ToList() ;
            return View("Index", notes); 
        }
        public ActionResult Search(string searchString)
        {
            List<Note> notes = noteManager.ListQueryable().ToList();
            if (searchString =="")
            {
                return View("Index", noteManager.ListQueryable().Where(x => x.IsDraft == false).OrderByDescending(x => x.ModifiedOn).ToList());
            }
            
            return View("Index", notes.Where(x=>x.Title.ToLower().Contains(searchString.ToLower())).ToList());
        }

        public ActionResult MostLiked()
        {
          
            return View("Index", noteManager.ListQueryable().OrderByDescending(x => x.LikeCount).ToList());
        }

        public ActionResult About()
        {

            return View();
        }
        [Auth]
        public ActionResult ShowProfile()
        { 
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.GetUserById(CurrentSession.User.Id);

            if (res.Errors.Count>0)
            {
                ErrorViewModel errorObj = new ErrorViewModel()
                {
                    Title = "Hata oluştu",
                    Items = res.Errors
                };

                return View("Error", errorObj);
            }
                        
            return View(res.Result);
        }
        [Auth]
        public ActionResult EditProfile()
        {  
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.GetUserById(CurrentSession.User.Id);

            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorObj = new ErrorViewModel()
                {
                    Title = "Hata oluştu",
                    Items = res.Errors
                };

                return View("Error", errorObj);
            }

            return View(res.Result);
        }
       
        [HttpPost]
        [Auth]
        public ActionResult EditProfile(EvernoteUser model, HttpPostedFileBase ProfileImage)
        {
            ModelState.Remove("ModifiedUsername");
            if (ModelState.IsValid)
            {
                if (ProfileImage != null &&
                   (ProfileImage.ContentType == "image/jpeg" ||
                    ProfileImage.ContentType == "image/jpg" ||
                    ProfileImage.ContentType == "image/png"))
                {
                    string filename = $"user_{model.Id}.{ProfileImage.ContentType.Split('/')[1]}";

                    ProfileImage.SaveAs(Server.MapPath($"~/Images/{filename}"));
                    model.ProfileImageFilename = filename;

                }
                
                BusinessLayerResult<EvernoteUser> res = evernoteUserManager.UpdateProfile(model);
                if (res.Errors.Count > 0)
                {
                    ErrorViewModel messages = new ErrorViewModel()
                    {
                        Items = res.Errors,
                        Title = "Profil Güncellemedi.",
                        RedirectingUrl = "/Home/Edit/Profile"
                    };
                    return View("Error", messages);
                }
                Session["login"] = res.Result;
                return RedirectToAction("ShowProfile");
            }
            return View(model);
        }
        [Auth]
        public ActionResult DeleteProfile()
        {
   
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.RemoveUserById(CurrentSession.User.Id);

            if (res.Errors.Count>0)
            {
                ErrorViewModel messages = new ErrorViewModel()
                {
                    Items = res.Errors,
                    Title ="Profil Silinmedi.",
                    RedirectingUrl="/Home/ShowProfile"
                };

                return View("Error", messages);
            }
            Session.Clear();
            return RedirectToAction("Index"); 
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                   
                     BusinessLayerResult<EvernoteUser> res = evernoteUserManager.LoginUser(model);
                     if (res.Errors.Count>0)
                     {
                         
                             if (res.Errors.Find(x => x.Code == ErrorMessageCode.UserIsNotActive) != null)
                               {
                                 ViewBag.SetLink = "http://home//Activate/1234,4567,78980";
                               }

                        res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                        return View(model);
                     }
                FormsAuthentication.SetAuthCookie(res.Result.Username, false);
                CurrentSession.Set<EvernoteUser>("login",res.Result);
                //Session["login"] = res.Result;//Session'a kullanıcı bilgi saklama..
                return RedirectToAction("Index"); // yönlendirme
                                                       
            }         
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }
         
        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {   //Kullanıcı username kontrolü
            //kullanıcı e-posta kontrolü
            //kayıt işlemi
            //Aktivasyon e-postası gönderimi

            if(ModelState.IsValid)
            {
              
                BusinessLayerResult<EvernoteUser> res= evernoteUserManager.RegisterUser(model);
                if (res.Errors.Count>0)
                {
                      res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                      return View();
                }

                OkViewModel notifyObj = new OkViewModel()
                {
                    Title = "Kayıt Başarılı",
                    RedirectingUrl = "/Home/Login",
                };

                notifyObj.Items.Add("Lütfen e-postanıza gönderilen aktivasyon koduna " +
                    "tıklayarak hesabınızı aktive ediniz.Akitive edilmeden not ekleyemez ve beğenme yapamazsınız. ");

                return View("Ok",notifyObj);
            }
            
           
            return View(model);
        }
         
        public ActionResult UserActivate(Guid id)
        {
           
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.ActivateUser(id);
            if (res.Errors.Count >0)
            {
                ErrorViewModel errorObj = new ErrorViewModel()
                {
                    Title = "Geçersiz işlem",
                    Items =res.Errors
                };
              
                return View("Error",errorObj);
            }
            OkViewModel notifyObj = new OkViewModel()
            {
                Title = "Hesap Aktifleştirildi",
                RedirectingUrl = "/Home/Login",
               
            };

            notifyObj.Items.Add("Hesabınız aktifleştirildi.");

            return View("Ok", notifyObj);
            //kullanıcı aktivasyonu sağlanacak..

        }
        [Auth]
        public ActionResult Logout()
        {
            
            Session.Clear();
            return RedirectToAction("Index");
        }
        public ActionResult NoteSh(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = noteManager.Find(x => x.Id == id);
            if (note == null)
            {
                return HttpNotFound();
            }
            
            return View(note);
        }

        public ActionResult AccessDenied()
        {
            return View();
        }
        public ActionResult Remember()
        {

            return View();
            

        }
       
        [HttpPost]
        public ActionResult Remember(string email)
        {
            if (email == null)
            {
                ErrorViewModel errorObj = new ErrorViewModel()
                {
                    Title = "Geçersiz işlem",
                };

                return View("Error", errorObj);
            }
            EvernoteUser usser = evernoteUserManager.Find(x => x.Email == email);
            if (usser == null)
            {
                ErrorViewModel errorObj = new ErrorViewModel()
                {
                    Title = "Geçersiz işlem",
                };

                return View("Error", errorObj);
            }
            evernoteUserManager.Rememberpass(usser);
            WarningViewModel notifyObj = new WarningViewModel()
            {
                Title = "E-postanızı Kontrol edin.",
                RedirectingUrl = "/Home/Login",

            };
            notifyObj.Items.Add("Hesabınızın şifresini değiştirmek için e-postanızı kontrol edin.");

            return View("Warning", notifyObj);
        }
       
        public ActionResult Change(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EvernoteUser uss = evernoteUserManager.Find(x => x.ActivateGuid == id);
            if (uss == null)
            {
                return HttpNotFound();
            }
            ChangeViewModel a = new ChangeViewModel();
            
            Session["guidid"] = id;
            return View();
            

        }
        [HttpPost]
        public ActionResult Change(ChangeViewModel model)
        {
            model.getid = (Guid)Session["guidid"];
            Session["guidid"] = null;
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.Changes(model);
            if (res.Errors.Count > 0)
            {
                res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                return View();
            }
            OkViewModel notifyObj = new OkViewModel()
            {
                Title = "şifre Değiştirildi.",
                RedirectingUrl = "/Home/Login",

            };

            notifyObj.Items.Add("Hesabınızın şifresi değiştirildi.");

            return View("Ok", notifyObj);
            

        }
        public ActionResult Contact()
        {

            return View();


        }

        [HttpPost]
        public ActionResult Contact(ContactViewModel conn)
        {
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.Contacts(conn);
            if (res.Errors.Count > 0)
            {
                res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                return View();
            }
            return View();
        }
    }
}