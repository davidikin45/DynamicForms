using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DynamicForms.Web.Controllers
{
    [Route("new")]
    public class ApiFormController : Controller
    {
        #region Edit
        [Route("form")]
        public IActionResult Edit()
        {
            var payload = new Payload();
            payload.Name = "David";
            payload.Addresses = new List<Payload2>();
            payload.Addresses.Add(new Payload2() { Address = "abc" });
            payload.Addresses.Add(new Payload2() { Address = "def" });
            payload.Payload3 = new Payload3() { Test = "Test" };
            payload.Numbers = new List<int>() { 1,2 };

            ViewBag.EditMode = true;
            ViewBag.CreateMode = false;

            return View("DynamicForm", payload);
        }

        [HttpGet]
        [Route("new/{*collection}")]
        public virtual ActionResult CreateCollectionItem(string collection)
        {
            ViewBag.Collection = collection.Replace("/", ".");
            ViewBag.CollectionIndex = Guid.NewGuid().ToString();

            var type = GetCollectionExpressionCreateType(collection, typeof(Payload));
            var instance = Activator.CreateInstance(type);

            return PartialView("_CreateCollectionItem", instance);
        }

        private Type GetCollectionExpressionCreateType(string collectionExpression, Type type)
        {
            var collectionExpressionParts = GetCollectionExpressionParts(collectionExpression);
            for (int i = 0; i < collectionExpressionParts.Count; i++)
            {
                var collection = collectionExpressionParts[i];
                type = GetGenericArguments(type, collection).Single();
            }

            return type;
        }

        private bool HasProperty(Type type, string propName)
        {
            return type.GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

        private Type[] GetGenericArguments(Type type, string propName)
        {
            if (HasProperty(type, propName))
            {
                return type.GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).PropertyType.GenericTypeArguments;
            }
            return null;
        }

        private List<string> GetCollectionExpressionParts(string collectionExpresion)
        {
            return collectionExpresion.Split('/').Select(p => p.Split('[')[0]).ToList();
        }

        #endregion

        #region Update
        [Route("contact")]
        [HttpGet]
        public IActionResult Contact()
        {

            return StatusCode(450);

            return View(new ContactViewModel());
        }

        [ValidateAntiForgeryToken]
        [Route("contact")]
        [HttpPost]
        public IActionResult Contact(IFormCollection formData, RouteData routeData, QueryString query)
        {

            
            if(ModelState.IsValid)
            {
                //Submit Contact Form

                return RedirectToAction("Edit");
            }

            return View(new ContactViewModel());
        }
        #endregion
    }

    public class ContactViewModel
    {
        [Required]
        public string Name { get; set; }
    }
    public class Payload
    {

        public string Name { get; set; }

        public DateTime DateTime { get; set; }
        public Payload3 Payload3 { get; set; }

        [DataType("ModelRepeater")]
        public List<Payload2> Addresses { get; set; }
        public List<int> Numbers { get; set; }
    }

    public class Payload2
    {
        public string Address { get; set; }
    }

    public class Payload3
    {
        public string Test { get; set; }
    }
}
