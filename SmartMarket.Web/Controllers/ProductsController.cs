﻿using BusinessEntities;
using SmartMarket.Web.Business;
using SmartMarket.Web.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SmartMarket.Web.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private SmartMarketDB db = new SmartMarketDB();

        [Authorize(Roles = "admin,company")]
        // GET: Products
        public ActionResult Index()
        {
            IQueryable<Product> products;
            var companyId = GetUserCompanyId();
            if (companyId != Guid.Empty)
            {
                products = db.Products.Include(p => p.Company).Include(p => p.SubCategory).Where(p => p.CompanyId == companyId);
            }
            else
            {
                products = db.Products.Include(p => p.Company).Include(p => p.SubCategory);
            }
            return View(products.ToList());
        }
        [Authorize(Roles = "admin,customer")]
        public ActionResult Search(Guid? companyId, Guid? categoryId, string search = "")
        {
            IQueryable<Product> products = db.Products.Include(p => p.Company).Include(p => p.SubCategory);
            CommonHelper oCommonHelper = new CommonHelper();
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

            if (companyId.HasValue)
            {
                products = products.Where(p => p.CompanyId == companyId.Value);
            }
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.SubCategory.CategoryId == categoryId.Value);
                ViewBag.lstCategories = oCommonHelper.GetNonEmptyCategories(categoryId.Value.ToString());
            }
            else
            {
                ViewBag.lstCategories = oCommonHelper.GetNonEmptyCategories();
            }

            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        [Authorize(Roles = "admin,company")]
        // GET: Products/Create
        public ActionResult Create()
        {
            var product = new Product();
            var companyId = GetUserCompanyId();
            if (companyId != Guid.Empty)
            {
                ViewBag.CompanyId = new SelectList(db.Companies.Where(c => c.Id == companyId), "Id", "CompanyName");
            }
            else
            {
                ViewBag.CompanyId = new SelectList(db.Companies, "Id", "CompanyName");
            }
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "SubCategoryName");
            return View(product);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,company")]
        public ActionResult Create([Bind(Include = "Id,ProductName,Description,SubCategoryId,CompanyId,Price,AvailableStock,Rate,ImageURL")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.Id = Guid.NewGuid();
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var companyId = GetUserCompanyId();
            if (companyId != Guid.Empty)
            {
                ViewBag.CompanyId = new SelectList(db.Companies.Where(c => c.Id == companyId), "Id", "CompanyName", product.CompanyId);
            }
            else
            {
                ViewBag.CompanyId = new SelectList(db.Companies, "Id", "CompanyName", product.CompanyId);
            }

            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "SubCategoryName", product.SubCategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "admin,company")]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            var companyId = GetUserCompanyId();
            if (companyId != Guid.Empty)
            {
                ViewBag.CompanyId = new SelectList(db.Companies.Where(c => c.Id == companyId), "Id", "CompanyName", product.CompanyId);
            }
            else
            {
                ViewBag.CompanyId = new SelectList(db.Companies, "Id", "CompanyName", product.CompanyId);
            }

            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "SubCategoryName", product.SubCategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,company")]
        public ActionResult Edit([Bind(Include = "Id,ProductName,Description,SubCategoryId,CompanyId,Price,AvailableStock,Rate,ImageURL")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var companyId = GetUserCompanyId();
            if (companyId != Guid.Empty)
            {
                ViewBag.CompanyId = new SelectList(db.Companies.Where(c => c.Id == companyId), "Id", "CompanyName", product.CompanyId);
            }
            else
            {
                ViewBag.CompanyId = new SelectList(db.Companies, "Id", "CompanyName", product.CompanyId);
            }
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "SubCategoryName", product.SubCategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "admin,company")]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,company")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private Guid GetUserCompanyId()
        {
            Guid companyId = Guid.Empty;
            Roles oRoles = new Roles();
            if (oRoles.IsUserInRole(User.Identity.Name, "Company"))
            {
                var oUser = new SmartMarket.Web.Business.User();
                long userId = oUser.GetUserId(User.Identity.Name);
                var oCompany = db.Companies.SingleOrDefault(c => c.UserId == userId);
                if (oCompany != default(Company))
                {
                    companyId = oCompany.Id;
                }
            }
            return companyId;
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetProdutPrice(Guid productId)
        {
            double pricePerItem = db.Products.SingleOrDefault(p => p.Id == productId).Price;

            return Json(new { PricePerItem = pricePerItem }, JsonRequestBehavior.AllowGet);
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
