using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyEvernote.BusinessLayer;
using MyEvernote.BusinessLayer.Results;
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

        
        public ActionResult Index()
        {
           
            return View(noteManager.ListQueryable().OrderByDescending(x => x.ModifiedOn).ToList());
            //return View(nm.GetAllNoteQueryble().OrderByDescending(x => x.ModifiedOn).ToList());

        }
        public ActionResult ByCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Category cat = categoryManager.Find(x=>x.Id==id.Value);
            if (cat == null)
            {
                return HttpNotFound();
            }

            return View("Index", cat.Notes.OrderByDescending(x => x.ModifiedOn).ToList());
        }

        public ActionResult MostLiked()
        {
          
            return View("Index", noteManager.ListQueryable().OrderByDescending(x => x.LikeCount).ToList());
        }

        public ActionResult About()
        {

            return View();
        }

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
        public ActionResult Logout()
        {
            
            Session.Clear();
            return RedirectToAction("Index");
        }



    }
}