using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MyEvernote.Entities;

namespace MyEvernote.DataAccessLayer.EntityFramework
{
    public class MyIntializer : CreateDatabaseIfNotExists<DatabaseContext>
    {
        protected override void Seed(DatabaseContext context)
        {

            EvernoteUser admin = new EvernoteUser()
            {
                Name = "Umut",
                Surname = "Durak",
                Email = "umutdurak@gmail.com",
                ActivateGuid = Guid.NewGuid(),
                IsActive = true,
                IsAdmin = true,
                Username = "Umutd",
                ProfileImageFilename = "user_boy.png",
                Password = "durak",
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now.AddMinutes(5),
                ModifiedUserName = "Umutd"
            };
            EvernoteUser stuser = new EvernoteUser()
            {
                Name = "berk",
                Surname = "Durak",
                Email = "berkdurak@gmail.com",
                ActivateGuid = Guid.NewGuid(),
                IsActive = true,
                IsAdmin = false,
                Username = "berkd",
                ProfileImageFilename = "user_boy.png",
                Password = "berk",
                CreatedOn = DateTime.Now.AddHours(1),
                ModifiedOn = DateTime.Now.AddMinutes(65),
                ModifiedUserName = "umutd"
            };
            context.EvernoteUsers.Add(admin);
            context.EvernoteUsers.Add(stuser);

            for (int i = 0; i < 8; i++)
            {
                EvernoteUser user = new EvernoteUser()
                {
                    Name = FakeData.NameData.GetFirstName(),
                    Surname = FakeData.NameData.GetSurname(),
                    Email = FakeData.NetworkData.GetEmail(),
                    ActivateGuid = Guid.NewGuid(),
                    IsActive = true,
                    IsAdmin = false,
                    Username = $"user{i}",
                    ProfileImageFilename = "user_boy.png",
                    Password = "123",
                    CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                    ModifiedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                    ModifiedUserName = $"user{i}"
                };
                context.EvernoteUsers.Add(user);
            }

            context.SaveChanges();

            List<EvernoteUser> userlist = context.EvernoteUsers.ToList();

            for (int i = 0; i < 10; i++)
            {
                Category cat = new Category()
                {
                    Title = FakeData.PlaceData.GetState(),
                    Description = FakeData.PlaceData.GetAddress(),
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    ModifiedUserName = "umutd"

                };
                context.Categories.Add(cat);

                for (int k = 0; k < FakeData.NumberData.GetNumber(5, 9); k++)
                {
                    EvernoteUser owner = userlist[FakeData.NumberData.GetNumber(0, userlist.Count - 1)];
                    Note note = new Note()
                    {
                        Title = FakeData.TextData.GetAlphabetical(FakeData.NumberData.GetNumber(5, 25)),
                        Text = FakeData.TextData.GetSentences(FakeData.NumberData.GetNumber(1, 3)),

                        IsDraft = false,
                        LikeCount = FakeData.NumberData.GetNumber(1, 9),
                        Owner = owner,
                        CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                        ModifiedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                        ModifiedUserName = owner.Username,
                    };
                    cat.Notes.Add(note);
                    for (int j = 0; j < FakeData.NumberData.GetNumber(3, 5); j++)
                    {
                        EvernoteUser comment_owner = userlist[FakeData.NumberData.GetNumber(0, userlist.Count - 1)];
                        Comment comment = new Comment()
                        {
                            Text = FakeData.TextData.GetSentence(),

                            Owner = comment_owner,
                            CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                            ModifiedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                            ModifiedUserName = comment_owner.Username,

                        };
                        note.Comments.Add(comment);
                    }

                    for (int l = 0; l < note.LikeCount; l++)
                    {
                        Liked liked = new Liked()
                        {
                            LikedUser = userlist[l]
                        };
                        note.Likes.Add(liked);
                    }
                }
            }

            context.SaveChanges();
        }
    }
}
