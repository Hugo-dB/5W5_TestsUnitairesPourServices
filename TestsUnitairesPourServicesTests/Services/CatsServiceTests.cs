using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsUnitairesPourServices.Data;
using TestsUnitairesPourServices.Exceptions;
using TestsUnitairesPourServices.Models;
using TestsUnitairesPourServices.Services;

namespace TestsUnitairesPourServices.Services.Tests
{
    [TestClass()]
    public class CatsServiceTests
    {
        private DbContextOptions<ApplicationDBContext> _options;

        public CatsServiceTests() 
        {
            _options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "CatsService")
                .UseLazyLoadingProxies(true)
                .Options;


        }

        [TestInitialize]
        public void Init()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);

            db.House.Add(new House { Id = 1, Address = "Adresse 1", OwnerName = "Billy", Cats = [] });
            db.House.Add(new House { Id = 2, Address = "Adresse 2", OwnerName = "Bobe", Cats = [] });
            db.SaveChanges();

            db.Cat.Add(new Cat { Id = 1, Age = 4, Name = "Bobi", House = db.House.Find(1) });
            db.Cat.Add(new Cat { Id = 2, Age = 3, Name = "Timmy", House = null });
            db.SaveChanges();

            House house = db.House.Find(1);
            house.Cats.Add(db.Cat.Find(1));
            db.SaveChanges();
        }

        [TestCleanup]
        public void Dispose()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            db.Cat.RemoveRange(db.Cat);
            db.House.RemoveRange(db.House);
            db.SaveChanges();
        }

        [TestMethod()]
        public void ValidMove()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);

            House MaisonFrom = db.House.Find(1);
            House MaisonTo = db.House.Find(2);

            service.Move(1, MaisonFrom, MaisonTo);

            Assert.AreEqual(MaisonTo, db.Cat.Find(1).House);
        }

        [TestMethod()]
        public void CatWithNoHouseMove()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);

            House MaisonFrom = db.House.Find(1);
            House MaisonTo = db.House.Find(2);

            Exception e = Assert.ThrowsException<WildCatException>(() => service.Move(2, MaisonFrom, MaisonTo));
            Assert.AreEqual("On n'apprivoise pas les chats sauvages", e.Message);
        }

        [TestMethod()]
        public void UnvalidOriginHouseMove()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);

            House MaisonTo = db.House.Find(2);

            Exception e = Assert.ThrowsException<DontStealMyCatException>(() => service.Move(1, MaisonTo, MaisonTo));

            Assert.AreEqual("Touche pas à mon chat!", e.Message);
        }

        [TestMethod()]
        public void UnvalidCatIdMove()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService service = new CatsService(db);

            House MaisonFrom = db.House.Find(1);
            House MaisonTo = db.House.Find(2);

            Assert.IsNull(service.Move(3, MaisonTo, MaisonTo));
        }
    }
}