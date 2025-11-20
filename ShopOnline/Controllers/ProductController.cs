using Microsoft.AspNetCore.Mvc;
using ShopOnline.Helpers;
using ShopOnline.Models;
using ShopOnline.ViewModels;

namespace ShopOnline.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            //recupero la lista dei prodotti
            List<Product> products = DbHelper.GetProducts();

            //List<ProductViewModel> productsViewModel = new List<ProductViewModel>();

            //foreach (Product product in products)
            //{
            //    var pViewModel = new ProductViewModel()
            //    {
            //        Id = product.Id,
            //        Name = product.Name,
            //        Description = product.Description,
            //        Price = product.Price,
            //    };

            //    productsViewModel.Add(pViewModel);
            //}

            List<ProductViewModel> productViewModels = products.Select(p => new ProductViewModel()
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
            }
            ).ToList();

            return View(productViewModels);
        }

        [HttpGet("{id:guid}")]
        public IActionResult Detail(Guid id)
        {
            Product? existingProduct = DbHelper.GetProductById(id);

            if (existingProduct == null)
            {
                TempData["FindError"] = "Prodotto non trovato";

                return RedirectToAction("Index", "Product");
            }

            var productDetailViewModel = new ProductDetailViewModel()
            {
                Id = existingProduct.Id,
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                Price = existingProduct.Price,
            };

            return View(productDetailViewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateProductViewModel createProductViewModel)
        {
            Product newProduct = new Product()
            {
                Id = Guid.NewGuid(),
                Name = createProductViewModel.Name,
                Description = createProductViewModel.Description,
                Price = createProductViewModel.Price,
            };

            bool creationResult = DbHelper.AddProduct(newProduct);

            if (!creationResult)
            {
                TempData["CreationError"] = "Errone durante la creazione del prodotto";

                return RedirectToAction("Create", "Product");
            }

            return RedirectToAction("Index", "Product");
        }

        [HttpPost("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var deleteResult = DbHelper.DeleteProductById(id);

            if (!deleteResult)
            {

                TempData["DeleteError"] = "Errone durante l'eliminazione del prodotto";
            }

            return RedirectToAction("Index", "Product");
        }
    }
}
