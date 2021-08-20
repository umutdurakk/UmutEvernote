using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MyEvernote.BusinessLayer;
using MyEvernote.Entities;
using MyEvernote.WebApp.Filters;
using MyEvernote.WebApp.Models;

namespace MyEvernote.WebApp.Controllers
{
    
    public class NoteController : Controller
    {
        NoteManager noteManager = new NoteManager();
        CategoryManager categoryManager = new CategoryManager();
        LikedManager likedManager = new LikedManager();
        [Auth]
        public ActionResult Index()
        {

            var notes = noteManager.ListQueryable().Include("Category").Include("Owner").Where(
                x => x.Owner.Id == CurrentSession.User.Id).OrderByDescending(
                x => x.ModifiedOn);
            return View(notes.ToList());
        }
        [Auth]
        public ActionResult MyLikedNotes()
        {
            var notes=likedManager.ListQueryable().Include("LikedUser")
                .Include("Note").Where(x => x.LikedUser.Id ==
            CurrentSession.User.Id).Select(x => x.Note).Include("Category").Include("Owner")
                .OrderByDescending(x => x.ModifiedOn);
            
            
            return View("Index",notes.ToList());
        }

        [Auth]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = noteManager.Find(x => x.Id==id); 
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

        [Auth]
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(categoryManager.List(), "Id", "Title");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        public ActionResult Create(Note note, HttpPostedFileBase noteImage)
        {
            ModelState.Remove("NoteImageFilename");
            ModelState.Remove("CreatedOn");
            ModelState.Remove("ModifiedOn");
            ModelState.Remove("ModifiedUsername");
            if (ModelState.IsValid)
            {
                if (noteImage != null &&
                  (noteImage.ContentType == "image/jpeg" ||
                   noteImage.ContentType == "image/jpg" ||
                   noteImage.ContentType == "image/png"))
                {
                    string filename = $"user_{note.Id}.{noteImage.ContentType.Split('/')[1]}";

                    noteImage.SaveAs(Server.MapPath($"~/NoteImages/{filename}"));
                    note.NoteImageFilename = filename;

                }
                note.Owner = CurrentSession.User;
                noteManager.Insert(note);
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(categoryManager.List(), "Id", "Title", note.CategoryId);
            return View(note);
        }
        [Auth]
        public ActionResult Edit(int? id)
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
            ViewBag.CategoryId = new SelectList(categoryManager.List(), "Id", "Title", note.CategoryId);
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Auth]
        public ActionResult Edit(Note note, HttpPostedFileBase noteImage)
        {
            ModelState.Remove("CreatedOn");
            ModelState.Remove("ModifiedOn");
            ModelState.Remove("ModifiedUsername");
            if (ModelState.IsValid)
            {
                string filename =null;
                if (noteImage != null &&
                     (noteImage.ContentType == "image/jpeg" ||
                      noteImage.ContentType == "image/jpg" ||
                      noteImage.ContentType == "image/png"))
                {
                     filename = $"note_{note.Id}.{noteImage.ContentType.Split('/')[1]}";

                    noteImage.SaveAs(Server.MapPath($"~/NoteImages/{filename}"));
                    

                }
                Note db_note= noteManager.Find(x => x.Id == note.Id);
                db_note.IsDraft = note.IsDraft;
                db_note.CategoryId = note.CategoryId;
                db_note.Text = note.Text;
                db_note.Title = note.Title;
                if (filename!=null)
                {
                    db_note.NoteImageFilename = filename;
                } 
                noteManager.Update(db_note);


                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(categoryManager.List(), "Id", "Title", note.CategoryId);
            return View(note);
        }

        [Auth]
        public ActionResult Delete(int? id)
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


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Auth]
        public ActionResult DeleteConfirmed(int id)
        {
            Note note = noteManager.Find(x => x.Id == id);
            noteManager.Delete(note);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Auth]
        public ActionResult GetLiked(int[] ids)
        {
            if (CurrentSession.User != null)
            {
                int userid = CurrentSession.User.Id;
                List<int> likedNoteIds = new List<int>();

                if (ids != null)
                {
                    likedNoteIds = likedManager.List(
                        x => x.LikedUser.Id == userid && ids.Contains(x.Note.Id)).Select(
                        x => x.Note.Id).ToList();
                }
                else
                {
                    likedNoteIds = likedManager.List(
                        x => x.LikedUser.Id == userid).Select(
                        x => x.Note.Id).ToList();
                }

                return Json(new { result = likedNoteIds });
            }
            else
            {
                return Json(new { result = new List<int>() });
            }
        }

        [HttpPost]
        [Auth]
        public ActionResult SetLikedState(int noteid,bool liked)
        {
            int res=0;
            Liked like = likedManager.Find(x => x.Note.Id == noteid && x.LikedUser.Id == CurrentSession.User.Id);
            Note note = noteManager.Find(x => x.Id == noteid);

            if (like != null && liked ==false)
            {
                Session["del"] = 1;
                res =likedManager.Delete(like);

            }
            else if (like==null && liked==true)
            {
                Session["ıns"] = 1;
                res = likedManager.Insert(new Liked()
                {
                   
                    LikedUser = CurrentSession.User,
                    Note = note,
                    
                });
                
            }

            if (res>0)
            {
                if (liked)
                {
                    note.LikeCount++;

                }
                else
                {
                    note.LikeCount--;
                }
                res = noteManager.LikeUpdate(note);
                return Json(new {hasError=false,errorMessage=string.Empty,result=note.LikeCount });
            }
            return Json(new { hasError = true, errorMessage = "Beğenme işlemi gerçekleştirilemedi.", result = note.LikeCount });
        }


        public ActionResult GetNoteText(int? id)
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

            return PartialView("_PartialNoteText", note);
        }


    }
}
