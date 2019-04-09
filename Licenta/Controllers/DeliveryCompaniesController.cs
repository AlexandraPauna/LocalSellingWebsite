using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Licenta.Common.Entities;
using Licenta.DataAccess;

namespace Licenta.Controllers
{
    public class DeliveryCompaniesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DeliveryCompanies
        [Authorize(Roles = "Administrator, Editor")]
        public ActionResult Index()
        {
            return View(db.DeliveryCompanies.ToList());
        }

        // GET: DeliveryCompanies/Details/5
        [Authorize(Roles = "Administrator, Editor")]
        public ActionResult Show(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeliveryCompany deliveryCompany = db.DeliveryCompanies.Find(id);
            if (deliveryCompany == null)
            {
                return HttpNotFound();
            }
            return View(deliveryCompany);
        }

        // GET: DeliveryCompanies/Create
        [Authorize(Roles = "Administrator, Editor")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: DeliveryCompanies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Editor")]
        public ActionResult Create([Bind(Include = "DeliveryCompanyId,DeliveryCompanyName")] DeliveryCompany deliveryCompany)
        {
            if (ModelState.IsValid)
            {
                db.DeliveryCompanies.Add(deliveryCompany);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(deliveryCompany);
        }

        // GET: DeliveryCompanies/Edit/5
        [Authorize(Roles = "Administrator, Editor")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeliveryCompany deliveryCompany = db.DeliveryCompanies.Find(id);
            if (deliveryCompany == null)
            {
                return HttpNotFound();
            }
            return View(deliveryCompany);
        }

        // POST: DeliveryCompanies/Edit/5
        [Authorize(Roles = "Administrator, Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DeliveryCompanyId,DeliveryCompanyName")] DeliveryCompany deliveryCompany)
        {
            if (ModelState.IsValid)
            {
                db.Entry(deliveryCompany).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(deliveryCompany);
        }

        // GET: DeliveryCompanies/Delete/5
        [Authorize(Roles = "Administrator, Editor")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeliveryCompany deliveryCompany = db.DeliveryCompanies.Find(id);
            if (deliveryCompany == null)
            {
                return HttpNotFound();
            }
            return View(deliveryCompany);
        }

        // POST: DeliveryCompanies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Editor")]
        public ActionResult DeleteConfirmed(int id)
        {
            DeliveryCompany deliveryCompany = db.DeliveryCompanies.Find(id);
            db.DeliveryCompanies.Remove(deliveryCompany);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
