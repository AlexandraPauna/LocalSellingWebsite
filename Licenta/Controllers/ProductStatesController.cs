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
    public class ProductStatesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: ProductStates
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            return View(db.ProductState.ToList());
        }

        // GET: ProductStates/Details/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductState productState = db.ProductState.Find(id);
            if (productState == null)
            {
                return HttpNotFound();
            }
            return View(productState);
        }

        // GET: ProductStates/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductStates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Include = "ProductStateId,ProductStateName")] ProductState productState)
        {
            if (ModelState.IsValid)
            {
                db.ProductState.Add(productState);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(productState);
        }

        // GET: ProductStates/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductState productState = db.ProductState.Find(id);
            if (productState == null)
            {
                return HttpNotFound();
            }
            return View(productState);
        }

        // POST: ProductStates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "ProductStateId,ProductStateName")] ProductState productState)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productState).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productState);
        }

        // GET: ProductStates/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductState productState = db.ProductState.Find(id);
            if (productState == null)
            {
                return HttpNotFound();
            }
            return View(productState);
        }

        // POST: ProductStates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductState productState = _db.ProductState.Find(id);

            var products = from prd in _db.Products
                           where prd.ProductStateId == id
                           select prd;
            foreach (var product in products)
            {
                var productImages = _db.ProductImages.Where(x => x.ProductId == product.ProductId);
                _db.ProductImages.RemoveRange(productImages);
                var conversations = _db.Conversations.Where(x => x.ProductId == id);
                foreach (var conversation in conversations)
                {
                    var messages = _db.Messages.Where(x => x.ConversationId == conversation.ConversationId);
                    _db.Messages.RemoveRange(messages);
                }
                _db.Conversations.RemoveRange(conversations);
                var interests = _db.Interests.Where(x => x.ProductId == id);
                _db.Interests.RemoveRange(interests);

                _db.Products.Remove(product);

            }

            _db.ProductState.Remove(productState);
            _db.SaveChanges();
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
