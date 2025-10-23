﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StoreMVC.Models;
using StoreMVC.Services;

namespace StoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var products = context.Products.ToList();
            return View(products);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            // 1️⃣ Model validation
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The ImageFile is required");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            // 2️⃣ Resim klasörünü kontrol et ve oluştur
            string uploadFolder = Path.Combine(environment.WebRootPath, "products");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // 3️⃣ Resim dosyasını kaydet
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDto.ImageFile!.FileName);
            string imageFullPath = Path.Combine(uploadFolder, newFileName);

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // 4️⃣ Product nesnesini oluştur ve database'e kaydet
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,  // decimal tipinde, form input'u number olmalı
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now
            };

            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            //create productDto from product
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,

            };
            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

            return View(productDto);

        }
        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
                return View(productDto);
            }
            // update the image file if we have e new image file
            string newFileName = product.ImageFileName;

            if (productDto.ImageFile != null)
            {

                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

                string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }
                // delete the old image
                string oldImageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }
            //update the product in the database

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();
            return RedirectToAction("Index", "Products");

        }
        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            string imageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);
            context.Products.Remove(product);
            context.SaveChanges(true);
            
            return RedirectToAction("Index", "Products");
        }
    }
}








    

